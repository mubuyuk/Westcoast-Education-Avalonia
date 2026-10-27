using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using WestcoastEducation.Desktop.Models;

namespace WestcoastEducation.Desktop.Services;

// Sköter all HTTP-kommunikation med RestDB-collectionen "teachers".
public class TeacherService : ITeacherService
{
    private const string Collection = "teachers";
    private readonly HttpClient _httpClient;

    public TeacherService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Teacher>> GetTeachersAsync()
    {
        List<Teacher>? teachers = await _httpClient.GetFromJsonAsync<List<Teacher>>(Collection);
        return teachers ?? new List<Teacher>();
    }

    public async Task<Teacher> AddTeacherAsync(Teacher teacher)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(Collection, teacher);
        response.EnsureSuccessStatusCode();

        // RestDB svarar med den sparade läraren, inklusive dess nya _id.
        Teacher? savedTeacher = await response.Content.ReadFromJsonAsync<Teacher>();
        return savedTeacher ?? throw new InvalidOperationException("RestDB returnerade ingen lärare.");
    }

    public async Task UpdateTeacherAsync(Teacher teacher)
    {
        HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"{Collection}/{teacher.Id}", teacher);
        response.EnsureSuccessStatusCode();
    }
}
