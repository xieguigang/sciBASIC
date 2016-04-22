Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization

Namespace ComponentModel.Ranges

    Public Class RangeList(Of T As IComparable, V) : Inherits List(Of RangeTagValue(Of T, V))

        Sub New()
            MyBase.New(128)
        End Sub

        Public Function [Select](x As T) As RangeTagValue(Of T, V)
            Dim LQuery = (From r As RangeTagValue(Of T, V)
                          In Me.AsQueryable
                          Where r.IsInside(x)
                          Select r).FirstOrDefault
            Return LQuery
        End Function

        Public Function SelectValue(x As T) As V
            Dim n As RangeTagValue(Of T, V) = [Select](x)
            If n Is Nothing Then
                Throw New DataException($"{x.GetJson} is not in any ranges!")
            Else
                Return n.Value
            End If
        End Function
    End Class
End Namespace