using AutoMapper;
using FluentValidation;
using MagicVilla_CouponAPI;
using MagicVilla_CouponAPI.Data;
using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(MappingConfig));
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/api/coupon", (ILogger<Program> _logger) =>
{
    APIResponse response = new()
    {
        Result = CouponStore.couponList,
        StatusCode = HttpStatusCode.OK
    };
    _logger.LogInformation("getting all coupons");
    return Results.Ok(response);
});

app.MapGet("/api/coupon/{id:int}", (int id) =>
{
    APIResponse response = new()
    {
        Result = CouponStore.couponList.FirstOrDefault(x => x.Id == id)!,
        StatusCode = HttpStatusCode.OK
    };
    return Results.Ok(response);
});

app.MapPost("/api/coupon", async (IValidator<CouponCreateDTO> _validator,IMapper _mapper,[FromBody] CouponCreateDTO couponDto) =>
{
    APIResponse response = new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };
    var validationResult=await _validator.ValidateAsync(couponDto);  
    if (!validationResult.IsValid)
    {
        response.ErrorMessages.Add(validationResult.Errors.FirstOrDefault()!.ToString());
        return Results.BadRequest(response);
    }
    if (CouponStore.couponList.FirstOrDefault(x => x.Name.ToLower() == couponDto.Name.ToLower()) != null)
    {
        response.ErrorMessages.Add("coupon name already exists");
        return Results.BadRequest(response);
    }
    Coupon coupon = _mapper.Map<Coupon>(couponDto);
    coupon.Id = CouponStore.couponList.OrderByDescending(x => x.Id).FirstOrDefault()!.Id + 1;
    CouponStore.couponList.Add(coupon);
    CouponDTO couponDTO = _mapper.Map<CouponDTO>(coupon);
    response.Result = couponDTO;
    response.IsSuccess = true;
    response.StatusCode = HttpStatusCode.OK;
    return Results.Ok(response);
});

app.MapPut("/api/coupon", async (IValidator<CouponUpdateDTO> _validator, IMapper _mapper, [FromBody] CouponUpdateDTO couponDto) =>
{
    APIResponse response = new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };
    var validationResult = await _validator.ValidateAsync(couponDto);
    if (!validationResult.IsValid)
    {
        response.ErrorMessages.Add(validationResult.Errors.FirstOrDefault()!.ToString());
        return Results.BadRequest(response);
    }
    var coupon = CouponStore.couponList.FirstOrDefault(x => x.Id == couponDto.Id);
    if(coupon == null)
    {
        response.ErrorMessages.Add("Coupon not exist against given id");
        return Results.BadRequest(response);
    }
    coupon.Name=couponDto.Name;
    coupon.IsActive= couponDto.IsActive;
    coupon.Percent=couponDto.Percent;
    response.Result = _mapper.Map<CouponDTO>(coupon);
    response.IsSuccess = true;

    response.StatusCode = HttpStatusCode.OK;
    return Results.Ok(response);
});

app.MapDelete("/api/coupon/{id:int}", (int id) =>
{
    APIResponse response = new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };
    var coupon = CouponStore.couponList.FirstOrDefault(x => x.Id == id);
    if (coupon == null)
    {
        response.ErrorMessages.Add("Coupon not exist against given id");
        return Results.BadRequest(response);
    }
    CouponStore.couponList.Remove(coupon);
    response.IsSuccess = true;
    response.StatusCode = HttpStatusCode.OK;
    return Results.Ok(response);
});

app.UseHttpsRedirection();


app.Run();

