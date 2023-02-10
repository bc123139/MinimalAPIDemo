using MagicVilla_CouponAPI.Data;
using MagicVilla_CouponAPI.Models;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.MapPost("/api/coupon", ([FromBody] Coupon coupon) =>
{
    if (coupon.Id != 0 && string.IsNullOrWhiteSpace(coupon.Name))
    {
        return Results.BadRequest("Invalid id or coupon name");
    }
    if (CouponStore.couponList.FirstOrDefault(x => x.Name.ToLower() == coupon.Name.ToLower()) != null)
    {
        return Results.BadRequest("coupon name already exists");
    }
    CouponStore.couponList.Add(coupon);

    return Results.Ok(coupon);
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

