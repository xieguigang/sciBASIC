Option Strict On
Option Explicit On
Option Infer On

' /********************************************************************************/
'  Microsoft.VisualBasic.Data.IO.Protobuf
'  Data\BinaryData\Protobuf\CodeGen\ProtoCodeGeneratorBase.vb
' /********************************************************************************/

Imports System.Globalization
Imports System.IO
Imports Microsoft.VisualBasic.Data.IO.Protobuf.Schema

Namespace CodeGen

    ''' <summary>
    ''' 代码生成器的公共骨架：负责类型命名、文件拆分、文档结构与成员特性拼装，
    ''' 具体语法细节由目标语言子类实现。
    ''' </summary>
    Public MustInherit Class ProtoCodeGeneratorBase : Implements IProtoCodeGenerator

        Private _names As Dictionary(Of String, String)

        Public MustOverride ReadOnly Property Language As CodeLanguage Implements IProtoCodeGenerator.Language

        ''' <summary>
        ''' 生成文件的扩展名（含点号）。
        ''' </summary>
        Protected MustOverride ReadOnly Property FileExtension As String

        ''' <summary>
        ''' 文档注释前缀（VB 为 <c>'''</c>，C# 为 <c>///</c>）。
        ''' </summary>
        Protected MustOverride ReadOnly Property DocPrefix As String

        Protected MustOverride Sub WriteFileHeader(writer As CodeWriter, sourceName As String)

        Protected MustOverride Sub WriteImports(writer As CodeWriter)

        Protected MustOverride Sub WriteNamespaceOpen(writer As CodeWriter, name As String)

        Protected MustOverride Sub WriteNamespaceClose(writer As CodeWriter, name As String)

        Protected MustOverride Sub WriteEnumDeclaration(writer As CodeWriter, item As EnumDescriptor, clrName As String)

        Protected MustOverride Sub WriteMessageDeclaration(writer As CodeWriter, message As MessageDescriptor, clrName As String)

        ''' <summary>
        ''' 写出单个字段的属性声明（不含特性行）。
        ''' </summary>
        Protected MustOverride Sub WritePropertyDeclaration(writer As CodeWriter, field As FieldDescriptor)

        ''' <summary>
        ''' 把特性文本包装成目标语言语法（VB: <c>&lt;...&gt;</c>，C#: <c>[...]</c>）。
        ''' </summary>
        Protected MustOverride Function AttributeText(text As String) As String

        ''' <summary>
        ''' 拼装命名参数（VB: <c>Name:=Value</c>，C#: <c>Name = Value</c>）。
        ''' </summary>
        Protected MustOverride Function NamedArgument(name As String, value As String) As String

        Protected MustOverride Function BoolLiteral(value As Boolean) As String

        ''' <summary>
        ''' 把标量类型映射为目标语言的 CLR 类型名。
        ''' </summary>
        Protected MustOverride Function GetScalarType(kind As FieldTypeKind) As String

        ''' <summary>
        ''' 拼装泛型集合类型名。
        ''' </summary>
        Protected MustOverride Function GetListType(elementType As String) As String

        ''' <summary>
        ''' 拼装字典类型名。
        ''' </summary>
        Protected MustOverride Function GetMapType(keyType As String, valueType As String) As String

        ''' <summary>
        ''' 转义标识符。
        ''' </summary>
        Protected MustOverride Function EscapeName(name As String) As String

        ''' <summary>
        ''' 拼装字段的完整成员类型（含 repeated / map 包装）。
        ''' </summary>
        Protected MustOverride Function GetMemberType(field As FieldDescriptor) As String

        ''' <summary>
        ''' 拼装集合字段的初始化表达式；不需要初始化时返回空字符串。
        ''' </summary>
        Protected MustOverride Function GetMemberInitializer(field As FieldDescriptor) As String

#Region "ENTRY API"

        Public Function Generate(file As ProtoFile) As String Implements IProtoCodeGenerator.Generate
            If file Is Nothing Then
                Throw New ArgumentNullException(NameOf(file))
            End If

            Call BuildNameMap(file)

            Dim writer As New CodeWriter()
            Dim messages As IEnumerable(Of MessageDescriptor) =
                file.AllMessages().Where(Function(m) Not m.MapEntry)

            Call WriteDocument(writer, If(file.SourceFile, "<memory>"), file.AllEnums(), messages,
                               NamingHelper.ToNamespace(file.Package))

            Return writer.ToString()
        End Function

        Public Function GenerateFiles(file As ProtoFile) As Dictionary(Of String, String) Implements IProtoCodeGenerator.GenerateFiles
            If file Is Nothing Then
                Throw New ArgumentNullException(NameOf(file))
            End If

            Call BuildNameMap(file)

            Dim result As New Dictionary(Of String, String)(StringComparer.Ordinal)
            Dim namespaceName As String = NamingHelper.ToNamespace(file.Package)
            Dim sourceName As String = If(file.SourceFile, "<memory>")
            Dim topLevelEnums As List(Of EnumDescriptor) =
                file.AllEnums().Where(Function(e) e.Parent Is Nothing).ToList()

            If topLevelEnums.Count > 0 Then
                Dim writer As New CodeWriter()

                Call WriteDocument(writer, sourceName, topLevelEnums,
                                   Enumerable.Empty(Of MessageDescriptor)(), namespaceName)
                result("ProtobufEnums" & FileExtension) = writer.ToString()
            End If

            For Each message As MessageDescriptor In file.Messages
                Dim writer As New CodeWriter()
                Dim enums As New List(Of EnumDescriptor)

                Call CollectEnums(message, enums)
                Call WriteDocument(writer, sourceName, enums, message.AllMessages(), namespaceName)

                result(GetTypeName(message.FullName) & FileExtension) = writer.ToString()
            Next

            Return result
        End Function

        Public Function GenerateToDirectory(file As ProtoFile, outputDir As String) As IReadOnlyList(Of String) Implements IProtoCodeGenerator.GenerateToDirectory
            If String.IsNullOrEmpty(outputDir) Then
                Throw New ArgumentNullException(NameOf(outputDir))
            End If

            Dim files As Dictionary(Of String, String) = GenerateFiles(file)
            Dim paths As New List(Of String)

            Call Directory.CreateDirectory(outputDir)

            For Each pair As KeyValuePair(Of String, String) In files
                Dim filePath As String = Path.Combine(outputDir, pair.Key)

                Call System.IO.File.WriteAllText(filePath, pair.Value)
                Call paths.Add(filePath)
            Next

            Return paths
        End Function

#End Region

#Region "DOCUMENT"

        Protected Sub WriteDocument(writer As CodeWriter, sourceName As String,
                                    enums As IEnumerable(Of EnumDescriptor),
                                    messages As IEnumerable(Of MessageDescriptor),
                                    namespaceName As String)

            Call WriteFileHeader(writer, sourceName)
            Call WriteImports(writer)
            Call writer.BlankLine()

            Dim hasNamespace As Boolean = Not String.IsNullOrEmpty(namespaceName)

            If hasNamespace Then
                Call WriteNamespaceOpen(writer, namespaceName)
                Call writer.Indent()
            End If

            For Each item As EnumDescriptor In enums
                Call WriteEnumDeclaration(writer, item, GetTypeName(item.FullName))
                Call writer.BlankLine()
            Next

            For Each message As MessageDescriptor In messages
                Call WriteMessageDeclaration(writer, message, GetTypeName(message.FullName))
                Call writer.BlankLine()
            Next

            If hasNamespace Then
                Call writer.Dedent()
                Call WriteNamespaceClose(writer, namespaceName)
            End If
        End Sub

        ''' <summary>
        ''' 依次写出消息的全部字段属性。
        ''' </summary>
        Protected Sub WriteMembers(writer As CodeWriter, message As MessageDescriptor)
            Dim fields As List(Of FieldDescriptor) = message.Fields

            If fields.Count = 0 Then
                Return
            End If

            For i As Integer = 0 To fields.Count - 1
                Call WriteProperty(writer, fields(i))

                If i < fields.Count - 1 Then
                    Call writer.BlankLine()
                End If
            Next
        End Sub

        Protected Sub WriteProperty(writer As CodeWriter, field As FieldDescriptor)
            Call writer.WriteLine($"{DocPrefix} <summary>")
            Call writer.WriteLine($"{DocPrefix} {field.Name}：{If(field.TypeName, field.FieldType.ToString())}，字段号 {field.Number}")
            Call writer.WriteLine($"{DocPrefix} </summary>")
            Call writer.WriteLine(AttributeText(GetProtoMemberText(field)))
            Call WritePropertyDeclaration(writer, field)
        End Sub

        ''' <summary>
        ''' 拼装 <c>ProtoMember(...)</c> 特性文本（不含包裹符号）。
        ''' </summary>
        Protected Function GetProtoMemberText(field As FieldDescriptor) As String
            Dim args As New List(Of String) From {field.Number.ToString(CultureInfo.InvariantCulture)}

            If field.IsMap Then
                Call args.Add(NamedArgument("IsMap", BoolLiteral(True)))
            End If

            If field.Packed.HasValue Then
                Call args.Add(NamedArgument("IsPacked", BoolLiteral(field.Packed.Value)))
            End If

            Dim format As String = GetDataFormatName(field.FieldType)

            If format IsNot Nothing Then
                Call args.Add(NamedArgument("DataFormat", $"ProtoDataFormat.{format}"))
            End If

            Return $"ProtoMember({String.Join(", ", args)})"
        End Function

        ''' <summary>
        ''' 获取字段声明使用的类型名：标量走目标语言的类型映射，消息 / 枚举走已生成的类型名。
        ''' </summary>
        Protected Function GetFieldTypeName(kind As FieldTypeKind, resolvedFullName As String) As String
            If kind = FieldTypeKind.Message OrElse kind = FieldTypeKind.[Enum] Then
                Return GetTypeName(resolvedFullName)
            Else
                Return GetScalarType(kind)
            End If
        End Function

        ''' <summary>
        ''' 获取解析并展平后的 CLR 类型名。
        ''' </summary>
        Protected Function GetTypeName(fullName As String) As String
            Dim name As String = Nothing

            If fullName IsNot Nothing AndAlso _names IsNot Nothing AndAlso _names.TryGetValue(fullName, name) Then
                Return name
            End If

            If fullName Is Nothing Then
                Return "Object"
            End If

            ' 未能解析的类型退化为名字的最后一段
            Dim dot As Integer = fullName.LastIndexOf("."c)

            If dot >= 0 Then
                Return NamingHelper.ToPascalCase(fullName.Substring(dot + 1))
            Else
                Return NamingHelper.ToPascalCase(fullName)
            End If
        End Function

#End Region

#Region "NAME MAP"

        Private Sub BuildNameMap(file As ProtoFile)
            Dim names As New Dictionary(Of String, String)(StringComparer.Ordinal)
            Dim used As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

            For Each item As EnumDescriptor In file.AllEnums()
                names(item.FullName) = RegisterName(used, GetFlattenedName(item.Name, item.Parent))
            Next

            For Each message As MessageDescriptor In file.AllMessages()
                If message.MapEntry Then
                    Continue For
                End If

                names(message.FullName) = RegisterName(used, GetFlattenedName(message.Name, message.Parent))
            Next

            _names = names
        End Sub

        Private Shared Function GetFlattenedName(name As String, parent As MessageDescriptor) As String
            Dim parts As New List(Of String)

            Call parts.Add(NamingHelper.ToPascalCase(name))

            Dim current As MessageDescriptor = parent

            Do While current IsNot Nothing
                Call parts.Insert(0, NamingHelper.ToPascalCase(current.Name))
                current = current.Parent
            Loop

            Return String.Join("_", parts)
        End Function

        Private Shared Function RegisterName(used As HashSet(Of String), baseName As String) As String
            Dim candidate As String = baseName
            Dim index As Integer = 1

            Do While used.Contains(candidate)
                index += 1
                candidate = baseName & "_" & index.ToString(CultureInfo.InvariantCulture)
            Loop

            Call used.Add(candidate)

            Return candidate
        End Function

        Private Shared Sub CollectEnums(message As MessageDescriptor, buffer As List(Of EnumDescriptor))
            For Each item As EnumDescriptor In message.NestedEnums
                Call buffer.Add(item)
            Next

            For Each child As MessageDescriptor In message.NestedMessages
                Call CollectEnums(child, buffer)
            Next
        End Sub

        ''' <summary>
        ''' 把标量类型映射为 <see cref="Serialization.ProtoDataFormat"/> 的成员名；无需显式指定时返回 Nothing。
        ''' </summary>
        Protected Shared Function GetDataFormatName(kind As FieldTypeKind) As String
            Select Case kind
                Case FieldTypeKind.SInt32, FieldTypeKind.SInt64
                    Return "ZigZag"
                Case FieldTypeKind.Fixed32, FieldTypeKind.SFixed32
                    Return "Fixed32"
                Case FieldTypeKind.Fixed64, FieldTypeKind.SFixed64
                    Return "Fixed64"
                Case FieldTypeKind.Decimal128
                    Return "Decimal128"
                Case FieldTypeKind.DateTimeUnixMilliseconds
                    Return "DateTimeUnixMilliseconds"
                Case FieldTypeKind.DateTimeTicks
                    Return "DateTimeTicks"
                Case FieldTypeKind.Guid128
                    Return "Guid128"
                Case Else
                    Return Nothing
            End Select
        End Function

#End Region

    End Class

End Namespace
