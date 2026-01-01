### Scene Management Setup Guide

This document outlines the rules and procedures for setting up and managing scenes using the `Systems.SceneManagement` framework.

---

### 1. Scene Types & Lifecycle Intent

| Scene Type | Persist across App | Persist across Gameplay | Notes |
| :--- | :---: | :---: | :--- |
| **Boot** | ❌ No | ❌ No | One-time splash/init. Explicitly unloaded. |
| **PersistentSystems** | ✅ Yes | ✅ Yes | Global managers/services. |
| **AppShell** | ✅ Yes | ✅ Yes | Main Menu, Shop, stable App Root. |
| **GameplayBase** | ❌ No | ❌ No | Player/Camera logic. |
| **World** | ❌ No | ❌ No | Level geometry/assets. |
| **Overlay** | ❌ No | ❌ No | Popups/Temporary UI. |
| **Cinematic** | ❌ No | ❌ No | Non-interactive sequences. |

**Important Rules**: 
- `Boot` scene is loaded **once** at cold-start and **never** returned to. It is explicitly unloaded after the App Shell is warm.
- `World` scenes must **NEVER** survive a transition back to the `Boot` scene (not that it should happen anyway) or during a major gameplay reset.
- Gameplay transitions should return to the `AppShell`, not `Boot`.

---

### 2. The Golden Rule (Editor Authoring)

**`SceneType.ActiveScene` must NEVER be authored in the editor.**

The "Active Scene" (the one Unity highlights in the hierarchy) is derived **at runtime** based on the following priority:
1. `World` (Highest)
2. `GameplayBase`
3. `Boot` (Lowest)

The custom property drawer for `SceneType` explicitly hides `ActiveScene` from the Inspector. If you attempt to force it (via code or debug mode), the system will automatically reset it to `World` during validation and log an error.

---

### 3. Creating and Configuring Scene Groups

1. **Create a SceneGroup Asset**:
   - Right-click in the Project window: `Create > Scriptable Objects > Scene Management > Scene Group`.
2. **Add Scenes**:
   - Drag and drop your scenes into the `Scenes` list.
   - Assign the correct `SceneType` for each scene.
3. **Configure Loading Flags**:
   - **Load Additive**: Usually `true` for all scenes except potentially the first one in a fresh load.
   - **Persistent**: If `true`, the scene won't be automatically unloaded when switching groups. 
     - `AppShell` and `PersistentSystems` **must** be persistent.
     - `Boot`, `GameplayBase`, `World`, `Overlay`, and `Cinematic` **must not** be persistent.

---

### 4. Validation Rules

The `SceneGroup` ScriptableObject performs automatic validation in the editor (`OnValidate`):

- **Core Groups**: If any Core-specific types (`Boot`, `PersistentSystems`, `AppShell`) are present, the group must have exactly one of each.
- **Gameplay Groups**: If any Gameplay-specific types (`GameplayBase`, `World`) are present, the group must have exactly one of each.
- **Persistence Check**: 
    - `Overlay`, `Cinematic`, `World`, `GameplayBase`, and `Boot` scenes will have their `Persistent` flag automatically unchecked if set to `true`.
    - `AppShell` and `PersistentSystems` will have their `Persistent` flag automatically checked if set to `false`.

Check the Console for **Warnings** and **Errors** if these rules are violated.

---

### 5. Startup Flow (Final)

1. **App Launch**
2. **SC_Boot** (One-time init)
3. **SC_Systems** (Additive, Persistent)
4. **SC_AppShell** (Additive, Persistent, Real Root)
5. **Unload SC_Boot** (Explicitly called after success)

After this point, the App is "warm", `Boot` is gone, and any return from Gameplay goes to `AppShell`.

---

### 6. Using the SceneLoader

The `SceneLoader` component (usually found in the `Boot` or a dedicated Loading scene) handles the orchestration:

- **Scene Groups Array**: Assign your `Core`, `Gameplay`, and `Overlay` groups to the `Scene Groups` list.
- **Editor Controls**: In the `SceneLoader` Inspector, use the "Scene Group Controls" buttons while in **Play Mode** to quickly swap between groups for testing.
- **Loading Bar**: The `SceneLoader` handles the visual progress representation using the `loadingBar` image.

---

### 7. Technical Implementation Details

- **Addressables Support**: The system automatically detects if a `SceneReference` is set to `Regular` or `Addressable` and uses the appropriate loading method.
- **Duplication Prevention**: By default, the `SceneGroupManager` will not reload scenes that are already loaded unless `reloadDupScenes` is set to true.
- **Automatic Cleanup**: When loading a new group, the system unloads all current scenes except those marked as `Persistent` and the `Bootstrapper`. If a new group contains a `Boot` scene, all previously persistent scenes are also unloaded to ensure a clean state.
