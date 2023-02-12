using AutoMapper;
using FluentValidation;
using MagicVilla_CouponAPI;
using MagicVilla_CouponAPI.Data;
using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Models.DTO;
using MagicVilla_CouponAPI.Repository.IRepository;
using MagicVilla_CouponAPI.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(MappingConfig));
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddDbContext<ApplicationDbContext>(option =>
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<ICouponRepository, CouponRepository>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/api/coupon", async (ICouponRepository _couponRepo,ILogger<Program> _logger) =>
{
    APIResponse response = new()
    {
        Result = await _couponRepo.GetAllAsync(),
        StatusCode = HttpStatusCode.OK
    };
    _logger.LogInformation("getting all coupons");
    return Results.Ok(response);
});

app.MapGet("/api/coupon/{id:int}", async (ICouponRepository _couponRepo, int id) =>
{
    var result = await _couponRepo.GetAsync(id);
    APIResponse response = new()
    {
        Result = result!,
        StatusCode = HttpStatusCode.OK
    };
    return Results.Ok(response);
});

app.MapPost("/api/coupon", async (ICouponRepository _couponRepo, IValidator<CouponCreateDTO> _validator,IMapper _mapper,[FromBody] CouponCreateDTO couponDto) =>
{
    APIResponse response = new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };
    var validationResult=await _validator.ValidateAsync(couponDto);  
    if (!validationResult.IsValid)
    {
        response.ErrorMessages.Add(validationResult.Errors.FirstOrDefault()!.ToString());
        return Results.BadRequest(response);
    }
    if (await _couponRepo.GetAsync(couponDto.Name) != null)
    {
        response.ErrorMessages.Add("coupon name already exists");
        return Results.BadRequest(response);
    }
    Coupon coupon = _mapper.Map<Coupon>(couponDto);
    await _couponRepo.CreateAsync(coupon);
    await _couponRepo.SaveAsync();
    CouponDTO couponDTO = _mapper.Map<CouponDTO>(coupon);
    response.Result = couponDTO;
    response.IsSuccess = true;
    response.StatusCode = HttpStatusCode.OK;
    return Results.Ok(response);
});

app.MapPut("/api/coupon", async (ICouponRepository _couponRepo, IValidator<CouponUpdateDTO> _validator, IMapper _mapper, [FromBody] CouponUpdateDTO couponDto) =>
{
    APIResponse response = new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };
    var validationResult = await _validator.ValidateAsync(couponDto);
    if (!validationResult.IsValid)
    {
        response.ErrorMessages.Add(validationResult.Errors.FirstOrDefault()!.ToString());
        return Results.BadRequest(response);
    }
    var coupon = await _couponRepo.GetAsync(couponDto.Id);
    if(coupon == null)
    {
        response.ErrorMessages.Add("Coupon not exist against given id");
        return Results.BadRequest(response);
    }
    coupon.Name=couponDto.Name;
    coupon.IsActive= couponDto.IsActive;
    coupon.Percent=couponDto.Percent;
    await _couponRepo.UpdateAsync(coupon);
    await _couponRepo.SaveAsync();
    response.Result = _mapper.Map<CouponDTO>(coupon);
    response.IsSuccess = true;

    response.StatusCode = HttpStatusCode.OK;
    return Results.Ok(response);
});

app.MapDelete("/api/coupon/{id:int}", async (ICouponRepository _couponRepo, int id) =>
{
    APIResponse response = new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };
    var coupon = await _couponRepo.GetAsync(id);
    if (coupon == null)
    {
        response.ErrorMessages.Add("Coupon not exist against given id");
        return Results.BadRequest(response);
    }
    await _couponRepo.RemoveAsync(coupon);
    await _couponRepo.SaveAsync();
    response.IsSuccess = true;
    response.StatusCode = HttpStatusCode.OK;
    return Results.Ok(response);
});

app.UseHttpsRedirection();


app.Run();

