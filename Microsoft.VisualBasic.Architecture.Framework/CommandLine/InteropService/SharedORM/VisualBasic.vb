Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.SymbolBuilder
Imports Microsoft.VisualBasic.Text

Namespace CommandLine.InteropService.SharedORM

    Public Class VisualBasic : Inherits CodeGenerator

        Dim namespace$

        Public Sub New(CLI As Type, namespace$)
            MyBase.New(CLI)
            Me.namespace = [namespace]
        End Sub

        Public Overrides Function GetSourceCode() As String
            Dim vb As New StringBuilder

            Call vb.AppendLine("Imports " & GetType(StringBuilder).Namespace)
            Call vb.AppendLine("Imports " & GetType(IIORedirectAbstract).Namespace)
            Call vb.AppendLine("Imports " & GetType(InteropService).Namespace)
            Call vb.AppendLine()
            Call vb.AppendLine("' Microsoft VisualBasic CommandLine Code AutoGenerator")
            Call vb.AppendLine("' assembly: " & App.Type.Assembly.Location.GetFullPath)
            Call vb.AppendLine()
            Call vb.AppendLine("Namespace " & [namespace])
            Call vb.AppendLine()
            Call vb.AppendLine(__xmlComments(App.Type.NamespaceEntry.Description))
            Call vb.AppendLine($"Public Class {MyBase.exe} : Inherits {GetType(InteropService).Name}")
            Call vb.AppendLine()
            Call vb.AppendLine()
            Call vb.AppendLine("Sub New(App$)")
            Call vb.AppendLine($"MyBase.{NameOf(InteropService._executableAssembly)} = App$")
            Call vb.AppendLine("End Sub")

            For Each API In Me.EnumeratesAPI
                Call __calls(vb, API)
            Next

            Call vb.AppendLine("End Class")
            Call vb.AppendLine("End Namespace")

            Return vb.ToString
        End Function

        Private Shared Function __xmlComments(description$) As String
            If description.StringEmpty Then
                description = "'''"
            Else
                description = description _
                    .lTokens _
                    .Select(Function(s) "'''" & s) _
                    .JoinBy(vbCrLf)
            End If

            Return $"
''' <summary>
{description}
''' </summary>
'''"
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="vb"></param>
        ''' <param name="API"></param>
        Private Sub __calls(vb As StringBuilder, API As NamedValue(Of CommandLine))
            ' Public Function CommandName(args$,....Optional args$....) As Integer
            ' Dim CLI$ = "commandname arguments"
            ' Dim proc As IIORedirectAbstract = RunDotNetApp(CLI$)
            ' Return proc.Run()
            ' End Function
            Dim func$ = __normalizedAsIdentifier(API.Name).Trim("_"c)
            Dim xmlComments$ = __xmlComments(API.Description)
            Dim params$()

            Try
                If func.First <= "9" AndAlso func.First >= "0"c Then
                    func = "_" & func  ' 有些命令行开关是以数字开头的？
                Else
                    ' 不是以数字开头的，则尝试解决关键词的问题
                    func = VBLanguage.AutoEscapeVBKeyword(func)
                End If
                params = __vbParameters(API.Value)
            Catch ex As Exception
                ex = New Exception("Check for your CLI Usage definition: " & API.Value.ToString, ex)
                Throw ex
            End Try

            Call vb.AppendLine(xmlComments)
            Call vb.AppendLine($"Public Function {func}({params.JoinBy(", ")}) As Integer")
            Call vb.AppendLine($"Dim CLI As New StringBuilder(""{API.Name}"")")
            Call vb.AppendLine(__CLI(+API))
            Call vb.AppendLine()
            Call vb.AppendLine($"Dim proc As {NameOf(IIORedirectAbstract)} = {NameOf(InteropService.RunDotNetApp)}(CLI.ToString())")
            Call vb.AppendLine($"Return proc.{NameOf(IIORedirectAbstract.Run)}()")
            Call vb.AppendLine("End Function")
        End Sub


        ''' <summary>
        ''' 在这个函数之中会生成函数的参数列表
        ''' </summary>
        ''' <param name="API"></param>
        ''' <returns></returns>
        Private Shared Function __vbParameters(API As CommandLine) As String()
            Dim out As New List(Of String)
            Dim param$

            For Each arg As NamedValue(Of String) In API.ParameterList
                param = $"{VisualBasic.__normalizedAsIdentifier(arg.Name)} As String"

                If Not arg.Description.StringEmpty Then
                    ' 可选参数
                    param = $"Optional {param} = ""{__defaultValue(arg.Value)}"""
                End If

                out += param
            Next
            For Each bool In API.BoolFlags
                out += $"Optional {VisualBasic.__normalizedAsIdentifier(bool)} As Boolean = False"
            Next

            Return out
        End Function

        ''' <summary>
        ''' 必须是以``default=``来作为前缀的，否则默认使用空字符串
        ''' </summary>
        ''' <param name="value$"></param>
        ''' <returns></returns>
        Private Shared Function __defaultValue(value$) As String
            If value.First = """"c AndAlso value.Last = """"c Then
                ' 如果是直接使用双引号包裹而不是使用<>尖括号进行包裹，则认为双引号所包裹的值都是默认值
                value = value.GetStackValue(ASCII.Quot, ASCII.Quot)
            ElseIf value.First = "<"c AndAlso value.Last = ">"c Then
                ' 而如果是使用尖括号的时候，则判断是否存在default=表达式，不存在则是空值
                value = value.GetStackValue("<", ">")

                If InStr(value, "default=") > 0 Then
                    value = Strings.Split(value, "default=").Last.Trim(""""c)
                Else
                    value = "" ' 没有表达式前缀，则使用默认的空字符串
                End If
            Else
                ' 其他情况都认为是使用空值为默认值
                value = ""
            End If

            value = value.Replace(""""c, New String(ASCII.Quot, 2))

            Return value
        End Function

        Private Shared Function __CLI(API As CommandLine) As String
            Dim CLI As New StringBuilder
            Dim vbcode$

            For Each param In API.ParameterList
                Dim var$ = __normalizedAsIdentifier(param.Name)

                ' 注意：在这句代码的最后有一个空格，是间隔参数所必需的，不可以删除
                vbcode = $"Call CLI.Append(""{param.Name} "" & """""""" & {var} & """""" "")"

                If param.Description.StringEmpty Then
                    ' 必须参数不需要进一步判断，直接添加                    
                    Call CLI.AppendLine(vbcode)
                Else
                    ' 可选参数还需要IF判断是否存在                  
                    Call CLI.AppendLine($"If Not {var}.{NameOf(StringEmpty)} Then")
                    Call CLI.AppendLine(vbcode)
                    Call CLI.AppendLine("End If")
                End If
            Next

            For Each b In API.BoolFlags
                Dim var$ = __normalizedAsIdentifier(b)

                Call CLI.AppendLine($"If {var} Then")
                Call CLI.AppendLine($"Call CLI.Append(""{b} "")") ' 逻辑参数后面有一个空格，是正确的生成CLI所必需的
                Call CLI.AppendLine("End If")
            Next

            Return CLI.ToString
        End Function

        Const SyntaxError$ = "'<' or '>' is using for the IO redirect in your terminal, unavailable for your commandline argument name!"

        Private Shared Function __normalizedAsIdentifier(arg$) As String
            Dim s As Char() = arg.ToArray
            Dim upper As Char() = arg.ToUpper.ToArray
            Dim c As Char

            If s.First = "<"c OrElse s.Last = ">"c Then
                Throw New SyntaxErrorException(SyntaxError)
            End If

            For i As Integer = 0 To s.Length - 1
                c = upper(i)

                If (c >= "A"c AndAlso c <= "Z"c) OrElse (c >= "0"c AndAlso c <= "9"c) OrElse (c = "_") Then
                    ' 合法的字符，不做处理
                Else
                    ' 非法字符串都被替换为下划线
                    s(i) = "_"c
                End If
            Next

            If s.First >= "0"c AndAlso s.First <= "9"c Then
                Return "_" & New String(s)
            Else
                Return New String(s)
            End If
        End Function
    End Class
End Namespace