import torch
import torchaudio
from resemble_enhance.enhancer.inference import load_enhancer
from resemble_enhance.denoiser.inference import load_denoiser

def export():
    # Load models
    enhancer = load_enhancer(None)
    denoiser = load_denoiser(None)
    
    # Dummy input (batch, samples) - resemble-enhance expects 2D or 1D
    dummy_audio = torch.randn(1, 44100)
    
    # Export Enhancer
    print("Exporting Enhancer to ONNX...")
    torch.onnx.export(
        enhancer, 
        (dummy_audio, torch.tensor([44100])), 
        "enhancer.onnx",
        input_names=['input', 'sr'],
        output_names=['output'],
        dynamic_axes={'input': {1: 'samples'}, 'output': {1: 'samples'}},
        opset_version=17
    )
    
    # Export Denoiser
    print("Exporting Denoiser to ONNX...")
    torch.onnx.export(
        denoiser,
        (dummy_audio, torch.tensor([44100])),
        "denoiser.onnx",
        input_names=['input', 'sr'],
        output_names=['output'],
        dynamic_axes={'input': {1: 'samples'}, 'output': {1: 'samples'}},
        opset_version=17
    )

if __name__ == "__main__":
    export()
