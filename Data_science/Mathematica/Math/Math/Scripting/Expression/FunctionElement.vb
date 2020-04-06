Namespace Scripting.MathExpression

    ''' <summary>
    ''' 将用户定义的函数持久化的保存在XML文件之中所使用到的格式
    ''' </summary>
    Public Class FunctionElement

        Public Property name As String
        Public Property parameters As String()
        Public Property lambda As String

        Public Overrides Function ToString() As String
            Return $"function({parameters.JoinBy(", ")}) {{
    return {lambda};
}}"
        End Function

    End Class
End Namespace