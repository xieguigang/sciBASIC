Namespace MathML

    ''' <summary>
    ''' 
    ''' </summary>
    Public Class LambdaExpression

        Public Property parameters As String()
        Public Property lambda As MathExpression

        Public Overrides Function ToString() As String
            Return $"function({parameters.JoinBy(", ")}) {{
    return {lambda};
}}"
        End Function

        Public Shared Function FromMathML(xmlText As String) As LambdaExpression
            Return XmlParser.ParseXml(xmlText).ParseXml
        End Function

        Public Shared Function FromMathML(xml As XmlElement) As LambdaExpression
            Return xml.ParseXml
        End Function

    End Class
End Namespace