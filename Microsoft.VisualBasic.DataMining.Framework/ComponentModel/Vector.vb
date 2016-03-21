Imports Microsoft.VisualBasic.Extensions
Imports Microsoft.VisualBasic.Linq.Extensions
Imports System.Text

Namespace ComponentModel

    ''' <summary>
    ''' 用于表示一个对象实体的属性值的一个向量
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Vector : Inherits EntityBase(Of Double)
        Implements IEnumerable(Of Double)

        Public Overrides Function ToString() As String
            If Properties.Length > 20 Then
                Return MyBase.ToString
            Else
                Return String.Join("; ", Properties)
            End If
        End Function

        Public Shared Function Randomize(Length As UInteger, Optional Upper As Double = 1, Optional Lower As Double = 0) As Vector
            Dim LQuery = From Handle As Integer In Length.Sequence Select RandomDouble() * Upper + Lower '
            Return New Vector With {
                .Properties = LQuery.ToArray
            }
        End Function

        Public Shared Widening Operator CType(x As Double()) As Vector
            Return New Vector With {
                .Properties = x
            }
        End Operator

        Public Shared Narrowing Operator CType(x As Vector) As Double()
            Return x.Properties
        End Operator

        Public Iterator Function GetEnumerator() As IEnumerator(Of Double) Implements IEnumerable(Of Double).GetEnumerator
            For i As Integer = 0 To Properties.Length - 1
                Yield Properties(i)
            Next
        End Function

        Public Iterator Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace