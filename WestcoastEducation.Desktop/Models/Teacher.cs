using System;

namespace WestcoastEducation.Desktop.Models;

public class Teacher : Person
{
    // Lärarens kompetensområde, till exempel "Programmering".
    public string KnowledgeArea { get; set; } = string.Empty;
    public DateTime EmploymentDate { get; set; }
}
