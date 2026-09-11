# Myers O(ND) Diff Algorithm for Text Comparison

Implements the Myers O(ND) difference algorithm to compare text files and string sequences, producing a shortest edit script, structured diff items and unified diff output at line level and character level.

## Overview
- Computes the shortest edit script between two sequences in O((N+M)D) time using the classic Myers greedy furthest-reaching path search with backtracking.
- Line-level comparison of files or string arrays and character-level comparison of plain text.
- Structured result model: per-item edit type with old/new indices, grouped into contiguous `DiffBlock`s.
- Ready-to-print outputs: unified diff (with context lines), side-by-side view and a one-line summary with a similarity score.

## Key Types
- `Microsoft.VisualBasic.Data.MyersDiff.MyersDiff` — the algorithm engine: `Compare`, `CompareLines`, `CompareFiles`, `CompareChars`.
- `Microsoft.VisualBasic.Data.MyersDiff.DiffResult` — full result: items, counts, `Similarity`, `ToUnifiedDiff`, `ToSideBySide`, `ToSummary`.
- `Microsoft.VisualBasic.Data.MyersDiff.DiffItem` — a single edit (keep/insert/delete) with old and new index and text value.
- `Microsoft.VisualBasic.Data.MyersDiff.DiffBlock` — a run of consecutive same-type diff items.
- `Microsoft.VisualBasic.Data.MyersDiff.EditType` — edit operation kind (equal, insert, delete).
- `Microsoft.VisualBasic.Data.MyersDiff.DiffUtils` — static convenience wrappers returning diff text or results directly.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.Data.MyersDiff

Dim differ As New MyersDiff()
Dim result As DiffResult = differ.CompareFiles("old.txt", "new.txt")

Console.WriteLine(result.ToUnifiedDiff("old.txt", "new.txt"))
Console.WriteLine(result.Similarity)
```

## Package
- Assembly: `Microsoft.VisualBasic.Data.MyersDiff`
- TargetFramework: `net10.0`
- Tags: `scibasic;diff;myers-algorithm;text-comparison;edit-script`

## License
GPL-3.0-or-later
