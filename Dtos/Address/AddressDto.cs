namespace icone_backend.Dtos.Address
{
    public class AddressDto
    {
        public string CountryCode { get; set; } = null!;
        public string PostalCode { get; set; } = null!;
        public string StateRegion { get; set; } = null!;
        public string City { get; set; } = null!;
        public string Line1 { get; set; } = null!;
        public string? Line2 { get; set; }
    }
}
