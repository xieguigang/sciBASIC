
Namespace PdfReader
    Public Class PdfInfo
        Inherits PdfDictionary

        Public Sub New(ByVal parent As PdfObject, ByVal parse As ParseDictionary)
            MyBase.New(parent, parse)
        End Sub

        Public Overrides Sub Visit(ByVal visitor As IPdfObjectVisitor)
            visitor.Visit(Me)
        End Sub

        Public ReadOnly Property Title As PdfString
            Get
                Return OptionalValue(Of PdfString)("Title")
            End Get
        End Property

        Public ReadOnly Property Author As PdfString
            Get
                Return OptionalValue(Of PdfString)("Author")
            End Get
        End Property

        Public ReadOnly Property Subject As PdfString
            Get
                Return OptionalValue(Of PdfString)("Subject")
            End Get
        End Property

        Public ReadOnly Property Keywords As PdfString
            Get
                Return OptionalValue(Of PdfString)("Keywords")
            End Get
        End Property

        Public ReadOnly Property Creator As PdfString
            Get
                Return OptionalValue(Of PdfString)("Creator")
            End Get
        End Property

        Public ReadOnly Property Producer As PdfString
            Get
                Return OptionalValue(Of PdfString)("Producer")
            End Get
        End Property

        Public ReadOnly Property CreationDate As PdfDateTime
            Get
                Return OptionalDateTime("CreationDate")
            End Get
        End Property

        Public ReadOnly Property ModDate As PdfDateTime
            Get
                Return OptionalDateTime("ModDate")
            End Get
        End Property

        Public ReadOnly Property Trapped As PdfName
            Get
                Return OptionalValue(Of PdfName)("Trapped")
            End Get
        End Property
    End Class
End Namespace
