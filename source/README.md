# Source Code Provenance

This directory contains a curated selection from the original private team repository.

The goal is not to claim ownership of the complete project codebase, but to make my own contributions and the necessary shared technical context clear.

## source/robin

### audio/MusicManager.cs

The current historical version is included because the later development history of this class is predominantly associated with my work on FMOD, music structure and dynamic music parameters.

### audio/AudioComboPlayer.cs

Introduced in my commit `64036c92` together with a new soundtrack and combo-dependent audio feedback.

### abilities/PlayerAbilityActivator.cs

This is intentionally an **earlier historical snapshot** from commit `4fd050b7`, authored by me.

That commit introduced beat-synchronized ability activation using input callback events. The ability system was later substantially refactored by my teammate, so the later shared version is not presented here as my implementation.

## source/shared-context

### rhythm/BeatSystem.cs

The BeatSystem is included because it provides important context for how FMOD timeline beat/marker callbacks connected the soundtrack to gameplay.

This file belongs to the collaborative codebase and is **not presented as solely authored by me**. Both developers contributed to the rhythm architecture over the course of the project.

## Intentionally excluded

The showcase does not present the following as my work:

- final enemy AI architecture,
- stack-based state-machine implementation,
- most of the final projectile architecture,
- unrelated implementation primarily authored by my teammate.

The selected code is preserved as historical portfolio material rather than rewritten to modern standards.
