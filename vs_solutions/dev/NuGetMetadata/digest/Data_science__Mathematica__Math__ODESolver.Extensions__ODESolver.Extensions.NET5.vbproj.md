# Data_science/Mathematica/Math/ODESolver.Extensions/ODESolver.Extensions.NET5.vbproj

- RootNamespace : Microsoft.VisualBasic.Math.Calculus
- AssemblyName  : Microsoft.VisualBasic.Math.ODEsSolver.Extensions
- TargetFramework: net10.0
- Source files  : 2
- Existing Title: ODE Solver Extensions: Result Export and Correlation Analysis
- Existing Desc : Extension helpers that turn ODE integration output into data frames and CSV files, reload them, and compute Pearson or Spearman correlations between the modelled variables. Part of sciBASIC#.
- Existing Tags : scibasic;ode-solver;data-frame;correlation;pearson;spearman;csv-export

## Namespaces
- (no explicit Namespace statement; every type lives directly under the RootNamespace Microsoft.VisualBasic.Math.Calculus)

## Public types
- Class NamespaceDoc (NamespaceDoc.vb) - Extension methods for the ODE solver library of the calculus namespace.

## Notable public members
- Public Function DataFrame(out As ODEOutput) As csv
- Public Function DataFrame(df As ODEsOut, Optional xDisp As String = "X", Optional fix% = -1) As IO.File
- Public Function LoadFromDataFrame(csv$, Optional noVars As Boolean = False) As ODEsOut

## Imports
- csv = Microsoft.VisualBasic.Data.Framework.IO.File
- Microsoft.VisualBasic.ComponentModel.Collection
- Microsoft.VisualBasic.ComponentModel.DataSourceModel
- Microsoft.VisualBasic.Data.Framework
- Microsoft.VisualBasic.Data.Framework.IO
- Microsoft.VisualBasic.Language
- Microsoft.VisualBasic.Linq
- Microsoft.VisualBasic.Math.Calculus.Dynamics.Data
- Microsoft.VisualBasic.Math.LinearAlgebra
- std = System.Math
- System.Runtime.CompilerServices

## File tree
- Extensions.vb
- NamespaceDoc.vb

