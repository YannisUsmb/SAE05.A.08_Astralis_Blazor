namespace Astralis_BlazorApp.Models
{
    public class User
    {
        public int Id { get; set; }

        public int? PhonePrefixId { get; set; }

        public int? DeliveryId { get; set; }

        public int? InvoicingId { get; set; }

        public int UserRoleId { get; set; }

        public string LastName { get; set; } = null!;

        public string FirstName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string? Phone { get; set; }

        public string Username { get; set; } = null!;

        public string Password { get; set; } = null!;

        public DateOnly InscriptionDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);

        public GenderType Gender { get; set; } = GenderType.Unknown;

        public bool IsPremium { get; set; }

        public bool MultiFactorAuthentification { get; set; }
    }
}
