# WinUI 3 SKILL

## Architecture
- **MVVM**: Use `CommunityToolkit.Mvvm`. No business logic in code-behind.
- **Localization**: English and Japanese support via Resource Dictionaries. No hardcoded strings.
- **Accessibility**: Keyboard navigation, screen scaling, screen readers, colorblind-safe UI.

---

# Queue & Task Management
- **Queue System**: Support reordering, priority, removing items, and retrying failed jobs.
- **Cancellation**: Robust "Cancel Current" and "Cancel All" functionality.
- **Recovery**: Detect and restore queue state after crash/interruption.

---

# UI Rules & Experience
- **Fluent Design**: Acrylic/Mica, Dark Mode, High DPI/Multi-monitor support.
- **Modes**:
    - **Beginner**: Simple presets (Voice Call, Podcast, etc.)
    - **Advanced**: Manual config of lambd, tau, nfe, chunk size, backend, etc.
- **Drag & Drop**: Files/Folders support.

---

# Progress & Error UX
- **Reporting**: ETA, FPS, Backend type, VRAM, Queue progress.
- **Validation Feedback**: Clear errors for corrupted files, missing audio, or unsupported formats.
- **Safety**: Never crash silently. Provide retry buttons and detailed logs.
