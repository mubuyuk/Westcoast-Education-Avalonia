using System.Text.Json.Serialization;

namespace WestcoastEducation.Desktop.Models;

// Basklass med fälten som Student och Teacher har gemensamt.
public class Person
{
    // RestDB skapar ett unikt _id för varje dokument. Det skickas inte med när en ny person skapas.
    [JsonPropertyName("_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Id { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string PersonalNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;

    // Räknas fram för visning och skickas därför inte till RestDB.
    [JsonIgnore]
    public string FullName => $"{FirstName} {LastName}";
}
