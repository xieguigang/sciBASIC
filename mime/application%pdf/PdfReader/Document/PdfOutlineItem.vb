
Namespace PdfReader
    Public Class PdfOutlineItem
        Inherits PdfOutlineLevel

        Private _dictionary As PdfDictionary

        Public Sub New(ByVal parent As PdfObject, ByVal dictionary As PdfDictionary)
            MyBase.New(parent, dictionary)
            _dictionary = dictionary
        End Sub

        Public Overrides Sub Visit(ByVal visitor As IPdfObjectVisitor)
            visitor.Visit(Me)
        End Sub

        Public ReadOnly Property Title As PdfString
            Get
                Return _dictionary.MandatoryValueRef(Of PdfString)("Title")
            End Get
        End Property

        Public ReadOnly Property Dest As PdfObject
            Get
                Return _dictionary.OptionalValueRef(Of PdfObject)("Dest")
            End Get
        End Property

        Public ReadOnly Property A As PdfDictionary
            Get
                Return _dictionary.OptionalValueRef(Of PdfDictionary)("A")
            End Get
        End Property

        Public ReadOnly Property SE As PdfDictionary
            Get
                Return _dictionary.OptionalValueRef(Of PdfDictionary)("SE")
            End Get
        End Property

        Public ReadOnly Property C As PdfArray
            Get
                Return _dictionary.OptionalValueRef(Of PdfArray)("C")
            End Get
        End Property

        Public ReadOnly Property F As PdfInteger
            Get
                Return _dictionary.OptionalValueRef(Of PdfInteger)("F")
            End Get
        End Property
    End Class
End Namespace
