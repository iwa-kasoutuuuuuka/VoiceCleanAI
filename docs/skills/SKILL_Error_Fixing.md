# Error Recovery & Logging SKILL

## Auto Recovery System
- **Job Recovery**: Detect interrupted/crashed jobs on launch.
- **State Recovery**: Restore queue state and clean abandoned temp files.
- **Rollback**: Rollback failed file operations to a clean state.

---

# Backend Protection
- **Python Workers**: Watchdog timeout, heartbeat monitoring, and automatic restart on crash/freeze.
- **Deadlock Prevention**: Ensure non-blocking communication between C# and Python.

---

# Logging Requirements
- **Structured Logging**: Debug, Info, Warning, Error, Fatal.
- **Diagnostics**: Export ZIP debug package containing logs (C#, Python, FFmpeg), hardware info, and stack traces.
- **User Feedback**: Non-technical messages for common errors, detailed logs for advanced troubleshooting.

---

# Stability Testing
- **Stress Test**: System must survive 10+ hour processing, queue spam, and GPU resets without corruption.
- **Corruption Safety**: No permanent data loss on power interruption or corrupted media input.
