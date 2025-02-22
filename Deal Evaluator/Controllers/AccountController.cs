using Deal_Evaluator.Models;
using Deal_Evaluator.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Deal_Evaluator.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public AccountController(UserManager<User> userManager, SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }
    
    // GET Account/Login
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (!string.IsNullOrEmpty(returnUrl))
        {
            TempData["Notification"] = "Please log in to access this page.";
            TempData["NotificationType"] = "warning";
        }
        
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    // POST Account/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        
        var user = await _userManager.FindByEmailAsync(model.Email);

        if (user == null)
        {
            TempData["Notification"] = "User not found!";
            TempData["NotificationType"] = "warning";
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }
        
        var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            TempData["Notification"] = "Successfully logged in.";
            TempData["NotificationType"] = "success";
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return View(model);
    }
    
    // GET Account/Logout
    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login", "Account");
    }
    
    // GET Account/Register
    [HttpGet]
    public IActionResult Register() => View();
    
    // POST Account/Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var search = await _userManager.FindByEmailAsync(model.Email);
        if (search != null)
        {
            TempData["Notification"] = "This email address already exists. Please try another one.";
            TempData["NotificationType"] = "error";
            ModelState.AddModelError(string.Empty, "Email already exists.");
            return View(model);
        }
        
        
        var user = new User { UserName = model.Name, Email = model.Email, CompanyName = model.CompanyName };
        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            TempData["Notification"] = "Account created successfully.";
            TempData["NotificationType"] = "success";
            return RedirectToAction("Index", "Home");
        }
        
        ModelState.AddModelError(string.Empty, "Invalid register attempt.");
        return View(model);
    }
}