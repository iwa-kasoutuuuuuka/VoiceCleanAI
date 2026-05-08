# Installer & Deployment SKILL

## Deployment Targets
- **Architecture**: x64, ARM64.
- **Self-contained**: Prefer self-contained deployment to minimize external dependencies (.NET Runtime, etc.).
- **Portable Mode**:
    - Provide a ZIP version that requires no installation.
    - Local config storage, no registry usage.

---

# Model Download Manager
- **Resumable**: Support resuming interrupted downloads.
- **Integrity**: Verify downloads using SHA256/integrity hashes.
- **Versioning**: Version management and rollback support for AI models.
- **Cache**: Store models in a user-local cache directory.

---

# Setup Requirements
- **Bundled Tools**: Include FFmpeg, ONNX Runtime DLLs, and VC++ Runtime if needed.
- **GPU Setup**: Detect CUDA/DirectML support during first launch/install.
- **Offline**: Fully offline capable after initial installation and model download.
