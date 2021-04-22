Imports Microsoft.VisualBasic.Language

Namespace PdfReader

    Public Class PdfContents
        Inherits PdfObject

        Private _Streams As List(Of PdfStream)

        Public Sub New(ByVal parent As PdfObject, ByVal obj As PdfObject)
            MyBase.New(parent)
            Streams = New List(Of PdfStream)()
            ResolveToStreams(obj)
        End Sub

        Public Property Streams As List(Of PdfStream)
            Get
                Return _Streams
            End Get
            Private Set(ByVal value As List(Of PdfStream))
                _Streams = value
            End Set
        End Property

        Public Overrides Sub Visit(ByVal visitor As IPdfObjectVisitor)
            visitor.Visit(Me)
        End Sub

        Public Function CreateParser() As PdfContentsParser
            Return New PdfContentsParser(Me)
        End Function

        Private Sub ResolveToStreams(ByVal obj As PdfObject)
            Dim stream As New Value(Of PdfStream)
            Dim reference As New Value(Of PdfObjectReference)
            Dim array As New Value(Of PdfArray)

            If (stream = TryCast(obj, PdfStream)) IsNot Nothing Then
                Streams.Add(stream)
            ElseIf (reference = TryCast(obj, PdfObjectReference)) IsNot Nothing Then
                ResolveToStreams(Document.ResolveReference(reference))
            ElseIf (array = TryCast(obj, PdfArray)) IsNot Nothing Then
                For Each entry As PdfObject In CType(array, PdfArray).Objects
                    ResolveToStreams(entry)
                Next
            End If
        End Sub
    End Class
End Namespace
