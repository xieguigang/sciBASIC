' ---------------------------------------------------------------------------
' JsonSchema —— 工具参数的 JSON Schema（轻量模型）
'
' 这个类型在 function calling 里扮演<b>双重角色</b>：
'
'   1. 对模型：它是被 chat template 序列化进 prompt 的"说明书文本"
'      （<see cref="RenderPrompt"/> 产出的就是这段文本），模型通过训练学会读懂它；
'
'   2. 对推理引擎：它是<b>约束解码</b>的语法依据，<see cref="ConstrainedDecoder"/> 会把它
'      编译成一个有限状态机，从而让"生成结构非法的参数"在物理上不可能发生。
'
' 因此这里的字段设计刻意保持精简：只保留约束解码真正用得上的信息
' （键的顺序、键是否必需、值的类型、枚举候选）。
' ---------------------------------------------------------------------------

Imports System.Text

Namespace Agent.ToolCalls

    ''' <summary>工具参数的 JSON Schema（对象型）。</summary>
    Public Class JsonSchema

        ''' <summary>固定为 <c>object</c>。</summary>
        Public Property Type As String = "object"

        ''' <summary>对象整体的说明。</summary>
        Public Property Description As String

        ''' <summary>
        ''' 参数列表。
        ''' </summary>
        ''' <remarks>
        ''' 刻意用 <see cref="List(Of T)"/> 而不是字典：约束解码要求键的出现顺序是确定的，
        ''' 这样才能把 schema 编译成"状态推进"而不是"分支搜索"。
        ''' </remarks>
        Public Property Properties As New List(Of JsonSchemaProperty)

        ''' <summary>Creates an empty schema, used by the deserializer.</summary>
        Public Sub New()
        End Sub

        ''' <summary>
        ''' Creates a schema with the given properties.
        ''' </summary>
        ''' <param name="description">Human readable description of the schema.</param>
        ''' <param name="properties">The properties of the schema, in their declared order.</param>
        Public Sub New(description As String, ParamArray properties As JsonSchemaProperty())
            Me.Description = description

            If properties IsNot Nothing Then Me.Properties.AddRange(properties)
        End Sub

        ''' <summary>按名查找参数；不存在时返回 <see langword="Nothing"/>。</summary>
        Public Function FindProperty(name As String) As JsonSchemaProperty
            If name Is Nothing Then Return Nothing

            For Each p In Properties
                If String.Equals(p.Name, name, StringComparison.Ordinal) Then Return p
            Next

            Return Nothing
        End Function

        ''' <summary>必需参数个数。</summary>
        Public ReadOnly Property RequiredCount As Integer
            Get
                Dim count As Integer = 0

                For Each p In Properties
                    If p.Required Then count += 1
                Next

                Return count
            End Get
        End Property

        ''' <summary>
        ''' 渲染成注入 prompt 的"说明书文本"。
        ''' </summary>
        ''' <remarks>
        ''' 注意这只是给模型看的自然语言描述；真正保证合法性的仍然是约束解码，
        ''' 因此即使模型不完全理解这段文本，输出的结构也依然合法。
        ''' </remarks>
        Public Function RenderPrompt() As String
            Dim text As New StringBuilder()

            Call text.AppendLine("parameters:")
            Call text.AppendLine("  {")

            For i As Integer = 0 To Properties.Count - 1
                Call text.Append(Properties(i).Describe())
                Call text.AppendLine(If(i < Properties.Count - 1, ",", ""))
            Next

            Call text.AppendLine("  }")

            If Not String.IsNullOrEmpty(Description) Then
                Call text.AppendLine("  note: " & Description)
            End If

            Return text.ToString()
        End Function

        ''' <summary>
        ''' 按 schema 的键顺序和类型，把参数表渲染成<b>紧凑</b> JSON。
        ''' </summary>
        ''' <remarks>
        ''' 它保证"训练时喂给模型的参数 JSON"与"约束解码能够生成出来的参数 JSON"
        ''' 是同一种形态：键顺序一致、无多余空白、字符串带引号、数字不加引号。
        ''' 少了这层保证，SFT 学到的格式与推理时的约束就会互相打架。
        ''' </remarks>
        Public Function BuildJson(values As Dictionary(Of String, String)) As String
            Dim quote As String = Chr(34)
            Dim text As New StringBuilder()

            Call text.Append("{")

            Dim first As Boolean = True

            For Each item In Properties
                Dim raw As String = Nothing

                If values Is Nothing OrElse Not values.TryGetValue(item.Name, raw) Then
                    raw = If(item.DefaultValue, "")
                End If

                If Not first Then Call text.Append(",")
                first = False

                Call text.Append(quote & item.Name & quote & ":")
                Call text.Append(FormatValue(item, raw))
            Next

            Call text.Append("}")

            Return text.ToString()
        End Function

        ''' <summary>按参数类型格式化一个取值。</summary>
        Private Shared Function FormatValue(item As JsonSchemaProperty, raw As String) As String
            Select Case item.Type

                Case "integer", "number"
                    If String.IsNullOrEmpty(raw) Then Return "0"
                    Return raw

                Case "boolean"
                    If String.IsNullOrEmpty(raw) Then Return "false"
                    Return raw.ToLower()

                Case Else
                    ' 字符串 / 枚举：转义反斜杠与引号后加引号
                    Dim backslash As String = Chr(92)
                    Dim quote As String = Chr(34)
                    Dim escaped = raw.Replace(backslash, backslash & backslash).Replace(quote, backslash & quote)

                    Return quote & escaped & quote

            End Select
        End Function

        ''' <summary>空对象的 schema（工具无参数时使用）。</summary>
        Public Shared Function Empty() As JsonSchema
            Return New JsonSchema("no arguments")
        End Function

    End Class

End Namespace
