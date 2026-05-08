# GPU Optimization SKILL

## Goal

Maximize AI inference performance while minimizing VRAM spikes and ensuring stability.

---

# Hardware Detection & Auto-Tuning
- **Detection**: Detect GPU model, VRAM, CUDA, DirectML, AVX/AVX2, RAM, CPU cores.
- **Auto-Tune**: Dynamically adjust batch size, chunk size, and parallelism based on hardware.
- **Benchmark**: Implement a mode to measure speed, realtime factor, and VRAM usage.

---

# Backend Priority
1. CUDA
2. DirectML
3. CPU
- Auto-switch dynamically and handle driver resets.

---

# VRAM & AI Safety
- **VRAM Management**: Release tensors immediately, clear CUDA cache (`torch.cuda.empty_cache()`).
- **Processing Safety**: Prevent clipping, robotic artifacts. Implement loudness protection/peak limiter.
- **Concurrency**: Limit jobs based on VRAM (4GB: 1, 6GB: 2, 8GB+: adaptive).

---

# Crash Prevention
- Handle CUDA OOM and initialization failures.
- Automatically retry on CPU fallback.
- Support "Stress Test" resilience (survive 10+ hour processing).
