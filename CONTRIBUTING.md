# Contributing to FINAVRSHOPPING

This project uses a lightweight GitHub collaboration workflow for a 3-5 person Unity team.

## Core Rules

- `main` is the stable branch.
- Do not push feature work directly to `main`.
- Every feature, bug fix, scene change, or documentation update should start from a GitHub Issue.
- Every change should be merged through a Pull Request.
- At least one teammate should review a Pull Request before merge.
- Prefer Squash and merge so the `main` history stays readable.

## Issue Workflow

Use Issues to describe work before creating a branch.

Recommended labels:

- `feature`
- `bug`
- `scene`
- `vr-device`
- `education-design`
- `accessibility`
- `asset`
- `documentation`
- `good-first-issue`

Before starting a task:

1. Confirm the Issue has a clear goal.
2. Assign yourself or comment that you are working on it.
3. Create a branch from the latest `main`.

## Branch Naming

Use short, descriptive branch names:

- `feature/issue-number-short-description`
- `fix/issue-number-short-description`
- `scene/issue-number-scene-name`
- `docs/issue-number-short-description`

Examples:

```bash
feature/12-checkout-guidance
fix/18-cart-button-collider
scene/21-supermarket-shelf-layout
docs/24-setup-guide
```

## Commit Messages

Use concise commit prefixes:

- `feat:` for new features
- `fix:` for bug fixes
- `docs:` for documentation
- `chore:` for repository or tooling maintenance
- `test:` for tests

Examples:

```bash
feat: add checkout guidance prompt
fix: correct cart grab collider
docs: document pico sdk setup
chore: add git lfs attributes
```

## Pull Request Checklist

Before requesting review:

- Link the related Issue.
- Describe what changed and why.
- Explain how you tested it.
- Mention any changed `.unity`, `.prefab`, `.asset`, or `ProjectSettings/` files.
- Mention any new large binary assets and confirm they are covered by Git LFS.
- If the change affects VR behavior, test on the target device when possible.

## Unity Asset Rules

- Keep Visible Meta Files enabled.
- Keep Asset Serialization set to Force Text.
- Do not commit `Library/`, `Temp/`, `Obj/`, `Build/`, `Builds/`, `Logs/`, or `UserSettings/`.
- Do not manually delete `.meta` files.
- Move and rename assets inside Unity when possible.
- Keep Prefabs reusable and focused.
- Avoid multiple people editing the same scene at the same time.
- For large scene changes, claim the Issue first and communicate in the Issue thread.

## Package Rules

- Do not commit `file:C:/...`, `file:D:/...`, or `file:F:/...` package paths.
- Prefer Unity registry packages or stable Git URLs.
- Vendor SDKs that cannot be redistributed must be documented in `README.md`.
- If a vendor SDK can be committed legally, add it in a dedicated PR and document its version.

## Review Standard

Reviewers should check:

- The change matches the Issue.
- The PR is focused and understandable.
- Unity scene or Prefab changes are intentional.
- Large files are managed by Git LFS.
- No personal absolute paths or local machine files were committed.
- The stated test steps are enough for the risk of the change.
