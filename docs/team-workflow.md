# Team Workflow

This guide is for the 4-person FINAVRSHOPPING Unity team.

## 1. Install Tools

Install:

- Unity `2022.3.57f1c1`
- Git
- Git LFS
- A code editor such as Visual Studio, Rider, or VS Code
- PICO/OpenXR tooling if you test on VR hardware

Run once:

```bash
git lfs install
```

## 2. Clone and Open

```bash
git clone https://github.com/eek8600-star/FINAVRSHOPPING.git
cd FINAVRSHOPPING
git lfs pull
```

Open the folder in Unity Hub with Unity `2022.3.57f1c1`.

## 3. Start Work

Always start from the latest `main`:

```bash
git checkout main
git pull
git checkout -b feature/short-task-name
```

Good branch names:

- `feature/shopping-list-ui`
- `feature/task-recording`
- `fix/websocket-reconnect`
- `asset/market-shelf-prefabs`

## 4. Lock Unity Assets Before Editing

Lock scenes, Prefabs, materials, input actions, and shared ScriptableObjects before editing:

```bash
git lfs lock "Assets/Scenes 1/Market.unity"
```

See current locks:

```bash
git lfs locks
```

Do not edit a locked file unless you own the lock or have coordinated with the owner.

## 5. Commit

Check what changed:

```bash
git status
```

Stage only files related to your task:

```bash
git add Assets/Scripts/ShoppingCart.cs
git add "Assets/Scenes 1/Market.unity"
```

Commit:

```bash
git commit -m "feat: add shopping task recording"
```

## 6. Push and Open PR

```bash
git push -u origin feature/short-task-name
```

Open a Pull Request on GitHub. Fill in the template completely, especially the asset-locking and test sections.

## 7. Review and Merge

Before merging:

- At least one teammate reviews the PR.
- Conflicts are resolved on the feature branch.
- Unity opens without missing packages.
- Changed gameplay or scene flow has been tested.

After merge:

```bash
git checkout main
git pull
git branch -d feature/short-task-name
git lfs unlock "Assets/Scenes 1/Market.unity"
```

## 8. GitHub Project Board

Recommended board columns:

- `Todo`
- `In Progress`
- `Review`
- `Done`

Recommended labels:

- `script`
- `scene`
- `asset`
- `vr-device`
- `bug`
- `feature`
- `docs`

## 9. Main Branch Protection

Recommended GitHub settings:

- Protect `main`.
- Require Pull Requests before merging.
- Require at least 1 approval.
- Require branches to be up to date before merging.
- Disable direct pushes to `main` for regular contributors.
