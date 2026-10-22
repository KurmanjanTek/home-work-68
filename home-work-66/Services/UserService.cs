using home_work_66.Models;
using home_work_66.Services.Interfaces;
using home_work_66.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace home_work_66.Services;

public class UserService: IUserService
{
    private readonly HeadHunterContext _db;
    private readonly UserManager<User> _userManager;
    private readonly IWebHostEnvironment _environment;
    
    public UserService(HeadHunterContext db, UserManager<User> userManager, IWebHostEnvironment environment, IUserService userService)
    {
        _db = db;
        _userManager = userManager;
        _environment = environment;
    }
    
    
    public async Task<User?> GetUserByIdAsync(int id)
    {
        var user = await _db.Users.FindAsync(id);
        
        return user;
    }

    public async Task<IdentityResult> UpdateProfileAsync(User user, EditProfileViewModel model)
    {
        user.UserName = model.UserName;
        user.Email = model.Email;
        user.PhoneNumber = model.PhoneNumber;
        
        if (model.Avatar != null && model.Avatar.Length > 0)
        {
            string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "avatars");
            
            Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.Avatar.FileName)}";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            await using FileStream fileStream = new FileStream(filePath, FileMode.Create);
            await model.Avatar.CopyToAsync(fileStream);

            user.AvatarUrl = $"/uploads/avatars/{uniqueFileName}";
        }
        
        IdentityResult result = await _userManager.UpdateAsync(user);
        return result;
    }

    public async Task<List<Resume>> GetRezumesByUserIdAsync(int userId)
    {
        var resumes = await _db.Resumes
            .Where(x => x.UserId == userId)
            .Include(x => x.Category)
            .ToListAsync();

        return resumes;
    }

    public async Task<List<Chat>> GetUserResponsesAsync(int userId)
    {
        List<Chat> responses = await _db.Chats
            .Where(c => c.UserId == userId)
            .Include(c => c.Vacancy)
            .Include(c => c.Company)
            .Include(c => c.Resume)
            .ToListAsync();

        return responses;
    }
}