Namespace ApplicationServices.Plugin

    <AttributeUsage(AttributeTargets.Property)>
    Public Class FormattedDoubleFormatString : Inherits Attribute

        Public Property FormatString As String

        Sub New(formatString As String)
            Me.FormatString = formatString
        End Sub

        Public Overrides Function ToString() As String
            Return $"format({FormatString})"
        End Function
    End Class
End Namespace