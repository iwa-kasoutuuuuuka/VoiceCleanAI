# FFmpeg Integration SKILL

## Pipeline & Validation
- **Validation**: Check codec compatibility, corrupted files, and missing audio streams *before* processing.
- **Audio Sync**: Verify A/V sync and duration alignment when rebuilding videos. Prevent drifting/truncation.
- **Preservation**: Preserve embedded subtitles, chapters, and metadata where possible.

---

# Process Protection
- **Monitoring**: Capture stderr, detect timeouts, and validate exit codes.
- **Retry**: Automatic retry on transient FFmpeg failures.
- **Zombie Prevention**: Ensure all child processes are killed on cancellation or app exit.

---

# Encoding & Hardware
- **Hardware Encoders**: Selectable NVENC (NVIDIA), QSV (Intel), AMF (AMD), or libx264 (CPU).
- **Auto-Selection**: Recommend the best encoder based on detected hardware.
- **Intermediate**: 48kHz PCM WAV.

---

# Long Media Rules
- **Segmented Workflow**: Process incrementally for very large files.
- **Disk Safety**: Monitor free disk space before and during processing.
- **Temp Cleanup**: Unique temp folders, auto-cleanup on completion or failure.
