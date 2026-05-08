# VoiceClean AI

![VoiceClean AI Mockup](docs/images/mockup.png)

**VoiceClean AI** は、Windows 11 向けに最適化された、完全にローカルで動作する AI 音声ノイズ除去・強化アプリケーションです。

最新の AI モデル（Resemble Enhance）と WinUI 3 を組み合わせ、プロフェッショナル品質の音声処理を、プライバシーを重視したローカル環境で提供します。

## 🌟 主な特徴

- **高性能 AI 推論**: Resemble Enhance による、強力なノイズ除去と音声の復元。
- **プライバシー重視**: すべての処理はローカルで完結。音声ファイルがクラウドにアップロードされることはありません。
- **モダンな UI**: Windows 11 のデザイン言語（Mica, Acrylic, Fluent Design）に準拠。
- **高速なメディア処理**: FFmpeg と連携し、動画ファイルの音声のみを入れ替えるなどの高度な処理が可能。
- **GPU 加速**: NVIDIA (CUDA) および DirectML をサポートし、ハードウェアの性能を最大限に引き出します。
- **バッチ処理**: 大量のファイルをドラッグ＆ドロップでキューに追加し、一括処理。

## 🛠 テクノロジースタック

- **Frontend**: WinUI 3, .NET 8 (MVVM)
- **Backend**: Python AI Inference (ONNX Runtime Ready)
- **Audio Core**: Resemble Enhance, FFmpeg
- **GPU Ops**: CUDA, DirectML

## 🚀 はじめかた

### 依存関係
- .NET 8 SDK
- Python 3.10+
- FFmpeg (パスが通っていること)

### 開発環境のセットアップ
1. リポジトリをクローンします。
2. `VoiceCleanAI.slnx` を Visual Studio 2022 で開きます。
3. Python 環境を作成し、依存関係をインストールします：
   ```bash
   pip install -r src/VoiceCleanAI.Backend/requirements.txt
   ```
4. プロジェクトをビルドして実行します。

## 📖 設計指針
本プロジェクトの設計思想や詳細な要件については、[docs/skills/](docs/skills/) 内のドキュメントを参照してください。

## 📄 ライセンス
このプロジェクトは MIT ライセンスの下で公開されています。詳細は [LICENSE](LICENSE) を参照してください。

---

Created by Antigravity (Advanced Agentic Coding AI)
