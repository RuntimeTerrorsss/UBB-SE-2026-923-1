using System;
using System.ComponentModel.DataAnnotations;

namespace BookingBoardGames.Web.Models.Payment
{
    public class PaymentViewModel
    {
        public int PaymentId { get; set; }

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
    }
}
