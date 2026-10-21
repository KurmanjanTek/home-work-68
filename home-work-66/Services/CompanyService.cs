using home_work_66.Enums;
using home_work_66.Models;
using home_work_66.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace home_work_66.Services.Interfaces;

public class CompanyService :ICompanyService
{
    private readonly HeadHunterContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public CompanyService(HeadHunterContext context, UserManager<User> userManager, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _userManager = userManager;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<User?> GetCompanyByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }
    
    
  
    public async Task<VacancyIndexViewModel> GetVacanciesAsync(string? searchString, int? categoryId,VacancySortState sortOrder, int page)
    {
        var query = _context.Vacancies
            .Include(v => v.Category)
            .Include(v => v.Company)
            .Where(v => v.IsPublished);
        if (!string.IsNullOrWhiteSpace(searchString))
        {
            query = query.Where(v => v.Title.Contains(searchString));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(v => v.CategoryId == categoryId.Value);
        }

        switch (sortOrder)
        {
            case VacancySortState.SalaryAsc: 
                query = query.OrderBy(v => v.Salary); 
                break;
            case VacancySortState.SalaryDesc:
                query = query.OrderByDescending(v => v.Salary);
                break;
            case VacancySortState.TitleAsc:
                query = query.OrderBy(v => v.Title);
                break;
            case VacancySortState.TitleDesc:
                query = query.OrderByDescending(v => v.Title);
                break;
            case VacancySortState.DateDesc:
            default:
                query = query.OrderByDescending(v => v.UpdatedAt ?? v.CreatedAt);
                break;
        }
        
        int pageSize = 20;
         var count= await query.CountAsync();
         var items= await query
             .Skip((page - 1) * pageSize)
             .Take(pageSize)
             .ToListAsync();
          
        var pageViewModel = new PageViewModel(count, page, pageSize);
        
        return new VacancyIndexViewModel()
        {
             Vacancies = items,
             PageViewModel = pageViewModel
        };
   }


    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await _context.Categories.ToListAsync();
    }
    
    
    public async Task<List<Vacancy>> GetCompanyVacanciesAsync( int companyId, bool isOwner) { var query = _context.Vacancies .Include(v => v.Category) .Where(v => v.CompanyId == companyId);
        if (!isOwner)
        {
            query = query.Where(v => v.IsPublished);
        }

        return await query
            .OrderByDescending(v => v.UpdatedAt)
            .ToListAsync();
    }


    public async Task<Vacancy?> GetVacancyByIdAsync(int id)
    {
        return await _context.Vacancies .Include(v => v.Category) .Include(v => v.Company) .FirstOrDefaultAsync(v => v.Id == id);
    }
    
    
    public async Task<bool> CreateVacancyAsync( CreateVacancyViewModel model, int companyId) 
    { 
        var vacancy = new Vacancy
        {
            Title = model.Title, 
            Salary = model.Salary, 
            Description = model.Description,
            Requirements = model.Requirements, 
            ExperienceFrom = model.ExperienceFrom, 
            ExperienceTo = model.ExperienceTo, 
            CategoryId = model.CategoryId, 
            CompanyId = companyId, 
            CreatedAt = DateTime.UtcNow, 
            UpdatedAt = null, 
            IsPublished = false
        };
        _context.Vacancies.Add(vacancy);
        await _context.SaveChangesAsync();
        return true;
    }
    
    
    public async Task<bool> EditVacancyAsync( int id, CreateVacancyViewModel model, int companyId) 
    { 
        var vacancy = await _context.Vacancies 
            .FirstOrDefaultAsync(v => v.Id == id && v.CompanyId == companyId);
        if (vacancy == null)
        {
            return false;
        }

        vacancy.Title = model.Title;
        vacancy.Salary = model.Salary;
        vacancy.Description = model.Description;
        vacancy.Requirements = model.Requirements;
        vacancy.ExperienceFrom = model.ExperienceFrom;
        vacancy.ExperienceTo = model.ExperienceTo;
        vacancy.CategoryId = model.CategoryId;
        vacancy.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }


    public async Task<bool> UpdateVacancyAsync(int id, int companyId)
    { 
        var vacancy = await _context.Vacancies 
            .FirstOrDefaultAsync(v => v.Id == id && v.CompanyId == companyId);
        if (vacancy == null)
        {
            return false;
        }

        vacancy.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }
    
    public async Task<bool> TogglePublishAsync(int id, int companyId) 
    { 
        var vacancy = await _context.Vacancies 
            .FirstOrDefaultAsync(v => v.Id == id && v.CompanyId == companyId);
        if (vacancy == null)
        {
            return false;
        }

        vacancy.IsPublished = !vacancy.IsPublished;
        vacancy.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }


    public EditCompanyProfileViewModel GetEditCompanyProfile(User user)
    {
        var model = new EditCompanyProfileViewModel()
        {
            CompanyName = user.CompanyName ?? user.UserName ?? "",
            PhoneNumber = user.PhoneNumber ?? "",
            Email = user.Email ?? "",
            AvatarUrl = user.AvatarUrl
        };
        return model;
    }
    
    public async Task<bool>UpdateCompanyProfileAsync(User user, EditCompanyProfileViewModel model)
    {
        user.CompanyName = model.CompanyName;
        user.PhoneNumber = model.PhoneNumber;
        user.Email = model.Email;

        if (model.Avatar != null && model.Avatar.Length > 0)
        {
            var extension = Path.GetExtension(model.Avatar.FileName).ToLower();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".jfif" };

            if (!allowedExtensions.Contains(extension))
            {
                return false;
            }

            var wwwPath = _webHostEnvironment.WebRootPath;
            var path = Path.Combine(wwwPath, "uploads", "avatars");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var newFileName = $"{Guid.NewGuid()}{extension}";
            user.AvatarUrl = $"/uploads/avatars/{newFileName}";
            var fileWithPath = Path.Combine(path, newFileName);
            using var fileStream = new FileStream(fileWithPath, FileMode.Create);
            await model.Avatar.CopyToAsync(fileStream);
        }

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }


   public async Task<List<CompanyResponsesViewModel>> GetMyResponsesAsync(int companyId)
    {
        List<Chat> chats = await _context.Chats
            .Where(c => c.CompanyId == companyId)
            .Include(c => c.Vacancy)
            .Include(c => c.User)
            .Include(c => c.Resume)
            .OrderByDescending(c => c.Id)
            .ToListAsync();

        List<CompanyResponsesViewModel> responses = chats
            .GroupBy(c => c.VacancyId)
            .Select(group => new CompanyResponsesViewModel
            {
                Vacancy = group.First().Vacancy!,
                Responses = group.ToList()
            })
            .ToList();

        return responses;
    }
}