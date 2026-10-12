# Data_science/Mathematica/Math/mHG/mHG.vbproj

- RootNamespace : Microsoft.VisualBasic.Math.Statistics.Hypothesis.mHG
- AssemblyName  : Microsoft.VisualBasic.Math.Statistics.Hypothesis.mHG
- TargetFramework: net10.0
- Source files  : 6
- Existing Title: Minimum Hypergeometric (mHG) Test for Ranked List Enrichment
- Existing Desc : Minimum hypergeometric (mHG) test for ranked list enrichment analysis in sciBASIC#: finds the optimal prefix cutoff, the mHG statistic and its exact p-value for gene set and feature enrichment studies.
- Existing Tags : scibasic;statistics;hypothesis-testing;minimum-hypergeometric;gene-set-enrichment;ranked-list;bioinformatics

## Namespaces
- (no explicit Namespace statement; every type lives directly under the RootNamespace Microsoft.VisualBasic.Math.Statistics.Hypothesis.mHG)

## Public types
- Class HG_row_ncalc (HG_row_n\HG_row_ncalc.vb) - The helper class for the hypergeometric row calculation. NOTE about the indexing convention: The 0-based ``Vector`` here stores ``HG_row(i) = Prob(X == i)``, while the R reference
- Class htest (htest.vb)
- Class mHGstatisticInfo (mHGstatisticInfo.vb) - Where ``HGT`` Is the hypergeometric tail: ``` HGT(b; N, B, n) = Probability(X >= b)
- Module mHGtest (mHGtest.vb)
- Class NamespaceDoc (NamespaceDoc.vb) - The minimum hypergeometric (mHG) test for ranked list enrichment analysis.

## Notable public members
- Public Delegate Function Calculator(HG_row_m As Vector, m%, ni%, b_n#, N%, B%) As Vector
- Public Shared Function recur(HG_row_m As Vector, m%, ni%, b_n#, N%, B%) As Vector
- Public Shared Function iter(HG_row_m As Vector, m%, ni%, b_n#, N%, B%) As Vector
- Public Property statistic As Dictionary(Of String, Double)
- Public Property parameters As Dictionary(Of String, Double)
- Public Property pvalue As Double
- Public Property n As Integer
- Public Property b As Double
- Public Property mHG As Double
- Public Property n As Double
- Public Property b As Double
- Public Const EPSILON# = 0.0000000001
- Public Function mHGtest(lambdas As Vector, Optional n_max# = Double.NaN) As htest
- Public Function mHGstatisticcalc(lambdas As Vector, Optional n_max# = Double.NaN) As mHGstatisticInfo
- Public Function mHGpvalcalc(p#, N%, B#, Optional n_max# = Double.NaN) As Double
- Public Function R_separation_linecalc(p#, N%, B%, n_max%) As Vector
- Public Function pi_rcalc(N%, B%, R_separation_line As Vector) As NumericMatrix
- Public Function d_ratio(ni#, bi#, N#, B#) As Double
- Public Function v_ratio(ni#, bi#, N#, B#) As Double
- Public Function v_ratio(ni#, bi As Vector, N#, B#) As Vector

## Imports
- Microsoft.VisualBasic.Language
- Microsoft.VisualBasic.Linq
- Microsoft.VisualBasic.Math
- Microsoft.VisualBasic.Math.LinearAlgebra
- Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
- std = System.Math
- System.Runtime.CompilerServices
- VB = Microsoft.VisualBasic.Language.Runtime

## File tree
- HG_row_n\Calculator.vb
- HG_row_n\HG_row_ncalc.vb
- htest.vb
- mHGstatisticInfo.vb
- mHGtest.vb
- NamespaceDoc.vb

