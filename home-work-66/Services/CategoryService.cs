using home_work_66.Models;
using home_work_66.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace home_work_66.Services;

public class CategoryService :ICategoryService
{
    private readonly  HeadHunterContext _db;

    public CategoryService(HeadHunterContext db)
    {
        _db = db;
    }

    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        var categories = await _db.Categories
            .OrderBy(c => c.Name)
            .ToListAsync();
        return categories;
    }
    
    
    public async Task<bool> CreateCategoryAsync(Category category) 
    {
        if (string.IsNullOrWhiteSpace(category.Name)) return false;
        string name = category.Name.Trim();

        bool exists = await _db.Categories
            .AnyAsync(c => c.Name.ToLower() == name.ToLower());

        if (exists)
            return false;

        category.Name = name;

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        return true;
    }
    
    
     public async Task<bool> DeleteCategoryAsync(int id) 
     { 
         var category = await _db.Categories.FindAsync(id);
        
         if (category == null)
            return false;

         bool hasResumes = await _db.Resumes
            .AnyAsync(r => r.CategoryId == id);

         if (hasResumes)
            return false;

         _db.Categories.Remove(category);
         await _db.SaveChangesAsync();

         return true;
    }
}