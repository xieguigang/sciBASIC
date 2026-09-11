# 2D Rigid Body Physics Engine With Particle And Fluid Solvers

A compact 2D physics toolkit: a fixed-step rigid-body world, a joint and contact constraint solver, particle and SPH fluid systems, plus force-directed graph and label layout.

## Overview
- `PhysicsWorld` runs a fixed-step loop with sub-stepping: force fields, velocity integration, broad phase, narrow phase, sequential-impulse contact and joint solving, then position integration and joint drift correction.
- Collision pipeline: uniform-grid broad phase, narrow phase that produces contact manifolds, and a sequential impulse solver with restitution, Coulomb friction and Baumgarte penetration correction.
- Constraints: revolute, prismatic, spring and weld joints behind a common `IConstraint` interface (velocity and position passes).
- Particle systems: a pooled lightweight `ParticleSystem`/`Emitter` for effects, plus 2D and 3D SPH fluid engines with smoothing kernels and spatial hashing.
- Boids flocking and a force-directed layout library with quad-tree acceleration, used for node placement and label adjustment.

## Key Types
- `Microsoft.VisualBasic.Imaging.Physics.PhysicsWorld` — the simulation world; exposes `Bodies`, `Joints`, `ForceFields`, `Gravity`, `FixedDt`, `Substeps` and `Step`, plus `Box`/`Circle` body factories.
- `Microsoft.VisualBasic.Imaging.Physics.RigidBody` / `PhysicsMaterial` — a body (position, rotation, velocity, mass, inertia) and its friction/restitution material.
- `Microsoft.VisualBasic.Imaging.Physics.Collision.BroadPhase` / `NarrowPhase` / `ContactSolver` / `Manifold` — collision detection and resolution.
- `Microsoft.VisualBasic.Imaging.Physics.Collision.CircleCollider` / `PolygonCollider` / `AABB` — collider shapes and their axis-aligned bounds.
- `Microsoft.VisualBasic.Imaging.Physics.Joints.RevoluteJoint` / `PrismaticJoint` / `SpringJoint` / `WeldJoint` — constraint implementations.
- `Microsoft.VisualBasic.Imaging.Physics.ForceFields.GravityField` / `MagneticField` / `WindField` — local force fields applied to bodies every step.
- `Microsoft.VisualBasic.Imaging.Physics.FluidEngine` / `FluidEngine3D` / `Particle` / `Particle3D` — SPH fluid solvers in 2D and 3D.
- `Microsoft.VisualBasic.Imaging.Physics.layout.QuadTree` / `LabelAdjust` / `ForceVector` — force-directed layout and label placement helpers.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.Imaging.Physics

Dim world As New PhysicsWorld With {
    .Gravity = New Vector2(0, 980),
    .FixedDt = 1 / 60,
    .Substeps = 4
}

Dim ball As RigidBody = PhysicsWorld.Circle(radius:=24, mass:=1)
Dim ground As RigidBody = PhysicsWorld.Box(800, 40, mass:=0)

ball.Position = New Vector2(400, 100)
ground.Position = New Vector2(400, 560)
ground.SetStatic()

world.Add(ball)
world.Add(ground)

For i = 1 To 300
    Call world.Step(1 / 60)
Next
```

## Package
- Assembly: `Microsoft.VisualBasic.Imaging.Physics`
- TargetFramework: `net10.0`
- Tags: `scibasic;physics-engine;rigid-body;collision-detection;particle-system`

## License
GPL-3.0-or-later
