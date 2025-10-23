using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using LuxAPI.DAL;
using LuxAPI.Models;
using System.Linq;

public class AdminOnlyFilter : IAsyncActionFilter
{
    private readonly AppDbContext _context;

    public AdminOnlyFilter(AppDbContext context)
    {
        _context = context;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var userEmail = context.HttpContext.User.Identity?.Name;
        if (string.IsNullOrEmpty(userEmail))
        {
            context.Result = new ForbidResult();
            return;
        }

        var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
        if (user == null || user.Role != UserRole.Admin)
        {
            context.Result = new ForbidResult();
            return;
        }

        await next();
    }
}
