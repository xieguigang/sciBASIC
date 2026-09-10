# Dynamic Programming Sequence Alignment and Knapsack Solvers

Dynamic programming kernels for sciBASIC#, covering global and local sequence alignment plus a 0-1 knapsack solver.

## Overview
- Needleman-Wunsch global alignment over a generic symbol type, with a scored substitution matrix, linear gap costs and traceback producing one or more `GlobalAlign` results.
- Smith-Waterman local alignment (`GSW(Of T)`) with a full DP score/direction matrix, HSP match extraction, best-HSP query and simple chaining of local matches.
- Performance and scale helpers: a k-banded global alignment heuristic (`KBandSearch`) and a three-phase center-star multiple sequence alignment driver with edit-distance centre selection.
- A classic 0-1 knapsack solver with value/weight items, plus generic sequence and symbol abstractions so the kernels work on characters, residues or any custom alphabet.

## Key Types
- `Microsoft.VisualBasic.DataMining.DynamicProgramming.NeedlemanWunsch.NeedlemanWunsch(Of T)` — global alignment engine; `Compute()` fills the matrix and returns itself for traceback.
- `Microsoft.VisualBasic.DataMining.DynamicProgramming.NeedlemanWunsch.Workspace(Of T)` — shared alignment state: aligned sequence pairs, score and `NumberOfAlignments`.
- `Microsoft.VisualBasic.DataMining.DynamicProgramming.NeedlemanWunsch.ScoreMatrix(Of T)` — match/mismatch scoring with gap and mismatch penalties.
- `Microsoft.VisualBasic.DataMining.DynamicProgramming.NeedlemanWunsch.GlobalAlign(Of T)` — one alignment result with `query`, `subject`, `score` and `Identities`.
- `Microsoft.VisualBasic.DataMining.DynamicProgramming.SmithWaterman.GSW(Of T)` — generic Smith-Waterman kernel: `BuildMatrix`, `AlignmentScore`, `Matches`, `GetBestHSP`.
- `Microsoft.VisualBasic.DataMining.DynamicProgramming.SmithWaterman.SimpleChaining` — chains local HSP matches into larger alignments.
- `Microsoft.VisualBasic.DataMining.DynamicProgramming.KBandSearch` — k-banded Needleman-Wunsch reducing O(l1*l2) to O(l1*min(l2, 2k)).
- `Microsoft.VisualBasic.DataMining.DynamicProgramming.Knapsack.KnapsackSolver` — `Solve(items, capacity)` returning a `KnapsackSolution`.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.DataMining.DynamicProgramming
Imports Microsoft.VisualBasic.DataMining.DynamicProgramming.NeedlemanWunsch

Dim symbol As GenericSymbol(Of Char) = GetGeneralCharSymbol()
Dim score As New ScoreMatrix(Of Char)(symbol)
Dim nw As New NeedlemanWunsch(Of Char)("GATTACA", "GCATGCU", score, symbol)

For Each align As GlobalAlign(Of Char) In nw.Compute().PopulateAlignments()
    Console.WriteLine(align.ToString())
Next

' 0-1 knapsack
Dim best As KnapsackSolution = Knapsack.KnapsackSolver.Solve(items, capacity:=50)
```

## Package
- Assembly: `Microsoft.VisualBasic.DataMining.DynamicProgramming`
- TargetFramework: `net10.0`
- Tags: `scibasic;dynamic-programming;sequence-alignment;smith-waterman;needleman-wunsch`

## License
GPL-3.0-or-later
