using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class MessagesController : BaseApiController
{
    private readonly IUserRepository _userRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IMapper _mapper;

    public MessagesController(IUserRepository userRepository, IMessageRepository messageRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _messageRepository = messageRepository;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
    {
        var senderUsername = User.GetUsername();
        if (createMessageDto.RecipientUsername != null && senderUsername == createMessageDto.RecipientUsername.ToLower())
            return BadRequest("You cannot send message to yourself");
        var sender = await _userRepository.GetUserByUsernameAsync(senderUsername);
        var recipient = await _userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);
        if (recipient == null) return NotFound("Could not find user");
        if (sender == null) return BadRequest("Failed to send message");
        var message = new Message
        {
            Sender = sender,
            Recipient = recipient,
            SenderUsername = sender.UserName,
            RecipientUsername = recipient.UserName,
            Content = createMessageDto.Content
        };
        _messageRepository.AddMessage(message);
        if (!await _messageRepository.SaveAllAsync()) 
            return BadRequest("Failed to send message");
        var messageDto = _mapper.Map<MessageDto>(message);
        return Ok(messageDto);
    }

    [HttpGet]
    public async Task<ActionResult<PagedList<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
    {
        messageParams.UserName = User.GetUsername();
        var messages = await _messageRepository.GetMessagesForUser(messageParams);
        Response.AddPaginationHeader(new PaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages));
        return Ok(messages);
    }
    
    [HttpGet("thread/{username}")]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
    {
        var currentUsername = User.GetUsername();
        if (currentUsername != null) return Ok(await _messageRepository.GetMessageThread(currentUsername, username));
        return BadRequest("Unauthorized");
    }
    
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteMessage(int id)
    {
        var username = User.GetUsername();
        var message = await _messageRepository.GetMessage(id);
        if (message == null) return NotFound();
        if (message.SenderUsername != username && message.RecipientUsername != username) return Unauthorized();
        if (message.SenderUsername == username) message.SenderDeleted = true;
        if (message.RecipientUsername == username) message.RecipientDeleted = true;
        if (message is { SenderDeleted: true, RecipientDeleted: true }) _messageRepository.DeleteMessage(message);
        if (!await _messageRepository.SaveAllAsync()) return BadRequest("Problem deleting the message");
        return Ok();
    }
}