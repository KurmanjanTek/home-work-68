using home_work_66.Models;
using home_work_66.Services.Interfaces;
using home_work_66.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace home_work_66.Controllers;

public class UserController : Controller
{
    private readonly IUserService _userService;
    private readonly UserManager<User> _userManager;
    private readonly IWebHostEnvironment _environment;
    private readonly HeadHunterContext _db;

    public UserController(IUserService userService, UserManager<User> userManager, IWebHostEnvironment environment, HeadHunterContext db)
    {
        _userService = userService;
        _userManager = userManager;
        _environment = environment;
        _db = db;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Index(int? id)
    {
        User? currentUser = await _userManager.GetUserAsync(User);

        if (id == null)
        {
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }
            id = currentUser.Id;
        }

        var user = await _userService.GetUserByIdAsync(id.Value);

        bool isOwner = currentUser != null && currentUser.Id == user.Id;
        ViewBag.IsOwner = isOwner;

        return View(user);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> EditProfile()
    {
        User? currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return Unauthorized();
        }

        EditProfileViewModel model = new EditProfileViewModel
        {
            UserName = currentUser.UserName,
            Email = currentUser.Email,
            PhoneNumber = currentUser.PhoneNumber
        };

        return PartialView("_EditProfilePartialView", model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(EditProfileViewModel model)
    {
        User? currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return Json(new { success = false, message = "Пользователь не найден" });
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Where(x => x.Value != null && x.Value.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors)
                .Select(e => e.ErrorMessage);
            string message = string.Join("\n", errors);

            return Json(new
            {
                success = false,
                message = string.IsNullOrWhiteSpace(message) ? "Пожалуйста, заполните все обязательные поля корректно" : message
            });
        }

       var result= await _userService.UpdateProfileAsync(currentUser, model);

        if (!result.Succeeded)
        {
            string message = string.Join("\n", result.Errors.Select(e => e.Description));
            return Json(new { success = false, message });
        }

        return Json(new
        {
            success = true,
            img = currentUser.AvatarUrl,
            name = currentUser.UserName,
            email = currentUser.Email
        });
    }
    
    
    [Authorize]
    public async Task<IActionResult> Respond(int vacancyId, int companyId)
    {
        int userId = int.Parse(_userManager.GetUserId(User)!);
        var resumes = await _userService.GetRezumesByUserIdAsync(userId);

        var model = new ChatCreateViewModel
        {
            VacancyId = vacancyId,
            CompanyId = companyId,
            Resumes = resumes
        };

        return PartialView("_RespondPartialView", model);
    }
    
    [HttpGet]
    [Authorize(Roles = "user")]
    public async Task<IActionResult> MyResponses()
    {
        User? currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account");
        }

        List<Chat> responses = await _userService.GetUserResponsesAsync(currentUser.Id);

        return View(responses);
    }
}