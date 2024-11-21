using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using Talabat.APIs.Errors;
using Talabat.Core.Entities;
using Talabat.Core.Repositories.Contract;
using Talabat.Core.Specifications.Employee_Specs;

namespace Talabat.APIs.Controllers
{
   
    public class EmployeesController : BaseApiController
    {
        private readonly IGenericRepository<Employee> _employeeRepo;

        public EmployeesController(IGenericRepository<Employee> employeeRepo)
        {
            _employeeRepo = employeeRepo;
        }
        [HttpGet] // GET : /api/Employees
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
        {
            var spec = new EmployeeWithDepartmentSpecifications();
            var employees = await _employeeRepo.GetAllWithSpecAsync(spec);

            return Ok(employees);
        }

        [ProducesResponseType(typeof(Employee), 200)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [HttpGet("{id}")]
        public async Task<ActionResult<Employee>> GetEmployeeById(int id)
        {
            var spec = new EmployeeWithDepartmentSpecifications(id);
            var employee = await _employeeRepo.GetWithSpecAsync(spec);

            if(employee is null)
                return NotFound(new ApiResponse(404)); // 404

            return Ok(employee);
        }


    }
}
