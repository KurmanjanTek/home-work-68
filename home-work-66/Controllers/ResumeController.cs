using home_work_66.Models;
using home_work_66.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace home_work_66.Controllers;

[Authorize(Roles = "user,admin,company")]
public class ResumeController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly IResumeService _resumeService;

    public ResumeController(IResumeService resumeService, UserManager<User> userManager)
    {
        _resumeService = resumeService;
        _userManager = userManager;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetResumes(int? userId)
    {
        User? currentUser = await _userManager.GetUserAsync(User);

        int targetUserId;
        bool isOwner;

        if (userId.HasValue)
        {
            targetUserId = userId.Value;
            isOwner = currentUser != null && currentUser.Id == targetUserId;
        }
        else
        {
            if (currentUser == null)
            {
                return Unauthorized();
            }
            targetUserId = currentUser.Id;
            isOwner = true;
        }

        var resumes=await _resumeService.GetResumesAsync(targetUserId, isOwner);

        ViewBag.IsOwner = isOwner;
        return PartialView("~/Views/Resume/Partials/_ResumesList.cshtml", resumes);
    }
    

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Categories = await _resumeService.GetCategoriesAsync();
        return PartialView("~/Views/Resume/Partials/_CreateResume.cshtml", new Resume());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Resume resume)
    {
        User? currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return Json(new
            { 
                success = false,
                message = "Пользователь не найден"
            });
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
        
        await _resumeService.CreateResumeAsync(resume, currentUser.Id);

        return Json(new
        {
            success = true,
            resumeId = resume.Id,
            redirectUrl = Url.Action("Details", "Resume", new { id = resume.Id })
        });
    }
    
    
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        User? currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return Unauthorized();
        }

        Resume? resume = await _resumeService.GetResumeForEditAsync(id, currentUser.Id);

        if (resume == null)
        {
            return NotFound();
        }

        ViewBag.Categories = await _resumeService.GetCategoriesAsync();
        return PartialView("~/Views/Resume/Partials/_EditResume.cshtml", resume);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Resume model)
    {
        User? currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return Json(new
            {
                success = false,
                message = "Пользователь не найден"
            });
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

       bool isUpdated= await _resumeService.UpdateResumeAsync(model, currentUser.Id);

        if (!isUpdated)
        {
            return Json(new
            {
                success = false,
                message = "Резюме не найдено"
            });
        }

        return Json(new
        {
            success = true
        });
    }
    
    [HttpPost]
    public async Task<IActionResult> UpdateDate(int id)
    {
        User? currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account");
        }

        bool isUpdated = await _resumeService.UpdateDateAsync(id, currentUser.Id);

        if (!isUpdated)
        {
            return NotFound();
        }

        return Json(new
        {
            success = true
        });
    }
    
    
    [HttpGet]
    public IActionResult AddWorkExperience(int resumeId)
    {
        ViewBag.ResumeId = resumeId;
        return View("~/Views/Resume/WorkExperience/AddWorkExperience.cshtml");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddWorkExperience(WorkExperience workExperience)
    {
        int currentYear = DateTime.Now.Year;

        if (workExperience.StartYear < 1900 || workExperience.StartYear > currentYear)
        {
            ModelState.AddModelError("StartYear", $"Год должен быть от 1900 до {currentYear}");
        }

        if (workExperience.EndYear < 1900 || workExperience.EndYear > currentYear)
        {
            ModelState.AddModelError("EndYear", $"Год должен быть от 1900 до {currentYear}");
        }

        if (workExperience.EndYear < workExperience.StartYear)
        {
            ModelState.AddModelError("EndYear", "Год окончания не может быть раньше года начала");
        }
        
        if (!ModelState.IsValid)
        {
            ViewBag.ResumeId = workExperience.ResumeId;
            return View("~/Views/Resume/WorkExperience/AddWorkExperience.cshtml", workExperience);
        }

         await _resumeService.AddWorkExperienceAsync(workExperience);
        
        return RedirectToAction("Index", "User");
    }
    
    [HttpGet]
    public IActionResult AddEducation(int resumeId)
    {
        ViewBag.ResumeId = resumeId;
        return View("~/Views/Resume/Education/AddEducation.cshtml");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddEducation(Education education)
    {
        int currentYear = DateTime.Now.Year;

        if (education.GraduationYear < 1900 || education.GraduationYear > currentYear)
        {
            ModelState.AddModelError("GraduationYear", $"Год должен быть от 1900 до {currentYear}");
        }
        
        if (!ModelState.IsValid)
        {
            ViewBag.ResumeId = education.ResumeId;
            return View("~/Views/Resume/Education/AddEducation.cshtml", education);
        }
        
        await _resumeService.AddEducationAsync(education);

        return RedirectToAction("Index", "User");
    }
    
    [HttpGet]
    public IActionResult AddCourse(int resumeId)
    {
        ViewBag.ResumeId = resumeId;
        return View("~/Views/Resume/Courses/AddCourse.cshtml");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCourse(Course course)
    {
        int currentYear = DateTime.Now.Year;

        if (course.Year < 1900 || course.Year > currentYear)
        {
            ModelState.AddModelError("Year", $"Год должен быть от 1900 до {currentYear}");
        }
        
        if (!ModelState.IsValid)
        {
            ViewBag.ResumeId = course.ResumeId;
            return View("~/Views/Resume/Courses/AddCourse.cshtml", course);
        }

       await _resumeService.AddCourseAsync(course);

        return RedirectToAction("Details", new { id = course.ResumeId });
    }
    
    [HttpGet]
    public async Task<IActionResult> EditWorkExperience(int id)
    {
        User? currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account");
        }

        WorkExperience? workExperience = await _resumeService.GetWorkExperienceForEditAsync(id, currentUser.Id);

        if (workExperience == null)
        {
            return NotFound();
        }

        return View("~/Views/Resume/WorkExperience/EditWorkExperience.cshtml", workExperience);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditWorkExperience(WorkExperience workExperience)
    {
        User? currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account");
        }
        
        if (!ModelState.IsValid)
        {
            return View("~/Views/Resume/WorkExperience/EditWorkExperience.cshtml", workExperience);
        }

        bool isUpdated = await _resumeService.UpdateWorkExperienceAsync(workExperience, currentUser.Id);

        if (!isUpdated)
        {
            return NotFound();
        }
            
        return RedirectToAction("Details", new { id = workExperience.ResumeId });
    }

    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteWorkExperience(int id)
    {
        User? currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account");
        }

        int? resumeId= await _resumeService.DeleteWorkExperienceAsync(id, currentUser.Id);

        if (resumeId== null)
        {
            return NotFound();
        }
        
        return RedirectToAction("Details", new { id = resumeId });
    }
    
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        User? currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account");
        }

       var resume= await _resumeService.GetResumeDetailsAsync(id, currentUser.Id);

        return View(resume);
    }
    
    [HttpGet]
    public async Task<IActionResult> EditEducation(int id)
    {
        User? currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var education = await _resumeService.GetEducationForEditAsync(id, currentUser.Id);
        
       return View("~/Views/Resume/Education/EditEducation.cshtml", education);
    }
    

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditEducation(Education education)
    {
        User? currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account");
        }
        
        int currentYear = DateTime.Now.Year;

        if (education.GraduationYear < 1900 || education.GraduationYear > currentYear)
        {
            ModelState.AddModelError(
                "GraduationYear",
                $"Год должен быть от 1900 до {currentYear}"
            );
        }

        if (!ModelState.IsValid)
        {
            return View("~/Views/Resume/Education/EditEducation.cshtml", education);
        }

        await _resumeService.UpdateEducationAsync(education, currentUser.Id);

        return RedirectToAction("Details", new { id = education.ResumeId });
    }
    

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteEducation(int id)
    {
        User? currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account");
        }

        
        var resumeId = await _resumeService.DeleteEducationAsync(id, currentUser.Id);

        return RedirectToAction("Details", new { id = resumeId });
    }
    
    [HttpGet]
    public async Task<IActionResult> EditCourse(int id)
    {
        User? currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account");
        }

       var course= await _resumeService.GetCourseForEditAsync(id, currentUser.Id);

        return View("~/Views/Resume/Courses/EditCourse.cshtml", course);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCourse(Course course)
    {
        User? currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account");
        }
        
        int currentYear = DateTime.Now.Year;

        if (course.Year < 1900 || course.Year > currentYear)
        {
            ModelState.AddModelError("Year", $"Год должен быть от 1900 до {currentYear}");
        }

        if (!ModelState.IsValid)
        {
            return View("~/Views/Resume/Courses/EditCourse.cshtml", course);
        }

        await _resumeService.UpdateCourseAsync(course, currentUser.Id);

        return RedirectToAction("Details", new { id = course.ResumeId });
    }

    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCourse(int id)
    {
        User? currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account");
        }

       var resumeId= await _resumeService.DeleteCourseAsync(id, currentUser.Id);

        return RedirectToAction("Details", new { id = resumeId });
    }
    
    
    [HttpPost]
    public async Task<IActionResult> TogglePublication(int id)
    {
        User? currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account");
        }

        bool result= await _resumeService.TogglePublicationAsync(id, currentUser.Id);

        if (!result)
        {
            return NotFound();
        }
        
        return Json(new { success = true });
    }
    
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Published()
    {
       var resumes= await _resumeService.GetPublishedResumesAsync();

        return PartialView("Partials/_PublishedResumesPartialView", resumes);
    }
    
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> PublicDetails(int id)
    {
        Resume? resume = await _resumeService.GetPublicDetails(id);

        if (resume == null)
        {
            return NotFound();
        }
        
        if (User.Identity != null && User.Identity.IsAuthenticated && User.IsInRole("company"))
        {
            User? currentCompany = await _userManager.GetUserAsync(User);

            if (currentCompany != null)
            {
                ViewBag.Vacancies = await _resumeService.GetCompanyVacanciesAsync(id);
            }
        }

        return View(resume);
    }
    

    [HttpGet]
    [Authorize(Roles = "company")]
    public async Task<IActionResult> BrowseByCategory(int? categoryId)
    {
        ViewBag.Categories = await _resumeService.GetCategoriesAsync();
        
        ViewBag.SelectedCategoryId = categoryId;
        var rezumes= await _resumeService.GetResumesByCategoryAsync(categoryId);
        
        return View(rezumes);
    }
}