Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.DataFrame

Public Class Comparison : Inherits ComparisonProvider

    ReadOnly d As DistanceMatrix

    Sub New(d As DistanceMatrix, equals As Double, gt As Double)
        MyBase.New(equals, gt)
        Me.d = d
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Protected Overrides Function GetSimilarity(x As String, y As String) As Double
        Return d(x, y)
    End Function
End Class
