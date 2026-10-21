using home_work_66.Enums;
using home_work_66.Models;
using home_work_66.ViewModels;

namespace home_work_66.Services.Interfaces;

public interface ICompanyService
{
     Task<User?> GetCompanyByIdAsync(int id);
     
     Task<VacancyIndexViewModel> GetVacanciesAsync(string? searchString, int? categoryId,
         VacancySortState sortOrder, int page);

     Task<List<Category>> GetCategoriesAsync();
     
     Task<List<Vacancy>> GetCompanyVacanciesAsync(int companyId, bool isOwner);
     
     Task<Vacancy?> GetVacancyByIdAsync(int id);

     Task<bool> CreateVacancyAsync(CreateVacancyViewModel model, int companyId);

     Task<bool> EditVacancyAsync(int id, CreateVacancyViewModel model, int companyId);
     
     Task<bool> UpdateVacancyAsync(int id, int companyId);
     
     Task<bool> TogglePublishAsync(int id, int companyId);

     EditCompanyProfileViewModel GetEditCompanyProfile(User user);
     
     Task<bool> UpdateCompanyProfileAsync(User user, EditCompanyProfileViewModel model);

     Task<List<CompanyResponsesViewModel>> GetMyResponsesAsync(int companyId);
}