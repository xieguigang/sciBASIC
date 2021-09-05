Imports Microsoft.VisualBasic.ComponentModel.TagData

Namespace util

    Public Class DMatrix

        Public Property label As Integer()
        Public Property matrix As FVec()

        Public ReadOnly Property size As Integer
            Get
                Return matrix.Length
            End Get
        End Property

        Public Iterator Function enumerateData() As IEnumerable(Of IntegerTagged(Of FVec))
            For i As Integer = 0 To size - 1
                Yield New IntegerTagged(Of FVec) With {
                    .Tag = label(i),
                    .Value = matrix(i)
                }
            Next
        End Function

        Public Shared Function Builder() As (add As Action(Of Integer, FVec), getMatrix As Func(Of DMatrix))
            Dim labels As New List(Of Integer)
            Dim matrix As New List(Of FVec)
            Dim add As Action(Of Integer, FVec) =
                Sub(label, vec)
                    Call labels.Add(label)
                    Call matrix.Add(vec)
                End Sub
            Dim getMatrix As Func(Of DMatrix) =
                Function()
                    Return New DMatrix With {
                        .label = labels.ToArray,
                        .matrix = matrix.ToArray
                    }
                End Function

            Return (add, getMatrix)
        End Function

    End Class
End Namespace