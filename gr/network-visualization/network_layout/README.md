# Graph Layout Algorithms For Network Visualization

Layout engine of the sciBASIC# network visualization stack: computes node coordinates (and edge bend points) for a `NetworkGraph` with force-directed, constrained Cola, orthogonal, circular, radial, HOLA and edge-bundling algorithms.

## Overview
- Force-directed family (`Planner`, `DegreeWeightedPlanner`, `EdgeWeightedPlanner`, `GroupPlanner`, `CircularPlanner`) driven by `ForceDirectedArgs`, with the spring-embedder physics engine in `SpringForce`.
- Constraint-based Cola layout with 2D/3D solvers, power-graph layout and grid routing; `IPlanner.Collide` advances a single iteration.
- Orthogonal layouts: planar and orthographic embedding, ST-numbering, plus HOLA stages for initial placement, layer-scan, align and spread relaxation and final orthogonal routing into `EdgeData.bends`.
- Circular and radial placement, and Mingle edge bundling for dense graphs.

## Key Types
- `Microsoft.VisualBasic.Data.visualize.Network.Layouts.forceNetwork` — `doForceLayout` and `doRandomLayout` extensions over `NetworkGraph`.
- `Microsoft.VisualBasic.Data.visualize.Network.Layouts.IPlanner` — single-step layout/physics update contract (`Collide`).
- `Microsoft.VisualBasic.Data.visualize.Network.Layouts.ForceDirected.Planner` — force-directed layout with stiffness, repulsion and damping.
- `Microsoft.VisualBasic.Data.visualize.Network.Layouts.ForceDirected.ForceDirectedParameters` — editable force-directed parameters.
- `Microsoft.VisualBasic.Data.visualize.Network.Layouts.Cola.Layout` — main constrained (Cola) layout interface.
- `Microsoft.VisualBasic.Data.visualize.Network.Layouts.Hola.HolaLayouter` — HOLA human-like orthogonal layout driver.
- `Microsoft.VisualBasic.Data.visualize.Network.Layouts.Circular.CircularLayout` — circular placement with degree sorting.
- `Microsoft.VisualBasic.Data.visualize.Network.Layouts.EdgeBundling.Mingle.Bundler` — edge bundling algorithm.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.Circular

g = g.doForceLayout(Stiffness:=80, Repulsion:=4000, Damping:=0.83, iterations:=1000)
g = CircularLayout.LayoutNodes(g)
```

## Package
- Assembly: `Microsoft.VisualBasic.Data.visualize.Network.Layouts`
- TargetFramework: `net10.0`
- Tags: `scibasic;graph-layout;force-directed;orthogonal-routing;edge-bundling`

## License
GPL-3.0-or-later
