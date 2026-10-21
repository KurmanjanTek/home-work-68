using home_work_66.Models;

namespace home_work_66.Services.Interfaces;

public interface ICategoryService
{
    Task<List<Category>> GetAllCategoriesAsync();
    
    Task<bool> CreateCategoryAsync(Category category);

    Task<bool> DeleteCategoryAsync(int id);
}