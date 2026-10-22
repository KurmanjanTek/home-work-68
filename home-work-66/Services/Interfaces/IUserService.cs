using home_work_66.Models;
using home_work_66.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace home_work_66.Services.Interfaces;

public interface IUserService
{
    Task<User?> GetUserByIdAsync(int id);
    
    Task<IdentityResult>UpdateProfileAsync(User user, EditProfileViewModel model);
    
    Task<List<Resume>>GetRezumesByUserIdAsync(int id);
    
    Task<List<Chat>>GetUserResponsesAsync (int userId);
}