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
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public MessagesController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
    {
        var senderUsername = User.GetUsername();
        if (createMessageDto.RecipientUsername != null && senderUsername == createMessageDto.RecipientUsername.ToLower())
            return BadRequest("You cannot send message to yourself");
        var sender = await _unitOfWork.UserRepository.GetUserByUsernameAsync(senderUsername);
        var recipient = await _unitOfWork.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);
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
        _unitOfWork.MessageRepository.AddMessage(message);
        if (!await _unitOfWork.Complete()) 
            return BadRequest("Failed to send message");
        var messageDto = _mapper.Map<MessageDto>(message);
        return Ok(messageDto);
    }

    [HttpGet]
    public async Task<ActionResult<PagedList<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
    {
        messageParams.UserName = User.GetUsername();
        var messages = await _unitOfWork.MessageRepository.GetMessagesForUser(messageParams);
        Response.AddPaginationHeader(new PaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages));
        return Ok(messages);
    }
    
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteMessage(int id)
    {
        var username = User.GetUsername();
        var message = await _unitOfWork.MessageRepository.GetMessage(id);
        if (message == null) return NotFound();
        if (message.SenderUsername != username && message.RecipientUsername != username) return Unauthorized();
        if (message.SenderUsername == username) message.SenderDeleted = true;
        if (message.RecipientUsername == username) message.RecipientDeleted = true;
        if (message is { SenderDeleted: true, RecipientDeleted: true }) _unitOfWork.MessageRepository.DeleteMessage(message);
        if (!await _unitOfWork.Complete()) return BadRequest("Problem deleting the message");
        return Ok();
    }
}