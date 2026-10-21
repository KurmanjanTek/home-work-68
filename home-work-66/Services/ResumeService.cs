using home_work_66.Models;
using home_work_66.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace home_work_66.Services;

public class ResumeService : IResumeService
{
    private readonly HeadHunterContext _db;

    public ResumeService(HeadHunterContext db)
    {
        _db = db;
    }


    public async Task<List<Resume>> GetResumesAsync(int? userId, bool isOwner)
    {
        var query = _db.Resumes
            .Include(r => r.Category)
            .Where(r => r.UserId == userId);

        if (!isOwner)
        {
            query = query.Where(r => r.IsPublished);
        }

        return await query
            .OrderByDescending(r => r.UpdatedAt)
            .ToListAsync();
    }


    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await _db.Categories.ToListAsync();
    }


    public async Task<Resume> CreateResumeAsync(Resume resume, int userId)
    {
        resume.UserId = userId;
        resume.UpdatedAt = DateTime.UtcNow;
        resume.IsPublished = false;

        _db.Resumes.Add(resume);
        await _db.SaveChangesAsync();

        return resume;
    }


    public async Task<Resume?> GetResumeForEditAsync(int id, int userId)
    {
        Resume? resume = await _db.Resumes
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

        return resume;
    }


    public async Task<bool> UpdateResumeAsync(Resume model, int userId)
    {
        Resume? resume = await _db.Resumes
            .FirstOrDefaultAsync(r => r.Id == model.Id && r.UserId == userId);
        if (resume == null)
        {
            return false;
        }

        resume.Title = model.Title;
        resume.CategoryId = model.CategoryId;
        resume.Salary = model.Salary;

        resume.Email = model.Email;
        resume.PhoneNumber = model.PhoneNumber;
        resume.Telegram = model.Telegram;
        resume.Facebook = model.Facebook;
        resume.LinkedIn = model.LinkedIn;

        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateDateAsync(int id, int userId)
    {
        Resume? resume = await _db.Resumes
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
        if (resume == null)
        {
            return false;
        }

        resume.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return true;
    }


    public async Task<bool> AddWorkExperienceAsync(WorkExperience workExperience)
    {
        _db.WorkExperiences.Add(workExperience);
        await _db.SaveChangesAsync();
        
        return true;
    }


    public async Task<bool> AddEducationAsync(Education education)
    {
        _db.Educations.Add(education);
        await _db.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> AddCourseAsync(Course course)
    {
        _db.Courses.Add(course);
        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<WorkExperience?> GetWorkExperienceForEditAsync(int id, int userId)
    {
        WorkExperience? workExperience = await _db.WorkExperiences
            .Include(w => w.Resume)
            .FirstOrDefaultAsync(w => w.Id == id && w.Resume!.UserId == userId);
        
        return workExperience;
    }

    public async Task<bool> UpdateWorkExperienceAsync(WorkExperience workExperience, int userId)
    {
        _db.WorkExperiences.Update(workExperience);
        await _db.SaveChangesAsync();

        return true;
    }


    public async Task<int?> DeleteWorkExperienceAsync(int id, int userId)
    {
        WorkExperience? workExperience = await _db.WorkExperiences
            .Include(w => w.Resume)
            .FirstOrDefaultAsync(w => w.Id == id && w.Resume!.UserId == userId);
        
        if (workExperience == null)
        {
            return null;
        }
        
        int resumeId= workExperience.ResumeId;
        
        _db.WorkExperiences.Remove(workExperience);
        await _db.SaveChangesAsync();
        
        return resumeId;
    }


    public async Task<Resume?> GetResumeDetailsAsync(int id, int userId)
    {
        Resume? resume = await _db.Resumes
            .Include(r => r.Category)
            .Include(r => r.WorkExperiences)
            .Include(r => r.Educations)
            .Include(r => r.Courses)
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

        if (resume == null)
        {
            return null;
        }

        return resume;
    }


    public async Task<Education?> GetEducationForEditAsync(int id, int userId)
    {
        Education? education = await _db.Educations
            .Include(e => e.Resume)
            .FirstOrDefaultAsync(e => e.Id == id && e.Resume!.UserId == userId);

        if (education == null)
        {
            return null;
        }

        return education;
    }


    public async Task<bool> UpdateEducationAsync(Education education, int userId)
    {
        Education? existingEducation = await _db.Educations
            .Include(e => e.Resume)
            .FirstOrDefaultAsync(e => e.Id == education.Id && e.Resume!.UserId == userId);

        if (existingEducation == null)
        {
            return false;
        }
        
        _db.Educations.Update(education);
        await _db.SaveChangesAsync();
        

        return true;
    }


    public async Task<int?> DeleteEducationAsync(int id, int userId)
    {
        Education? education = await _db.Educations
            .Include(e => e.Resume)
            .FirstOrDefaultAsync(e => e.Id == id && e.Resume!.UserId == userId);
        
        if (education == null)
        {
            return null;
        }

        int resumeId = education.ResumeId;

        _db.Educations.Remove(education);
        await _db.SaveChangesAsync();

        return resumeId;
    }

    public async Task<bool> UpdateCourseAsync(Course course, int userId)
    {
        Course? existingCourse= await _db.Courses
            .Include(c=>c.Resume)
            .FirstOrDefaultAsync(c=> c.Id==course.Id && c.Resume!.UserId == userId);

        if (existingCourse == null)
        {
            return false;
        }
        _db.Courses.Update(course);
        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<Course?> GetCourseForEditAsync(int id, int userId)
    {
        Course? course = await _db.Courses
            .Include(c => c.Resume)
            .FirstOrDefaultAsync(c => c.Id == id && c.Resume!.UserId == userId);

        if (course == null)
        {
            return null;
        }

        return course;
    }


    public async Task<int?> DeleteCourseAsync(int id, int userId)
    {
        Course? course = await _db.Courses
            .Include(c => c.Resume)
            .FirstOrDefaultAsync(c => c.Id == id && c.Resume!.UserId == userId);

        if (course == null)
        {
            return null;
        }
        
        int resumeId = course.ResumeId;

        _db.Courses.Remove(course);
        await _db.SaveChangesAsync();
        
        return resumeId;
    }
    
    
public async Task<Resume?> GetResumeAsync(int id)
    { 
        return await _db.Resumes 
        .Include(r => r.Category) 
        .Include(r => r.WorkExperiences)
        .Include(r => r.Educations) 
        .Include(r => r.Courses) 
        .FirstOrDefaultAsync(r => r.Id == id); 
    }


    public async Task<List<Resume>> GetPublishedResumesAsync()
    {
        return await _db.Resumes 
            .Include(r => r.Category) 
            .Include(r => r.User) 
            .Where(r => r.IsPublished)
            .OrderByDescending(r => r.UpdatedAt) 
            .ToListAsync();
    }
    
    
    public async Task<bool> TogglePublicationAsync(int id, int userId)
    { 
        var resume = await _db.Resumes 
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
        if (resume == null)
            return false;

        resume.IsPublished = !resume.IsPublished;
        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<Resume?> GetPublicDetails(int id)
    {
        Resume? resume = await _db.Resumes
            .Include(r => r.Category)
            .Include(r => r.WorkExperiences)
            .Include(r => r.Educations)
            .Include(r => r.Courses)
            .FirstOrDefaultAsync(r => r.Id == id && r.IsPublished);

        return resume;

    }
    
    
    public async Task<List<Vacancy>> GetCompanyVacanciesAsync(int companyId)
    {
        List<Vacancy> vacancies = await _db.Vacancies
            .Where(v => v.CompanyId == companyId && v.IsPublished)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync();

        return vacancies;
    }


    public async Task<List<Resume>> GetResumesByCategoryAsync(int? categoryId)
    {
        var query = _db.Resumes
            .Include(r => r.Category)
            .Include(r => r.User)
            .Where(r => r.IsPublished);
        
        if (categoryId.HasValue)
        {
            query=query.Where(r=>r.CategoryId==categoryId.Value);
        }

        return await query
            .OrderByDescending(r => r.UpdatedAt)
            .ToListAsync();
    }
}