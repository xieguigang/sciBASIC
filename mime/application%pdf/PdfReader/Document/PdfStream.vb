
Namespace PdfReader
    Public Class PdfStream
        Inherits PdfObject

        Private _dictionary As PdfDictionary

        Public Sub New(ByVal parent As PdfObject, ByVal stream As ParseStream)
            MyBase.New(parent, stream)
        End Sub

        Public Overrides Sub Visit(ByVal visitor As IPdfObjectVisitor)
            visitor.Visit(Me)
        End Sub

        Public ReadOnly Property ParseStream As ParseStream
            Get
                Return TryCast(ParseObject, ParseStream)
            End Get
        End Property

        Public ReadOnly Property HasFilter As Boolean
            Get
                Return ParseStream.HasFilter
            End Get
        End Property

        Public ReadOnly Property Dictionary As PdfDictionary
            Get
                If _dictionary Is Nothing Then _dictionary = New PdfDictionary(Me, ParseStream.Dictionary)
                Return _dictionary
            End Get
        End Property

        Public ReadOnly Property Value As String
            Get
                Return Decrypt.DecodeStream(Me)
            End Get
        End Property

        Public ReadOnly Property ValueAsBytes As Byte()
            Get
                Return Decrypt.DecodeStreamAsBytes(Me)
            End Get
        End Property
    End Class
End Namespace
