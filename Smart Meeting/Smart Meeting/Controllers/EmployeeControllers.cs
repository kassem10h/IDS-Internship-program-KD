using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Smart_Meeting.Data;
using Smart_Meeting.DTOs;
using Smart_Meeting.Models;
using System;

namespace Smart_Meeting.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeControllers : ControllerBase
    {
        private readonly AppDBContext _context;
        private readonly IMapper _mapper;
        public EmployeeControllers(AppDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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
        [HttpPost]
        public async Task<ActionResult<EmployeeDto>> CreateEmployee(CreateEmployeeDto employee)
        {
            var EmployeeExist = await _context.Employees.FirstOrDefaultAsync(e => e.Email == employee.Email);
            if (EmployeeExist != null) return Conflict("Employee exist");

            var NewEmployee = _mapper.Map<Employee>(employee);
            _context.Employees.Add(NewEmployee);
            await _context.SaveChangesAsync();
            var dtoResult = _mapper.Map<EmployeeDto>(NewEmployee);
            return CreatedAtAction(nameof(GetEmployee), new { id = NewEmployee.EmployeeId }, NewEmployee);
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
