using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces;

public interface IUserRepository
{
    void Update(AppUser user);
    Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams);
    Task<MemberDto?> GetMemberByUsernameAsync(string username, bool isCurrentUser);
    Task<AppUser?> GetUserByUsernameAsync(string? username);
    Task<AppUser?> GetUserByIdAsync(int userId);
    Task<string> GetUserGender(string username);
    Task<AppUser?> GetUserByPhotoId(int id);
}