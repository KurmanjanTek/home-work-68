namespace home_work_66.Services.Interfaces;

public interface IValidationService
{
    Task<bool> IsEmailAvailableAsync(string? email, int? id);
    
    bool IsPasswordValid(string? password);
}