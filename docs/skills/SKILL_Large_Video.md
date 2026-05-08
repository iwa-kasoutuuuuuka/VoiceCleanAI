# Large Video Optimization SKILL

## Goal

Process very large video files safely.

Target:
- 4K
- multi-hour recordings
- low VRAM systems

---

# Rules

Never fully load videos into memory.

Use:
- streaming
- chunk processing
- segmented extraction

---

# Audio Chunking

Split:
- long audio streams

Process incrementally.

---

# Temp Storage

Use fast SSD temp directory.

Monitor:
- free disk space

before processing.

---

# Queue Safety

Throttle:
- simultaneous jobs
- VRAM allocation

---

# Recovery

Support:
- resume after failure
- partial retry

---

# FFmpeg Rules

Prefer:
- stream copy for video

Avoid unnecessary re-encoding.

---

# Memory Rules

Cap:
- RAM usage
- VRAM usage

dynamically.
