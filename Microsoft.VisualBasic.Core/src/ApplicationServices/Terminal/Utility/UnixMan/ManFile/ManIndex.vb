
Namespace ApplicationServices.Terminal.Utility

    ''' <summary>
    ''' the document index
    ''' </summary>
    Public Class ManIndex

        Public Property index As String
        Public Property category As Integer
        Public Property [date] As Date = Now
        Public Property keyword As String
        Public Property title As String

        Public Overrides Function ToString() As String
            Return $".TH {Strings.UCase(index)} {category} {[date].ToString("yyyy-MMM")} ""{keyword}"" ""{title}"""
        End Function

    End Class
End Namespace