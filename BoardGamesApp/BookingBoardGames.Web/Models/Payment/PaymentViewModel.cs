using System;
using System.ComponentModel.DataAnnotations;

namespace BookingBoardGames.Web.Models.Payment
{
    public class PaymentViewModel
    {
        public int PaymentId { get; set; }

        public int MessageId { get; set; }

        public string GameName { get; set; } = string.Empty;

        public string OwnerName { get; set; } = string.Empty;

        public string RentalPeriod { get; set; } = string.Empty;

        public decimal AccountBalance { get; set; }

        [Required]
        public int RequestIdentifier { get; set; }

        [Required]
        public int ClientIdentifier { get; set; }

        [Required]
        public int OwnerIdentifier { get; set; }

        [Required]
        [Display(Name = "Transaction Amount")]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Payment Method")]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Transaction Date")]
        [DataType(DataType.Date)]
        public DateTime DateOfTransaction { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Card Number")]
        [StringLength(19, MinimumLength = 12)]
        public string CardNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "CVV")]
        [StringLength(4, MinimumLength = 3)]
        public string Cvv { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Cardholder Name")]
        [StringLength(100)]
        public string CardholderName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Expiry")]
        [StringLength(5, MinimumLength = 4)]
        public string Expiry { get; set; } = string.Empty;
    }
}