using home_work_66.Models;
using home_work_66.Services.Interfaces;
using home_work_66.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace home_work_66.Services;

public class ChatService : IChatService
{
    private readonly HeadHunterContext _db;
    
    public ChatService(HeadHunterContext db)
    {
      _db = db;
    }
    
    public async Task<Chat?> CreateChatAsync(ChatCreateViewModel model, int userId)
    {
        var existingChat = await _db.Chats
            .FirstOrDefaultAsync(c => c.UserId == userId && c.VacancyId == model.VacancyId);
        if (existingChat != null)
            return existingChat;
        Chat chat = new Chat
        {
            UserId = userId,
            CompanyId = model.CompanyId,
            VacancyId = model.VacancyId,
            ResumeId = model.ResumeId!.Value

        };
        await _db.Chats.AddAsync(chat);
        await _db.SaveChangesAsync();
        return chat;
    }


    public async Task<Chat?> GetChatAsync(int id)
    {
        return await _db.Chats
            .Include(x => x.Vacancy)
            .Include(x => x.Resume)
            .Include(x => x.User)
            .Include(x => x.Company)
            .FirstOrDefaultAsync(x => x.Id == id);
    }



    public async Task<List<Message>> GetMessagesAsync(int chatId)
    {
        return await _db.Messages
            .Where(x => x.ChatId == chatId)
            .Include(x => x.User)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();

    }


    public async Task<bool> SendMessageAsync(MessageCreateViewModel model, int userId)
    {
        Message message = new Message
        {
            Content = model.Content,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            ChatId = model.ChatId
        };
        await _db.Messages.AddAsync(message);
        await _db.SaveChangesAsync();

        return true;

    }

    public async Task<Chat?> CreateChatFromResumeAsync(int resumeId, int vacancyId, int companyId)
    {
        var resume = await _db.Resumes
            .FirstOrDefaultAsync(r => r.Id == resumeId && r.IsPublished);
        if (resume == null)
            return null;

        var vacancy = await _db.Vacancies
            .FirstOrDefaultAsync(v => v.Id == vacancyId && v.CompanyId == companyId);

        if (vacancy == null)
            return null;

        var existingChat = await _db.Chats.FirstOrDefaultAsync(c =>
            c.UserId == resume.UserId &&
            c.CompanyId == companyId &&
            c.VacancyId == vacancyId);

        if (existingChat != null)
            return existingChat;

        Chat chat = new Chat
        {
            UserId = resume.UserId,
            CompanyId = companyId,
            ResumeId = resume.Id,
            VacancyId = vacancy.Id
        };
        await _db.Chats.AddAsync(chat);
        await _db.SaveChangesAsync();

        return chat;
    }

}
