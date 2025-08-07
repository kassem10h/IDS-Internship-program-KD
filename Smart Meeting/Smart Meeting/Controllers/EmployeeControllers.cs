using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Smart_Meeting.DTOs;
using SmartMeeting.Data;
using SmartMeeting.DTOs;
using SmartMeeting.Models;
using System.Security.Claims;

namespace SmartMeeting.Controllers
{
    [Route("api/employee")]
    [ApiController]
    [Authorize]
    public class EmployeeControllers : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public EmployeeControllers(UserManager<ApplicationUser> userManager, ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
        }

        // GET: api/Employee
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployees()
        {
            var employees = await _context.Users.Where(u => u.IsActive).ToListAsync();
            var dtoResult = _mapper.Map<List<EmployeeDto>>(employees);
            return Ok(dtoResult);
        }

        // GET: api/Employee/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeeDto>> GetEmployee(string id)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");

            // Users can only view their own profile unless they're admin
            if (!isAdmin && currentUserId != id)
            {
                return Forbid();
            }

            var employee = await _context.Users.FindAsync(id);
            if (employee == null || !employee.IsActive)
                return NotFound();

            var dtoResult = _mapper.Map<EmployeeDto>(employee);
            return Ok(dtoResult);
        }

        // POST: api/Employee
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<EmployeeDto>> CreateEmployee(CreateEmployeeDto employee)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var allowedRoles = new[] { "Admin", "Employee", "User" };
                if (!allowedRoles.Contains(employee.Role))
                    return BadRequest("Invalid role specified.");

                // Check if an employee with the same email already exists
                var employeeExist = await _userManager.FindByEmailAsync(employee.Email);
                if (employeeExist != null) return Conflict("Employee exists");

                // Map the incoming DTO to the ApplicationUser entity
                var newEmployee = _mapper.Map<ApplicationUser>(employee);

                // Create the new employee in the Identity system
                var createdEmployee = await _userManager.CreateAsync(newEmployee, employee.Password);

                if (!createdEmployee.Succeeded)
                    return StatusCode(500, createdEmployee.Errors);

                // Assign the given role to the newly created user
                var roleResult = await _userManager.AddToRoleAsync(newEmployee, employee.Role);
                if (!roleResult.Succeeded)
                    return StatusCode(500, roleResult.Errors);

                // Map the created ApplicationUser entity to a DTO for response
                var employeeDto = _mapper.Map<EmployeeDto>(newEmployee);
                return CreatedAtAction(nameof(GetEmployee), new { id = newEmployee.Id }, employeeDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // PUT: api/Employee/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(string id, EmployeeDto employee)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");

            // Users can only update their own profile unless they're admin
            if (!isAdmin && currentUserId != id)
            {
                return Forbid();
            }

            var existEmployee = await _context.Users.FindAsync(id);
            if (existEmployee == null || !existEmployee.IsActive)
                return NotFound();

            _mapper.Map(employee, existEmployee);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Employee/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteEmployee(string id)
        {
            var employee = await _context.Users.FindAsync(id);
            if (employee == null)
                return NotFound();

            employee.IsActive = false; // Soft delete
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
