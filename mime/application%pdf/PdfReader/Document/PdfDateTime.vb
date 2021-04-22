Imports System
Imports System.Text

Namespace PdfReader
    Public Class PdfDateTime
        Inherits PdfReader.PdfString

        Private _DateTime As System.DateTime

        Public Sub New(ByVal parent As PdfReader.PdfObject, ByVal str As PdfReader.PdfString)
            MyBase.New(parent, TryCast(str.ParseObject, PdfReader.ParseString))
            Me.DateTime = str.ValueAsDateTime
        End Sub

        Public Overrides Function ToString() As String
            Return Me.DateTime.ToString()
        End Function

        Public Overrides Sub Visit(ByVal visitor As PdfReader.IPdfObjectVisitor)
            visitor.Visit(Me)
        End Sub

        Public Property DateTime As System.DateTime
            Get
                Return _DateTime
            End Get
            Private Set(ByVal value As System.DateTime)
                _DateTime = value
            End Set
        End Property
    End Class
End Namespace
