using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using UserManagement.Data;

namespace UserManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly UserManagementDataContext context;

        public UsersController(UserManagementDataContext context)
        {
            this.context = context;
        }

        [HttpGet("current")]
        public async Task<ActionResult<UserResponse>> GetCurrentUser()
        {
            var nameIdentifier = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var user = await context.Users.FirstAsync(user => user.NameIdentifier == nameIdentifier);
            return Ok(new UserResponse(user.Id, user.NameIdentifier, user.Email, user.FirstName, user.LastName));
        }

        [HttpGet]
        public async Task<ActionResult<List<UserResponse>>> GetAllUsers([FromQuery(Name = "filter")] string? filter)
        {
            List<UserResponse> users;
            if (filter is not null)
            {
                Debug.WriteLine(filter);
                users = await context.Users.Where(u => u.Email.Contains(filter) ||
                    (u.FirstName != null && u.FirstName.Contains(filter)) ||
                    (u.LastName != null && u.LastName.Contains(filter)))
                    .Select(u => new UserResponse(u.Id, u.NameIdentifier, u.Email, u.FirstName, u.LastName))
                    .ToListAsync();
            } else
            {
                users = await context.Users.Select(u => new UserResponse(u.Id, u.NameIdentifier, u.Email, u.FirstName, u.LastName)).ToListAsync();
            }
            return Ok(users);
        }
    }

    public record UserResponse(int Id, string NameIdentifier, string Email, string? FirstName, string? LastName);
}
