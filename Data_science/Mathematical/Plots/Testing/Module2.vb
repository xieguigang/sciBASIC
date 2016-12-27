Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical.Correlations.Correlations

Module Module2

    <Extension>
    Public Function Correlations(a#(), b#(), Optional compute As ICorrelation = Nothing) As Dictionary(Of String, Dictionary(Of String, String))
        If compute Is Nothing Then
            compute = AddressOf GetPearson
        End If

        Dim Time#() = a.Sequence.ToArray(AddressOf Val)
        Dim ta = compute(a, Time)
        Dim tb = compute(b, Time)


    End Function
End Module
