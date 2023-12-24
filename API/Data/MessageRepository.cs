using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class MessageRepository : IMessageRepository
{
    private readonly DataContext _dataContext;
    private readonly IMapper _mapper;

    public MessageRepository(DataContext dataContext, IMapper mapper)
    {
        _dataContext = dataContext;
        _mapper = mapper;
    }

    public void AddMessage(Message message)
    {
        _dataContext.Messages.Add(message);
    }

    public void DeleteMessage(Message message)
    {
        _dataContext.Messages.Remove(message);
    }

    public async Task<Message?> GetMessage(int id)
    {
        return await _dataContext.Messages.FindAsync(id);
    }

    public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
    {
        var query = _dataContext.Messages.OrderByDescending(x => x!.MessageSent).AsQueryable();
        query = messageParams.Container switch
        {
            "Inbox" => query.Where(m =>
                m.RecipientUsername == messageParams.UserName && !m.RecipientDeleted),
            "Outbox" => query.Where(m => m.SenderUsername == messageParams.UserName && !m.SenderDeleted),
            _ => query.Where(m => m.RecipientUsername == messageParams.UserName && !m.RecipientDeleted && m.DateRead == null)
        };
        return await PagedList<MessageDto>.CreateAsync(
            query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider).AsNoTracking(), messageParams.PageNumber,
            messageParams.PageSize);
    }

    public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername)
    {
        var messages = await _dataContext.Messages
            .Include(u => u!.Sender).ThenInclude(p => p!.Photos)
            .Include(u => u!.Recipient).ThenInclude(p => p!.Photos)
            .Where(m => (m.RecipientUsername == currentUsername && !m.RecipientDeleted && m.SenderUsername == recipientUsername ||
                         m.RecipientUsername == recipientUsername && !m.SenderDeleted && m.SenderUsername == currentUsername))
            .OrderBy(m => m.MessageSent)
            .ToListAsync();

        var unreadMessages = messages.Where(m => m.DateRead == null && m.RecipientUsername == currentUsername).ToList();

        if (!unreadMessages.Any()) return _mapper.Map<IEnumerable<MessageDto>>(messages);
        foreach (var message in unreadMessages.OfType<Message>())
        {
            message.DateRead = DateTime.UtcNow;
        }

        await _dataContext.SaveChangesAsync();

        return _mapper.Map<IEnumerable<MessageDto>>(messages);
    }

    public async Task<bool> SaveAllAsync()
    {
        return await _dataContext.SaveChangesAsync() > 0;
    }

    public void AddGroup(Group? group)
    {
        if (group != null) _dataContext.Groups.Add(group);
    }

    public void RemoveConnection(Connection connection)
    {
        _dataContext.Connections.Remove(connection);
    }

    public async Task<Connection?> GetConnection(string connectionId)
    {
        return await _dataContext.Connections.FindAsync(connectionId);
    }

    public async Task<Group?> GetMessageGroup(string groupName)
    {
        return await _dataContext.Groups.Include(x => x!.Connections)
            .FirstOrDefaultAsync(x => x.Name == groupName);
    }

    public async Task<Group?> GetGroupForConnection(string connectionId)
    {
        return await _dataContext.Groups.Include(x => x!.Connections)
            .Where(x => x!.Connections.Any(c => c.ConnectionId == connectionId))
            .FirstOrDefaultAsync();
    }
}