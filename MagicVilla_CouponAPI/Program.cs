using AutoMapper;
using MagicVilla_CouponAPI;
using MagicVilla_CouponAPI.Data;
using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Models.DTO;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(MappingConfig));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/api/coupon", (ILogger<Program> _logger) =>
{
    _logger.LogInformation("getting all coupons");
    return Results.Ok(CouponStore.couponList);
});

app.MapGet("/api/coupon/{id:int}", (int id) =>
{
    return Results.Ok(CouponStore.couponList.FirstOrDefault(x => x.Id == id));
});

app.MapPost("/api/coupon", (IMapper _mapper,[FromBody] CouponCreateDTO couponDto) =>
{
    if (string.IsNullOrWhiteSpace(couponDto.Name))
    {
        return Results.BadRequest("Invalid coupon name");
    }
    if (CouponStore.couponList.FirstOrDefault(x => x.Name.ToLower() == couponDto.Name.ToLower()) != null)
    {
        return Results.BadRequest("coupon name already exists");
    }
    Coupon coupon = _mapper.Map<Coupon>(couponDto);
    coupon.Id = CouponStore.couponList.OrderByDescending(x => x.Id).FirstOrDefault()!.Id + 1;
    CouponStore.couponList.Add(coupon);
    CouponDTO couponDTO = _mapper.Map<CouponDTO>(coupon);
    return Results.Ok(couponDTO);
});

app.MapPut("/api/coupon", (int id) =>
{
    return Results.Ok(CouponStore.couponList.FirstOrDefault(x => x.Id == id));
});

app.MapDelete("/api/coupon/{id:int}", (int id) =>
{
    return Results.Ok(CouponStore.couponList.FirstOrDefault(x => x.Id == id));
});

app.UseHttpsRedirection();


app.Run();

