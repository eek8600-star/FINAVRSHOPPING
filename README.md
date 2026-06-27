# FINAVRSHOPPING

FINAVRSHOPPING is a Unity VR education project for children with mild autism. It simulates realistic shopping and daily-life scenes so children can practice recognition, interaction, task completion, and adaptation in a controlled virtual environment.

## Requirements

- Unity Editor: `2022.3.57f1c1`
- Git
- Git LFS
- PICO/OpenXR development environment for VR device testing

## First Setup

```bash
git clone https://github.com/eek8600-star/FINAVRSHOPPING.git
cd FINAVRSHOPPING
git lfs install
git lfs pull
```

Open the project from Unity Hub with Unity `2022.3.57f1c1`.

The project vendors required local packages under `Packages/vendor`, so team members should not need machine-specific `F:/...` package paths.

## Team Workflow

Work through Pull Requests instead of pushing directly to `main`.

```bash
git checkout main
git pull
git checkout -b feature/short-task-name
```

Before editing a scene, Prefab, material, or input asset, lock it:

```bash
git lfs lock "Assets/Scenes 1/Market.unity"
```

After the PR is merged and the file is no longer being edited:

```bash
git lfs unlock "Assets/Scenes 1/Market.unity"
```

See `CONTRIBUTING.md` and `docs/team-workflow.md` for the full collaboration rules.

## Important Notes

- Do not commit Unity generated folders such as `Library`, `Temp`, `Obj`, `Build`, `Logs`, or `UserSettings`.
- Do not commit signing keys such as `.keystore`, `.jks`, or `.p12`.
- If `user.keystore` was ever used for real releases, treat it as exposed and replace it before publishing.
