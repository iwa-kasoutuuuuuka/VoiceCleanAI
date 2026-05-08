using System;
using System.Linq;
using NWaves.Transforms;
using NWaves.Signals;
using NWaves.Windows;

class Program {
    static void Main() {
        float[] audio = new float[44100]; // 1 second dummy
        for(int i=0; i<audio.Length; i++) audio[i] = (float)Math.Sin(2 * Math.PI * 440 * i / 44100);
        
        var signal = new DiscreteSignal(44100, audio);
        var stft = new Stft(1680, 420, WindowType.Hann);
        var spec = stft.Direct(signal);
        
        Console.WriteLine($"Frames: {spec.Count}, Bins: {spec[0].Length}");
    }
}
