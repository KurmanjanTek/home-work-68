using home_work_66.Models;
using home_work_66.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace home_work_66.Services;

public class ValidationService : IValidationService
{
    private readonly HeadHunterContext _context;

    public ValidationService(HeadHunterContext context)
    {
        _context = context;
    }

    public async Task<bool> IsEmailAvailableAsync(string? email, int? id)
    {
        if (string.IsNullOrWhiteSpace(email))
            return true;
        
        if(id.HasValue)
            return ! await _context.Users
                .AnyAsync(u => u.Email == email &&  u.Id != id.Value);
        
        return !await _context.Users.AnyAsync(u => u.Email == email);
    }

    public bool IsPasswordValid(string? password)
    {
        if(string.IsNullOrWhiteSpace(password))
            return true;
        return (password.Any(char.IsUpper) && password
            .Any(char.IsLower) && password
            .Any(char.IsDigit));
    }
}