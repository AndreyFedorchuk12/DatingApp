using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class LikesRepository : ILikesRepository
{
    private readonly DataContext _dataContext;

    public LikesRepository(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public async Task<UserLike?> GetUserLike(int sourceUserId, int targetUserId)
    {
        return await _dataContext.Likes.FindAsync(sourceUserId, targetUserId);
    }

    public async Task<AppUser?> GetUserWithLikes(int userId)
    {
        return await _dataContext.Users.Include(user => user!.LikedUsers)
            .FirstOrDefaultAsync(user => user != null && user.Id == userId);
    }

    public async Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams)
    {
        var users = _dataContext.Users.OrderBy(user => user.UserName).AsQueryable();
        var likes = _dataContext.Likes.AsQueryable();

        switch (likesParams.Predicate)
        {
            case "liked":
                likes = likes.Where(like => like.SourceUserId == likesParams.UserId);
                users = likes.Select(like => like!.TargetUser)!;
                break;
            case "likedBy":
                likes = likes.Where(like => like.TargetUserId == likesParams.UserId);
                users = likes.Select(like => like!.SourceUser)!;
                break;
        }

        var likedUsers = users.Select(user => new LikeDto
        {
            Id = user.Id,
            UserName = user.UserName!,
            Age = user.DateOfBirth.CalculateAge(),
            KnownAs = user.KnownAs,
            City = user.City,
            PhotoUrl = user.Photos.FirstOrDefault(photo => photo.IsMain)!.Url
        });
        
        return await PagedList<LikeDto>.CreateAsync(likedUsers, likesParams.PageNumber, likesParams.PageSize);
    }
    
    public async Task<bool> SaveAllAsync()
    {
        return await _dataContext.SaveChangesAsync() > 0;
    }
}