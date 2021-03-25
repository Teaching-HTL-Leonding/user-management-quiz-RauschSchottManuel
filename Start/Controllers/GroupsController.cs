using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using UserManagement.Data;

namespace UserManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "administrator")]
    public class GroupsController : ControllerBase
    {
        private readonly UserManagementDataContext context;

        public GroupsController(UserManagementDataContext context)
        {
            this.context = context;
        }

        [HttpGet("single/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GroupResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GroupResponse>> GetSingleGroup(int id)
        {
            var group = await context.Groups.FirstOrDefaultAsync(g => g.Id == id);
            if (group is null) return NotFound();

            return Ok(new GroupResponse(group.Id, group.Name));
        }

        [HttpGet]
        public async Task<ActionResult<GroupResponse>> GetAllGroups()
        {
            var groups = await context.Groups.Select(g => new GroupResponse(g.Id, g.Name)).ToListAsync();
            return Ok(groups);
        }

        [HttpGet("childGroup")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<GroupResponse>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GroupResponse>> GetChildGroups([FromQuery(Name = "parentId")] int id)
        {
            if (!await context.Groups.AnyAsync(g => g.Id == id)) return NotFound();
            var group = await context.Groups.Where(g => g.ParentGroup != null && g.ParentGroup.Id.Equals(id))
                .Select(g => new GroupResponse(g.Id, g.Name))
                .ToListAsync();

            return Ok(group);
        }

        [HttpGet("users")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<UserResponse>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllUsers([FromQuery(Name = "id")]int id, [FromQuery(Name = "recursive")] bool recursive)
        {
            var group = await context.Groups.Include(g => g.Members).FirstOrDefaultAsync(g => g.Id == id);

            if (group is null) return NotFound();

            if (!recursive)
            {
                if (group.Members is null) return Ok(new List<UserResponse>());
                return Ok(group.Members.Select(u => new UserResponse(u.Id, u.NameIdentifier, u.Email, u.FirstName, u.LastName)).ToList());
            }

            return Ok(await GetUsersRecursive(group.Id));
        }
        
        private async Task<List<UserResponse>> GetUsersRecursive(int id)
        {
            var users = new List<UserResponse>();
            var groups = await context.Groups.Include(g => g.Members).Where(g => g.ParentGroup != null && g.ParentGroup.Id == id).ToListAsync();

            var parentGroup = await context.Groups.FirstOrDefaultAsync(g => g.Id == id);

            if (parentGroup.Members != null)
                users.AddRange(parentGroup.Members.Select(u => new UserResponse(u.Id, u.NameIdentifier, u.Email, u.FirstName, u.LastName)).ToList());

            foreach (var group in groups)
            {
                users.AddRange(await GetUsersRecursive(group.Id));
            }
            return users.Distinct().ToList();
        }
    }

    public record GroupResponse(int Id, string Name);
}
