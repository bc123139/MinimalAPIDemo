namespace MagicVilla_CouponAPI.Models.DTO
{
    public class CouponCreateDTO
    {
        public string Name { get; set; } = null!;
        public int Percent { get; set; }
        public bool IsActive { get; set; }
    }
}
