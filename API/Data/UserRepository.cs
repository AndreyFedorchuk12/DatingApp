using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class UserRepository : IUserRepository
{
    private readonly DataContext _dataContext;
    private readonly IMapper _mapper;

    public UserRepository(DataContext dataContext, IMapper mapper)
    {
        _dataContext = dataContext;
        _mapper = mapper;
    }

    public void Update(AppUser user)
    {
        _dataContext.Entry(user).State = EntityState.Modified;
    }

    public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
    {
        var query = _dataContext.Users.AsQueryable();
        query = query.Where(user => user.UserName != userParams.CurrentUsername);
        query = query.Where(user => user.Gender == userParams.Gender);

        var minDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MaxAge - 1));
        var maxDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MinAge));

        query = query.Where(user => user.DateOfBirth >= minDob);
        query = query.Where(user => user.DateOfBirth <= maxDob);

        query = userParams.OrderBy switch
        {
            "createdAt" => query.OrderByDescending(user => user.CreatedAt),
            _ => query.OrderByDescending(user => user.LastActiveAt)
        };

        return await PagedList<MemberDto>.CreateAsync(
            query.AsNoTracking().ProjectTo<MemberDto>(_mapper.ConfigurationProvider),
            userParams.PageNumber,
            userParams.PageSize);
    }

    public async Task<MemberDto?> GetMemberByUsernameAsync(string username, bool isCurrentUser)
    {
        var query = _dataContext.Users
            .Where(user => user.UserName == username)
            .ProjectTo<MemberDto>(_mapper.ConfigurationProvider);
        
        if (isCurrentUser)
        {
            query = query.IgnoreQueryFilters();
        }
        
        return await query.SingleOrDefaultAsync();
    }

    public async Task<AppUser?> GetUserByUsernameAsync(string? username)
    {
        return await _dataContext.Users
            .Include(user => user.Photos)
            .SingleOrDefaultAsync(user => user.UserName == username);
    }

    public async Task<AppUser?> GetUserByIdAsync(int userId)
    {
        return await _dataContext.Users.FindAsync(userId);
    }

    public async Task<string> GetUserGender(string username)
    {
        return (await _dataContext.Users.Where(user => user.UserName == username)
            .Select(user => user.Gender)
            .SingleOrDefaultAsync())!;
    }

    public async Task<AppUser?> GetUserByPhotoId(int id)
    {
        return await _dataContext.Users.Include(user => user.Photos).IgnoreQueryFilters()
            .SingleOrDefaultAsync(user => user.Photos.Any(photo => photo.Id == id));
    }
}