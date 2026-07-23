# GMTK2026 — Coding Standard

Guidance for how to write code in this project. Follow it unless the user says otherwise.

## Naming
- No shorthand or abbreviated names. Write `playerRigidbody`, not `rb2d`; `movementInput`, not `moveInput`; `deltaTime`, not `dt`.
- Names should be descriptive enough that the reader never has to expand an acronym in their head.
- Common, universally-understood loop indices (`i`, `x`/`y` for coordinates) are acceptable.

## Comments
- The user dislikes comments. Code should be self-documenting through good names.
- Only add a comment when the code is genuinely complex or does something out of the ordinary (a non-obvious workaround, a subtle ordering dependency, a reference to an external spec).
- Do NOT add XML doc comments (`/// <summary>`) as a default. Public members should explain themselves by name.
- If something inside a function is complex, do not comment it — extract it into a new, well-named function. The function name is the documentation.
- Unity `[Tooltip]` / `[Header]` attributes are inspector UI hints, not code comments — these are fine and encouraged.

## Structure
- Prefer many small, well-named functions over large functions with commented sections.
- Functions and classes should be self-documenting by their names.

## Style conventions (existing codebase)
- Allman braces (opening brace on its own line), as in `Timer.cs` and `PlayerController.cs`.
- Serialize configuration as `[SerializeField] private` fields rather than `public` fields.
- Target the Unity 6 API (e.g. `Rigidbody2D.linearVelocity`, `FindFirstObjectByType`).
