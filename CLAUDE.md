# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a Unity 3D platformer game project (Unity 2022.3.13f1c1) that demonstrates advanced game architecture patterns including State Machines, Event Systems, and modular player controllers. The project uses Universal Render Pipeline (URP) and includes various asset packages for 3D models, effects, and UI.

## Key Architecture Patterns

### State Machine System
- Location: `Assets/_Project/Scripts/StateMachine/`
- Core implementation: `StateMachine.cs` - Generic state machine with transition management
- Player states include: GroundedState, FallingState, JumpingState, SlidingState, RisingState, FightState
- Enemy states: EnemyBaseState, EnemyAttackState, EnemyChaseState, EnemyWanderState, EnemyDieState

### Event System
Two event systems are implemented:
1. **EventBus System** (`Assets/_Project/Scripts/EventBus/`)
   - Generic event bus with type-safe event handling
   - Uses `EventBinding<T>` for subscriptions
   - Auto-cleanup functionality to prevent memory leaks

2. **EventChannel System** (`Assets/_Project/Scripts/EventSystem/`)
   - ScriptableObject-based event channels
   - Supports various data types (Int, Float)
   - Editor-friendly for visual scripting

### Player Controller Architecture
- Main implementation: `PlayerControllerAdvanced.cs`
- Features: Multi-jump system, momentum-based movement, slope sliding, ceiling detection
- Integrates with: State Machine, Attack System, Input System
- Supports both local and world space momentum

### Spawn System
- Location: `Assets/_Project/Scripts/SpawnSystem/`
- Strategy pattern for spawn point selection (Linear, Random)
- Factory pattern for entity creation
- Enemy dead spawn system for loot generation

### UI Framework
- Location: `Assets/_Project/UIFramework/`
- Panel-based system with BasePanel inheritance
- UI Manager for panel transitions
- Custom cyclic scroll view implementation

## Common Development Commands

### Unity Editor
- Open Unity Hub and select this project
- Use Unity 2022.3.13f1c1 (LTS)
- Build target: Set in File > Build Settings

### Testing
- Unity Test Framework is included (package version 1.1.33)
- Create test scripts in `Assets/Tests/` directory
- Run tests via Window > General > Test Runner

### Scene Management
Main scenes:
- `InitScene.unity` - Initial loading scene
- `SampleScene.unity` - Main gameplay scene
- `UIScene.unity` - UI demonstration

## Project Structure

### Core Game Code (`Assets/_Project/`)
- `Scripts/` - All gameplay scripts organized by system
- `UIFramework/` - UI management system
- `Runtime/` - Utility classes and timers
- `Scenes/` - Project-specific scenes

### Third-Party Assets
- `Assets/JMO Assets/` - Cartoon FX effects
- `Assets/polyperfect/` - Low poly 3D models
- `Assets/Plugins/` - DOTween, Odin Inspector
- `Assets/TextMesh Pro/` - Text rendering system

## Development Guidelines

### Code Organization
- Use namespaces (e.g., `Platformer` for game-specific code)
- Follow existing folder structure when adding new features
- Keep custom code in `_Project` directory to avoid conflicts with assets

### State Machine Usage
```csharp
// Example of adding states and transitions
stateMachine.SetState(initialState);
At(fromState, toState, condition); // Direct transition
Any(toState, condition); // Transition from any state
```

### Event System Usage
```csharp
// EventBus usage
EventBinding<SomeEvent> binding = new(OnSomeEvent);
EventBus<SomeEvent>.Register(binding);

// Raise events
EventBus<SomeEvent>.Raise(new SomeEvent { data = value });
```

### Input System
- Uses Unity's new Input System package
- Input actions defined in `PlayerInputActions.cs`
- Input handling through `InputRead.cs` component

### Editor Tools
- Odin Inspector is used for enhanced editor experience
- Custom editors in `Assets/Editor/`
- Scene reference attributes for better scene linking

## Performance Considerations
- Object pooling is implemented for spawn systems
- Event systems include proper cleanup mechanisms
- State machine uses efficient transition checking
- Timer system optimized for performance-critical code

## Package Management
- Managed through Unity Package Manager
- Git dependencies for SceneReference tools
- Custom packages in `Packages/` directory