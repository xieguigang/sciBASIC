Option Strict On
Option Explicit On
Option Infer On

' /********************************************************************************/
'  Microsoft.VisualBasic.Data.IO.Protobuf
'  Data\BinaryData\Protobuf\Schema\ProtoParser.vb
' /********************************************************************************/

Imports System.Globalization
Imports System.IO
Imports System.Text

Namespace Schema

    ''' <summary>
    ''' .proto 源文本的递归下降解析器，产出 <see cref="ProtoFile"/> 描述符树。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 支持 proto2 / proto3 的常用语法：<c>syntax</c>、<c>package</c>、<c>import</c>、
    ''' <c>option</c>、message（含嵌套）、enum、<c>repeated</c>/<c>optional</c>/<c>required</c>、
    ''' <c>map&lt;K,V&gt;</c>、<c>oneof</c>、<c>reserved</c>、<c>extensions</c>、service/rpc
    ''' 以及字段选项 <c>[packed=true, default=..., json_name="..."]</c>。
    ''' </para>
    ''' <para>
    ''' 类型引用采用"尽力而为"的解析策略：解析不到的引用被记入
    ''' <see cref="ProtoFile.UnresolvedTypes"/> 而不会抛出异常，便于在缺少 import 文件时
    ''' 仍然获得可用的描述符树。
    ''' </para>
    ''' </remarks>
    Public Class ProtoParser

        Private ReadOnly _tokens As List(Of ProtoToken)
        Private ReadOnly _file As ProtoFile
        Private _index As Integer

        Private Sub New(tokens As List(Of ProtoToken), file As ProtoFile)
            _tokens = tokens
            _file = file
        End Sub

        ''' <summary>
        ''' 从 .proto 文本解析。
        ''' </summary>
        ''' <param name="text">.proto 源文本。</param>
        ''' <param name="fileName">可选的源文件名，用于错误信息与生成代码的头部注释。</param>
        Public Shared Function Parse(text As String, Optional fileName As String = Nothing) As ProtoFile
            Dim lexer As New ProtoLexer(text, fileName)
            Dim tokens As List(Of ProtoToken) = lexer.Tokenize()
            Dim file As New ProtoFile With {.SourceFile = fileName}
            Dim parser As New ProtoParser(tokens, file)

            Call parser.ParseDocument()

            Return file
        End Function

        ''' <summary>
        ''' 从磁盘上的 .proto 文件解析。
        ''' </summary>
        Public Shared Function ParseFile(protoFilePath As String) As ProtoFile
            If String.IsNullOrEmpty(protoFilePath) Then
                Throw New ArgumentNullException(NameOf(protoFilePath))
            End If

            If Not File.Exists(protoFilePath) Then
                Throw New FileNotFoundException("找不到指定的 .proto 文件", protoFilePath)
            End If

            Return Parse(File.ReadAllText(protoFilePath), Path.GetFileName(protoFilePath))
        End Function

#Region "DOCUMENT"

        Private Sub ParseDocument()
            If AcceptKeyword("syntax") Then
                Call ExpectSymbol("=")

                Dim syntaxText As String = ExpectStringLiteral()

                Call ExpectSymbol(";")
                _file.Syntax = If(String.Equals(syntaxText, "proto3", StringComparison.OrdinalIgnoreCase),
                                  ProtoSyntax.Proto3, ProtoSyntax.Proto2)
            End If

            Do While Not IsEndOfFile
                If AcceptSymbol(";") Then
                    Continue Do
                End If

                If AcceptKeyword("package") Then
                    _file.Package = ParseQualifiedName()
                    Call ExpectSymbol(";")
                ElseIf AcceptKeyword("import") Then
                    If AcceptKeyword("public") Then
                        ' 仅记录，不递归加载
                    ElseIf AcceptKeyword("weak") Then
                        ' 同上
                    End If

                    Call _file.[Imports].Add(ExpectStringLiteral())
                    Call ExpectSymbol(";")
                ElseIf AcceptKeyword("option") Then
                    Call ParseOptionInto(_file.Options, Nothing)
                ElseIf AcceptKeyword("message") Then
                    Call _file.Messages.Add(ParseMessage(Nothing))
                ElseIf AcceptKeyword("enum") Then
                    Call _file.Enums.Add(ParseEnum(Nothing))
                ElseIf AcceptKeyword("service") Then
                    Call _file.Services.Add(ParseService())
                ElseIf AcceptKeyword("extend") Then
                    Call SkipBlockOrStatement()
                Else
                    Throw ParseError($"无法识别的顶层声明 '{Current.Text}'")
                End If
            Loop

            Call ResolveTypes()
        End Sub

#End Region

#Region "MESSAGE / ENUM / SERVICE"

        Private Function ParseMessage(parent As MessageDescriptor) As MessageDescriptor
            Dim name As String = ExpectIdentifier()
            Dim message As New MessageDescriptor With {
                .Name = name,
                .Package = _file.Package,
                .Parent = parent
            }

            Call ExpectSymbol("{")

            Do While Not AcceptSymbol("}")
                If IsEndOfFile Then
                    Throw ParseError($"消息 '{name}' 缺少闭合的 '}}'")
                End If

                If AcceptSymbol(";") Then
                    Continue Do
                End If

                If AcceptKeyword("message") Then
                    Call message.NestedMessages.Add(ParseMessage(message))
                ElseIf AcceptKeyword("enum") Then
                    Call message.NestedEnums.Add(ParseEnum(message))
                ElseIf AcceptKeyword("oneof") Then
                    Call ParseOneof(message)
                ElseIf IsKeyword("map") AndAlso PeekToken(1).IsSymbol("<") Then
                    Call ParseMapField(message)
                ElseIf AcceptKeyword("reserved") Then
                    Call message.ReservedRanges.Add(SkipStatementToSemicolon())
                ElseIf AcceptKeyword("extensions") Then
                    Call SkipStatementToSemicolon()
                ElseIf AcceptKeyword("option") Then
                    Call ParseOptionInto(Nothing, Nothing)
                ElseIf AcceptKeyword("extend") Then
                    Call SkipBlockOrStatement()
                Else
                    Call ParseField(message, -1)
                End If
            Loop

            Return message
        End Function

        Private Sub ParseField(message As MessageDescriptor, oneofIndex As Integer)
            Dim label As FieldLabel = FieldLabel.[Optional]

            If oneofIndex < 0 Then
                If AcceptKeyword("repeated") Then
                    label = FieldLabel.Repeated
                ElseIf AcceptKeyword("required") Then
                    label = FieldLabel.Required
                ElseIf AcceptKeyword("optional") Then
                    label = FieldLabel.[Optional]
                End If
            End If

            Dim typeName As String = ParseQualifiedName()
            Dim fieldName As String = ExpectIdentifier()

            Call ExpectSymbol("=")

            Dim number As Integer = ExpectInteger()

            If Not Wire.ProtoWireFormat.IsValidFieldNumber(number) Then
                Throw ParseError($"字段号 {number} 非法：有效范围为 1..{Wire.ProtoWireFormat.MaxFieldNumber}，且不能落在 19000..19999 保留区间")
            End If

            Dim field As New FieldDescriptor With {
                .Name = fieldName,
                .Number = number,
                .Label = label,
                .TypeName = typeName,
                .OneofIndex = oneofIndex
            }

            Dim kind As FieldTypeKind

            If TryGetScalarKind(typeName, kind) Then
                field.FieldType = kind
            Else
                field.FieldType = FieldTypeKind.Message
            End If

            If AcceptSymbol("[") Then
                Call ParseFieldOptions(field)
            End If

            Call ExpectSymbol(";")
            Call message.AddField(field)
        End Sub

        Private Sub ParseMapField(message As MessageDescriptor)
            Call AcceptKeyword("map")
            Call ExpectSymbol("<")

            Dim keyTypeName As String = ParseQualifiedName()

            Call ExpectSymbol(",")

            Dim valueTypeName As String = ParseQualifiedName()

            Call ExpectSymbol(">")

            Dim fieldName As String = ExpectIdentifier()

            Call ExpectSymbol("=")

            Dim number As Integer = ExpectInteger()

            If Not Wire.ProtoWireFormat.IsValidFieldNumber(number) Then
                Throw ParseError($"字段号 {number} 非法：有效范围为 1..{Wire.ProtoWireFormat.MaxFieldNumber}，且不能落在 19000..19999 保留区间")
            End If

            Dim keyKind As FieldTypeKind

            If Not TryGetScalarKind(keyTypeName, keyKind) OrElse Not IsValidMapKeyKind(keyKind) Then
                Throw ParseError($"map 的键类型 '{keyTypeName}' 非法：只允许整型、布尔或 string")
            End If

            Dim valueKind As FieldTypeKind

            If Not TryGetScalarKind(valueTypeName, valueKind) Then
                valueKind = FieldTypeKind.Message
            End If

            Dim field As New FieldDescriptor With {
                .Name = fieldName,
                .Number = number,
                .Label = FieldLabel.Repeated,
                .TypeName = valueTypeName,
                .IsMap = True,
                .MapKeyType = keyKind,
                .MapValueType = valueKind,
                .MapValueTypeName = valueTypeName
            }

            If AcceptSymbol("[") Then
                Call ParseFieldOptions(field)
            End If

            Call ExpectSymbol(";")
            Call message.AddField(field)

            ' protoc 会为每个 map 字段合成一个 entry 消息：key = 1, value = 2
            Dim entry As New MessageDescriptor With {
                .Name = ToPascalIdentifier(fieldName) & "Entry",
                .Package = _file.Package,
                .Parent = message,
                .MapEntry = True
            }

            Call entry.AddField(New FieldDescriptor With {
                .Name = "key", .Number = 1, .Label = FieldLabel.[Optional],
                .FieldType = keyKind, .TypeName = keyTypeName
            })
            Call entry.AddField(New FieldDescriptor With {
                .Name = "value", .Number = 2, .Label = FieldLabel.[Optional],
                .FieldType = valueKind, .TypeName = valueTypeName
            })

            Call message.NestedMessages.Add(entry)

            field.MapEntryTypeName = entry.FullName
        End Sub

        Private Sub ParseOneof(message As MessageDescriptor)
            Dim name As String = ExpectIdentifier()
            Dim index As Integer = message.Oneofs.Count

            Call message.Oneofs.Add(name)
            Call ExpectSymbol("{")

            Do While Not AcceptSymbol("}")
                If IsEndOfFile Then
                    Throw ParseError($"oneof '{name}' 缺少闭合的 '}}'")
                End If

                If AcceptSymbol(";") Then
                    Continue Do
                End If

                If AcceptKeyword("option") Then
                    Call ParseOptionInto(Nothing, Nothing)
                    Continue Do
                End If

                Call ParseField(message, index)
            Loop
        End Sub

        Private Function ParseEnum(parent As MessageDescriptor) As EnumDescriptor
            Dim name As String = ExpectIdentifier()
            Dim item As New EnumDescriptor With {
                .Name = name,
                .Package = _file.Package,
                .Parent = parent
            }

            Call ExpectSymbol("{")

            Do While Not AcceptSymbol("}")
                If IsEndOfFile Then
                    Throw ParseError($"枚举 '{name}' 缺少闭合的 '}}'")
                End If

                If AcceptSymbol(";") Then
                    Continue Do
                End If

                If AcceptKeyword("reserved") Then
                    Call SkipStatementToSemicolon()
                    Continue Do
                End If

                If AcceptKeyword("option") Then
                    Call ParseOptionInto(Nothing, item)
                    Continue Do
                End If

                Dim valueName As String = ExpectIdentifier()

                Call ExpectSymbol("=")

                Dim number As Integer = ExpectInteger()

                If AcceptSymbol("[") Then
                    Do Until AcceptSymbol("]")
                        If IsEndOfFile Then
                            Throw ParseError($"枚举取值 '{valueName}' 的选项缺少闭合的 ']'")
                        End If

                        Call Advance()
                    Loop
                End If

                Call ExpectSymbol(";")
                Call item.AddValue(valueName, number)
            Loop

            Return item
        End Function

        Private Function ParseService() As ServiceDescriptor
            Dim name As String = ExpectIdentifier()
            Dim service As New ServiceDescriptor With {.Name = name, .Package = _file.Package}

            Call ExpectSymbol("{")

            Do While Not AcceptSymbol("}")
                If IsEndOfFile Then
                    Throw ParseError($"service '{name}' 缺少闭合的 '}}'")
                End If

                If AcceptSymbol(";") Then
                    Continue Do
                End If

                If AcceptKeyword("option") Then
                    Call ParseOptionInto(Nothing, Nothing)
                    Continue Do
                End If

                If Not AcceptKeyword("rpc") Then
                    Throw ParseError($"service '{name}' 内只能是 rpc 或 option 声明，实际遇到 '{Current.Text}'")
                End If

                Dim method As New MethodDescriptor With {.Name = ExpectIdentifier()}

                Call ExpectSymbol("(")

                If AcceptKeyword("stream") Then
                    method.ClientStreaming = True
                End If

                method.InputType = ParseQualifiedName()

                Call ExpectSymbol(")")

                If Not AcceptKeyword("returns") Then
                    Throw ParseError("rpc 声明缺少 returns 关键字")
                End If

                Call ExpectSymbol("(")

                If AcceptKeyword("stream") Then
                    method.ServerStreaming = True
                End If

                method.OutputType = ParseQualifiedName()

                Call ExpectSymbol(")")

                ' 方法体（选项）或直接分号结束
                If Current.IsSymbol("{") Then
                    Call SkipBalancedBraces()
                    Call AcceptSymbol(";")
                Else
                    Call ExpectSymbol(";")
                End If

                Call service.Methods.Add(method)
            Loop

            Return service
        End Function

#End Region

#Region "OPTIONS / SKIP"

        Private Sub ParseFieldOptions(field As FieldDescriptor)
            Do
                If AcceptSymbol("]") Then
                    Exit Do
                End If

                Dim optionName As String

                If AcceptSymbol("(") Then
                    optionName = "(" & ParseQualifiedName() & ")"
                    Call ExpectSymbol(")")

                    If AcceptSymbol(".") Then
                        optionName &= "." & ExpectIdentifier()
                    End If
                Else
                    optionName = ExpectIdentifier()
                End If

                If AcceptSymbol("=") Then
                    If Current.IsSymbol("{") Then
                        Call SkipBalancedBraces()
                    Else
                        Dim valueToken As ProtoToken = Current

                        Call Advance()

                        Select Case optionName.ToLowerInvariant()
                            Case "packed"
                                field.Packed = String.Equals(valueToken.Text, "true", StringComparison.OrdinalIgnoreCase)
                            Case "default"
                                field.DefaultValue = valueToken.Text
                            Case "json_name"
                                field.JsonName = valueToken.Text
                        End Select
                    End If
                End If

                If Not AcceptSymbol(",") Then
                    Exit Do
                End If
            Loop

            Call ExpectSymbol("]")
        End Sub

        Private Sub ParseOptionInto(options As Dictionary(Of String, String), enumDesc As EnumDescriptor)
            Dim optionName As String

            If AcceptSymbol("(") Then
                optionName = "(" & ParseQualifiedName() & ")"
                Call ExpectSymbol(")")

                If AcceptSymbol(".") Then
                    optionName &= "." & ExpectIdentifier()
                End If
            Else
                optionName = ExpectIdentifier()
            End If

            Call ExpectSymbol("=")

            Dim value As String

            If Current.IsSymbol("{") Then
                value = SkipBalancedBraces()
            Else
                value = Current.Text
                Call Advance()
            End If

            Call ExpectSymbol(";")

            If options IsNot Nothing AndAlso Not options.ContainsKey(optionName) Then
                options(optionName) = value
            End If

            If enumDesc IsNot Nothing AndAlso String.Equals(optionName, "allow_alias", StringComparison.OrdinalIgnoreCase) Then
                enumDesc.AllowAlias = String.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
            End If
        End Sub

        ''' <summary>
        ''' 跳过 <c>{ ... }</c> 聚合体，返回其原文（仅用于调试与选项记录）。
        ''' </summary>
        Private Function SkipBalancedBraces() As String
            Dim builder As New StringBuilder()
            Dim depth As Integer = 0
            Dim started As Boolean = False

            Do
                If IsEndOfFile Then
                    Throw ParseError("聚合值缺少闭合的 '}'")
                End If

                Dim token As ProtoToken = Current

                If token.IsSymbol("{") Then
                    depth += 1
                    started = True
                ElseIf token.IsSymbol("}") Then
                    depth -= 1
                End If

                If builder.Length > 0 Then
                    Call builder.Append(" "c)
                End If

                Call builder.Append(token.Text)
                Call Advance()

                If started AndAlso depth = 0 Then
                    Exit Do
                End If
            Loop

            Return builder.ToString()
        End Function

        ''' <summary>
        ''' 跳过到当前语句结尾的分号，返回语句原文。
        ''' </summary>
        Private Function SkipStatementToSemicolon() As String
            Dim builder As New StringBuilder()
            Dim depth As Integer = 0

            Do While Not IsEndOfFile
                Dim token As ProtoToken = Current

                If token.IsSymbol("{") Then
                    depth += 1
                ElseIf token.IsSymbol("}") Then
                    depth -= 1
                ElseIf token.IsSymbol(";") AndAlso depth <= 0 Then
                    Call Advance()
                    Exit Do
                End If

                If builder.Length > 0 Then
                    Call builder.Append(" "c)
                End If

                Call builder.Append(token.Text)
                Call Advance()
            Loop

            Return builder.ToString()
        End Function

        Private Sub SkipBlockOrStatement()
            If Current.IsSymbol("{") Then
                Call SkipBalancedBraces()
            Else
                Call SkipStatementToSemicolon()
            End If
        End Sub

#End Region

#Region "TYPE RESOLUTION"

        Private Sub ResolveTypes()
            Dim messages As List(Of MessageDescriptor) = _file.AllMessages().ToList()
            Dim enums As List(Of EnumDescriptor) = _file.AllEnums().ToList()
            Dim messageByName As New Dictionary(Of String, MessageDescriptor)(StringComparer.Ordinal)
            Dim enumByName As New Dictionary(Of String, EnumDescriptor)(StringComparer.Ordinal)

            For Each message As MessageDescriptor In messages
                If String.IsNullOrEmpty(message.Package) Then
                    message.Package = _file.Package
                End If

                If Not messageByName.ContainsKey(message.FullName) Then
                    messageByName(message.FullName) = message
                End If
            Next

            For Each item As EnumDescriptor In enums
                If String.IsNullOrEmpty(item.Package) Then
                    item.Package = _file.Package
                End If

                If Not enumByName.ContainsKey(item.FullName) Then
                    enumByName(item.FullName) = item
                End If
            Next

            For Each service As ServiceDescriptor In _file.Services
                If String.IsNullOrEmpty(service.Package) Then
                    service.Package = _file.Package
                End If
            Next

            For Each message As MessageDescriptor In messages
                For Each field As FieldDescriptor In message.Fields
                    If field.IsMap Then
                        Call ResolveMapValueType(field, message, messages, enums, messageByName, enumByName)
                    ElseIf field.FieldType = FieldTypeKind.Message Then
                        Call ResolveFieldType(field, message, messages, enums, messageByName, enumByName)
                    End If
                Next
            Next
        End Sub

        Private Sub ResolveFieldType(field As FieldDescriptor,
                                     scope As MessageDescriptor,
                                     messages As List(Of MessageDescriptor),
                                     enums As List(Of EnumDescriptor),
                                     messageByName As Dictionary(Of String, MessageDescriptor),
                                     enumByName As Dictionary(Of String, EnumDescriptor))

            Dim kind As FieldTypeKind = FieldTypeKind.Message
            Dim fullName As String = ResolveTypeName(field.TypeName, scope, messages, enums, messageByName, enumByName, kind)

            If fullName Is Nothing Then
                Call AddUnresolved(field.TypeName)
            Else
                field.ResolvedTypeFullName = fullName
                field.FieldType = kind
            End If
        End Sub

        Private Sub ResolveMapValueType(field As FieldDescriptor,
                                        scope As MessageDescriptor,
                                        messages As List(Of MessageDescriptor),
                                        enums As List(Of EnumDescriptor),
                                        messageByName As Dictionary(Of String, MessageDescriptor),
                                        enumByName As Dictionary(Of String, EnumDescriptor))

            If field.MapValueType <> FieldTypeKind.Message Then
                Return
            End If

            Dim kind As FieldTypeKind = FieldTypeKind.Message
            Dim fullName As String = ResolveTypeName(field.MapValueTypeName, scope, messages, enums, messageByName, enumByName, kind)

            If fullName Is Nothing Then
                Call AddUnresolved(field.MapValueTypeName)
            Else
                field.MapValueResolvedTypeFullName = fullName
                field.MapValueType = kind
            End If
        End Sub

        Private Function ResolveTypeName(name As String,
                                         scope As MessageDescriptor,
                                         messages As List(Of MessageDescriptor),
                                         enums As List(Of EnumDescriptor),
                                         messageByName As Dictionary(Of String, MessageDescriptor),
                                         enumByName As Dictionary(Of String, EnumDescriptor),
                                         ByRef kind As FieldTypeKind) As String

            If String.IsNullOrEmpty(name) Then
                Return Nothing
            End If

            Dim candidate As String = name

            If candidate.StartsWith(".", StringComparison.Ordinal) Then
                candidate = candidate.Substring(1)
            End If

            Dim scopes As New List(Of String)
            Dim current As MessageDescriptor = scope

            Do While current IsNot Nothing
                Call scopes.Add(current.FullName)
                current = current.Parent
            Loop

            If Not String.IsNullOrEmpty(_file.Package) Then
                Call scopes.Add(_file.Package)
            End If

            For Each prefix As String In scopes
                Dim fullName As String = prefix & "." & candidate

                If messageByName.ContainsKey(fullName) Then
                    kind = FieldTypeKind.Message
                    Return fullName
                End If

                If enumByName.ContainsKey(fullName) Then
                    kind = FieldTypeKind.[Enum]
                    Return fullName
                End If
            Next

            If messageByName.ContainsKey(candidate) Then
                kind = FieldTypeKind.Message
                Return candidate
            End If

            If enumByName.ContainsKey(candidate) Then
                kind = FieldTypeKind.[Enum]
                Return candidate
            End If

            ' 兜底：唯一短名匹配
            Dim simpleName As String = candidate
            Dim dot As Integer = simpleName.LastIndexOf("."c)

            If dot >= 0 Then
                simpleName = simpleName.Substring(dot + 1)
            End If

            Dim messageMatches As List(Of MessageDescriptor) =
                messages.Where(Function(m) String.Equals(m.Name, simpleName, StringComparison.Ordinal)).ToList()

            If messageMatches.Count = 1 Then
                kind = FieldTypeKind.Message
                Return messageMatches(0).FullName
            End If

            Dim enumMatches As List(Of EnumDescriptor) =
                enums.Where(Function(e) String.Equals(e.Name, simpleName, StringComparison.Ordinal)).ToList()

            If enumMatches.Count = 1 Then
                kind = FieldTypeKind.[Enum]
                Return enumMatches(0).FullName
            End If

            Return Nothing
        End Function

        Private Sub AddUnresolved(typeName As String)
            If String.IsNullOrEmpty(typeName) Then
                Return
            End If

            If Not _file.UnresolvedTypes.Contains(typeName) Then
                Call _file.UnresolvedTypes.Add(typeName)
            End If
        End Sub

#End Region

#Region "TOKEN HELPERS"

        Private ReadOnly Property Current As ProtoToken
            Get
                Dim index As Integer = _index

                If index > _tokens.Count - 1 Then
                    index = _tokens.Count - 1
                End If

                Return _tokens(index)
            End Get
        End Property

        Private ReadOnly Property IsEndOfFile As Boolean
            Get
                Return Current.Type = ProtoTokenType.EndOfFile
            End Get
        End Property

        Private Sub Advance()
            If _index < _tokens.Count - 1 Then
                _index += 1
            End If
        End Sub

        Private Function PeekToken(offset As Integer) As ProtoToken
            Dim index As Integer = _index + offset

            If index > _tokens.Count - 1 Then
                index = _tokens.Count - 1
            End If

            If index < 0 Then
                index = 0
            End If

            Return _tokens(index)
        End Function

        Private Function AcceptSymbol(symbol As String) As Boolean
            If Current.IsSymbol(symbol) Then
                Call Advance()
                Return True
            End If

            Return False
        End Function

        Private Function AcceptKeyword(keyword As String) As Boolean
            If Current.IsKeyword(keyword) Then
                Call Advance()
                Return True
            End If

            Return False
        End Function

        Private Function IsKeyword(keyword As String) As Boolean
            Return Current.IsKeyword(keyword)
        End Function

        Private Sub ExpectSymbol(symbol As String)
            If Not AcceptSymbol(symbol) Then
                Throw ParseError($"期望 '{symbol}'，实际遇到 '{Current.Text}'")
            End If
        End Sub

        Private Function ExpectIdentifier() As String
            If Current.Type <> ProtoTokenType.Identifier Then
                Throw ParseError($"期望标识符，实际遇到 '{Current.Text}'")
            End If

            Dim text As String = Current.Text

            Call Advance()

            Return text
        End Function

        Private Function ExpectStringLiteral() As String
            If Current.Type <> ProtoTokenType.StringLiteral Then
                Throw ParseError($"期望字符串字面量，实际遇到 '{Current.Text}'")
            End If

            Dim text As String = Current.Text

            Call Advance()

            Return text
        End Function

        Private Function ExpectInteger() As Integer
            Dim negative As Boolean = AcceptSymbol("-")

            If Current.Type <> ProtoTokenType.Number Then
                Throw ParseError($"期望整数字面量，实际遇到 '{Current.Text}'")
            End If

            Dim text As String = Current.Text

            Call Advance()

            Dim value As Long = ParseIntegerText(text)

            If negative Then
                value = -value
            End If

            If value < Integer.MinValue OrElse value > Integer.MaxValue Then
                Throw ParseError($"整数字面量 '{text}' 超出 32 位范围")
            End If

            Return CInt(value)
        End Function

        Private Function ParseQualifiedName() As String
            If AcceptSymbol(".") Then
                Return "." & ExpectIdentifier()
            Else
                Return ExpectIdentifier()
            End If
        End Function

        Private Function ParseError(message As String) As ProtoParseException
            Return New ProtoParseException(message, _file.SourceFile, Current.Line, Current.Column)
        End Function

        Private Shared Function ParseIntegerText(text As String) As Long
            If String.IsNullOrEmpty(text) Then
                Return 0L
            End If

            Dim value As String = text.Trim()

            If value.StartsWith("0x", StringComparison.OrdinalIgnoreCase) Then
                Return Convert.ToInt64(value.Substring(2), 16)
            End If

            If value.Length > 1 AndAlso value(0) = "0"c AndAlso value.All(Function(c) c >= "0"c AndAlso c <= "7"c) Then
                Return Convert.ToInt64(value, 8)
            End If

            Return Long.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture)
        End Function

        Private Shared Function TryGetScalarKind(typeName As String, ByRef kind As FieldTypeKind) As Boolean
            Dim name As String = If(typeName, "").Trim()

            If name.StartsWith(".", StringComparison.Ordinal) Then
                name = name.Substring(1)
            End If

            Select Case name.ToLowerInvariant()
                Case "double"
                    kind = FieldTypeKind.Double
                Case "float"
                    kind = FieldTypeKind.Float
                Case "int32"
                    kind = FieldTypeKind.Int32
                Case "int64"
                    kind = FieldTypeKind.Int64
                Case "uint32"
                    kind = FieldTypeKind.UInt32
                Case "uint64"
                    kind = FieldTypeKind.UInt64
                Case "sint32"
                    kind = FieldTypeKind.SInt32
                Case "sint64"
                    kind = FieldTypeKind.SInt64
                Case "fixed32"
                    kind = FieldTypeKind.Fixed32
                Case "fixed64"
                    kind = FieldTypeKind.Fixed64
                Case "sfixed32"
                    kind = FieldTypeKind.SFixed32
                Case "sfixed64"
                    kind = FieldTypeKind.SFixed64
                Case "bool"
                    kind = FieldTypeKind.Bool
                Case "string"
                    kind = FieldTypeKind.[String]
                Case "bytes"
                    kind = FieldTypeKind.Bytes
                Case Else
                    Return False
            End Select

            Return True
        End Function

        Private Shared Function IsValidMapKeyKind(kind As FieldTypeKind) As Boolean
            Select Case kind
                Case FieldTypeKind.Int32, FieldTypeKind.Int64, FieldTypeKind.UInt32, FieldTypeKind.UInt64,
                     FieldTypeKind.SInt32, FieldTypeKind.SInt64, FieldTypeKind.Fixed32, FieldTypeKind.Fixed64,
                     FieldTypeKind.SFixed32, FieldTypeKind.SFixed64, FieldTypeKind.Bool, FieldTypeKind.[String]
                    Return True

                Case Else
                    Return False
            End Select
        End Function

        Private Shared Function ToPascalIdentifier(name As String) As String
            If String.IsNullOrEmpty(name) Then
                Return name
            End If

            Dim parts As String() = name.Split("_"c)
            Dim builder As New StringBuilder()

            For Each part As String In parts
                If part.Length = 0 Then
                    Continue For
                End If

                Call builder.Append(Char.ToUpperInvariant(part(0)))

                If part.Length > 1 Then
                    Call builder.Append(part.Substring(1))
                End If
            Next

            If builder.Length = 0 Then
                Return name
            Else
                Return builder.ToString()
            End If
        End Function

#End Region

    End Class

End Namespace
