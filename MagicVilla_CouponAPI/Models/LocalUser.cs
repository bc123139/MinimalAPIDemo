namespace MagicVilla_CouponAPI.Models
{
    public class LocalUser
    {
        public int ID { get; set; }
        public string UserName { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Role { get; set; } = null!;
    }
}
