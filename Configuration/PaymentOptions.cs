using System.ComponentModel.DataAnnotations;

namespace TmsApi.Configuration
{
    public class PaymentOptions
    {
        [Required(ErrorMessage = "The GatewayUrl field is required.")]
        [Url(ErrorMessage = "GatewayUrl must be a valid URL.")]
        public required string GatewayUrl { get; init; }

        [Range(100.0, 100000.0, ErrorMessage = "MaxDepositBirr must be between 100 and 100,000.")]
        public decimal MaxDepositBirr { get; init; }
    }
}