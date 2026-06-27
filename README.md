# FINAVRSHOPPING

FINAVRSHOPPING is a Unity VR educational game for children with mild autism. The project simulates realistic shopping and daily-life scenes so children can practice recognition, interaction, and adaptation skills in a safer virtual environment before transferring them to real-world contexts.

## Project Requirements

- Unity: `2022.3.57f1c1`
- Render pipeline: Universal Render Pipeline `14.0.11`
- XR stack: OpenXR, XR Hands, and PICO device support
- Target platform: Android VR device builds, with PICO devices as the primary validation target
- Version control: Git + Git LFS

## First-Time Setup

1. Install Git LFS.

   ```bash
   git lfs install
   ```

2. Clone the repository.

   ```bash
   git clone https://github.com/eek8600-star/FINAVRSHOPPING.git
   cd FINAVRSHOPPING
   git lfs pull
   ```

3. Open the project with Unity Hub using Unity `2022.3.57f1c1`.

4. Let Unity restore registry packages. If Unity asks to regenerate project files or the package lock file, accept it.

5. Install vendor packages that are not committed to the repository. See [Vendor Package Setup](#vendor-package-setup).

## Vendor Package Setup

The repository must not contain personal absolute paths such as `F:/...` or `C:/...` in `Packages/manifest.json`. Packages that are not available through the Unity registry or a stable Git URL must be installed by each developer from the same approved vendor release.

### NativeWebSocket

NativeWebSocket is referenced through a public Git URL in `Packages/manifest.json`:

```json
"com.endel.nativewebsocket": "https://github.com/endel/NativeWebSocket.git#upm"
```

Unity Package Manager should restore it automatically when the project opens.

### PICO Unity Integration SDK

The previous local dependency was:

```text
file:F:/Edg_Download/PICO Unity Integration SDK_3.1.0_20250109
```

To avoid broken paths for teammates, this local dependency has been removed from `Packages/manifest.json`. Install the same PICO Unity Integration SDK release manually if your task needs device-specific PICO APIs, then record any package or settings changes in a pull request.

Recommended team rule:

- Use the same SDK release across the team.
- Do not commit a personal `file:C:/...` or `file:F:/...` package path.
- If the SDK license allows redistribution, place it under a documented `Packages/vendor/` package path in a dedicated pull request.
- If the SDK license does not allow redistribution, keep the package out of Git and document the exact download source and version in this README.

### PICO Live Preview

The previous local dependency was:

```text
file:F:/Edg_Download/Unity Live Preview Plugin-1.0.5-20250211
```

This package is also removed from `Packages/manifest.json` because it is a local editor/device workflow dependency. Developers who need Live Preview should install the same plugin locally and avoid committing machine-specific package paths.

## Collaboration Workflow

All work should flow through Issues, branches, and Pull Requests.

1. Create or claim a GitHub Issue.
2. Create a branch from `main`.
3. Make a focused change.
4. Open a Pull Request that links the Issue.
5. Get at least one teammate review.
6. Squash merge into `main`.

Branch names:

- `feature/issue-number-short-description`
- `fix/issue-number-short-description`
- `scene/issue-number-scene-name`
- `docs/issue-number-short-description`

Example:

```bash
git switch main
git pull
git switch -c feature/12-shopping-checkout-flow
```

## Unity Collaboration Rules

- Keep `ProjectSettings/EditorSettings.asset` configured with Visible Meta Files and Force Text serialization.
- Never commit `Library/`, `Temp/`, `Obj/`, `Build/`, `Builds/`, `Logs/`, or `UserSettings/`.
- Never delete `.meta` files manually.
- Move, rename, and delete assets from inside Unity when possible so `.meta` references stay correct.
- Avoid having multiple developers edit the same `.unity` scene at the same time.
- Prefer reusable Prefabs for interaction objects.
- Prefer ScriptableObject assets or data files for teaching flow configuration when that keeps scenes smaller and easier to review.
- Mention scene, Prefab, or `ProjectSettings/` changes clearly in the Pull Request.

## Git LFS

Large binary assets are tracked through Git LFS using `.gitattributes`. This includes common image, model, audio, video, archive, Unity package, and native plugin formats.

After cloning or switching branches, run:

```bash
git lfs pull
```

If a large asset looks tiny or broken, it may be an LFS pointer that has not been downloaded.

## Common Problems

### Unity reports missing packages

Run Unity Package Manager restore by reopening the project. If the missing package is PICO-specific, follow the vendor setup section instead of adding a personal local path.

### A scene has missing scripts or Prefabs

Check whether the PR deleted or failed to include a `.meta` file. Restore the asset and its `.meta` together.

### Git reports conflicts in `.unity` or `.prefab` files

These files are text, but conflicts can still be difficult. Prefer resolving with UnityYAMLMerge when available. If the conflict is large, coordinate with the teammate who changed the same scene or Prefab.

### A teammate cannot see a model, texture, or audio file

Ask them to run:

```bash
git lfs install
git lfs pull
```

## Roadmap for Automation

The first team phase prioritizes stable collaboration. After the team is comfortable with PRs and reviews, add:

- Unity Test Runner automation
- GitHub Actions C# compile checks
- Android/PICO build verification
- Release packaging
- milestone-based delivery, such as `v0.1 Scene Interaction`, `v0.2 Shopping Task Flow`, and `v0.3 Teaching Feedback`
