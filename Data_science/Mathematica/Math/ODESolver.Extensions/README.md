# ODE Solver Extensions: Result Export and Correlation Analysis

Small extension layer that turns sciBASIC# ODE integration output into tabular data frames and CSV files, reloads them, and scores variable-to-variable correlation.

## Overview
- Converts `ODEOutput` (single equation) and `ODEsOut` (system) results into data-frame row objects with an X column plus one column per variable.
- Appends initial values (`y0`) and model parameters as extra columns, with optional numeric rounding via `fix`.
- Persists the data frame to CSV and reads a previously saved CSV back into an `ODEsOut` instance.
- Computes pairwise Pearson (`Pcc`) or Spearman (`SPcc`) correlation matrices between the modelled variables.

## Key Types
- `Microsoft.VisualBasic.Math.Calculus.Extensions` — the (module-hidden) extension module providing every helper below.
- `Microsoft.VisualBasic.Math.Calculus.Dynamics.Data.ODEsOut` — system integration output consumed and produced here.
- `Microsoft.VisualBasic.Math.Calculus.ODEOutput` — single-equation integration output that can be flattened to two columns.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.Math.Calculus

' out is an ODEsOut returned by ODEs.Solve(...)
Dim table = out.DataFrame(xDisp:="Time", fix:=6)

Call table.Save("./ode.csv")

' reload and score variable-to-variable correlation
Dim reloaded = Extensions.LoadFromDataFrame("./ode.csv")

For Each row In reloaded.Pcc()
    Console.WriteLine(row.ID)
Next
```

## Package
- Assembly: `Microsoft.VisualBasic.Math.ODEsSolver.Extensions`
- TargetFramework: `net10.0`
- Tags: `scibasic;ode-solver;data-frame;correlation;csv-export;pearson`

## License
GPL-3.0-or-later
