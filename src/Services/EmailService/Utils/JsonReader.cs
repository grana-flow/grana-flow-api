using System.Text.Json;

namespace EmailServices.Utils;

public class JsonReader
{
    private readonly string _filePath;

    public JsonReader()
    {
        string currentPath = Directory.GetCurrentDirectory();
        string parentPath = Directory.GetParent(currentPath)!.FullName;
        _filePath = Path.Combine(
            parentPath,
            "Services/EmailService/Utils/Template/templates.email.json"
        );
    }

    public JsonReader(string filePath)
    {
        _filePath = filePath;
    }

    public string GetValue(string section, string key)
    {
        try
        {
            var jsonContent = File.ReadAllText(_filePath);
            using var document = JsonDocument.Parse(jsonContent);
            var root = document.RootElement;

            if (
                root.TryGetProperty(section, out var sectionElement)
                && sectionElement.TryGetProperty(key, out var keyElement)
            )
            {
                return keyElement.GetString()
                    ?? throw new ArgumentException(
                        $"Key '{key}' or section '{section}' not found."
                    );
                ;
            }

            throw new ArgumentException($"Key '{key}' or section '{section}' not found.");
        }
        catch (Exception ex)
        {
            throw new ArgumentException("Error reading json. " + ex.Message);
        }
    }
}
