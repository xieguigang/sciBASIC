
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix

Namespace math

    Public MustInherit Class DoubleFunction

        Public MustOverride Function apply(x As Double) As Double

    End Class

    Public MustInherit Class DoubleDoubleFunction

        Public MustOverride Function apply(x As Double, y As Double) As Double

    End Class

    Module Helpers

        <Extension>
        Public Function assign(m As GeneralMatrix, f As DoubleFunction) As NumericMatrix
            Dim rows As New List(Of Double())

            For Each row As Vector In m.RowVectors
                rows.Add(row.Select(AddressOf f.apply).ToArray)
            Next

            Return New NumericMatrix(rows)
        End Function

        <Extension>
        Public Function assign(m1 As GeneralMatrix, m2 As GeneralMatrix, f As DoubleDoubleFunction) As NumericMatrix
            Dim rows As New List(Of Double())
            Dim a = m1.RowVectors.ToArray
            Dim b = m2.RowVectors.ToArray
            Dim x, y As Vector

            For i As Integer = 0 To a.Length - 1
                x = a(i)
                y = b(i)
                rows.Add(x.Select(Function(xi, offset) f.apply(xi, y(offset))).ToArray)
            Next

            Return New NumericMatrix(rows)
        End Function

    End Module
End Namespace