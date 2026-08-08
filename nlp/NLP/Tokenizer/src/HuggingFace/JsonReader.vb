#Region "Microsoft.VisualBasic::af2973305a0b08c92be52382ad03eb21, nlp\NLP\Tokenizer\src\HuggingFace\JsonReader.vb"

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

    '   Total Lines: 478
    '    Code Lines: 297 (62.13%)
    ' Comment Lines: 96 (20.08%)
    '    - Xml Docs: 87.50%
    ' 
    '   Blank Lines: 85 (17.78%)
    '     File Size: 18.45 KB


    '     Enum JsonNodeType
    ' 
    '         [Boolean], [Object], [String], Array, Null
    '         Number
    ' 
    '  
    ' 
    ' 
    ' 
    '     Class JsonNode
    ' 
    '         Properties: BoolValue, IsNull, Items, Members, NumberValue
    '                     StringValue, Type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: AsBoolean, AsDouble, AsInteger, AsString, NewArray
    '                   NewBoolean, NewNumber, NewObject, NewString, ToString
    ' 
    '     Module JsonReader
    ' 
    '         Function: HexValue, Parse, ParseArray, ParseFile, ParseLiteral
    '                   ParseNumber, ParseObject, ParseString, ParseValue
    ' 
    '         Sub: SkipWhitespace
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Globalization
Imports System.Text
' the sciBASIC framework declares its own 'File' / 'Encoding' symbols in the
' global namespace, so the explicit aliases are required here.
Imports SysFile = System.IO.File
Imports TextEncoding = System.Text.Encoding

Namespace ChineseTokenizer.HuggingFace

    ''' <summary>
    ''' json 节点的数据类型。
    ''' </summary>
    Public Enum JsonNodeType
        [Object]
        Array
        [String]
        Number
        [Boolean]
        Null
    End Enum

    ''' <summary>
    ''' 一个轻量级的 json 文档对象模型。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 这里没有直接复用 <c>Microsoft.VisualBasic.Serialization.JSON</c> 中基于
    ''' <c>DataContractJsonSerializer</c> 的反序列化器，原因有两点：
    ''' </para>
    ''' <list type="number">
    ''' <item><description>
    ''' tokenizer.json 中的 <c>vocab</c> 是一个拥有十余万个<b>任意 Unicode 字符</b>
    ''' 作为键名的字典，并且 <c>normalizer</c> / <c>pre_tokenizer</c> 等节点是依据
    ''' <c>type</c> 字段区分的多态结构，契约式序列化器无法描述这样的模式；
    ''' </description></item>
    ''' <item><description>
    ''' 该文件体积约为 7.8 MB，此处的实现直接在字符缓冲区上扫描、不产生中间字符串，
    ''' 解析开销与内存占用都显著低于反射式反序列化。
    ''' </description></item>
    ''' </list>
    ''' <para>
    ''' 本实现不引入任何新的工程引用，因此可以同时用于 <c>net10.0</c> 与
    ''' <c>net10.0-windows</c> 两个目标框架。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class JsonNode

        ''' <summary>
        ''' 当前节点的数据类型。
        ''' </summary>
        Public ReadOnly Property Type As JsonNodeType

        ''' <summary>
        ''' 字符串字面值，仅当 <see cref="Type"/> 为 <see cref="JsonNodeType.String"/> 时有效。
        ''' </summary>
        Public ReadOnly Property StringValue As String
        ''' <summary>
        ''' 数值字面值，仅当 <see cref="Type"/> 为 <see cref="JsonNodeType.Number"/> 时有效。
        ''' </summary>
        Public ReadOnly Property NumberValue As Double
        ''' <summary>
        ''' 布尔字面值，仅当 <see cref="Type"/> 为 <see cref="JsonNodeType.Boolean"/> 时有效。
        ''' </summary>
        Public ReadOnly Property BoolValue As Boolean
        ''' <summary>
        ''' 对象成员表，仅当 <see cref="Type"/> 为 <see cref="JsonNodeType.Object"/> 时有效。
        ''' </summary>
        Public ReadOnly Property Members As Dictionary(Of String, JsonNode)
        ''' <summary>
        ''' 数组元素表，仅当 <see cref="Type"/> 为 <see cref="JsonNodeType.Array"/> 时有效。
        ''' </summary>
        Public ReadOnly Property Items As List(Of JsonNode)

        Private Sub New(type As JsonNodeType)
            Me.Type = type
        End Sub

        Friend Shared Function NewObject(members As Dictionary(Of String, JsonNode)) As JsonNode
            Return New JsonNode(JsonNodeType.Object) With {._Members = members}
        End Function

        Friend Shared Function NewArray(items As List(Of JsonNode)) As JsonNode
            Return New JsonNode(JsonNodeType.Array) With {._Items = items}
        End Function

        Friend Shared Function NewString(value As String) As JsonNode
            Return New JsonNode(JsonNodeType.String) With {._StringValue = value}
        End Function

        Friend Shared Function NewNumber(value As Double) As JsonNode
            Return New JsonNode(JsonNodeType.Number) With {._NumberValue = value}
        End Function

        Friend Shared Function NewBoolean(value As Boolean) As JsonNode
            Return New JsonNode(JsonNodeType.Boolean) With {._BoolValue = value}
        End Function

        Friend Shared ReadOnly NullNode As New JsonNode(JsonNodeType.Null)

        ''' <summary>
        ''' 当前节点是否为 json 的 <c>null</c> 字面量。
        ''' </summary>
        Public ReadOnly Property IsNull As Boolean
            Get
                Return Type = JsonNodeType.Null
            End Get
        End Property

        ''' <summary>
        ''' 按键名读取对象成员，键不存在或当前节点不是对象时返回 <see langword="Nothing"/>。
        ''' </summary>
        Default Public ReadOnly Property Item(key As String) As JsonNode
            Get
                If Type <> JsonNodeType.Object Then
                    Return Nothing
                End If

                Dim value As JsonNode = Nothing

                If Members.TryGetValue(key, value) Then
                    ' json 的 null 字面量在语义上等同于"该配置项未设置"，
                    ' 因此统一折叠为 Nothing，避免调用方到处判断 IsNull
                    Return If(value Is Nothing OrElse value.IsNull, Nothing, value)
                Else
                    Return Nothing
                End If
            End Get
        End Property

        ''' <summary>
        ''' 读取字符串值，节点为空或类型不符时返回 <paramref name="default"/>。
        ''' </summary>
        Public Function AsString(Optional [default] As String = Nothing) As String
            If Type = JsonNodeType.String Then
                Return StringValue
            ElseIf Type = JsonNodeType.Number Then
                Return NumberValue.ToString(CultureInfo.InvariantCulture)
            Else
                Return [default]
            End If
        End Function

        ''' <summary>
        ''' 读取整数值，节点为空或类型不符时返回 <paramref name="default"/>。
        ''' </summary>
        Public Function AsInteger(Optional [default] As Integer = 0) As Integer
            If Type = JsonNodeType.Number Then
                Return CInt(NumberValue)
            Else
                Return [default]
            End If
        End Function

        ''' <summary>
        ''' 读取双精度值，节点为空或类型不符时返回 <paramref name="default"/>。
        ''' </summary>
        Public Function AsDouble(Optional [default] As Double = 0.0) As Double
            If Type = JsonNodeType.Number Then
                Return NumberValue
            Else
                Return [default]
            End If
        End Function

        ''' <summary>
        ''' 读取布尔值，节点为空或类型不符时返回 <paramref name="default"/>。
        ''' </summary>
        Public Function AsBoolean(Optional [default] As Boolean = False) As Boolean
            If Type = JsonNodeType.Boolean Then
                Return BoolValue
            Else
                Return [default]
            End If
        End Function

        Public Overrides Function ToString() As String
            Select Case Type
                Case JsonNodeType.Object : Return $"{{ {Members.Count} members }}"
                Case JsonNodeType.Array : Return $"[ {Items.Count} items ]"
                Case JsonNodeType.String : Return $"""{StringValue}"""
                Case JsonNodeType.Number : Return NumberValue.ToString(CultureInfo.InvariantCulture)
                Case JsonNodeType.Boolean : Return If(BoolValue, "true", "false")
                Case Else : Return "null"
            End Select
        End Function

    End Class

    ''' <summary>
    ''' 供 HuggingFace 分词器模型文件使用的轻量级 json 解析器。
    ''' </summary>
    Public Module JsonReader

        ''' <summary>
        ''' 解析一个 json 文件。
        ''' </summary>
        ''' <param name="file">json 文件的文件路径。</param>
        ''' <exception cref="IO.FileNotFoundException">文件不存在时抛出。</exception>
        ''' <exception cref="FormatException">json 文本格式非法时抛出。</exception>
        Public Function ParseFile(file As String) As JsonNode
            If Not SysFile.Exists(file) Then
                Throw New IO.FileNotFoundException($"the required tokenizer model file is not found: {file}", file)
            End If

            Return Parse(SysFile.ReadAllText(file, TextEncoding.UTF8))
        End Function

        ''' <summary>
        ''' 解析一段 json 文本。
        ''' </summary>
        ''' <exception cref="FormatException">json 文本格式非法时抛出。</exception>
        Public Function Parse(json As String) As JsonNode
            If String.IsNullOrEmpty(json) Then
                Throw New FormatException("the given json text content is empty!")
            End If

            Dim offset As Integer = 0
            Dim result As JsonNode = ParseValue(json, offset)

            Call SkipWhitespace(json, offset)

            If offset < json.Length Then
                Throw New FormatException($"unexpected trailing character '{json(offset)}' at offset {offset}.")
            End If

            Return result
        End Function

        Private Function ParseValue(s As String, ByRef i As Integer) As JsonNode
            Call SkipWhitespace(s, i)

            If i >= s.Length Then
                Throw New FormatException("unexpected end of the json text content.")
            End If

            Select Case s(i)
                Case "{"c : Return ParseObject(s, i)
                Case "["c : Return ParseArray(s, i)
                Case """"c : Return JsonNode.NewString(ParseString(s, i))
                Case "t"c : Return ParseLiteral(s, i, "true", JsonNode.NewBoolean(True))
                Case "f"c : Return ParseLiteral(s, i, "false", JsonNode.NewBoolean(False))
                Case "n"c : Return ParseLiteral(s, i, "null", JsonNode.NullNode)
                Case Else : Return JsonNode.NewNumber(ParseNumber(s, i))
            End Select
        End Function

        Private Function ParseLiteral(s As String, ByRef i As Integer, literal As String, node As JsonNode) As JsonNode
            If i + literal.Length > s.Length OrElse String.CompareOrdinal(s, i, literal, 0, literal.Length) <> 0 Then
                Throw New FormatException($"invalid json literal at offset {i}, '{literal}' is expected.")
            End If

            i += literal.Length
            Return node
        End Function

        Private Function ParseObject(s As String, ByRef i As Integer) As JsonNode
            ' skip the '{' symbol
            i += 1

            Dim members As New Dictionary(Of String, JsonNode)

            Call SkipWhitespace(s, i)

            If i < s.Length AndAlso s(i) = "}"c Then
                i += 1
                Return JsonNode.NewObject(members)
            End If

            Do
                Call SkipWhitespace(s, i)

                If i >= s.Length OrElse s(i) <> """"c Then
                    Throw New FormatException($"a json object member name is expected at offset {i}.")
                End If

                Dim key As String = ParseString(s, i)

                Call SkipWhitespace(s, i)

                If i >= s.Length OrElse s(i) <> ":"c Then
                    Throw New FormatException($"the ':' delimiter is expected at offset {i}.")
                End If

                ' skip the ':' symbol
                i += 1
                ' duplicated member name: the last one wins, keeps consistent with most json parsers
                members(key) = ParseValue(s, i)

                Call SkipWhitespace(s, i)

                If i >= s.Length Then
                    Throw New FormatException("unexpected end of the json object.")
                ElseIf s(i) = ","c Then
                    i += 1
                ElseIf s(i) = "}"c Then
                    i += 1
                    Exit Do
                Else
                    Throw New FormatException($"unexpected character '{s(i)}' in a json object at offset {i}.")
                End If
            Loop

            Return JsonNode.NewObject(members)
        End Function

        Private Function ParseArray(s As String, ByRef i As Integer) As JsonNode
            ' skip the '[' symbol
            i += 1

            Dim items As New List(Of JsonNode)

            Call SkipWhitespace(s, i)

            If i < s.Length AndAlso s(i) = "]"c Then
                i += 1
                Return JsonNode.NewArray(items)
            End If

            Do
                items.Add(ParseValue(s, i))

                Call SkipWhitespace(s, i)

                If i >= s.Length Then
                    Throw New FormatException("unexpected end of the json array.")
                ElseIf s(i) = ","c Then
                    i += 1
                ElseIf s(i) = "]"c Then
                    i += 1
                    Exit Do
                Else
                    Throw New FormatException($"unexpected character '{s(i)}' in a json array at offset {i}.")
                End If
            Loop

            Return JsonNode.NewArray(items)
        End Function

        ''' <summary>
        ''' 解析一个 json 字符串字面量并处理转义序列。
        ''' </summary>
        ''' <remarks>
        ''' 对于不含任何转义字符的常见情形（tokenizer.json 中绝大多数词表键名都是如此），
        ''' 这里直接使用 <see cref="String.Substring"/> 切片，避免逐字符拼接
        ''' <see cref="StringBuilder"/> 带来的额外开销。
        ''' </remarks>
        Private Function ParseString(s As String, ByRef i As Integer) As String
            ' skip the opening quote symbol
            i += 1

            Dim start As Integer = i

            ' fast path: scan for a closing quote without any escape sequence
            Do While i < s.Length
                Dim c As Char = s(i)

                If c = """"c Then
                    Dim value As String = s.Substring(start, i - start)
                    i += 1
                    Return value
                ElseIf c = "\"c Then
                    Exit Do
                End If

                i += 1
            Loop

            If i >= s.Length Then
                Throw New FormatException("unexpected end of a json string literal.")
            End If

            ' slow path: the string literal contains escape sequences
            Dim sb As New StringBuilder(s, start, i - start, (s.Length - start) \ 8 + 16)

            Do While i < s.Length
                Dim c As Char = s(i)

                If c = """"c Then
                    i += 1
                    Return sb.ToString()
                ElseIf c <> "\"c Then
                    sb.Append(c)
                    i += 1
                    Continue Do
                End If

                ' handle of the escape sequence
                i += 1

                If i >= s.Length Then
                    Throw New FormatException("unexpected end of a json escape sequence.")
                End If

                Dim esc As Char = s(i)
                i += 1

                Select Case esc
                    Case """"c : sb.Append(""""c)
                    Case "\"c : sb.Append("\"c)
                    Case "/"c : sb.Append("/"c)
                    Case "b"c : sb.Append(ChrW(8))
                    Case "f"c : sb.Append(ChrW(12))
                    Case "n"c : sb.Append(ChrW(10))
                    Case "r"c : sb.Append(ChrW(13))
                    Case "t"c : sb.Append(ChrW(9))
                    Case "u"c
                        If i + 4 > s.Length Then
                            Throw New FormatException("an incomplete \u escape sequence in a json string literal.")
                        End If

                        Dim code As Integer = 0

                        For k As Integer = 0 To 3
                            code = code * 16 + HexValue(s(i + k))
                        Next

                        sb.Append(ChrW(code))
                        i += 4

                    Case Else
                        Throw New FormatException($"unsupported json escape sequence '\{esc}' at offset {i - 2}.")
                End Select
            Loop

            Throw New FormatException("unexpected end of a json string literal.")
        End Function

        Private Function HexValue(c As Char) As Integer
            If c >= "0"c AndAlso c <= "9"c Then
                Return AscW(c) - AscW("0"c)
            ElseIf c >= "a"c AndAlso c <= "f"c Then
                Return AscW(c) - AscW("a"c) + 10
            ElseIf c >= "A"c AndAlso c <= "F"c Then
                Return AscW(c) - AscW("A"c) + 10
            Else
                Throw New FormatException($"'{c}' is not a valid hexadecimal character.")
            End If
        End Function

        Private Function ParseNumber(s As String, ByRef i As Integer) As Double
            Dim start As Integer = i

            If i < s.Length AndAlso (s(i) = "-"c OrElse s(i) = "+"c) Then
                i += 1
            End If

            Do While i < s.Length
                Dim c As Char = s(i)

                If (c >= "0"c AndAlso c <= "9"c) OrElse c = "."c OrElse c = "e"c OrElse c = "E"c OrElse c = "+"c OrElse c = "-"c Then
                    i += 1
                Else
                    Exit Do
                End If
            Loop

            Dim text As String = s.Substring(start, i - start)
            Dim value As Double

            If Not Double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, value) Then
                Throw New FormatException($"'{text}' at offset {start} is not a valid json number.")
            End If

            Return value
        End Function

        Private Sub SkipWhitespace(s As String, ByRef i As Integer)
            Do While i < s.Length
                Select Case s(i)
                    Case " "c, ChrW(9), ChrW(10), ChrW(13) : i += 1
                    Case Else : Return
                End Select
            Loop
        End Sub

    End Module

End Namespace
