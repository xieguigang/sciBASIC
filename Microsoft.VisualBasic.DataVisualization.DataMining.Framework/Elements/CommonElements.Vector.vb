Imports Microsoft.VisualBasic.Extensions
Imports System.Text

Namespace CommonElements

    ''' <summary>
    ''' 用于表示一个对象实体的属性值的一个向量
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Vector : Implements Generic.IEnumerable(Of Double)

        <Xml.Serialization.XmlAttribute> Public Property Elements As Double()

        Public Overrides Function ToString() As String
            If Elements.Count > 20 Then
                Return MyBase.ToString
            Else
                Dim sBuilder As StringBuilder = New StringBuilder(1024)
                For i As Integer = 0 To Elements.Count - 1
                    sBuilder.Append(Elements(i) & ", ")
                Next
                sBuilder.Remove(sBuilder.Length - 2, 2)

                Return sBuilder.ToString
            End If
        End Function

        Public Shared Function Randomize(Length As UInteger, Optional Upper As Double = 1, Optional Lower As Double = 0) As Vector
            Dim LQuery = From Handle As Integer In Length.Sequence Select RandomDouble() * Upper + Lower '
            Return New Vector With {.Elements = LQuery.ToArray}
        End Function

        Public Shared Widening Operator CType(e As Double()) As Vector
            Return New Vector With {.Elements = e}
        End Operator

        Public Shared Narrowing Operator CType(VEC As Vector) As Double()
            Return VEC.Elements
        End Operator

        Public Iterator Function GetEnumerator() As IEnumerator(Of Double) Implements IEnumerable(Of Double).GetEnumerator
            For i As Integer = 0 To Elements.Count - 1
                Yield Elements(i)
            Next
        End Function

        Public Iterator Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace