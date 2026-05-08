# Memory & Resource Safety SKILL

## Cancellation & Cleanup
- **CancellationToken**: Mandatory use for all async operations.
- **Process Cleanup**: Explicitly kill FFmpeg and Python workers on cancel.
- **Temp Management**:
    - Location: `%LOCALAPPDATA%/VoiceCleanAI/temp`.
    - Auto-cleanup on success, failure, or crash recovery.
    - Monitor disk space to prevent OOM/Disk Full errors.

---

# C# & Python Resource Rules
- **C#**: `using`, `await using`, `IDisposable`. Avoid event handler leaks.
- **Python**: Release tensors, call `gc.collect()`. Unload inactive models.
- **GPU**: Clear CUDA cache after batches.

---

# Queue & Storage Safety
- **Queue**: Bounded capacity, memory-safe collections to prevent growth leaks.
- **File Locks**: Ensure files are released immediately after processing to prevent access errors.
