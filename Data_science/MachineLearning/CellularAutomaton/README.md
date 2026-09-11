# Cellular Automaton Grid Simulator with netCDF Snapshot Debugging

A generic, typed cellular automaton engine for sciBASIC# with tick-and-commit generations, pluggable neighbourhood topologies and netCDF state debugging.

## Overview
- Runs synchronized generations over a 2D grid of `CellEntity(Of T)` cells: every cell computes its next state from the current neighbour frame (`Tick`) and the whole grid is then committed (`Commit`), in scan order or random order.
- Configurable neighbourhood topologies (von Neumann, Moore, extended Moore) and boundary handling (bounded or toroidal) produced by a single offset table module.
- netCDF snapshot debugging: a `Debugger(Of T)` caches per-cell state per generation and flushes the whole series to a `.nc` file on dispose; helpers convert snapshots into typed matrices.
- Ships with a Conway's Game of Life WinForms demo (`GameOfLife.WinForms`, excluded from the library build).

## Key Types
- `Microsoft.VisualBasic.MachineLearning.CellularAutomaton.Simulator(Of T)` — the grid engine: `Run(Optional random As Boolean = True)`, `Snapshot()`, `CellData(i, j)`, `size`.
- `Microsoft.VisualBasic.MachineLearning.CellularAutomaton.Individual` — cell contract: `Tick(adjacents)` then `Commit()`.
- `Microsoft.VisualBasic.MachineLearning.CellularAutomaton.CellEntity(Of T)` — a grid position holding one `Individual` plus its pre-computed neighbour list.
- `Microsoft.VisualBasic.MachineLearning.CellularAutomaton.Neighborhoods` / `NeighborhoodType` / `BoundaryMode` — offset tables, topology enum and border policy enum.
- `Microsoft.VisualBasic.MachineLearning.CellularAutomaton.Debugger(Of T)` — accumulates per-cell snapshots and writes them on `Dispose`.
- `Microsoft.VisualBasic.MachineLearning.CellularAutomaton.WriteCDF` — `Flush(path, cache, type)` writes the snapshot matrix to netCDF.
- `Microsoft.VisualBasic.MachineLearning.CellularAutomaton.Extensions` — snapshot buckets and typed snapshot matrix conversion.

## Quick Start
```vbnet
Imports System.Drawing
Imports Microsoft.VisualBasic.MachineLearning.CellularAutomaton

' MyCell implements Individual (Tick(adjacents) / Commit())
Dim sim As New Simulator(Of MyCell)(New Size(64, 64),
                                    Function() New MyCell(),
                                    NeighborhoodType.Moore,
                                    BoundaryMode.Toroidal)

Using dbg As New Debugger(Of MyCell)("sim.nc", sim, Function(c) c.Value)
    For i As Integer = 1 To 100
        Call sim.Run(random:=False)
        Call dbg.TakeSnapshots()
    Next
End Using
```

## Package
- Assembly: `Microsoft.VisualBasic.MachineLearning.CellularAutomaton`
- TargetFramework: `net10.0`
- Tags: `scibasic;cellular-automaton;simulation;netcdf;grid-model`

## License
GPL-3.0-or-later
