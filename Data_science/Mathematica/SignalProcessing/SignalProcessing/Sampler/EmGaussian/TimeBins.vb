Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace EmGaussian

    Public Module TimeBins

        <Extension>
        Public Function TimeBins(sig As GeneralSignal, Optional max As Double = 100) As Double()
            Dim norm As Integer() = ((New Vector(sig.Strength) / sig.Strength.Max) * max).AsInteger
            Dim pool As New List(Of Double)

            For i As Integer = 0 To norm.Length - 1
                pool.AddRange(Replicate(sig.Measures(i), n:=norm(i)))
            Next

            Return pool.ToArray
        End Function

    End Module
End Namespace