Imports System.Collections.Generic

Namespace PdfReader
    Public Class PdfOutlineLevel
        Inherits PdfObject

        Private _Items As System.Collections.Generic.List(Of PdfReader.PdfOutlineItem)

        Public Sub New(ByVal parent As PdfObject, ByVal dictionary As PdfDictionary)
            MyBase.New(parent)
            Items = New List(Of PdfOutlineItem)()

            If dictionary IsNot Nothing Then
                Dim item = dictionary.OptionalValueRef(Of PdfDictionary)("First")

                While item IsNot Nothing
                    Items.Add(New PdfOutlineItem(Me, item))
                    item = item.OptionalValueRef(Of PdfDictionary)("Next")
                End While
            End If
        End Sub

        Public Overrides Sub Visit(ByVal visitor As IPdfObjectVisitor)
            visitor.Visit(Me)
        End Sub

        Public ReadOnly Property Count As Integer
            Get
                Return Items.Count
            End Get
        End Property

        Public Property Items As List(Of PdfOutlineItem)
            Get
                Return _Items
            End Get
            Private Set(ByVal value As List(Of PdfOutlineItem))
                _Items = value
            End Set
        End Property
    End Class
End Namespace
