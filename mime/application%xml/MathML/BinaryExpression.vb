Imports System.ComponentModel
Imports Microsoft.VisualBasic.Language

Namespace MathML

    Public Class BinaryExpression

        Public Property [operator] As String

        Public Property applyleft As [Variant](Of BinaryExpression, String)
        Public Property applyright As [Variant](Of BinaryExpression, String)

        Public Overrides Function ToString() As String
            Return ContentBuilder.ToString(Me)
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

    Public Enum mathOperators
        <Description("+")> plus
        <Description("*")> times
        <Description("/")> divide
        <Description("^")> power
        <Description("-")> minus
    End Enum
End Namespace