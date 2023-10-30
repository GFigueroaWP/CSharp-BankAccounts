#pragma warning disable CS8618
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankAccounts.Models;
public class Transaction
{
    [Key]
    public int TransactionId { get; set; }
    [Required]
    public decimal Amount { get; set; }
    public DateTime CreatedAt {get; set;} = DateTime.Now;
    public DateTime UpdatedAt {get; set;} = DateTime.Now;
    public int UserId {get; set;}
    public User? User {get; set;}
}
