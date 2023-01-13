using System.Text.Json;

namespace TarkovAutoSettings;

public class Settings
{
    public string ProcessName = "EscapeFromTarkov";

    public int VibranceOff { get; set; } = 50;
    public int VibranceOn { get; set; } = 80;
    
    public double BrightnessOn { get; set; } = 0.75;
    public double ContrastOn { get; set; } = 0.65;
    public double GammaOn { get; set; } = 1.5;

    
    public double BrightnessOff { get; set; } = 0.5;
    public double ContrastOff { get; set; } = 0.5;
    public double GammaOff { get; set; } = 1;
    
    public static async Task<Settings> LoadAsync(string path, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(path))
        {
            return new();
        }
        
        using var fileStream = File.OpenRead(path);
        return (await JsonSerializer.DeserializeAsync<Settings>(fileStream, cancellationToken: cancellationToken)) ?? new();
    }

    public override string ToString()
    {
        return $"""
            Settings On:
                Vibrance: {VibranceOn}
                Brightness: {BrightnessOn}
                Contrast: {ContrastOn}
                Gamma: {GammaOn}
            Settings Off:
                Vibrance: {VibranceOff}
                Brightness: {BrightnessOff}
                Contrast: {ContrastOff}
                Gamma: {GammaOff}
            """;
    }
}