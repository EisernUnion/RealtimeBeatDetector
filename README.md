### Audio source 
Captures only the system default playback output via WasapiLoopbackCapture (no mic input). To use a different device, switch the OS default playback device or change the capture source.

### Beat detection
Uses float samples in [-1, 1]; energy-based detection with adaptive threshold. Initial BeatThreshold set to 10f, updated dynamically based on recent energy differences.

### BPM
On each detected beat, computes instantaneous BPM and keeps a small rolling average (buffer length 4) before raising BpmDetected.

### UI behavior:
The form background flashes on beat and shows the averaged BPM in the label
