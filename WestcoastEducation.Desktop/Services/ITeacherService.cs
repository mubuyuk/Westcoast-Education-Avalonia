using System.Collections.Generic;
using System.Threading.Tasks;
using WestcoastEducation.Desktop.Models;

namespace WestcoastEducation.Desktop.Services;

public interface ITeacherService
{
    Task<List<Teacher>> GetTeachersAsync();
    Task<Teacher> AddTeacherAsync(Teacher teacher);
    Task UpdateTeacherAsync(Teacher teacher);
}
