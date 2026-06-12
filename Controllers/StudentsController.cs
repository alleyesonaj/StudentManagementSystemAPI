using Microsoft.AspNetCore.Mvc;
using StudentManagementSystemModels;
using StudentManagementSystemDataService;

namespace AccountManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentDataService dataService = new StudentDataService();

        
        [HttpGet]
        public IActionResult GetAll()
        {
            var students = dataService.GetStudents();

            if (students.Count == 0)
                return NotFound(new { message = "No students found." });

            return Ok(students);
        }

        
        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            var students = dataService.GetStudents();
            var student = students.Find(s => s.StudentID == id);

            if (student == null)
                return NotFound(new { message = $"Student with ID {id} not found." });

            return Ok(student);
        }

        
        [HttpGet("status")]
        public IActionResult GetByStatus([FromQuery] string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return BadRequest(new { message = "Status query parameter is required." });

            var allowedStatuses = new List<string>
            {
                "Enrolled", "Graduated", "Applied", "Dropped",
                "Transferred", "Waitlisted", "Deactivated", "Not yet Enrolled!"
            };

            if (!allowedStatuses.Contains(status, StringComparer.OrdinalIgnoreCase))
                return BadRequest(new { message = $"'{status}' is not a valid status." });

            var students = dataService.GetStudentsByStatus(status);

            if (students.Count == 0)
                return NotFound(new { message = $"No students found with status '{status}'." });

            return Ok(students);
        }

        
        [HttpPost]
        public IActionResult Add([FromBody] Student student)
        {
            if (string.IsNullOrWhiteSpace(student.Name))
                return BadRequest(new { message = "Name cannot be empty." });

            student.Status = "Not yet Enrolled!";
            dataService.AddStudent(student);

            return CreatedAtAction(nameof(GetAll), null, student);
        }


        [HttpPatch("{id:int}/status")]
        public IActionResult UpdateStatus(int id, [FromBody] string newStatus)
        {
            var students = dataService.GetStudents();
            var selected = students.Find(s => s.StudentID == id);

            if (selected == null)
                return NotFound(new { message = $"Student with ID {id} not found." });

            if (selected.Status == "Deactivated")
                return BadRequest(new { message = "ACCESS DENIED: This student is Deactivated and cannot be updated." });

            var allowedStatuses = new List<string>
            {
                "Enrolled", "Graduated", "Applied",
                "Dropped", "Transferred", "Waitlisted", "Deactivated"
            };

            if (!allowedStatuses.Contains(newStatus))
                return BadRequest(new { message = $"'{newStatus}' is not a valid status." });

            if (newStatus == "Enrolled" && selected.Status != "Applied")
                return BadRequest(new { message = "REJECTED: Student must be 'Applied' before being 'Enrolled'." });

            if (newStatus == "Graduated" && selected.Status != "Enrolled")
                return BadRequest(new { message = "REJECTED: Student must be 'Enrolled' before 'Graduating'." });

            if (newStatus == "Dropped" && selected.Status == "Graduated")
                return BadRequest(new { message = "REJECTED: Cannot drop a graduated student." });

            dataService.UpdateStatusById(id, newStatus);
            return Ok(new { message = $"Status updated to '{newStatus}' successfully!" });
        }

        
        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var students = dataService.GetStudents();
            var selected = students.Find(s => s.StudentID == id);

            if (selected == null)
                return NotFound(new { message = $"Student with ID {id} not found." });

            dataService.DeleteStudentById(id);
            return NoContent();
        }
    }
}