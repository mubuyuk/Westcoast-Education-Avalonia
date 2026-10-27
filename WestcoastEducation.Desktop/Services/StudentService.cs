using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using WestcoastEducation.Desktop.Models;

namespace WestcoastEducation.Desktop.Services;

// Sköter all HTTP-kommunikation med RestDB-collectionen "students".
public class StudentService : IStudentService
{
    private const string Collection = "students";
    private readonly HttpClient _httpClient;

    public StudentService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Student>> GetStudentsAsync()
    {
        List<Student>? students = await _httpClient.GetFromJsonAsync<List<Student>>(Collection);
        return students ?? new List<Student>();
    }

    public async Task<Student> AddStudentAsync(Student student)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(Collection, student);
        response.EnsureSuccessStatusCode();

        // RestDB svarar med den sparade studenten, inklusive dess nya _id.
        Student? savedStudent = await response.Content.ReadFromJsonAsync<Student>();
        return savedStudent ?? throw new InvalidOperationException("RestDB returnerade ingen student.");
    }
}
