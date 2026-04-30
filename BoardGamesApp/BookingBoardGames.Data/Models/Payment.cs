using System;
using System.ComponentModel.DataAnnotations;

public class Payment
{
    [Key] 
    public int TransactionIdentifier { get; set; }
    
    public decimal PaidAmount { get; set; }
    public string PaymentMethod { get; set; }
    public DateTime? DateOfTransaction { get; set; }
    public DateTime? DateConfirmedBuyer { get; set; }
    public DateTime? DateConfirmedSeller { get; set; }
    public int PaymentState { get; set; }
    public string? ReceiptFilePath { get; set; }

    public int RequestId { get; set; }
    public int ClientId { get; set; }
    public int OwnerId { get; set; }

    public Rental Request { get; set; }
    public User Client { get; set; }
    public User Owner { get; set; }
}

public class HistoryPayment : Payment
{
    public string GameName { get; set; }
    public string OwnerName { get; set; }
}