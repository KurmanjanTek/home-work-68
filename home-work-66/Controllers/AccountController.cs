using home_work_66.Models;
using home_work_66.Services.Interfaces;
using home_work_66.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace home_work_66.Controllers;

public class AccountController : Controller
{
    private readonly IAccountService _accountService;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IWebHostEnvironment _environment;
    private readonly HeadHunterContext _db;
    private readonly IValidationService _validationService;
    public AccountController(IAccountService accountService, IValidationService validationService, UserManager<User> userManager, SignInManager<User> signInManager, IWebHostEnvironment environment,  HeadHunterContext db)
    {
        _accountService= accountService;
        _validationService = validationService;
        _signInManager = signInManager;
        _userManager = userManager;
        _environment = environment;
        _db = db;
    }
    
    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterAsync(RegisterViewModel model)
    {
        if (!string.IsNullOrWhiteSpace(model.Email) && !await _validationService.IsEmailAvailableAsync(model.Email, null))
            ModelState.AddModelError(nameof(model.Email), "В базе уже существует аккаунт с данным адресом почты");
        
        if (!string.IsNullOrWhiteSpace(model.Password) && !_validationService.IsPasswordValid(model.Password))
            ModelState.AddModelError(nameof(model.Password), "Пароль должен содержать в себе 1 букву верхнего регистра, 1 букву нижнего регистра, а также минимум 1 цифру");
        
        if (ModelState.IsValid)
        {
            var result= await _accountService.RegisterAsync(model); 
            
            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                }

                if (model.IsCompany)
                {
                    return RedirectToAction("Index", "Company");
                }
                
                return RedirectToAction("Index", "User");
            }
            
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }
    
    public IActionResult Login(string? returnUrl)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoginAsync(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var result= await _accountService.LoginAsync(model);
           
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    
                    if(user !=null && await _userManager.IsInRoleAsync(user,"company"))
                        return RedirectToAction("Index", "Company");
                    else 
                        return RedirectToAction("Index", "User");
                }
            }
            ModelState.AddModelError("", "Неправильный логин и (или) пароль");
        
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LogOffAsync()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }
    
}