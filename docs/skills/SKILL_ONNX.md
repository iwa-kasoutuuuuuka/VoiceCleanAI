# ONNX Migration SKILL

## Goal

Migrate from Python inference to ONNX Runtime progressively.

---

# Migration Phases

## Phase 1

Python backend only.

## Phase 2

Hybrid:
- Python preprocessing
- ONNX inference

## Phase 3

Full ONNX Runtime pipeline.

---

# Optimization

Use:
- FP16
- graph optimization
- dynamic axes

---

# Providers

Support:
- CUDA
- DirectML
- CPU

---

# Validation

Always compare:
- waveform quality
- latency
- output consistency

against original PyTorch inference.

---

# Packaging

Bundle:
- ONNX Runtime DLLs

Avoid requiring Python in final architecture.
