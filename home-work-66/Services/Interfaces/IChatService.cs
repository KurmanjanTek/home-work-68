using home_work_66.Models;
using home_work_66.ViewModels;

namespace home_work_66.Services.Interfaces;

public interface IChatService
{
    Task<Chat?> CreateChatAsync(ChatCreateViewModel model, int userId);
    
    Task<Chat?> CreateChatFromResumeAsync(int resumeId, int vacancyId, int companyId);
    
    Task<Chat?> GetChatAsync(int id);
    
    Task<List<Message>> GetMessagesAsync(int chatId);
    
    Task<bool> SendMessageAsync(MessageCreateViewModel model, int userId);
    
}