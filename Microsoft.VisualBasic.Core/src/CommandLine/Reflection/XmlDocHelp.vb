Imports System
Imports System.Collections.Concurrent
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Reflection
Imports System.Text.RegularExpressions
Imports System.Threading
Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Assembly

Namespace CommandLine.Reflection

    ''' <summary>
    ''' The xml documentation of one cli command method.
    ''' </summary>
    ''' <remarks>
    ''' The text values are normalized when this object is created, so that
    ''' the help renderer does not have to deal with xml or markdown tags.
    ''' </remarks>
    Public NotInheritable Class XmlMethodDoc

        ReadOnly parameterDocs As Dictionary(Of String, String)
        ReadOnly summaryText As String
        ReadOnly noDocument As Boolean

        ''' <summary>
        ''' The &lt;summary&gt; text of the target method, an empty string when no
        ''' xml documentation is available.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Summary As String
            Get
                Return summaryText
            End Get
        End Property

        ''' <summary>
        ''' The &lt;param&gt; documents indexed by the method parameter name.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Parameters As IReadOnlyDictionary(Of String, String)
            Get
                Return parameterDocs
            End Get
        End Property

        ''' <summary>
        ''' No summary and no parameter documents were found for this method.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsEmpty As Boolean
            Get
                Return noDocument
            End Get
        End Property

        ''' <summary>
        ''' A shared empty document, used as the fallback value when the xml
        ''' documentation file is missing, stale or has no matching member.
        ''' </summary>
        Public Shared ReadOnly Empty As New XmlMethodDoc()

        Friend Sub New(Optional summary As String = Nothing,
                       Optional parameters As Dictionary(Of String, String) = Nothing)

            summaryText = If(summary, "")
            parameterDocs = If(parameters, New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase))
            noDocument = summaryText.Length = 0 AndAlso parameterDocs.Count = 0
        End Sub

        ''' <summary>
        ''' Gets the &lt;param&gt; document of a specific method parameter.
        ''' </summary>
        ''' <param name="name">The method parameter name.</param>
        ''' <returns>The normalized document text, or an empty string.</returns>
        Public Function ParameterDocument(name As String) As String
            If String.IsNullOrEmpty(name) OrElse parameterDocs.Count = 0 Then
                Return ""
            End If

            Dim text As String = Nothing

            If parameterDocs.TryGetValue(name, text) Then
                Return text
            Else
                Return ""
            End If
        End Function
    End Class

    ''' <summary>
    ''' Bridges the .net assembly xml documentation files into the commandline
    ''' help model: &lt;summary&gt; maps to the command description and
    ''' &lt;param&gt; maps to the argument description.
    ''' </summary>
    ''' <remarks>
    ''' All of the methods in this module are exception safe: ``Interpreter.getAPI``
    ''' drops a command when its API entry point throws, so a missing or broken
    ''' xml documentation file must never break the command loading process.
    ''' </remarks>
    Public Module XmlDocHelp

        ''' <summary>
        ''' one parsed xml documentation file per assembly location (a Nothing value
        ''' is cached as well, so that a missing file is checked only once).
        ''' </summary>
        ReadOnly docCache As New ConcurrentDictionary(Of String, Lazy(Of Dictionary(Of String, ProjectMember)))(StringComparer.OrdinalIgnoreCase)

        ''' <summary>
        ''' Loads the xml documentation of the given cli command method.
        ''' </summary>
        ''' <param name="method">The exported commandline API method.</param>
        ''' <returns>Never returns Nothing; <see cref="XmlMethodDoc.Empty"/> on any failure.</returns>
        Public Function ForMethod(method As MethodInfo) As XmlMethodDoc
            Try
                If method Is Nothing OrElse method.DeclaringType Is Nothing Then
                    Return XmlMethodDoc.Empty
                End If

                Dim table As Dictionary(Of String, ProjectMember) = GetDocumentTable(method.DeclaringType.Assembly.Location)

                If table Is Nothing OrElse table.Count = 0 Then
                    Return XmlMethodDoc.Empty
                End If

                Dim key As String = DocKey(method)
                Dim member As ProjectMember = Nothing

                If Not table.TryGetValue(key, member) Then
                    member = FindByShortName(table, method)
                End If

                If member Is Nothing Then
                    Return XmlMethodDoc.Empty
                End If

                Return New XmlMethodDoc(
                    summary:=NormalizeText(member.Summary),
                    parameters:=ParameterDocuments(member, method))
            Catch ex As Exception
                ' 命令行解释器会丢弃装载过程中抛异常的命令，这里必须静默降级
                Call System.Diagnostics.Debug.WriteLine($"XmlDocHelp: {ex.GetType.Name} while reading the cli method xml documentation.")
                Return XmlMethodDoc.Empty
            End Try
        End Function

        ''' <summary>
        ''' Maps a CLR parameter type onto the <see cref="CLITypes"/> token type.
        ''' </summary>
        ''' <param name="parameterType">The type of a <see cref="ParameterInfo"/>.</param>
        ''' <returns></returns>
        Public Function ToCLITypes(parameterType As Type) As CLITypes
            If parameterType Is Nothing Then
                Return CLITypes.String
            End If

            Dim type As Type = Nullable.GetUnderlyingType(parameterType)

            If type Is Nothing Then
                type = parameterType
            End If
            If type.IsByRef Then
                type = type.GetElementType()
            End If
            If type Is Nothing Then
                Return CLITypes.String
            End If

            Select Case System.Type.GetTypeCode(type)
                Case TypeCode.Boolean
                    Return CLITypes.Boolean
                Case TypeCode.Byte, TypeCode.SByte, TypeCode.Int16, TypeCode.UInt16,
                     TypeCode.Int32, TypeCode.UInt32, TypeCode.Int64, TypeCode.UInt64
                    Return CLITypes.Integer
                Case TypeCode.Single, TypeCode.Double, TypeCode.Decimal
                    Return CLITypes.Double
                Case Else
                    Return CLITypes.String
            End Select
        End Function

        ''' <summary>
        ''' The commandline object parameter is injected by <see cref="EntryPoints.APIDelegate"/>
        ''' and is not a real commandline switch, so it should not show up in the help.
        ''' </summary>
        ''' <param name="parameter"></param>
        ''' <returns></returns>
        Public Function IsInjectedParameter(parameter As ParameterInfo) As Boolean
            Return parameter Is Nothing OrElse parameter.ParameterType Is GetType(CommandLine)
        End Function

        ''' <summary>
        ''' locates the xml documentation file next to the assembly and builds
        ''' a lookup table keyed by the xml document member identity.
        ''' </summary>
        Private Function GetDocumentTable(assemblyLocation As String) As Dictionary(Of String, ProjectMember)
            If String.IsNullOrEmpty(assemblyLocation) Then
                Return Nothing
            End If

            Dim xmlFile As String

            Try
                xmlFile = Path.ChangeExtension(assemblyLocation, ".xml")
            Catch ex As Exception
                Return Nothing
            End Try

            Dim factory As Func(Of String, Lazy(Of Dictionary(Of String, ProjectMember))) =
                Function(path As String) New Lazy(Of Dictionary(Of String, ProjectMember))(
                    Function() LoadDocumentTable(path),
                    LazyThreadSafetyMode.ExecutionAndPublication)

            Return docCache.GetOrAdd(xmlFile, factory).Value
        End Function

        Private Function LoadDocumentTable(xmlFile As String) As Dictionary(Of String, ProjectMember)
            Try
                If Not File.Exists(xmlFile) Then
                    Return Nothing
                End If

                Dim project As Project = ProjectSpace.CreateDocProject(New FileInfo(xmlFile))

                If project Is Nothing Then
                    Return Nothing
                End If

                Dim table As New Dictionary(Of String, ProjectMember)(StringComparer.OrdinalIgnoreCase)

                For Each ns As ProjectNamespace In project.Namespaces
                    For Each declaredType As ProjectType In ns.Types
                        For Each member As ProjectMember In declaredType.AllMethods
                            ' Declare 保留了去掉 "M:" 之后的完整签名，可以区分重载
                            If Not String.IsNullOrEmpty(member.[Declare]) Then
                                table("M:" & member.[Declare]) = member
                            End If
                        Next
                    Next
                Next

                Return table
            Catch ex As Exception
                Call System.Diagnostics.Debug.WriteLine($"XmlDocHelp: {ex.GetType.Name} while parsing '{xmlFile}'.")
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Builds the xml document member identity of a method, for example
        ''' ``M:test.Program.test_arguments(System.Int32,System.Int32,System.Double)``.
        ''' </summary>
        Private Function DocKey(method As MethodInfo) As String
            Dim typeName As String = DeclaringTypeName(method.DeclaringType)
            Dim methodName As String = method.Name

            If method.IsGenericMethod Then
                methodName &= "``" & method.GetGenericArguments().Length
            End If

            Dim parameters As ParameterInfo() = method.GetParameters()

            If parameters.Length = 0 Then
                Return "M:" & typeName & "." & methodName
            End If

            Dim signatures As New List(Of String)

            For Each parameter As ParameterInfo In parameters
                Call signatures.Add(ParameterTypeName(parameter.ParameterType))
            Next

            Return "M:" & typeName & "." & methodName & "(" & String.Join(",", signatures) & ")"
        End Function

        Private Function DeclaringTypeName(type As Type) As String
            ' 反射用 + 表示嵌套类型，xml document id 用 . 表示
            Return If(type.FullName, type.Name).Replace("+"c, "."c)
        End Function

        Private Function ParameterTypeName(type As Type) As String
            If type Is Nothing Then
                Return ""
            End If

            If type.IsByRef Then
                Return ParameterTypeName(type.GetElementType()) & "@"
            End If
            If type.IsPointer Then
                Return ParameterTypeName(type.GetElementType()) & "*"
            End If
            If type.IsArray Then
                Dim rank As Integer = type.GetArrayRank()
                Dim suffix As String

                If rank = 1 Then
                    suffix = "[]"
                Else
                    suffix = "[" & String.Join(",", Enumerable.Repeat("0:", rank)) & "]"
                End If

                Return ParameterTypeName(type.GetElementType()) & suffix
            End If
            If type.IsGenericParameter Then
                If type.DeclaringMethod Is Nothing Then
                    Return "`" & type.GenericParameterPosition
                Else
                    Return "``" & type.GenericParameterPosition
                End If
            End If
            If type.IsGenericType Then
                Dim definition As Type = type.GetGenericTypeDefinition()
                Dim name As String = If(definition.FullName, definition.Name)

                ' 泛型实参写作 {T1,T2}，类型定义上的 `1 元数后缀不属于 xml doc id
                name = Regex.Replace(name, "`\d+", "").Replace("+"c, "."c)

                Dim typeArgs As New List(Of String)

                For Each arg As Type In type.GetGenericArguments()
                    Call typeArgs.Add(ParameterTypeName(arg))
                Next

                Return name & "{" & String.Join(",", typeArgs) & "}"
            End If

            Return If(type.FullName, type.Name).Replace("+"c, "."c)
        End Function

        ''' <summary>
        ''' Fallback for the docKey formats that can not be reproduced from
        ''' reflection (generic/ByRef/nested signatures): match the type + method
        ''' name and the parameter count, but only when the result is unambiguous.
        ''' </summary>
        Private Function FindByShortName(table As Dictionary(Of String, ProjectMember), method As MethodInfo) As ProjectMember
            Dim prefix As String = "M:" & DeclaringTypeName(method.DeclaringType) & "." & method.Name
            Dim count As Integer = method.GetParameters().Length
            Dim found As ProjectMember = Nothing
            Dim matches As Integer = 0

            For Each entry In table
                If entry.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) AndAlso
                   ParameterCount(entry.Key) = count Then

                    found = entry.Value
                    matches += 1

                    If matches > 1 Then
                        Return Nothing
                    End If
                End If
            Next

            Return found
        End Function

        Private Function ParameterCount(docKey As String) As Integer
            Dim left As Integer = docKey.IndexOf("("c)
            Dim right As Integer = docKey.LastIndexOf(")"c)

            If left < 0 OrElse right <= left Then
                Return 0
            End If

            Dim body As String = docKey.Substring(left + 1, right - left - 1)

            If String.IsNullOrWhiteSpace(body) Then
                Return 0
            End If

            Dim depth As Integer = 0
            Dim count As Integer = 1

            For Each ch As Char In body
                If ch = "{"c OrElse ch = "["c OrElse ch = "("c Then
                    depth += 1
                ElseIf ch = "}"c OrElse ch = "]"c OrElse ch = ")"c Then
                    depth -= 1
                ElseIf ch = ","c AndAlso depth = 0 Then
                    count += 1
                End If
            Next

            Return count
        End Function

        Private Function ParameterDocuments(member As ProjectMember, method As MethodInfo) As Dictionary(Of String, String)
            Dim docs As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)

            For Each parameter As ParameterInfo In method.GetParameters()
                ' 空的 <param> 会被 XmlDoc 解析器填成 "-" 哨兵值，需要过滤掉
                Dim text As String = NormalizeText(member.GetParameterDocument(parameter.Name))

                If text.Length > 0 Then
                    docs(parameter.Name) = text
                End If
            Next

            Return docs
        End Function

        ''' <summary>
        ''' Flattens the xml documentation text into one line and removes the
        ''' markdown cref links produced by ``TrimAssemblyDoc``.
        ''' </summary>
        Private Function NormalizeText(text As String) As String
            If String.IsNullOrWhiteSpace(text) Then
                Return ""
            End If

            text = Regex.Replace(text, "\[([^\]]+)\]\(cref:[^)]*\)", "$1")
            text = text.Replace(vbCr, " ").Replace(vbLf, " ").Replace(vbTab, " ")
            text = Regex.Replace(text, "\s{2,}", " ").Trim()

            If text = "-" Then
                Return ""
            End If

            Return text
        End Function
    End Module
End Namespace
