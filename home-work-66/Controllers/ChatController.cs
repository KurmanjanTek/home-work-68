using home_work_66.Models;
using home_work_66.Services.Interfaces;
using home_work_66.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace home_work_66.Controllers;

[Authorize]
public class ChatController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly IChatService _chatService;

    public ChatController(UserManager<User> userManager, IChatService chatService)
    {
        _userManager = userManager;
        _chatService = chatService;
       
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateChatAsync(ChatCreateViewModel model)
    {
        int userId = int.Parse(_userManager.GetUserId(User)!);

        if (!ModelState.IsValid)
        {
            return NotFound();
        }
            
        var chat = await _chatService.CreateChatAsync(model, userId);

        if (chat == null)
            return NotFound();

        return RedirectToAction("Index", new
        {
            id = chat.Id,
            companyId = chat.CompanyId
        });
    }


    public async Task<IActionResult> Index(int id, int companyId) 
    { 
        var chat = await _chatService.GetChatAsync(id);
        if (chat == null)
            return NotFound();

        var messages = await _chatService.GetMessagesAsync(id);

        ViewBag.UserId = int.Parse(_userManager.GetUserId(User)!);
        ViewBag.CompanyId = companyId;
        ViewBag.ChatId = id;

        return View(new ChatPageViewModel
        {
            Chat = chat,
            Messages = messages
        });
    }
    
    public async Task<IActionResult> UpdateListAsync(int? id) 
    { 
        if (id == null) return NotFound();
        var messages = await _chatService.GetMessagesAsync(id.Value);

        ViewBag.UserId = int.Parse(_userManager.GetUserId(User)!);

        return PartialView("_ChatPartialView", messages);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendMessageAsync(MessageCreateViewModel model) 
    { 
        int userId = int.Parse(_userManager.GetUserId(User)!);
        if (!ModelState.IsValid)
            return Json(new { success = false });

        await _chatService.SendMessageAsync(model, userId);

        var messages = await _chatService.GetMessagesAsync(model.ChatId);

        ViewBag.UserId = userId;

        return PartialView("_ChatPartialView", messages);
    }
    
    [HttpPost]
    [Authorize(Roles = "company")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateFromResume(int resumeId, int vacancyId) 
    { 
        int companyId = int.Parse(_userManager.GetUserId(User)!);
        var chat = await _chatService.CreateChatFromResumeAsync(
            resumeId, vacancyId, companyId);

        if (chat == null)
            return NotFound();

        return RedirectToAction("Index", new
        {
            id = chat.Id,
            companyId = chat.CompanyId
        });
    }
}