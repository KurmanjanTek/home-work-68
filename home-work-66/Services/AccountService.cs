using home_work_66.Models;
using home_work_66.Services.Interfaces;
using home_work_66.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace home_work_66.Services;

public class AccountService : IAccountService
{
    private readonly HeadHunterContext _db;
    private readonly UserManager<User> _userManager;
    private readonly IWebHostEnvironment _environment;
    SignInManager<User> _signInManager;

    public AccountService(HeadHunterContext db, UserManager<User> userManager, IWebHostEnvironment environment, SignInManager<User> signInManager)
    {
        _db = db;
        _userManager = userManager;
        _environment = environment;
        _signInManager = signInManager;
    }

    public async Task<IdentityResult> RegisterAsync(RegisterViewModel model)
    {
        string avatarPath = "/default.jpg";

        if (model.Avatar != null)
        {
            string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "avatars");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.Avatar.FileName)}";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);
            await using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await model.Avatar.CopyToAsync(fileStream);
            }

            avatarPath = $"/uploads/avatars/{uniqueFileName}";
        }

            User userToCreate;
            string roleToAssign;

            if (model.IsCompany)
            {
                userToCreate = new User
                {
                    Email = model.Email,
                    UserName = model.Email,
                    CompanyName = model.UserName,
                    PhoneNumber = model.PhoneNumber,
                    AvatarUrl = avatarPath

                };
                roleToAssign = "company";
            }
            else
            {
                userToCreate = new User
                {
                    Email = model.Email,
                    UserName = model.UserName,
                    PhoneNumber = model.PhoneNumber,
                    AvatarUrl = avatarPath

                };
                roleToAssign = "user";

            }

            var result = await _userManager.CreateAsync(userToCreate, model.Password);

            if (!result.Succeeded)
            {
                return result;
            }

            var roleResult = await _userManager.AddToRoleAsync(userToCreate, roleToAssign);

            if (!roleResult.Succeeded)
            {
                return roleResult;
            }

            return IdentityResult.Success;
            
    }

    public async Task<SignInResult> LoginAsync(LoginViewModel model)
    {
        User? user = await _userManager.FindByEmailAsync(model.Email);
        
        if (user== null)
        {
            return SignInResult.Failed;
        }
        var result = await _signInManager.PasswordSignInAsync(
            user,
            model.Password,
            model.RememberMe,
            false
        );
        
        return result;
    }
    
}
