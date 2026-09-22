#Region "Microsoft.VisualBasic::5b30f7ed831a5430098a500af93276fa, llm\Agent\ToolCalls\ToolRegistry.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 186
    '    Code Lines: 94 (50.54%)
    ' Comment Lines: 53 (28.49%)
    '    - Xml Docs: 60.38%
    ' 
    '   Blank Lines: 39 (20.97%)
    '     File Size: 8.46 KB


    '     Class ToolRegistry
    ' 
    '         Properties: Names, Tools
    ' 
    '         Function: ErrorOf, Find, Invoke, Register, RenderCompactCatalog
    '                   RenderToolCatalog
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ---------------------------------------------------------------------------
' ToolRegistry —— 工具注册表（"框架负责做"的那一半）
'
' readme 里的那句话："模型负责'想'，框架负责'做'，两者以 token 序列为契约。"
' 本类就是"做"的那一半：把工具名映射到真实的 .NET 委托，并在调用之前做三层校验。
'
' 三层校验分别对应 readme 里列举的三类常见失败模式：
'
'   1. 幻觉工具名 —— schema 里不存在的函数名。约束解码保证不了这一点（工具名不在
'      参数对象的约束范围内），必须由框架校验并把错误信息回传给模型自我修正；
'   2. 参数类型/枚举不匹配 —— 纯 prompt 方案下最常见。有了约束解码这类问题基本消失，
'      但工具仍应校验（万一路径上被注入了未经约束的文本）；
'   3. 循环调用 —— 由严格单调的上下文（上一轮的工具结果被回填）自然缓解，
'      再由 <see cref="AgentLoop"/> 的最大轮次上限兜底。
'
' 任何一层校验失败都不抛异常，而是返回一段结构化的错误字符串 —— 因为这段字符串
' 会被当作"工具结果"回填进上下文，让模型有机会自我修正。
' ---------------------------------------------------------------------------

Imports System.Text

Namespace Agent.ToolCalls

    ''' <summary>工具注册表：承载"外部代码执行解析"职责。</summary>
    Public Class ToolRegistry

        Private ReadOnly _tools As New List(Of ToolDefinition)

        ''' <summary>已注册的工具（按注册顺序）。</summary>
        Public ReadOnly Property Tools As IList(Of ToolDefinition)
            Get
                Return _tools
            End Get
        End Property

        ''' <summary>注册一个工具；同名工具会被拒绝，避免静默覆盖。</summary>
        Public Function Register(name As String, description As String, schema As JsonSchema,
                                 handler As Func(Of Dictionary(Of String, String), String)) As ToolDefinition

            If String.IsNullOrEmpty(name) Then Throw New ArgumentException("工具名不能为空")
            If Find(name) IsNot Nothing Then Throw New ArgumentException($"工具 '{name}' 已经注册过")

            Dim tool As New ToolDefinition With {
                .Name = name,
                .Description = description,
                .Schema = If(schema, JsonSchema.Empty()),
                .Handler = handler
            }

            _tools.Add(tool)

            Return tool
        End Function

        ''' <summary>按名查找工具；不存在时返回 <see langword="Nothing"/>。</summary>
        Public Function Find(name As String) As ToolDefinition
            If name Is Nothing Then Return Nothing

            For Each t In _tools
                If String.Equals(t.Name, name, StringComparison.Ordinal) Then Return t
            Next

            Return Nothing
        End Function

        ''' <summary>工具名列表（供"工具名约束"或提示信息使用）。</summary>
        Public ReadOnly Property Names As String()
            Get
                Return _tools.Select(Function(t) t.Name).ToArray()
            End Get
        End Property

        ''' <summary>
        ''' 渲染注入 prompt 的工具清单（readme 里"把 Schema 变成 token"的第一步）。
        ''' </summary>
        Public Function RenderToolCatalog() As String
            Dim text As New StringBuilder()

            Call text.AppendLine("Available tools:")

            For Each t In _tools
                Call text.AppendLine($"- {t.Name}: {t.Description}")
                Call text.Append(t.Schema.RenderPrompt())
            Next

            Call text.AppendLine()
            Call text.AppendLine("To call a tool, output exactly:")
            Call text.AppendLine("  " & ToolCallProtocol.CallsBeginMarker & ToolCallProtocol.CallBeginMarker &
                                 ToolCallProtocol.CallTypeFunction & ToolCallProtocol.SepMarker & "{tool_name}")
            Call text.AppendLine("  ```json")
            Call text.AppendLine("  {""argument"": value, ...}")
            Call text.AppendLine("  ```")
            Call text.AppendLine("  " & ToolCallProtocol.CallEndMarker & ToolCallProtocol.CallsEndMarker)
            Call text.AppendLine("Otherwise answer the user directly in plain text.")

            Return text.ToString()
        End Function

        ''' <summary>
        ''' 渲染<b>极简</b>工具清单：只列出"工具名 + 参数名"，一行搞定。
        ''' </summary>
        ''' <remarks>
        ''' 与 <see cref="RenderToolCatalog"/> 的关系：后者是完整形态（含逐参数的 JSON Schema
        ''' 与自然语言描述），用于展示"Schema 变成 token"这件事本身；本方法则是给模型实际
        ''' 消费的版本。
        '''
        ''' 为什么不用完整版：小模型的上下文窗口很窄，而一份带描述的 Schema 清单要一百多个
        ''' token，会把"用户问题 + 工具调用片段"整个挤出训练窗口。调用格式本身由训练样本
        ''' 教会模型，清单只需要告诉它"有哪些工具、各要什么参数"就够了。
        ''' 训练与推理统一使用这一版，避免分布不一致。
        ''' </remarks>
        Public Function RenderCompactCatalog() As String
            Dim text As New StringBuilder()

            Call text.Append("tools: ")

            For i As Integer = 0 To _tools.Count - 1
                If i > 0 Then Call text.Append(" | ")

                Dim args = String.Join(", ", _tools(i).Schema.Properties.Select(Function(p) p.Name))

                Call text.Append($"{_tools(i).Name}({args})")
            Next

            Call text.AppendLine()

            Return text.ToString()
        End Function

        ''' <summary>
        ''' 执行一次工具调用。
        ''' </summary>
        ''' <param name="toolCall">解析出来的调用</param>
        ''' <returns>工具的真实返回值，或者一段"为什么没能调用"的结构化错误说明</returns>
        Public Function Invoke(toolCall As ToolCall) As String
            If toolCall Is Nothing Then Return ErrorOf("the tool call is empty")

            Dim tool = Find(toolCall.Name)

            If tool Is Nothing Then
                ' 失败模式 1：幻觉工具名。把可用清单回传，让模型自我修正。
                Return ErrorOf($"tool '{toolCall.Name}' does not exist; available tools: {String.Join(", ", Names)}")
            End If

            Dim args = If(toolCall.Arguments, New Dictionary(Of String, String)())

            ' 失败模式 2：参数缺失 / 枚举越界
            For Each p In tool.Schema.Properties
                Dim has As Boolean = args.ContainsKey(p.Name)

                If p.Required AndAlso Not has Then
                    Return ErrorOf($"tool '{tool.Name}' requires argument '{p.Name}'")
                End If

                If Not has Then Continue For

                If p.EnumValues IsNot Nothing AndAlso p.EnumValues.Length > 0 Then
                    If Array.IndexOf(p.EnumValues, args(p.Name)) < 0 Then
                        Return ErrorOf($"argument '{p.Name}' must be one of: {String.Join(" | ", p.EnumValues)}")
                    End If
                End If
            Next

            Try
                Return tool.Handler(args)
            Catch ex As Exception
                Return ErrorOf($"tool '{tool.Name}' failed: {ex.Message}")
            End Try
        End Function

        ''' <summary>
        ''' 把失败原因包装成一段 JSON 错误对象。
        ''' </summary>
        ''' <remarks>
        ''' 用 JSON 而不是自然语言，是因为这段文本会被当作"工具结果"回填进上下文，
        ''' 保持与成功结果同构可以让模型更容易学到"出错时该如何纠正"。
        ''' </remarks>
        Private Shared Function ErrorOf(message As String) As String
            Dim q = Chr(34)

            Return "{" & q & "error" & q & ": " & q & message.Replace(q, "'") & q & "}"
        End Function

    End Class

End Namespace
