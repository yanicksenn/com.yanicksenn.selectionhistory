# Selection History

Selection History is a Unity Editor extension that helps you keep track of your recently selected assets and scene objects, and allows you to create customizable selection profiles to quickly access frequently used items.

## Features

### Selection History Window
The Selection History Window automatically records the objects you select within the Unity Editor.

- **Auto-tracking**: Automatically records newly selected objects.
- **Adjustable Size**: Configure the maximum number of items the history should remember.
- **Quick Selection**: Single-click an item in the history to prepare it for drag-and-drop, or double-click to ping and select it in the project/scene.
- **Drag-and-Drop**: Drag objects directly out of your selection history into scene references or inspector fields.
- **Persistent**: Your selection history is saved and restored when reopening the Unity Editor.

To open: `Window > Selection History`

### Selection Profile Window
The Selection Profile Window allows you to create and manage custom lists of objects (stored as `ScriptableObject` assets) that you want to keep handy for quick access.

- **Profile Management**: Create new profiles (`+`), delete them, and switch between them using the dropdown menu.
- **Drag-and-Drop Addition**: Drag objects from the Project, Hierarchy, or Scene directly into the list to add them. Duplicates are automatically prevented.
- **Reordering**: Drag and drop items within the list to reorder them exactly how you need.
- **Quick Removal**: Right-click any item to quickly remove it from the profile.
- **Locking**: Lock the profile using the lock toggle button. When locked, accidental additions, removals, and reordering are prevented, while still allowing you to drag items *out* of the profile for use in your project.

To open: `Window > Selection Profile`

## Installation

This package can be added to your Unity project by referencing this repository in your `manifest.json` or by importing the package directly into your `Packages` folder.
