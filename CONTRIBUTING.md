# Contributing

This project uses GitHub Flow with Unity-specific resource locking.

## Branches

`main` is the stable branch. Do not develop directly on `main`.

Use short, descriptive branches:

- `feature/shopping-task-recording`
- `fix/pico-movement`
- `docs/team-workflow`
- `asset/shelf-prefab-update`

## Commits

Use concise conventional-style messages:

- `feat: add shopping task recording`
- `fix: correct cart item count`
- `docs: add team workflow`
- `asset: update supermarket shelf prefab`

Keep commits focused. Avoid mixing unrelated scene, script, and documentation changes in one commit.

## Pull Requests

Every change goes through a Pull Request.

Before opening a PR:

1. Pull the latest `main`.
2. Confirm Unity opens the project.
3. Test the changed scene or workflow.
4. Mention every scene, Prefab, material, or large asset changed.
5. Confirm any locked assets are still intentionally locked, or unlock them if work is finished.

At least one teammate should review the PR before merge.

## Unity Asset Locking

Unity scenes, Prefabs, materials, ScriptableObjects, controller files, and Input Actions are high-conflict files.

Lock before editing:

```bash
git lfs lock "Assets/Scenes 1/Market.unity"
```

Check locks:

```bash
git lfs locks
```

Unlock when work is merged or handed off:

```bash
git lfs unlock "Assets/Scenes 1/Market.unity"
```

If you need a file locked by someone else, ask that person before changing it.

## Scene Editing Rules

- Only one person edits the same `.unity` scene at a time.
- Prefer smaller Prefabs over repeated scene-only objects.
- If two tasks touch the same scene, split the work by time and communicate in the task or team chat.
- Do not re-save unrelated scenes just because Unity opened them.
- Avoid broad scene formatting or mass reserialization unless the team agrees first.

## Dependencies

Local packages are vendored under `Packages/vendor`. Do not add personal absolute paths such as `F:/...`, `D:/...`, or `C:/Users/...` to `Packages/manifest.json`.

If a new local SDK or plugin is needed:

1. Confirm its license allows team sharing.
2. Add it under `Packages/vendor/<package-name>`.
3. Reference it with a repository-relative `file:` path.
4. Document the reason in the PR.

## Secrets

Never commit signing keys, passwords, tokens, or publishing credentials.

Android signing files such as `.keystore` must be shared outside GitHub through a private channel.
