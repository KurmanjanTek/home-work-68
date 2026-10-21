using home_work_66.Models;

namespace home_work_66.Services.Interfaces;

public interface IResumeService
{
    Task<List<Resume>> GetResumesAsync(int? userId, bool isOwner);
    
    Task<List<Category>> GetCategoriesAsync();
    Task<Resume> CreateResumeAsync(Resume resume, int userId);
    
    Task<Resume?> GetResumeForEditAsync(int id, int userId);
    
    Task<bool> UpdateResumeAsync(Resume model, int userId);

    Task<bool> UpdateDateAsync(int id, int userId);

    Task<bool> AddWorkExperienceAsync(WorkExperience workExperience);

    Task<bool> AddEducationAsync(Education education);

    Task<bool> AddCourseAsync(Course course);

    Task<WorkExperience?> GetWorkExperienceForEditAsync(int id, int userId);
    
    Task<bool>UpdateWorkExperienceAsync(WorkExperience workExperience, int userId);
    
    Task<int?> DeleteWorkExperienceAsync(int id, int userId);
    
    Task<Resume?>GetResumeDetailsAsync(int id, int userId);
    
    Task<Education?> GetEducationForEditAsync(int id, int userId);
    
    Task<bool>UpdateEducationAsync(Education education, int userId);
    
    
    Task<int?> DeleteEducationAsync(int id, int userId);
    
    Task<Course?>GetCourseForEditAsync(int id, int userId);

    Task<bool> UpdateCourseAsync(Course course, int userId);
    
    Task<int?> DeleteCourseAsync(int id, int userId);
    
    Task<List<Resume>> GetPublishedResumesAsync();

    Task<bool> TogglePublicationAsync(int id, int userId);

    Task<Resume?> GetPublicDetails(int id);

    Task<List<Vacancy>> GetCompanyVacanciesAsync(int companyId);

    Task<List<Resume>> GetResumesByCategoryAsync(int? categoryId);


}