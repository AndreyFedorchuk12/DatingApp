using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class PhotoRepository : IPhotoRepository
{
    private readonly DataContext _dataContext;

    public PhotoRepository(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public async Task<IEnumerable<PhotoForApprovalDto>> GetUnapprovedPhotosAsync()
    {
        return await _dataContext.Photos
            .IgnoreQueryFilters()
            .Where(p => p.IsApproved == false)
            .Select(u => new PhotoForApprovalDto
            {
                Id = u.Id,
                Username = u.AppUser!.UserName!,
                Url = u.Url,
                IsApproved = u.IsApproved
            }).ToListAsync();
    }

    public async Task<Photo?> GetPhotoByIdAsync(int id)
    {
        return await _dataContext.Photos
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.Id == id);
    }

    public void RemovePhoto(Photo photo)
    {
        _dataContext.Photos.Remove(photo);
    }
}