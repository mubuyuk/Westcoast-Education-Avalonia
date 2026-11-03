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

    // HttpClient injiceras via DI istället för att skapas här.
    public CourseService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // async/await gör att gränssnittet inte fryser medan vi väntar på svar från RestDB.
    public async Task<List<Course>> GetCoursesAsync()
    {
        // JSON-svaret deserialiseras direkt till en List<Course>.
        List<Course>? courses = await _httpClient.GetFromJsonAsync<List<Course>>(Collection);
        return courses ?? new List<Course>();
    }

    public async Task<Course> AddCourseAsync(Course course)
    {
        // Kursen serialiseras till JSON och skickas i anropet.
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(Collection, course);
        // Säkerställer att HTTP-anropet lyckades, annars kastas ett exception som ViewModeln fångar.
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
