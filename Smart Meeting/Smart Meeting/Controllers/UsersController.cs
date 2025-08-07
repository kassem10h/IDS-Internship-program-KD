using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartMeeting.Attributes;
using SmartMeeting.DTOs;
using SmartMeeting.Models;
using System.Security.Claims;

namespace SmartMeeting.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        /// Get all users (Admin only)
        /// </summary>
        [HttpGet]
        [Roles(RoleConstants.Admin)]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = _userManager.Users.Where(u => u.IsActive).ToList();
            var userDtos = new List<UserInfoDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userDtos.Add(new UserInfoDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = user.FullName,
                    Roles = roles.ToList()
                });
            }

            return Ok(new { success = true, data = userDtos });
        }

        /// <summary>
        /// Get user by ID (Admin only)
        /// </summary>
        [HttpGet("{id}")]
        [Roles(RoleConstants.Admin)]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null || !user.IsActive)
            {
                return NotFound(new { success = false, message = "User not found" });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var userDto = new UserInfoDto
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                Roles = roles.ToList()
            };

            return Ok(new { success = true, data = userDto });
        }

        /// <summary>
        /// Update user profile (Users can update their own profile, Admins can update any)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDto updateUserDto)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole(RoleConstants.Admin);

            // Users can only update their own profile, unless they're admin
            if (!isAdmin && currentUserId != id)
            {
                return Forbid();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null || !user.IsActive)
            {
                return NotFound(new { success = false, message = "User not found" });
            }

            // Update user properties
            user.FirstName = updateUserDto.FirstName;
            user.LastName = updateUserDto.LastName;

            // Only admin can update email
            if (isAdmin && !string.IsNullOrEmpty(updateUserDto.Email) && updateUserDto.Email != user.Email)
            {
                user.Email = updateUserDto.Email;
                user.UserName = updateUserDto.Email;
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Failed to update user",
                    errors = result.Errors.Select(e => e.Description).ToList()
                });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var userDto = new UserInfoDto
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                Roles = roles.ToList()
            };

            return Ok(new { success = true, message = "User updated successfully", data = userDto });
        }

        /// <summary>
        /// Deactivate user (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Roles(RoleConstants.Admin)]
        public async Task<IActionResult> DeactivateUser(string id)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            // Prevent admin from deactivating themselves
            if (currentUserId == id)
            {
                return BadRequest(new { success = false, message = "You cannot deactivate your own account" });
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { success = false, message = "User not found" });
            }

            user.IsActive = false;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return Ok(new { success = true, message = "User deactivated successfully" });
            }

            return BadRequest(new
            {
                success = false,
                message = "Failed to deactivate user",
                errors = result.Errors.Select(e => e.Description).ToList()
            });
        }

        /// <summary>
        /// Assign role to user (Admin only)
        /// </summary>
        [HttpPost("{id}/roles")]
        [Roles(RoleConstants.Admin)]
        public async Task<IActionResult> AssignRole(string id, [FromBody] AssignRoleDto assignRoleDto)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null || !user.IsActive)
            {
                return NotFound(new { success = false, message = "User not found" });
            }

            if (!RoleConstants.AllRoles.Contains(assignRoleDto.Role))
            {
                return BadRequest(new { success = false, message = "Invalid role" });
            }

            var result = await _userManager.AddToRoleAsync(user, assignRoleDto.Role);
            if (result.Succeeded)
            {
                return Ok(new { success = true, message = $"Role '{assignRoleDto.Role}' assigned successfully" });
            }

            return BadRequest(new
            {
                success = false,
                message = "Failed to assign role",
                errors = result.Errors.Select(e => e.Description).ToList()
            });
        }

        /// <summary>
        /// Remove role from user (Admin only)
        /// </summary>
        [HttpDelete("{id}/roles/{role}")]
        [Roles(RoleConstants.Admin)]
        public async Task<IActionResult> RemoveRole(string id, string role)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null || !user.IsActive)
            {
                return NotFound(new { success = false, message = "User not found" });
            }

            if (!RoleConstants.AllRoles.Contains(role))
            {
                return BadRequest(new { success = false, message = "Invalid role" });
            }

            var result = await _userManager.RemoveFromRoleAsync(user, role);
            if (result.Succeeded)
            {
                return Ok(new { success = true, message = $"Role '{role}' removed successfully" });
            }

            return BadRequest(new
            {
                success = false,
                message = "Failed to remove role",
                errors = result.Errors.Select(e => e.Description).ToList()
            });
        }
    }

    public class UpdateUserDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Email { get; set; }
    }

    public class AssignRoleDto
    {
        public string Role { get; set; } = string.Empty;
    }
}
