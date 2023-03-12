
''' <summary>
''' Annotation action base class
''' </summary>
Public Class AnnotAction
    ''' <summary>
    ''' Gets the PDF PdfAnnotation object subtype
    ''' </summary>
    Private _Subtype As String

    Public Property Subtype As String
        Get
            Return _Subtype
        End Get
        Friend Set(value As String)
            _Subtype = value
        End Set
    End Property

    Friend Sub New(Subtype As String)
        Me.Subtype = Subtype
        Return
    End Sub

    Friend Overridable Function IsEqual(Other As AnnotAction) As Boolean
        Throw New ApplicationException("AnnotAction IsEqual not implemented")
    End Function

    Friend Shared Function IsEqual(One As AnnotAction, Two As AnnotAction) As Boolean
        If One Is Nothing AndAlso Two Is Nothing Then Return True
        If One Is Nothing AndAlso Two IsNot Nothing OrElse One IsNot Nothing AndAlso Two Is Nothing OrElse One.GetType() IsNot Two.GetType() Then Return False
        Return One.IsEqual(Two)
    End Function
End Class
