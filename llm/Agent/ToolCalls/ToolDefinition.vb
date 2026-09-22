Namespace Agent.ToolCalls

    ''' <summary>一个已注册的工具：名称 + 说明 + 参数 schema + 真实实现。</summary>
    Public Class ToolDefinition

        ''' <summary>Name the model uses to call the tool.</summary>
        Public Property Name As String
        ''' <summary>Description of the tool shown to the model.</summary>
        Public Property Description As String
        ''' <summary>JSON schema describing the arguments accepted by the tool.</summary>
        Public Property Schema As JsonSchema
        ''' <summary>真实实现：接收解析后的参数表，返回结果字符串。</summary>
        Public Property Handler As Func(Of Dictionary(Of String, String), String)

        ''' <summary>Returns a short description of the tool.</summary>
        ''' <returns>A text of the form <c>name - description</c>.</returns>
        Public Overrides Function ToString() As String
            Return $"{Name} - {Description}"
        End Function

    End Class
End Namespace