Imports System.Text

Namespace PdfReader

    ''' <summary>
    ''' PDF的实际内容存储的位置
    ''' </summary>
    Public Class PdfArray : Inherits PdfObject

        Private _wrapped As List(Of PdfObject)

        Public Sub New(ByVal parent As PdfObject, ByVal array As ParseArray)
            MyBase.New(parent, array)
        End Sub

        Public Overrides Sub Visit(ByVal visitor As IPdfObjectVisitor)
            visitor.Visit(Me)
        End Sub

        Public ReadOnly Property ParseArray As ParseArray
            Get
                Return TryCast(ParseObject, ParseArray)
            End Get
        End Property

        Public ReadOnly Property Objects As List(Of PdfObject)
            Get

                If _wrapped Is Nothing Then
                    _wrapped = New List(Of PdfObject)()

                    For Each obj In ParseArray.Objects
                        _wrapped.Add(WrapObject(obj))
                    Next
                End If

                Return _wrapped
            End Get
        End Property

        Public Iterator Function GetWords() As IEnumerable(Of ParseString)
            For Each obj As PdfObject In Objects
                If TypeOf obj.ParseObject Is ParseString Then
                    Yield DirectCast(obj.ParseObject, ParseString)
                End If
            Next
        End Function

        ''' <summary>
        ''' show text content
        ''' </summary>
        ''' <returns></returns>
        Public Function GetAllTextContent() As String
            Dim sb As StringBuilder = New StringBuilder()

            For Each word As ParseString In GetWords()
                sb.Append(word.Value)
            Next

            Return sb.ToString()
        End Function
    End Class
End Namespace
