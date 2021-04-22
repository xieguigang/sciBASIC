
Namespace PdfReader
    Public Class PdfVersion
        Inherits PdfObject

        Private _Major As Integer, _Minor As Integer

        Public Sub New(ByVal parent As PdfObject, ByVal major As Integer, ByVal minor As Integer)
            MyBase.New(parent)
            Me.Major = major
            Me.Minor = minor
        End Sub

        Public Overrides Function ToString() As String
            Return $"{Major}.{Minor}"
        End Function

        Public Overrides Sub Visit(ByVal visitor As IPdfObjectVisitor)
            visitor.Visit(Me)
        End Sub

        Public Property Major As Integer
            Get
                Return _Major
            End Get
            Private Set(ByVal value As Integer)
                _Major = value
            End Set
        End Property

        Public Property Minor As Integer
            Get
                Return _Minor
            End Get
            Private Set(ByVal value As Integer)
                _Minor = value
            End Set
        End Property
    End Class
End Namespace
