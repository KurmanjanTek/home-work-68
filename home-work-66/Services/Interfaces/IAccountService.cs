using home_work_66.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace home_work_66.Services.Interfaces;

public interface IAccountService
{
    Task<IdentityResult> RegisterAsync(RegisterViewModel model);
    
    Task<Microsoft.AspNetCore.Identity.SignInResult> LoginAsync(LoginViewModel model);
}