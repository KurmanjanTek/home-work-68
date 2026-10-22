using home_work_66.Models;
using home_work_66.Services;
using home_work_66.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace home_work_66.Controllers;

public class ValidationController : Controller
{
    private readonly IValidationService _validationService;
    public ValidationController(ValidationService validationService)
    {
        _validationService=validationService;
    }
    
    [AcceptVerbs("GET", "POST")]
    public async Task<bool> CheckEmail(string? email, int? id)
    {
        return await _validationService.IsEmailAvailableAsync(email, id);
    }
    
    [AcceptVerbs("GET", "POST")]
    public bool CheckPassword(string? password)
    {
        return _validationService.IsPasswordValid(password);
    }
}