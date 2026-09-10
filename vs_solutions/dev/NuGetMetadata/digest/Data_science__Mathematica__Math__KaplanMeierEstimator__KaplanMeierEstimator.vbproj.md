# Data_science/Mathematica/Math/KaplanMeierEstimator/KaplanMeierEstimator.vbproj

- RootNamespace : Microsoft.VisualBasic.Math.KaplanMeierEstimator
- AssemblyName  : Microsoft.VisualBasic.Math.KaplanMeierEstimator
- TargetFramework: net10.0
- Source files  : 10
- Existing Title: Kaplan-Meier Survival Estimator for Two-Group Studies
- Existing Desc : Computes Kaplan-Meier survival curves for two patient cohorts, merges time events, tests group significance and batch-runs gene-expression split strategies in parallel. Part of sciBASIC#.
- Existing Tags : scibasic;survival-analysis;kaplan-meier;log-rank;gene-expression;biostatistics

## Namespaces
- Models  [files: 3]
- SplitStrategies  [files: 4]

## Public types
- Enum EventFreeSurvival (EventFreeSurvival.vb)
- Class KaplanMeierEstimate (KaplanMeierEstimate.vb) - Performs the Kaplan Meier algorithm over 2 groups, and computes the statistic significance for the groups behaving similarly over time.
- Class KaplanMeierStatus (KaplanMeierEstimate.vb) - Group time event
- Class GeneExpression (Models\GeneExpression.vb) - Holds the gene expression before and after a procedure, referring to a specific patient
- Class GeneResult (Models\GeneResult.vb)
- Class Patient (Models\Patient.vb)
- Interface ISplitStrategy (SplitStrategies\ISplitStrategy.vb)
- Class T0_TopNPercentSplitStrategy (SplitStrategies\T0_TopNPercentSplitStrategy.vb)
- Class T0T2Diff_TopNPercentSplitStrategy (SplitStrategies\T0T2Diff_TopNPercentSplitStrategy.vb)
- Class T2_TopNPercentSplitStrategy (SplitStrategies\T2_TopNPercentSplitStrategy.vb)
- Class StrategyRunner (StrategyRunner.vb)

## Notable public members
- Public Property Time As Integer
- Public Property NumberAtRisk As Integer
- Public Property NumberFailing As Integer
- Public Property SurvivalProbability As Double
- Public Property Time As Integer
- Public Property AtRiskA As Integer
- Public Property AtRiskB As Integer
- Public Property FailingA As Integer
- Public Property FailingB As Integer
- Public Property GroupAEvents As IReadOnlyList(Of KaplanMeierStatus)
- Public Property GroupBEvents As IReadOnlyList(Of KaplanMeierStatus)
- Public Property TotalFailingA As Integer
- Public Property TotalFailingB As Integer
- Public Property PValue As Double
- Public Sub New(groupA As IEnumerable(Of Patient), groupB As IEnumerable(Of Patient))
- Public Sub RunEstimate()
- Public Property GeneId As String
- Public Property PatientId As Integer
- Public Property Before As Double = Double.NaN
- Public Property After As Double = Double.NaN
- Public ReadOnly Property AbsoluteDifference As Double
- Public Property GeneId As String
- Public Property Estimate As KaplanMeierEstimate
- Public Property GroupSize As Integer
- Public Property FDR As Double
- Public Property Id As Integer
- Public Property CensorEvent As EventFreeSurvival
- Public Property CensorEventTime As Integer
- Public Sub New(percent As Integer, patients As IDictionary(Of Integer, Patient))
- Public Sub DoSplit(genes As IEnumerable(Of GeneExpression), <Out> ByRef groupA As IEnumerable(Of Patient), <Out> ByRef groupB As IEnumerable(Of Patien…
- Public ReadOnly Property Name As String Implements ISplitStrategy.Name
- Public Sub New(percent As Integer, patients As IDictionary(Of Integer, Patient))
- Public Sub DoSplit(genes As IEnumerable(Of GeneExpression), <Out> ByRef groupA As IEnumerable(Of Patient), <Out> ByRef groupB As IEnumerable(Of Patien…
- Public ReadOnly Property Name As String Implements ISplitStrategy.Name
- Public Sub New(percent As Integer, patients As IDictionary(Of Integer, Patient))
- Public Sub DoSplit(genes As IEnumerable(Of GeneExpression), <Out> ByRef groupA As IEnumerable(Of Patient), <Out> ByRef groupB As IEnumerable(Of Patien…
- Public ReadOnly Property Name As String Implements ISplitStrategy.Name
- Public Sub New(strategy As ISplitStrategy)
- Public Function Run(genes As List(Of IEnumerable(Of GeneExpression))) As IOrderedEnumerable(Of GeneResult)

## Imports
- Microsoft.VisualBasic.Math.KaplanMeierEstimator.Models
- Microsoft.VisualBasic.Math.KaplanMeierEstimator.SplitStrategies
- Microsoft.VisualBasic.Math.Statistics.Distributions
- std = System.Math
- System.Collections.Concurrent
- System.Runtime.InteropServices
- TasksParallel = System.Threading.Tasks.Parallel

## File tree
- EventFreeSurvival.vb
- KaplanMeierEstimate.vb
- Models\GeneExpression.vb
- Models\GeneResult.vb
- Models\Patient.vb
- SplitStrategies\ISplitStrategy.vb
- SplitStrategies\T0_TopNPercentSplitStrategy.vb
- SplitStrategies\T0T2Diff_TopNPercentSplitStrategy.vb
- SplitStrategies\T2_TopNPercentSplitStrategy.vb
- StrategyRunner.vb

