Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Language.Vectorization
Imports Microsoft.VisualBasic.Linq

Namespace Scripting.BasicR

    Public Module base

        Public Function c(Of T)(ParamArray v As IEnumerable(Of T)()) As T()
            Return v.IteratesALL.ToArray
        End Function

        Public Function range(x As IEnumerable(Of Double)) As Double()
            Return New DoubleRange(x).MinMax
        End Function

        Public Function diff(x As IEnumerable(Of Double)) As Double()
            Return NumberGroups.diff(x.SafeQuery.ToArray)
        End Function

        Public Function cor(x As Double()(), y As Double()(), Optional method As String = "pearson") As Double()()
            Dim cor_func As Func(Of Double(), Double(), Double)

            Select Case method
                Case "pearson" : cor_func = AddressOf Correlations.GetPearson
                Case Else
                    Throw New NotSupportedException(method)
            End Select

            Dim cor_mat As Double()() = RectangularArray.Matrix(Of Double)(x.Length, y.Length)

            For i As Integer = 0 To x.Length - 1
                For j As Integer = 0 To y.Length - 1
                    cor_mat(i)(j) = cor_func(x(i), y(j))
                Next
            Next

            Return cor_mat
        End Function

    End Module

    Public Module [is]

        Public Function na(x As IEnumerable(Of Double)) As BooleanVector
            Return New BooleanVector(x.Select(Function(xi) xi.IsNaNImaginary))
        End Function
    End Module
End Namespace