# RealtimeBeatDetector

A .NET library for real-time beat detection and BPM (beats per minute) calculation from system audio output. Uses energy-based detection with adaptive thresholds to identify beats in music and audio streams.

## Features

- **Real-time beat detection** from system audio output
- **BPM calculation** with rolling average
- **Adaptive threshold** algorithm that adjusts to different audio levels
- **Easy integration** - works with any .NET project (Console, ASP.NET Core, WPF, etc.)
- **Event-driven API** for beat and BPM notifications
- Uses **CSCore** for audio capture via WASAPI loopback

## Installation

Install via NuGet Package Manager:

```bash
dotnet add package RealtimeBeatDetector
```

Or via Package Manager Console:

```powershell
Install-Package RealtimeBeatDetector
```

## Quick Start

### Basic Usage (Console Application)

```csharp
using RealtimeBeatDetector;
using System;

class Program
{
    static void Main()
    {
        using var beatService = new RealtimeBeatDetectionService();
        
        // Subscribe to beat detection events
        beatService.BeatDetected += (s, e) => Console.WriteLine("♪ Beat detected!");
        beatService.BpmDetected += (s, e) => Console.WriteLine($"BPM: {e.AvgBpm:F1} (Current: {e.Bpm:F1})");
        
        // Start detection
        beatService.Start();
        
        Console.WriteLine("Listening for beats... Press any key to stop.");
        Console.ReadKey();
        
        beatService.Stop();
    }
}
```

### ASP.NET Core Integration

Register as a singleton service in `Program.cs` or `Startup.cs`:

```csharp
using RealtimeBeatDetector;

var builder = WebApplication.CreateBuilder(args);

// Register the beat detection service
builder.Services.AddSingleton<RealtimeBeatDetectionService>();

var app = builder.Build();

// Start the service when the application starts
var beatService = app.Services.GetRequiredService<RealtimeBeatDetectionService>();
beatService.BeatDetected += (s, e) => 
{
    // Handle beat - e.g., broadcast via SignalR
    Console.WriteLine("Beat detected!");
};
beatService.BpmDetected += (s, e) => 
{
    // Handle BPM update
    Console.WriteLine($"BPM: {e.AvgBpm:F1}");
};
beatService.Start();

app.Run();
```

### WPF Integration

```csharp
public partial class MainWindow : Window
{
    private RealtimeBeatDetectionService _beatService;

    public MainWindow()
    {
        InitializeComponent();
        
        _beatService = new RealtimeBeatDetectionService();
        _beatService.BeatDetected += OnBeatDetected;
        _beatService.BpmDetected += OnBpmDetected;
        _beatService.Start();
    }

    private void OnBeatDetected(object sender, EventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            // Animate or update UI on beat
            BeatIndicator.Visibility = Visibility.Visible;
        });
    }

    private void OnBpmDetected(object sender, BpmEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            BpmLabel.Content = $"BPM: {e.AvgBpm:F1}";
        });
    }

    protected override void OnClosed(EventArgs e)
    {
        _beatService?.Dispose();
        base.OnClosed(e);
    }
}
```

## API Documentation

### RealtimeBeatDetectionService

Main service class that wraps beat detection with audio capture.

#### Properties

- `bool IsRunning` - Gets whether the service is currently running
- `float BeatThreshold` - Gets or sets the beat detection threshold (default: 10f). Higher values make detection less sensitive

#### Methods

- `void Start()` - Starts capturing audio and detecting beats
- `void Stop()` - Stops capturing audio and detecting beats
- `void Dispose()` - Disposes the service and releases resources

#### Events

- `event EventHandler BeatDetected` - Fired when a beat is detected
- `event EventHandler<BpmEventArgs> BpmDetected` - Fired when BPM is calculated

### BpmEventArgs

Event arguments containing BPM information.

#### Properties

- `double Bpm` - The instantaneous BPM from the last beat interval
- `double AvgBpm` - The rolling average BPM (averaged over last 4 beats)

### BeatDetector

Core beat detection algorithm (typically used via `RealtimeBeatDetectionService`).

#### Properties

- `float BeatThreshold` - The current detection threshold (dynamically adjusted)

#### Methods

- `void ProcessBuffer(byte[] buffer, int length)` - Process an audio buffer for beat detection

#### Events

- `event EventHandler BeatDetected` - Fired when a beat is detected
- `event EventHandler<BpmEventArgs> BpmDetected` - Fired when BPM is calculated

## Configuration Options

### Adjusting Beat Sensitivity

The beat detection threshold can be adjusted to make detection more or less sensitive:

```csharp
var beatService = new RealtimeBeatDetectionService();
beatService.BeatThreshold = 15f; // Less sensitive (higher threshold)
// or
beatService.BeatThreshold = 5f;  // More sensitive (lower threshold)
```

The default value is 10f and the threshold is automatically adjusted over time based on the audio energy levels.

## How It Works

### Audio Source

The library captures audio from the system's default playback output using WASAPI loopback capture. This means it detects beats from whatever audio is playing on your computer (music players, browsers, etc.). No microphone input is used.

### Beat Detection Algorithm

1. **Energy Calculation**: Converts PCM audio samples to energy values
2. **Local Averaging**: Computes moving averages of energy over recent samples
3. **Difference Detection**: Identifies sudden increases in energy (potential beats)
4. **Adaptive Threshold**: Dynamically adjusts detection sensitivity based on recent audio
5. **Beat Spacing**: Enforces minimum 400ms between beats to avoid false positives

### BPM Calculation

When a beat is detected:
1. Calculates the time interval since the last beat
2. Converts the interval to instantaneous BPM
3. Maintains a rolling average over the last 4 beats
4. Fires the `BpmDetected` event with both instantaneous and averaged BPM

## Requirements

- .NET 6.0 or later
- Windows (WASAPI is Windows-only)
- A default audio playback device configured in Windows

## License

MIT License - see [LICENSE](LICENSE) file for details.

## Credits

Original implementation by Wojciech Berdowski (EisernUnion)

## Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues on GitHub.

## Related Projects

This library is designed to integrate easily with projects like:
- **HueLightDJ** - Sync Philips Hue lights to music beats
- Audio visualizers
- Music-reactive applications
- DJ software
- Live performance tools
