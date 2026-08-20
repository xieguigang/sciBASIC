Namespace ComponentModel.DataSourceModel.Repository

    Public Class FindResult

        Public Property text As String
        Public Property similarity As Double
        Public Property levenshtein As Double
        Public Property index As Integer

        Sub New()
        End Sub

        Sub New(text As String, similairty As Double, levenshtein As Double)
            _text = text
            _similarity = similairty
            _levenshtein = levenshtein
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{index}] {text} = {similarity}"
        End Function
    End Class
End Namespace