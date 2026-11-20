using DoctorBook.Data;
using DoctorBook.Models;
using DoctorBook.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoctorBook.Controllers;
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly ApplicationDbContext _db;
    public AccountController(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn, ApplicationDbContext db) { _users=users; _signIn=signIn; _db=db; }

    [HttpGet] public IActionResult Register() => View(new RegisterViewModel());
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel m)
    {
        if (m.AccountType == "Doctor")
        {
            if (string.IsNullOrWhiteSpace(m.Specialty)) ModelState.AddModelError(nameof(m.Specialty), "اكتب التخصص");
            if (string.IsNullOrWhiteSpace(m.City)) ModelState.AddModelError(nameof(m.City), "اكتب المدينة");
        }
        if (!ModelState.IsValid) return View(m);
        var user = new ApplicationUser { UserName=m.Email, Email=m.Email, FullName=m.FullName, PhoneNumber=m.PhoneNumber };
        var result = await _users.CreateAsync(user, m.Password);
        if (!result.Succeeded) { foreach(var e in result.Errors) ModelState.AddModelError("", e.Description); return View(m); }
        var role = m.AccountType == "Doctor" ? "Doctor" : "Patient";
        await _users.AddToRoleAsync(user, role);
        if (role == "Doctor")
        {
            _db.Doctors.Add(new DoctorProfile { FullName=m.FullName, Specialty=m.Specialty!, City=m.City!, Bio=m.Bio ?? "", ConsultationPrice=m.ConsultationPrice, UserId=user.Id, IsApproved=false });
            await _db.SaveChangesAsync();
        }
        await _signIn.SignInAsync(user, isPersistent:false);
        if (role == "Doctor") return RedirectToAction("Index", "DoctorDashboard");
        return RedirectToAction("Index", "Home");
    }

    [HttpGet] public IActionResult Login(string? returnUrl=null) { ViewBag.ReturnUrl=returnUrl; return View(new LoginViewModel()); }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel m, string? returnUrl=null)
    {
        if (!ModelState.IsValid) return View(m);
        var result = await _signIn.PasswordSignInAsync(m.Email, m.Password, m.RememberMe, lockoutOnFailure:true);
        if (!result.Succeeded) { ModelState.AddModelError("", "البريد الإلكتروني أو كلمة المرور غير صحيحة."); return View(m); }
        var user = await _users.FindByEmailAsync(m.Email);
        if (user != null && await _users.IsInRoleAsync(user,"Admin")) return RedirectToAction("Index","Admin");
        if (user != null && await _users.IsInRoleAsync(user,"Doctor")) return RedirectToAction("Index","DoctorDashboard");
        return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : Url.Action("Index","Home")!);
    }
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Logout() { await _signIn.SignOutAsync(); return RedirectToAction("Index","Home"); }
    public IActionResult AccessDenied() => View();
}
