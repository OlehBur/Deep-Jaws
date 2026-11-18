# Deep Jaws (Unity 3D)

A high-performance 3D underwater action game developed and published independently on Google Play, featuring cross-platform support (Android & Google Play Games on PC). 

This repository showcases a clean, decoupled service-based architecture utilizing **VContainer (Dependency Injection)**, custom URP shaders, and strict optimization practices. 

*Note: The core architecture has been successfully refactored to a decoupled, service-based VContainer structure. Some minor legacy utilities and helper scripts are undergoing ongoing incremental cleanup.*

---

## Architecture & Naming Conventions

To maintain a scalable and modular codebase, the project follows strict naming conventions and architectural patterns:

* **`*Info` (Data Containers):** Read-only `ScriptableObject` configurations designed to store static game data and parameters safely without runtime modification.
* **`*Service` (Core Services):** Pure C# classes containing business logic and data processing. They are instantiated and injected across the project via VContainer.
* **`*LifetimeScope` (Dependency Injection Scopes):** VContainer scopes (Root and Scene-specific) responsible for wiring dependencies, bootstrapping, and initializing Entry Points.
* **`*Controller` (Gameplay Controllers):** Lightweight components attached to world objects to handle direct, localized gameplay behavior.
* **`*Manager` (Global/Scene Managers):** Unique, specialized MonoBehaviour systems handling overarching game loops, audio pipelines, effect pooling, and global state management.
* **`*Wnd` (UI Window Controllers):** Dedicated UI panel controllers that manage view bindings, open/close animations, and panel state logic through a robust base class.
* **`*View` (UI Components):** Low-level UI elements and modular interface components.
* **`*Wrapper` (Serializable Component Groups):** Lightweight `[System.Serializable]` classes used to group related Inspector references (UI elements, colliders, transforms) into clean, collapsible structures without bloating parent scripts.