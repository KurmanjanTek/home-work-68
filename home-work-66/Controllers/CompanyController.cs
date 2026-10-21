using home_work_66.Enums;
using home_work_66.Models;
using home_work_66.Services.Interfaces;
using home_work_66.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace home_work_66.Controllers;

public class CompanyController : Controller
{
    private readonly ICompanyService _companyService;
    private readonly HeadHunterContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public CompanyController(HeadHunterContext context, UserManager<User> userManager,
        IWebHostEnvironment webHostEnvironment, ICompanyService companyService)
    {
        _context = context;
        _userManager = userManager;
        _webHostEnvironment = webHostEnvironment;
        _companyService = companyService;
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
            if (!await _userManager.IsInRoleAsync(currentUser, "company"))
            {
                return Forbid();
            }
            id = currentUser.Id;
        }

        var user = await _companyService.GetCompanyByIdAsync(id.Value);
        if (user == null)
        {
            return NotFound();
        }

        bool isOwner = currentUser != null && currentUser.Id == user.Id;
        ViewBag.IsOwner = isOwner;

        return View(user);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetVacancies(string? searchString, int? categoryId,VacancySortState sortOrder=VacancySortState.DateDesc, int page=1)
    {
        var model=await _companyService.GetVacanciesAsync(searchString, categoryId, sortOrder, page);
       
         ViewBag.SalarySort = sortOrder == VacancySortState.SalaryAsc ? VacancySortState.SalaryDesc : VacancySortState.SalaryAsc;
         ViewBag.TitleSort = sortOrder == VacancySortState.TitleAsc ? VacancySortState.TitleDesc : VacancySortState.TitleAsc; 
         ViewBag.SortOrder = sortOrder;
         ViewBag.Categories = await _context.Categories.ToListAsync();
         ViewBag.SearchString=searchString;
         ViewBag.CategoryId=categoryId;

         ViewBag.Categories = await _companyService.GetCategoriesAsync();
         
        return PartialView( "~/Views/Company/Partials/_PublishedVacanciesPartialView.cshtml");
        
    }
    
    [HttpGet] 
    [AllowAnonymous]
    public async Task<IActionResult> GetCompanyVacancies(int? companyId)
    {
        User? currentUser = await _userManager.GetUserAsync(User);
        int targetCompanyId;
        bool isOwner;

        if (companyId.HasValue)
        {
            targetCompanyId = companyId.Value;
            isOwner = currentUser != null && currentUser.Id == targetCompanyId;
        }
        else
        {
            if (currentUser == null)
            {
                return Unauthorized();
            }

            targetCompanyId = currentUser.Id;
            isOwner = true;
        }

        var vacancies = await _companyService.GetCompanyVacanciesAsync(
            targetCompanyId,
            isOwner);

        ViewBag.IsOwner = isOwner;

        return PartialView(
            "~/Views/Company/Partials/_VacanciesList.cshtml",
            vacancies);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var vacancy = await _companyService.GetVacancyByIdAsync(id);
           
        if (vacancy == null)
            return NotFound();

        return View(vacancy);
    }

    [HttpGet]
    [Authorize(Roles = "company")]
    public async Task<IActionResult> CreateVacancy()
    {
        ViewBag.Categories = await _companyService.GetCategoriesAsync();
        return PartialView("~/Views/Company/Partials/_CreateVacancy.cshtml", new CreateVacancyViewModel());
    }

    [HttpPost]
    [Authorize(Roles = "company")]
    public async Task<IActionResult> CreateVacancy(CreateVacancyViewModel model)
    {
        if (model.ExperienceFrom > model.ExperienceTo)
        {
            ModelState.AddModelError("ExperienceFrom", "Минимальный опыт не может быть больше максимального");
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

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Json(new { success = false, message = "Пользователь не найден" });


        await _companyService.CreateVacancyAsync(model, user.Id);

        return Json(new { success = true });
    }
    
    

    [HttpGet]
    [Authorize(Roles = "company")]
    public async Task<IActionResult> EditVacancy(int id) 
    { 
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        var vacancy = await _companyService.GetVacancyByIdAsync(id);

        if (vacancy == null || vacancy.CompanyId != user.Id)
        {
            return NotFound();
        }

        var model = new CreateVacancyViewModel
        {
            Title = vacancy.Title,
            Salary = vacancy.Salary,
            Description = vacancy.Description,
            Requirements = vacancy.Requirements,
            ExperienceFrom = vacancy.ExperienceFrom,
            ExperienceTo = vacancy.ExperienceTo,
            CategoryId = vacancy.CategoryId
        };

        ViewBag.Categories = await _companyService.GetCategoriesAsync();

        return PartialView(
            "~/Views/Company/Partials/_EditVacancy.cshtml",
            model);
    }
    

    [HttpPost] 
    [Authorize(Roles = "company")] 
    public async Task<IActionResult> EditVacancy(int id, CreateVacancyViewModel model) 
    { 
        if (model.ExperienceFrom > model.ExperienceTo) { ModelState.AddModelError( "ExperienceFrom", "Минимальный опыт не может быть больше максимального"); }
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value != null && x.Value.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors)
                .Select(e => e.ErrorMessage);

            var message = string.Join("\n", errors);

            return Json(new
            {
                success = false,
                message = string.IsNullOrWhiteSpace(message)
                    ? "Пожалуйста, заполните все обязательные поля корректно"
                    : message
            });
        }

        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Json(new
            {
                success = false,
                message = "Пользователь не найден"
            });
        }

        var success = await _companyService.EditVacancyAsync(id, model, user.Id);

        if (!success)
        {
            return Json(new
            {
                success = false,
                message = "Вакансия не найдена или у вас нет прав на её редактирование"
            });
        }

        return Json(new { success = true });
    }

    
    [HttpPost]
    [Authorize(Roles = "company")]
    public async Task<IActionResult> UpdateVacancy(int id) 
    { 
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Json(new
            {
                success = false,
                message = "Пользователь не найден"
            });
        }

        var success = await _companyService.UpdateVacancyAsync(id, user.Id);

        if (!success)
        {
            return Json(new
            {
                success = false,
                message = "Вакансия не найдена или у вас нет прав на её изменение"
            });
        }

        return Json(new { success = true });
    }
    
    

    [HttpPost] 
    [Authorize(Roles = "company")] 
    public async Task<IActionResult> TogglePublish(int id) 
    { var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Json(new
            {
                success = false,
                message = "Пользователь не найден"
            });
        }

        var success = await _companyService.TogglePublishAsync(id, user.Id);

        if (!success)
        {
            return Json(new
            {
                success = false,
                message = "Вакансия не найдена или у вас нет прав на её изменение"
            });
        }

        return Json(new { success = true });
    }

    [HttpGet]
    [Authorize(Roles = "company")]
    public async Task<IActionResult> EditProfile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound();

      var model=_companyService.GetEditCompanyProfile(user);
        return PartialView("~/Views/Company/Partials/_EditProfile.cshtml", model);
    }

    [HttpPost]
    [Authorize(Roles = "company")]
    public async Task<IActionResult> EditProfile(EditCompanyProfileViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Json(new { success = false, message = "Пользователь не найден" });

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

        var success=await _companyService.UpdateCompanyProfileAsync(user, model);
        
        if (!success)
        {
            return Json(new 
                { success = false, 
                    message="Не удалось сохранить профилью Проверьте формат аватара и данные" 
                });
        }

        return Json(new
        {
            success = true,
            img = user.AvatarUrl,
            name = user.CompanyName,
            email = user.Email
        });
    }
    
    
    [HttpGet]
    [Authorize(Roles = "company")]
    public async Task<IActionResult> MyResponses()
    {
        User? currentCompany = await _userManager.GetUserAsync(User);

        if (currentCompany == null)
        {
            return RedirectToAction("Login", "Account");
        }

       var responses=await _companyService.GetMyResponsesAsync(currentCompany.Id);

        return View(responses);
    }
}