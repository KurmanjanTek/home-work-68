using home_work_66.Models;
using home_work_66.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace home_work_66.Controllers;

[Authorize(Roles = "admin")]
public class CategoryController : Controller
{
    private readonly HeadHunterContext _db;
    private ICategoryService _categoryService;

    public CategoryController(HeadHunterContext db, ICategoryService categoryService)
    {
        _db = db;
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var categories = await _categoryService.GetAllCategoriesAsync();
        return View(categories);
    }

    [HttpPost]
    [ValidateAntiForgeryToken] 
    public async Task<IActionResult> Create(Category category) 
    {
        if (string.IsNullOrWhiteSpace(category.Name))
        {
            return Json(new
            {
                success = false, message = "Название категории не может быть пустым"
            });
        }
        bool success = await _categoryService.CreateCategoryAsync(category);

        if (!success)
        {
            return Json(new
            {
                success = false,
                message = "Категория с таким названием уже существует"
            });
        }

        return Json(new
        {
            success = true,
            id = category.Id,
            name = category.Name
        });
    }
    
    [HttpPost] 
    [ValidateAntiForgeryToken] 
    public async Task<IActionResult> Delete(int id) 
    { 
        bool success = await _categoryService.DeleteCategoryAsync(id);
        if (!success)
        {
            return Json(new
            {
                success = false,
                message = "Категория не найдена или к ней привязаны резюме"
            });
        }

        return Json(new
        {
            success = true,
            id = id
        });
    }
}