using System.Collections.Generic;
using System.Threading.Tasks;
using WestcoastEducation.Desktop.Models;

namespace WestcoastEducation.Desktop.Services;

// Interfacet beskriver VAD servicen kan göra; CourseService står för HUR (HTTP mot RestDB).
// ViewModels beror bara på interfacet, så implementationen kan bytas, t.ex. mot en fejk i tester.
public interface ICourseService
{
    Task<List<Course>> GetCoursesAsync();
    Task<Course> AddCourseAsync(Course course);
    Task UpdateCourseAsync(Course course);
    Task DeleteCourseAsync(string id);
}
