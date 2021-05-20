Public Class XPathParser

    Public Shared Function Parse(expression As String) As XPath
        Dim path As XPath

        If expression.StringEmpty Then
            Return Nothing
        ElseIf expression.StartsWith("//") Then
            path = New CurrentNodes
            expression = expression.Substring(2)
        ElseIf expression.StartsWith("/") Then
            path = New RootPathSelector
            expression = expression.Substring(1)
        ElseIf expression.StartsWith("..") Then
            path = New ParentNode
            expression = expression.Substring(2)
        ElseIf expression.StartsWith(".") Then
            path = New CurrentNode
            expression = expression.Substring(1)
        ElseIf expression.StartsWith("@") Then
            path = New SelectAttributes
            expression = expression.Substring(1)
        Else
            path = New SelectByNodeName
        End If

        Dim i As Integer = InStrAny(expression, "/", "@")

        If i > 0 Then
            path.expression = expression.Substring(0, i)
            path.selectNext = Parse(expression)
        Else
            path.expression = expression
        End If

        Return path
    End Function
End Class
