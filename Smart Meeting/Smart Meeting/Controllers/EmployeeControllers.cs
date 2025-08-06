using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Smart_Meeting.Data;
using Smart_Meeting.DTOs;
using Smart_Meeting.JWT;
using Smart_Meeting.Models;
using System;
using System.IdentityModel.Tokens.Jwt;

namespace Smart_Meeting.Controllers
{
    [Route("api/employee")]
    [ApiController]
    public class EmployeeControllers : ControllerBase
    {
        private readonly AppDBContext _context;
        private readonly UserManager<Employee> _userManager;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        public EmployeeControllers(UserManager<Employee> userManager,AppDBContext context ,IMapper mapper, IConfiguration config)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
            _config = config;
        }

        // GET: api/Employee
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployees()
        {
            var employees =await _context.Employees.ToListAsync();
            var dtoResult = _mapper.Map<List<EmployeeDto>>(employees);
            return Ok(dtoResult);

        }

        // GET: api/Employee/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeeDto>> GetEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
                return NotFound();
            var dtoResult = _mapper.Map<EmployeeDto>(employee);
            return Ok(dtoResult);
        }

        // POST: api/Employee
        [Authorize(Roles = "Admin")] //only admins can create new user and give him/her a role
        [HttpPost]
        public async Task<ActionResult<EmployeeDto>> CreateEmployee(CreateEmployeeDto employee)
        {
            try 
            {   
                if(!ModelState.IsValid) return BadRequest(ModelState);

                var allowedRoles = new[] { "Admin", "Employee", "User" };
                if (!allowedRoles.Contains(employee.Role))
                    return BadRequest("Invalid role specified.");


                // Check if an employee with the same email already exists
                var EmployeeExist = await _userManager.FindByEmailAsync(employee.Email);
                if (EmployeeExist != null) return Conflict("Employee exist");

                // Map the incoming DTO to the Employee entity using AutoMapper
                var NewEmployee = _mapper.Map<Employee>(employee);

                // Create the new employee in the Identity system with the provided password
                var createdEmployee = await _userManager.CreateAsync(NewEmployee,employee.Password);

                if (!createdEmployee.Succeeded) return StatusCode(500, createdEmployee.Errors); // Return error if creation failed
                                                                                                
                // Assign the given role to the newly created user
                var roleResult = await _userManager.AddToRoleAsync(NewEmployee, employee.Role);
                if (!roleResult.Succeeded)  return StatusCode(500, roleResult.Errors);

                // Map the created Employee entity to a DTO for response
                var employeeDto = _mapper.Map<EmployeeDto>(NewEmployee);
                return CreatedAtAction(nameof(GetEmployee), new { id = NewEmployee.EmployeeId }, employeeDto);
            }
            catch (Exception ex) 
            {
                return StatusCode(500, ex);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LogInDto emp)
        {
            var employee = await _userManager.FindByEmailAsync(emp.Email);
            if(employee == null || !await _userManager.CheckPasswordAsync(employee, emp.Password))
            {
                return BadRequest("Wrong Credentials");
            }

            var token = GenerateJWT.GenerateJwtToken(employee, _config);

            Response.Cookies.Append("access_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,   // Only over HTTPS
                SameSite = SameSiteMode.Strict, //CSRF protection
                Expires = DateTimeOffset.UtcNow.AddDays(5),
                IsEssential = true
            });

            return Ok(new { message = "Login successful" });

        }









        // PUT: api/Employee/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, EmployeeDto employee)
        {
            var ExistEmployee = await _context.Employees.FindAsync(id);
            if (ExistEmployee == null)
                return NotFound();

            _mapper.Map(employee, ExistEmployee);
            await _context.SaveChangesAsync();
            return NoContent();
        }


        // DELETE: api/Employee/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
                return NotFound();

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
