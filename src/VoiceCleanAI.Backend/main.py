import os
import sys
import argparse
import torch
import json

def check_hardware():
    info = {
        "has_cuda": torch.cuda.is_available(),
        "cuda_device_count": torch.cuda.device_count() if torch.cuda.is_available() else 0,
        "gpu_name": torch.cuda.get_device_name(0) if torch.cuda.is_available() else "Unknown/CPU",
        "vram_total": torch.cuda.get_device_properties(0).total_memory if torch.cuda.is_available() else 0,
        "has_directml": False # TODO: Add torch-directml check if relevant
    }
    print(f"HW_INFO:{json.dumps(info)}")

def main():
    parser = argparse.ArgumentParser(description="VoiceClean AI Backend Worker")
    parser.add_argument("--check-gpu", action="store_true", help="Check GPU hardware capabilities")
    parser.add_argument("--input", help="Input audio file path")
    parser.add_argument("--output", help="Output audio file path")
    parser.add_argument("--lambd", type=float, default=0.5)
    parser.add_argument("--tau", type=float, default=0.5)
    parser.add_argument("--nfe", type=int, default=64)
    parser.add_argument("--solver", default="midpoint")
    
    args = parser.parse_args()

    if args.check_gpu:
        check_hardware()
        return

    # ... rest of the code ...
    if not args.input:
        parser.print_help()
        return
    
    # (Original inference logic here)
    process_inference(args)

def process_inference(args):
    # Existing inference logic moved here
    import torchaudio
    from resemble_enhance.enhancer.inference import denoise, enhance
    
    print(f"INFO: Processing {args.input} -> {args.output}")
    try:
        print("PROGRESS: 0.1")
        dwav, sr = torchaudio.load(args.input)
        dwav = dwav.to('cuda' if torch.cuda.is_available() else 'cpu')
        print("PROGRESS: 0.3")
        hwav, sr = enhance(dwav, sr, device=dwav.device, solver=args.solver, nfe=args.nfe, lambd=args.lambd, tau=args.tau)
        print("PROGRESS: 0.8")
        os.makedirs(os.path.dirname(args.output), exist_ok=True)
        torchaudio.save(args.output, hwav.cpu(), sr)
        print("PROGRESS: 1.0")
    except Exception as e:
        print(f"ERROR: {str(e)}", file=sys.stderr)
        sys.exit(1)

if __name__ == "__main__":
    main()
