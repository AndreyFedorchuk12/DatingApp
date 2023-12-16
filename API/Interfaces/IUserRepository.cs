using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces;

public interface IUserRepository
{
    void Update(AppUser user);
    Task<bool> SaveAllAsync();
    Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams);
    Task<MemberDto?> GetMembersByUsernameAsync(string username);
    Task<AppUser?> GetUserByUsernameAsync(string? username);
    Task<AppUser?> GetUserByIdAsync(int userId);
}