Imports Microsoft.VisualBasic.Language

Namespace MathML

    Public Class BinaryExpression

        Public Property [operator] As String

        Public Property applyleft As [Variant](Of BinaryExpression, SymbolExpression)
        Public Property applyright As [Variant](Of BinaryExpression, SymbolExpression)

        Public Overrides Function ToString() As String
            Return ContentBuilder.ToString(Me)
        End Function

    End Class

    Public Class SymbolExpression

        Public Property text As String
        Public Property isNumericLiteral As Boolean

        Public Overrides Function ToString() As String
            Return text
        End Function

    End Class

    Public Class LambdaExpression

        Public Property parameters As String()
        Public Property lambda As BinaryExpression

        Public Overrides Function ToString() As String
            Return $"function({parameters.JoinBy(", ")}) {{
    return {lambda};
}}"
        End Function

        Public Shared Function FromMathML(xmlText As String) As LambdaExpression
            Return XmlParser.ParseXml(xmlText).ParseXml
        End Function

    End Class
End Namespace