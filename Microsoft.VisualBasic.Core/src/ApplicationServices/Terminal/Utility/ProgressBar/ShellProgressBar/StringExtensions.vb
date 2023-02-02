
Namespace ApplicationServices.Terminal.ProgressBar.ShellProgressBar
    Friend Module StringExtensions
        Public Function Excerpt(phrase As String, Optional length As Integer = 60) As String
            If String.IsNullOrEmpty(phrase) OrElse phrase.Length < length Then Return phrase
            Return phrase.Substring(0, length - 3) & "..."
        End Function
    End Module
End Namespace
