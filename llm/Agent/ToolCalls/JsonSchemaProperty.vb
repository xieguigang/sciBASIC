Imports System.Text

Namespace Agent.ToolCalls

    ''' <summary>工具的一个参数（JSON Schema 的 property）。</summary>
    Public Class JsonSchemaProperty

        ''' <summary>参数名（也就是 JSON 里的键）。</summary>
        Public Property Name As String

        ''' <summary>
        ''' 取值类型：<c>string</c> / <c>integer</c> / <c>number</c> / <c>boolean</c>。
        ''' </summary>
        Public Property Type As String = "string"

        ''' <summary>参数说明（会写进给模型看的说明文本）。</summary>
        Public Property Description As String

        ''' <summary>
        ''' 枚举候选值。
        ''' </summary>
        ''' <remarks>
        ''' 这是最能体现约束解码价值的场景：schema 声明 <c>units ∈ {celsius, fahrenheit}</c>
        ''' 之后，解码器就<b>物理上无法</b>生成 <c>"very hot"</c>。
        ''' </remarks>
        Public Property EnumValues As String()

        ''' <summary>是否必填。</summary>
        Public Property Required As Boolean = True

        ''' <summary>默认值（仅用于说明书文本）。</summary>
        Public Property DefaultValue As String

        ''' <summary>Creates an empty property, used by the deserializer.</summary>
        Public Sub New()
        End Sub

        ''' <summary>
        ''' Creates a schema property.
        ''' </summary>
        ''' <param name="name">Name of the property.</param>
        ''' <param name="type">JSON type of the property, for example <c>string</c> or <c>integer</c>.</param>
        ''' <param name="description">Human readable description shown to the model.</param>
        ''' <param name="enumValues">Optional list of allowed values.</param>
        ''' <param name="required">Whether the property must be present.</param>
        Public Sub New(name As String, type As String, description As String,
                       Optional enumValues As String() = Nothing,
                       Optional required As Boolean = True)

            Me.Name = name
            Me.Type = type
            Me.Description = description
            Me.EnumValues = enumValues
            Me.Required = required
        End Sub

        ''' <summary>枚举候选的 JSON 字面量形式（带引号）。</summary>
        Public Function EnumLiterals() As String()
            If EnumValues Is Nothing Then Return New String() {}

            Return EnumValues.Select(Function(v) """" & v & """").ToArray()
        End Function

        ''' <summary>把类型与取值范围写成一行说明文本。</summary>
        Public Function Describe() As String
            Dim text As New StringBuilder()

            Call text.Append($"    - {Name} ({Type}")

            If Required Then text.Append(", required") Else text.Append(", optional")
            If EnumValues IsNot Nothing AndAlso EnumValues.Length > 0 Then
                text.Append(", one of: " & String.Join(" | ", EnumLiterals()))
            End If
            If Not String.IsNullOrEmpty(DefaultValue) Then text.Append($", default: {DefaultValue}")

            Call text.Append(")")

            If Not String.IsNullOrEmpty(Description) Then
                Call text.Append("：")
                Call text.Append(Description)
            End If

            Return text.ToString()
        End Function

    End Class

End Namespace