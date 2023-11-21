using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class BuggyController: BaseApiController
{
    private readonly DataContext _dataContext;

    public BuggyController(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    [Authorize]
    [HttpGet("auth")]
    public ActionResult<string> GetSecret()
    {
        return "secret text";
    }
    
    [HttpGet("not-found")]
    public ActionResult<AppUser> GetNotFound()
    {
        var notFoundUser = _dataContext.Users.Find(-1);

        if (notFoundUser == null)
            return NotFound();
        return notFoundUser;
    }
    
    [HttpGet("server-error")]
    public ActionResult<string> GetServerError()
    {
        var notFoundUser = _dataContext.Users.Find(-1);
        return notFoundUser!.UserName;
    }
    
    [HttpGet("bad-request")]
    public ActionResult<string> GetBadRequest()
    {
        return BadRequest("This was a not good request");
    }
}