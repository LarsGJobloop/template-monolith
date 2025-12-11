# Contracts

This project contains domain transfer objects (DTOs) that define the agreements between services and their consumers.

## Purpose

Contracts represent agreements between you and external parties (other services, clients, etc.). Once published, these contracts create dependencies that require negotiation for changes because you've given away part of the ownership.

By externalizing contracts into a separate project:

- **Clear Ownership**: Contract changes impact consumers and require coordination
- **Versioning Strategy**: Contracts can be versioned independently from implementation
- **Breaking Change Awareness**: Modifications require careful consideration of downstream effects
- **API Stability**: Consumers can rely on contracts while implementations evolve
