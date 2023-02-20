Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.Math.KaplanMeierEstimator.Models

Namespace SplitStrategies
    Public Interface ISplitStrategy
        ReadOnly Property Name As String

        Sub DoSplit(ByVal genes As IEnumerable(Of GeneExpression), <Out> ByRef groupA As IEnumerable(Of Patient), <Out> ByRef groupB As IEnumerable(Of Patient))
    End Interface
End Namespace
