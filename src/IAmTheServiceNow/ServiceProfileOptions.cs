namespace IAmTheServiceNow;

public class ServiceProfileOptions
{
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }

    public string WorkingDirectory { get; set; }
    public string PathToExecutable { get; set; }
    public string Arguments { get; set; }

    public Dictionary<string, string> EnvironmentVariables { get; } = new();
}