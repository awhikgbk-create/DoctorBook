using DoctorBook.Data;
using DoctorBook.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace DoctorBook.Controllers;
[Authorize(Roles="Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _db;
    public AdminController(ApplicationDbContext db)=>_db=db;
    public async Task<IActionResult> Index()
    {
        ViewBag.Doctors=await _db.Doctors.CountAsync();
        ViewBag.PendingDoctors=await _db.Doctors.CountAsync(x=>!x.IsApproved);
        ViewBag.Appointments=await _db.Appointments.CountAsync();
        ViewBag.PendingAppointments=await _db.Appointments.CountAsync(x=>x.Status=="Pending");
        return View(await _db.Appointments.Include(x=>x.Doctor).OrderByDescending(x=>x.CreatedAt).Take(15).ToListAsync());
    }
    public async Task<IActionResult> Doctors()=>View(await _db.Doctors.OrderBy(x=>x.IsApproved).ThenBy(x=>x.FullName).ToListAsync());
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> ApproveDoctor(int id)
    {
        var d=await _db.Doctors.FindAsync(id); if(d!=null){d.IsApproved=true; if(!string.IsNullOrEmpty(d.UserId))_db.Notifications.Add(new SiteNotification{UserId=d.UserId,Title="تم اعتماد حسابك",Message="تمت الموافقة على ملفك وأصبح ظاهرًا في دليل الأطباء."});await _db.SaveChangesAsync();}
        return RedirectToAction(nameof(Doctors));
    }
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> RejectDoctor(int id)
    {
        var d=await _db.Doctors.FindAsync(id); if(d!=null){d.IsApproved=false;await _db.SaveChangesAsync();}
        return RedirectToAction(nameof(Doctors));
    }
    public async Task<IActionResult> Appointments()=>View(await _db.Appointments.Include(x=>x.Doctor).Include(x=>x.Payment).OrderByDescending(x=>x.CreatedAt).ToListAsync());
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> UpdateAppointment(int id,string status)
    {
        var a=await _db.Appointments.FindAsync(id);
        if(a!=null && new[]{"Pending","Confirmed","Cancelled","Completed"}.Contains(status)){a.Status=status;await _db.SaveChangesAsync();}
        return RedirectToAction(nameof(Appointments));
    }
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> MarkPaid(int id)
    {
        var p=await _db.Payments.FirstOrDefaultAsync(x=>x.AppointmentId==id);
        if(p!=null){p.Status="Paid";await _db.SaveChangesAsync();}
        return RedirectToAction(nameof(Appointments));
    }
}
