# PRODUCT NAME

VoiceClean AI

Tagline:
AI Voice Denoise & Enhancement for Windows 11

---

# 1. Application Identity

Application Name: VoiceClean AI
Executable: VoiceCleanAI.exe

Company-safe naming:
- No spaces in internal executable paths
- Safe ASCII-only internal identifiers

---

# 2. Audio Safety Rules

Never permanently overwrite original files.
Always:
- create new output files
- preserve originals

Implement automatic filename suffixes (e.g., _clean, _enhanced).

---

# 3. Smart Output Naming

Implement customizable output naming rules:
- {filename}_clean
- {filename}_{mode}
- {filename}_{date}

---

# 4. Cancellation System

Implement robust cancellation support.
- Cancel current/all tasks
- Safe FFmpeg termination
- GPU/Temp resource cleanup
- Use CancellationToken

---

# 5. Auto Recovery System

Implement crash-safe recovery.
- Detect interrupted jobs
- Recover/clean temp state
- Restore queue state

---

# 6. Queue System Requirements

Queue architecture:
- Reorder, priority, remove items
- Retry failed jobs
- Duplicate prevention
- Bounded capacity

---

# 7. File Validation

Validate before processing:
- Codec compatibility
- Corrupted/zero-byte files
- Missing audio streams

---

# 8. Audio Sync Protection

When rebuilding videos:
- Verify A/V sync and duration
- Prevent drifting/offset/truncation

---

# 9. FFmpeg Process Protection

- Timeout detection
- stderr monitoring
- Exit code validation
- Automatic retry
- Prevent zombie processes

---

# 10. Python Backend Protection

- Watchdog timeout
- Automatic restart
- Backend heartbeat
- Graceful shutdown

---

# 11. Model Download Manager

- Resumable downloads
- Integrity hash verification
- Version management
- Store in user-local cache directory

---

# 12. Configuration System

- JSON configuration
- Schema versioning & migration
- Support portable and installed modes

---

# 13. Portable Mode

- Portable ZIP version
- No installer required
- Self-contained runtime
- Local config storage

---

# 14. Hardware Detection

Detect: GPU, VRAM, CUDA, DirectML, AVX/AVX2, RAM, CPU.
Auto-tune: Batch size, chunk size, parallelism.

---

# 15. AI Processing Safety

- Prevent clipping, robotic artifacts
- Loudness protection, peak limiter
- Output normalization safety

---

# 16. Preset System

- Voice Call, Streaming, Podcast, Outdoor, Old Tape, AI Auto
- Adjusts: lambd, tau, nfe, normalization, strength

---

# 17. Advanced Mode

- Manual config of lambd, tau, nfe, chunk size, backend, etc.
- Beginner mode hides advanced settings.

---

# 18. Benchmark Mode

- Measure speed, realtime factor, VRAM usage.

---

# 19. Subtitle Preservation

- Preserve embedded subtitles, chapters, metadata.

---

# 20. Hardware Encoding Options

- Selectable encoders: NVENC, QSV, AMF, libx264.
- Auto-select recommended.

---

# 21. Localization

- Multilingual UI (English, Japanese).
- Use resource dictionaries.

---

# 22. Accessibility

- Keyboard navigation, screen scaling, screen readers, colorblind-safe UI.

---

# 23. Safe Temp Management

- Location: %LOCALAPPDATA%/VoiceCleanAI/temp
- Auto cleanup, disk space monitoring.

---

# 24. Telemetry Policy

- Default: OFF
- Opt-in only, never upload media.

---

# 25. Plugin-ready Architecture

- Modular service interfaces for future Whisper, Voice separation, etc.

---

# 26. Future Mobile Compatibility

- Prepare for ARM64 Windows, future mobile/Linux backends.

---

# 27. Stress Testing Requirements

- Survive 10+ hour processing, queue spam, GPU reset, etc.

---

# 28. Logging Requirements

- Structured logging (Debug to Fatal).
- Export ZIP debug package (logs, hardware info, stack traces).

---

# 29. Deployment Targets

- x64, ARM64.
- Self-contained deployment.

---

# 30. Final Product Goal

- Comparable to Adobe Podcast AI, NVIDIA Broadcast.
- Fully local, privacy-focused, Windows-native.
