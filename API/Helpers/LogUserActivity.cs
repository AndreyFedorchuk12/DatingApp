using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Helpers;

public class LogUserActivity : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var resultContext = await next();

        if (resultContext.HttpContext.User.Identity is { IsAuthenticated: false })
            return;

        var userId = resultContext.HttpContext.User.GetUserId();
        var userRepository = resultContext.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
        var user = await userRepository.GetUserByIdAsync(userId);
        if (user != null)
        {
            user.LastActiveAt = DateTime.UtcNow;
            await userRepository.SaveAllAsync();
        }
    }
}