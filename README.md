# Unity Modular Weapons

A simple modular weapon customization demo built in Unity, allowing users to dynamically attach and remove different weapon components such as the weapon body, handguard, stock, and more.

## Overview

This is a lightweight demonstration project showcasing a modular attachment system for weapons. The project is designed to demonstrate how weapon parts can be:

- Attached dynamically
- Remove dynamically
- Swapped at runtime
- Structured in a modular and extendable way

The system begins with a weapon body (core component) and allows additional attachments such as:

- Handguard
- Stock
- Magazine
- Optics
- Grips
- Muzzles
- And more

## How It Works

The system is built around a modular component-based structure:

- The weapon body acts as the core object.
- Attachments are separate objects.
- Each attachment fits into a predefined slot.
- Attachments can be added or removed at runtime.

This structure makes it easy to:

- Add new attachment types
- Restrict incompatible parts
- Expand into a full customization system

## Built With

- Unity (recommended version: 6000.3.5f2)
- C#

## Getting Started

1) Clone the repository
```
git clone https://github.com/danielpokladek/unity-modular-weapons.git
```

2) Open in Unity

- Open Unity Hub
- Click Add Project
- Select the cloned folder
- Open the project

## Roadmap

- Add some examples (currently using Synty assets which aren't available in repo).
- Add stat system.