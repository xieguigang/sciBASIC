Imports System
Imports System.Collections.Generic

Namespace PdfReader
    Public Class PdfPages
        Inherits PdfPageInherit

        Private _Children As System.Collections.Generic.List(Of PdfReader.PdfPageInherit)

        Public Sub New(ByVal dictionary As PdfDictionary)
            MyBase.New(dictionary.Parent, dictionary.ParseDictionary)
            Children = New List(Of PdfPageInherit)()

            For Each reference As PdfObjectReference In MandatoryValue(Of PdfArray)("Kids").Objects
                Dim childDictionary = Document.IndirectObjects.MandatoryValue(Of PdfDictionary)(reference)
                Dim type = childDictionary.MandatoryValue(Of PdfName)("Type").Value

                If Equals(type, "Page") Then
                    Children.Add(New PdfPage(childDictionary))
                ElseIf Equals(type, "Pages") Then
                    Children.Add(New PdfPages(childDictionary))
                Else
                    Throw New ArgumentException($"Unrecognized dictionary type references from page tree '{type}'.")
                End If
            Next
        End Sub

        Public Overrides Sub Visit(ByVal visitor As IPdfObjectVisitor)
            visitor.Visit(Me)
        End Sub

        Public Overrides Sub FindLeafPages(ByVal pages As List(Of PdfPage))
            For Each child In Children
                child.FindLeafPages(pages)
            Next
        End Sub

        Public Property Children As List(Of PdfPageInherit)
            Get
                Return _Children
            End Get
            Private Set(ByVal value As List(Of PdfPageInherit))
                _Children = value
            End Set
        End Property
    End Class
End Namespace
