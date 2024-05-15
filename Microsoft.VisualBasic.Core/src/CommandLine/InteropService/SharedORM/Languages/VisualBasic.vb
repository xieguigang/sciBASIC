#Region "Microsoft.VisualBasic::aeb358e8499bbd9140c6456c36fa2915, Microsoft.VisualBasic.Core\src\CommandLine\InteropService\SharedORM\Languages\VisualBasic.vb"

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

    '   Total Lines: 380
    '    Code Lines: 248
    ' Comment Lines: 72
    '   Blank Lines: 60
    '     File Size: 16.81 KB


    '     Class VisualBasic
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: addXmlComments, ArgumentXmlDocs, createCliCalls, GetSourceCode, normAsVisualBasicName
    '                   optionalDefaultValue, vbParameters
    ' 
    '         Sub: cliCallsInternal
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.SymbolBuilder.VBLanguage
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Xml

Namespace CommandLine.InteropService.SharedORM

    Public Class VisualBasic : Inherits CodeGenerator

        Dim namespace$

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub New(CLI As Type, namespace$)
            Me.New(New Interpreter(type:=CLI), [namespace])
        End Sub

        Sub New(App As Interpreter, Optional namespace$ = Nothing)
            Call MyBase.New(App)
            Me.namespace = [namespace] Or App.Type.Name.AsDefault
        End Sub

        Public Overrides Function GetSourceCode() As String
            Dim vb As New StringBuilder
            Dim className$ = normAsVisualBasicName(MyBase.exe)
            Dim rel$ = PathExtensions.RelativePath(App.Type.Assembly.Location.GetFullPath)
            Dim info$ = App.Type.NamespaceEntry.Description
            Dim appName$ = KeywordProcessor.AutoEscapeVBKeyword(className)

            Call vb.AppendLine("Imports System.Runtime.CompilerServices")
            Call vb.AppendLine("Imports " & GetType(StringBuilder).Namespace)
            Call vb.AppendLine("Imports " & GetType(IIORedirectAbstract).Namespace)
            Call vb.AppendLine("Imports " & GetType(InteropService).Namespace)
            Call vb.AppendLine("Imports Microsoft.VisualBasic.ApplicationServices")
            Call vb.AppendLine()
            Call vb.AppendLine("' Microsoft VisualBasic CommandLine Code AutoGenerator")
            Call vb.AppendLine("' assembly: " & rel)
            Call vb.AppendLine()
            Call vb.AppendLine(GetManualPage.LineTokens.Select(Function(l) "' " & l).JoinBy(vbCrLf))
            Call vb.AppendLine()
            Call vb.AppendLine("Namespace " & [namespace])
            Call vb.AppendLine()
            Call vb.AppendLine(addXmlComments(XmlEntity.EscapingXmlEntity(info)))
            Call vb.AppendLine($"Public Class {appName} : Inherits {GetType(InteropService).Name}")
            Call vb.AppendLine()
            Call vb.AppendLine($"    Public Const App$ = ""{exe}.exe""")
            Call vb.AppendLine()
            Call vb.AppendLine("    Sub New(App$)")
            Call vb.AppendLine($"        Call MyBase.New(app:=App$)")
            Call vb.AppendLine("    End Sub")
            Call vb.AppendLine("        
''' <summary>
''' Create an internal CLI pipeline invoker from a given environment path. 
''' </summary>
''' <param name=""directory"">A directory path that contains the target application</param>
''' <returns></returns>")
            Call vb.AppendLine("     <MethodImpl(MethodImplOptions.AggressiveInlining)>")
            Call vb.AppendLine($"    Public Shared Function FromEnvironment(directory As String) As {appName}")
            Call vb.AppendLine($"          Return New {appName}(App:=directory & ""/"" & {appName}.App)")
            Call vb.AppendLine("     End Function")

            For Each api As APITuple In Me.EnumeratesAPI
                Call cliCallsInternal(vb, api, incompatible:=Not InCompatibleAttribute.CLRProcessCompatible(api.API))
            Next

            Call vb.AppendLine("End Class")
            Call vb.AppendLine("End Namespace")

            Return vb.ToString
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="description$"></param>
        ''' <returns></returns>
        Private Shared Function addXmlComments(description$) As String
            If description.StringEmpty Then
                description = "'''"
            Else
                description = description _
                    .LineTokens _
                    .Select(Function(s) "''' " & s.Trim(" "c, ASCII.TAB)) _
                    .JoinBy(vbCrLf)
            End If

            Return $"
''' <summary>
{description}
''' </summary>
'''"
        End Function

        ''' <summary>
        ''' 生成一个命令行API的调用代码
        ''' </summary>
        ''' <param name="vb"></param>
        ''' <param name="cli"></param>
        ''' <remarks>
        ''' </remarks>
        Private Sub cliCallsInternal(vb As StringBuilder, cli As APITuple, incompatible As Boolean)

#Region "Code template"

            ' Public Function CommandName(args$,....Optional args$....) As Integer
            '     Dim CLI$ = "commandname arguments"
            '     Dim proc As IIORedirectAbstract = RunDotNetApp(CLI$)
            '
            '     Return proc.Run()
            ' End Function
#End Region

            Dim api As NamedValue(Of CommandLine) = cli.CLI
            ' 直接使用函数原型的名字了，会比较容易辨别一些
            Dim func$ = api.Name
            ' Xml comment 已经是经过转义了的，所以不需要再做xml entity的转义了
            Dim xmlComments$ = addXmlComments(api.Description)
            Dim params$()
            Dim usage As ArgumentAttribute() = cli.API _
                .GetCustomAttributes(True) _
                .Where(Function(a)
                           Return a.GetType Is GetType(ArgumentAttribute)
                       End Function) _
                .Select(Function(use) DirectCast(use, ArgumentAttribute)) _
                .ToArray

            Try
                If func.First <= "9" AndAlso func.First >= "0"c Then
                    func = "_" & func  ' 有些命令行开关是以数字开头的？
                Else
                    ' 不是以数字开头的，则尝试解决关键词的问题
                    func = KeywordProcessor.AutoEscapeVBKeyword(func)
                End If

                params = vbParameters(api.Value, argumentNames:=False)
            Catch ex As Exception
                ex = New Exception("Check for your CLI Usage definition: " & api.Value.ToString, ex)
                Throw ex
            End Try

            Call vb.AppendLine(xmlComments)
            Call vb.AppendLine(usage.DoCall(AddressOf ArgumentXmlDocs).JoinBy(vbCrLf))

            Dim deli$

            If params.Length > 4 Then
                deli = ", " & vbCrLf & New String(" "c, 20 + func.Length)
            Else
                deli = ", "
            End If

            Dim funcDeclare$ = $"Public Function {func}({params.JoinBy(deli)}) As Integer"
            Dim funcGetCommandLine$ = $"Public Function Get{func}CommandLine({params.JoinBy(deli)}{If(params.Length > 0, ", ", "")}Optional internal_pipelineMode As Boolean = True) As String"

            Call vb.AppendLine(funcDeclare)
            Call vb.AppendLine($"Dim cli = Get{func}CommandLine({vbParameters(api.Value, argumentNames:=True).Select(Function(aName) $"{aName}:={aName}").JoinBy(deli)}{If(params.Length > 0, ", ", "")}internal_pipelineMode:=True)")

            If incompatible Then
                ' 这个CLI是不兼容的方法
                Call vb.AppendLine($"    Dim proc As {NameOf(IIORedirectAbstract)} = {NameOf(InteropService.RunProgram)}(cli, Nothing)")
            Else
                ' 兼容的
                Call vb.AppendLine($"    Dim proc As {NameOf(IIORedirectAbstract)} = {NameOf(InteropService.RunDotNetApp)}(cli)")
            End If

            Call vb.AppendLine($"    Return proc.{NameOf(IIORedirectAbstract.Run)}()")
            Call vb.AppendLine("End Function")



            Call vb.AppendLine(funcGetCommandLine)
            Call vb.AppendLine($"    Dim CLI As New StringBuilder(""{api.Value.Name}"")")
            ' 插入命令名称和参数值之间的一个必须的空格
            Call vb.AppendLine("    Call CLI.Append("" "")")
            Call vb.AppendLine(createCliCalls(+api))
            Call vb.AppendLine()
            Call vb.AppendLine("Return CLI.ToString()")

            Call vb.AppendLine("End Function")
        End Sub

        Private Shared Function ArgumentXmlDocs(args As ArgumentAttribute()) As String()
            Dim out As New List(Of String)
            Dim param$
            Dim comments$

            For Each arg As ArgumentAttribute In args
                comments = XmlEntity.EscapingXmlEntity(arg.Description) _
                    .Replace("\n", vbCrLf) _
                    .LineTokens _
                    .Select(Function(l) "''' " & l) _
                    .JoinBy(vbCrLf) _
                    .Trim("'"c)
                param = $"''' <param name=""{VisualBasic.normAsVisualBasicName(arg.Name)}"">{comments}
''' </param>"
                out += param
            Next

            Return out
        End Function

        ''' <summary>
        ''' 在这个函数之中会生成函数的参数列表
        ''' </summary>
        ''' <param name="API"></param>
        ''' <returns></returns>
        Private Shared Function vbParameters(API As CommandLine, argumentNames As Boolean) As String()
            Dim out As New List(Of String)
            Dim param$

            If API.arguments = 1 AndAlso API.arguments(Scan0).Name.StringEmpty Then
                ' /command <term>
                out += If(argumentNames, "term", $"term As String")
            Else
                For Each arg As NamedValue(Of String) In API.ParameterList
                    param = VisualBasic.normAsVisualBasicName(arg.Name)

                    If Not argumentNames Then
                        param = $"{param} As String"

                        If Not arg.Description.StringEmpty Then
                            ' 可选参数
                            param = $"Optional {param} = ""{optionalDefaultValue(arg.Value)}"""
                        End If
                    End If

                    out += param
                Next
            End If

            For Each bool In API.BoolFlags
                param = VisualBasic.normAsVisualBasicName(bool)

                If argumentNames Then
                    out += param
                Else
                    out += $"Optional {param} As Boolean = False"
                End If
            Next

            Return out
        End Function

        ''' <summary>
        ''' 必须是以``default=``来作为前缀的，否则默认使用空字符串
        ''' </summary>
        ''' <param name="value$"></param>
        ''' <returns></returns>
        Private Shared Function optionalDefaultValue(value$) As String
            If value.First = """"c AndAlso value.Last = """"c Then
                ' 如果是直接使用双引号包裹而不是使用<>尖括号进行包裹，则认为双引号所包裹的值都是默认值
                value = value.GetStackValue(ASCII.Quot, ASCII.Quot)
            ElseIf value.First = "<"c AndAlso value.Last = ">"c Then
                ' 而如果是使用尖括号的时候，则判断是否存在default=表达式，不存在则是空值
                value = value.GetStackValue("<", ">")

                If InStr(value, "default=") > 0 Then
                    value = Strings.Split(value, "default=").Last.Trim(""""c)
                Else
                    ' 没有表达式前缀，则使用默认的空字符串
                    value = ""
                End If
            Else
                ' 其他情况都认为是使用空值为默认值
                value = ""
            End If

            value = value.Replace(""""c, New String(ASCII.Quot, 2))

            Return value
        End Function

        ''' <summary>
        ''' 创建命令行调用字符串
        ''' </summary>
        ''' <param name="api"></param>
        ''' <returns></returns>
        Private Shared Function createCliCalls(api As CommandLine) As String
            Dim CLI As New StringBuilder
            Dim vbcode$
            ' 可能在参数中存在/@set环境参数的设置的调用
            Dim envir As String = Nothing

            If api.arguments = 1 AndAlso api.arguments(Scan0).Name.StringEmpty Then
                ' /command <term>
                vbcode = "    Call CLI.Append($""{term}"")"
                CLI.AppendLine(vbcode)
            Else
                For Each param As NamedValue(Of String) In api.ParameterList
                    Dim var$ = normAsVisualBasicName(param.Name)

                    If param.Name.TextEquals("/@set") Then
                        envir = param.Name
                        Continue For
                    End If

                    ' 注意：在这句代码的最后有一个空格，是间隔参数所必需的，不可以删除
                    vbcode = $"    Call CLI.Append(""{param.Name} "" & """""""" & {var} & """""" "")"

                    If param.Description.StringEmpty Then
                        ' 必须参数不需要进一步判断，直接添加                    
                        Call CLI.AppendLine(vbcode)
                    Else
                        ' 可选参数还需要IF判断是否存在                  
                        Call CLI.AppendLine($"    If Not {var}.{NameOf(StringEmpty)} Then")
                        Call CLI.AppendLine("        " & vbcode)
                        Call CLI.AppendLine("    End If")
                    End If
                Next
            End If

            For Each b In api.BoolFlags
                Dim var$ = normAsVisualBasicName(b)

                Call CLI.AppendLine($"    If {var} Then")
                ' 逻辑参数后面有一个空格，是正确的生成CLI所必需的
                Call CLI.AppendLine($"        Call CLI.Append(""{b} "")")
                Call CLI.AppendLine("    End If")
            Next

            ' 在最后将/@set环境参数组装起来
            If Not envir.StringEmpty Then
                envir = normAsVisualBasicName(envir)

                Call CLI.AppendLine($"    If Not {envir}.{NameOf(StringEmpty)} Then")
                Call CLI.AppendLine($"     Call CLI.Append($""/@set """"""""{Microsoft.VisualBasic.App.FlagInternalPipeline}={{internal_pipelineMode.ToString.ToUpper()}};'{{{envir}}}'"""""""" "")")
                Call CLI.AppendLine("Else")
                ' 没有需要组装的,直接添加调用
                Call CLI.AppendLine($"     Call CLI.Append($""/@set {Microsoft.VisualBasic.App.FlagInternalPipeline}={{internal_pipelineMode.ToString.ToUpper()}} "")")
                Call CLI.AppendLine("    End If")
            Else
                ' 没有需要组装的,直接添加调用
                Call CLI.AppendLine($"     Call CLI.Append($""/@set {Microsoft.VisualBasic.App.FlagInternalPipeline}={{internal_pipelineMode.ToString.ToUpper()}} "")")
            End If

            Return CLI.ToString
        End Function

        Const SyntaxError$ = "'<' or '>' is using for the IO redirect in your terminal, unavailable for your commandline argument name!"

        ''' <summary>
        ''' 将命令行参数的名称转义为VB之中有效的对象标识符
        ''' </summary>
        ''' <param name="arg$"></param>
        ''' <returns></returns>
        Private Shared Function normAsVisualBasicName(arg$) As String
            ' 在命令行的参数名称前面一般都会有/-之类的控制符前缀，在这里去掉
            Dim name$ = arg.Trim("/"c, "\"c, "-"c)
            Dim s As Char() = name.ToArray
            Dim upper As Char() = name.ToUpper.ToArray
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
                ' 可能会存在in, byref, class这类的名字，需要在这里转义一下
                Return KeywordProcessor.AutoEscapeVBKeyword(New String(s))
            End If
        End Function
    End Class
End Namespace
