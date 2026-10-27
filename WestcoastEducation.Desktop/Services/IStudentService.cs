using System.Collections.Generic;
using System.Threading.Tasks;
using WestcoastEducation.Desktop.Models;

namespace WestcoastEducation.Desktop.Services;

public interface IStudentService
{
    Task<List<Student>> GetStudentsAsync();
    Task<Student> AddStudentAsync(Student student);
}
