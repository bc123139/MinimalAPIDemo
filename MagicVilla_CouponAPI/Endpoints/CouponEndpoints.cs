using AutoMapper;
using FluentValidation;
using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Models.DTO;
using MagicVilla_CouponAPI.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace MagicVilla_CouponAPI.Endpoints
{
    public static class CouponEndpoints
    {
        public static void ConfigureCouponEndpoints(this WebApplication app)
        {
            app.MapGet("/api/coupon", GetAllCoupons);

            app.MapGet("/api/coupon/{id:int}", GetCoupon);

            app.MapPost("/api/coupon", CreateCoupon);

            app.MapPut("/api/coupon", UpdateCoupon);

            app.MapDelete("/api/coupon/{id:int}", DeleteCoupon);
        }

        private async static Task<IResult> GetAllCoupons(ICouponRepository _couponRepo, ILogger<Program> _logger)
        {
            APIResponse response = new()
            {
                Result = await _couponRepo.GetAllAsync(),
                StatusCode = HttpStatusCode.OK
            };
            _logger.LogInformation("getting all coupons");
            return Results.Ok(response);
        }

        private async static Task<IResult> GetCoupon(ICouponRepository _couponRepo, int id)
        {
            var result = await _couponRepo.GetAsync(id);
            APIResponse response = new()
            {
                IsSuccess=true,
                Result = result!,
                StatusCode = HttpStatusCode.OK
            };
            return Results.Ok(response);
        }

        private async static Task<IResult> CreateCoupon(ICouponRepository _couponRepo, IValidator<CouponCreateDTO> _validator, IMapper _mapper, [FromBody] CouponCreateDTO couponDto)
        {
            APIResponse response = new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };
            var validationResult = await _validator.ValidateAsync(couponDto);
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
        }

        private async static Task<IResult> UpdateCoupon(ICouponRepository _couponRepo, IValidator<CouponUpdateDTO> _validator, IMapper _mapper, [FromBody] CouponUpdateDTO couponDto)
        {
            APIResponse response = new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };
            var validationResult = await _validator.ValidateAsync(couponDto);
            if (!validationResult.IsValid)
            {
                response.ErrorMessages.Add(validationResult.Errors.FirstOrDefault()!.ToString());
                return Results.BadRequest(response);
            }
            var coupon = await _couponRepo.GetAsync(couponDto.Id);
            if (coupon == null)
            {
                response.ErrorMessages.Add("Coupon not exist against given id");
                return Results.BadRequest(response);
            }
            coupon.Name = couponDto.Name;
            coupon.IsActive = couponDto.IsActive;
            coupon.Percent = couponDto.Percent;
            await _couponRepo.UpdateAsync(coupon);
            await _couponRepo.SaveAsync();
            response.Result = _mapper.Map<CouponDTO>(coupon);
            response.IsSuccess = true;

            response.StatusCode = HttpStatusCode.OK;
            return Results.Ok(response);
        }

        private async static Task<IResult> DeleteCoupon(ICouponRepository _couponRepo, int id)
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
        }
    }
}
