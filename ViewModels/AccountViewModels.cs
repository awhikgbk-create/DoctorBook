using System.ComponentModel.DataAnnotations;
namespace DoctorBook.ViewModels;
public class RegisterViewModel
{
    [Required, StringLength(120)] public string FullName { get; set; } = "";
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, Phone] public string PhoneNumber { get; set; } = "";
    [Required, MinLength(8), DataType(DataType.Password)] public string Password { get; set; } = "";
    [Required, DataType(DataType.Password), Compare(nameof(Password))] public string ConfirmPassword { get; set; } = "";
    [Required] public string AccountType { get; set; } = "Patient";
    public string? Specialty { get; set; }
    public string? City { get; set; }
    public string? Bio { get; set; }
    public decimal ConsultationPrice { get; set; }
}
public class LoginViewModel
{
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, DataType(DataType.Password)] public string Password { get; set; } = "";
    public bool RememberMe { get; set; }
}
