using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using WestcoastEducation.Desktop.Models;

namespace WestcoastEducation.Desktop.Services;

// Sköter all HTTP-kommunikation med RestDB-collectionen "courses".
public class CourseService : ICourseService
{
    private const string Collection = "courses";
    private readonly HttpClient _httpClient;

    public CourseService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Course>> GetCoursesAsync()
    {
        List<Course>? courses = await _httpClient.GetFromJsonAsync<List<Course>>(Collection);
        return courses ?? new List<Course>();
    }

    public async Task<Course> AddCourseAsync(Course course)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(Collection, course);
        response.EnsureSuccessStatusCode();

        // RestDB svarar med den sparade kursen, inklusive dess nya _id.
        Course? savedCourse = await response.Content.ReadFromJsonAsync<Course>();
        return savedCourse ?? throw new InvalidOperationException("RestDB returnerade ingen kurs.");
    }

    public async Task UpdateCourseAsync(Course course)
    {
        HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"{Collection}/{course.Id}", course);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteCourseAsync(string id)
    {
        HttpResponseMessage response = await _httpClient.DeleteAsync($"{Collection}/{id}");
        response.EnsureSuccessStatusCode();
    }
}
