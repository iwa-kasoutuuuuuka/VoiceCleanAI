using System;
using Microsoft.ML.OnnxRuntime;

class Program {
    static void Main() {
        var session = new InferenceSession(@"E:\app\VoiceCleanAI\publish\app\Models\denoiser.onnx");
        Console.WriteLine("Inputs:");
        foreach (var input in session.InputMetadata) {
            Console.WriteLine($"- {input.Key}: {string.Join(",", input.Value.Dimensions)} ({input.Value.ElementType})");
        }
        Console.WriteLine("Outputs:");
        foreach (var output in session.OutputMetadata) {
            Console.WriteLine($"- {output.Key}: {string.Join(",", output.Value.Dimensions)} ({output.Value.ElementType})");
        }
    }
}
