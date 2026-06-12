using System.Collections.Generic;
using StudentManagementSystemModels;

namespace StudentManagementSystemDataService
{
    public interface IStudentDataService
    {
        List<Student> GetStudents();
        List<Student> GetStudentsByStatus(string status);
        Student? SearchStudentInDb(string name);
        void AddStudent(Student student);
        void UpdateStatusById(int id, string newStatus);
        void DeleteStudentById(int id);
    }
}