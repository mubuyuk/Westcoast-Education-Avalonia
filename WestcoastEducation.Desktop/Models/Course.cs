using System;
using System.Text.Json.Serialization;

namespace WestcoastEducation.Desktop.Models;

public class Course
{
    // RestDB skapar ett unikt _id för varje dokument. Det skickas inte med när en ny kurs skapas.
    [JsonPropertyName("_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Id { get; set; }

    public int CourseNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Duration { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    // Sparas som text ("Classroom" / "Distance") i RestDB i stället för som en siffra.
    [JsonConverter(typeof(JsonStringEnumConverter<CourseType>))]
    public CourseType TypeOfCourse { get; set; }
}
