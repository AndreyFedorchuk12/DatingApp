using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class UsersController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IPhotoService _photoService;

    public UsersController(IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _photoService = photoService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedList<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
    {
        var gender = await _unitOfWork.UserRepository.GetUserGender(User.GetUsername()!);
        userParams.CurrentUsername = User.GetUsername();
        if (string.IsNullOrEmpty(userParams.Gender))
            userParams.Gender = gender == "male" ? "female" : "male";

        var users = await _unitOfWork.UserRepository.GetMembersAsync(userParams);
        Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount,
            users.TotalPages));
        return Ok(users);
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<MemberDto?>> GetUser(string username)
    {
        var currentUsername = User.GetUsername();
        return await _unitOfWork.UserRepository.GetMemberByUsernameAsync(username, isCurrentUser: currentUsername == username);
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
    {
        var username = User.GetUsername();
        var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);

        if (user == null)
            return NotFound();

        _mapper.Map(memberUpdateDto, user);

        if (await _unitOfWork.Complete())
            return NoContent();

        return BadRequest("Failed to update user");
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
    {
        var username = User.GetUsername();
        var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);

        if (user == null)
            return NotFound();

        var result = await _photoService.AddPhotoAsync(file);

        if (result.Error != null)
            return BadRequest(result.Error.Message);

        var photo = new Photo { Url = result.SecureUrl.AbsoluteUri, PublicId = result.PublicId, AppUser = user, IsMain = false, IsApproved = false};

        user.Photos.Add(photo);
        if (await _unitOfWork.Complete())
        {
            return CreatedAtAction(nameof(GetUser), new { username = user.UserName }, _mapper.Map<PhotoDto>(photo));
        }

        return BadRequest("Failed to add photo");
    }

    [HttpPut("set-main-photo/{photoId}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        var username = User.GetUsername();
        var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);

        if (user == null)
            return NotFound();

        var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);

        if (photo == null)
            return NotFound();
        
        if (!photo.IsApproved)
            return BadRequest("Cannot set as main unapproved photo");

        if (photo.IsMain)
            return BadRequest("This is already the main photo");

        var currentMainPhoto = user.Photos.FirstOrDefault(p => p.IsMain);

        if (currentMainPhoto != null)
            currentMainPhoto.IsMain = false;

        photo.IsMain = true;

        if (await _unitOfWork.Complete())
            return NoContent();

        return BadRequest("Failed to set main photo");
    }

    [HttpDelete("delete-photo/{photoId}")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        var photo = await _unitOfWork.PhotoRepository.GetPhotoByIdAsync(photoId);
        switch (photo)
        {
            case { IsMain: true }:
                return BadRequest("You cannot delete your main photo");
            case { IsApproved: false }:
                return BadRequest("You cannot delete unapproved photo");
            case { PublicId: not null }:
            {
                _unitOfWork.PhotoRepository.RemovePhoto(photo);
                break;
            }
        }
        var user = await _unitOfWork.UserRepository.GetUserByPhotoId(photoId);
        if (photo != null) user?.Photos.Remove(photo);
        if (await _unitOfWork.Complete()) return Ok();
        return BadRequest("Failed to delete photo");
    }
}