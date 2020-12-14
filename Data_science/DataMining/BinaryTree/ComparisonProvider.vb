Imports System.Runtime.CompilerServices

Public MustInherit Class ComparisonProvider

    Protected ReadOnly equalsDbl As Double
    Protected ReadOnly gt As Double

    Sub New(equals#, gt#)
        Me.equalsDbl = equals
        Me.gt = gt
    End Sub

    Protected MustOverride Function GetSimilarity(x As String, y As String) As Double

    Public Function Compares(x As String, y As String) As Integer
        Dim similarity As Double = GetSimilarity(x, y)

        If similarity >= equalsDbl Then
            Return 0
        ElseIf similarity >= gt Then
            Return 1
        Else
            Return -1
        End If
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function GetComparer() As Comparison(Of String)
        Return Me
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Narrowing Operator CType(c As ComparisonProvider) As Comparison(Of String)
        Return New Comparison(Of String)(AddressOf c.Compares)
    End Operator
End Class
