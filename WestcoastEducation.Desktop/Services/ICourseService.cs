using System.Collections.Generic;
using System.Threading.Tasks;
using WestcoastEducation.Desktop.Models;

namespace WestcoastEducation.Desktop.Services;

public interface ICourseService
{
    Task<List<Course>> GetCoursesAsync();
    Task<Course> AddCourseAsync(Course course);
    Task UpdateCourseAsync(Course course);
    Task DeleteCourseAsync(string id);
}
