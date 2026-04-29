using Microsoft.AspNetCore.Mvc;
using FileVault.Web.Models.ViewModels;

namespace FileVault.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();
    public IActionResult About() => View();
    public IActionResult Features() => View();
    public IActionResult Contact() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Contact(ContactViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        TempData["Success"] = "Thank you for your message! We'll get back to you soon.";
        return RedirectToAction(nameof(Contact));
    }

    public IActionResult Privacy() => View();
    public IActionResult Terms() => View();
    public IActionResult Error() => View();
}
