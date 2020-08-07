#Region "Microsoft.VisualBasic::3d2e2ee35c9e5aa0eed2f3faae608746, Microsoft.VisualBasic.Core\CommandLine\CLITools.vb"

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

    '     Module CLITools
    ' 
    '         Function: Args, checkKeyDuplicated, CreateObject, CreateParameterValues, Equals
    '                   GetFileList, GetLogicalFlags, GetTokens, IsPossibleLogicFlag, Join
    '                   makesureQuot, Print, ShellExec, SingleValueOrStdIn, TrimParamPrefix
    '                   (+3 Overloads) TryParse
    ' 
    '         Sub: AppSummary, tupleParser
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.ApplicationServices.Terminal
Imports Microsoft.VisualBasic.CommandLine.ManView
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text
Imports StringList = System.Collections.Generic.IEnumerable(Of String)
Imports ValueTuple = System.Collections.Generic.KeyValuePair(Of String, String)

Namespace CommandLine

    ''' <summary>
    ''' CLI parser and <see cref="CommandLine"/> object creates.
    ''' </summary>
    <Package("CommandLine",
                        Url:="http://gcmodeller.org",
                        Publisher:="xie.guigang@gcmodeller.org",
                        Description:="",
                        Revision:=52)>
    Public Module CLITools

        ''' <summary>
        ''' 在命令行之中使用逗号作为分隔符分隔多个文件
        ''' </summary>
        ''' <param name="input"></param>
        ''' <returns></returns>
        Public Function GetFileList(input As String) As IEnumerable(Of String)
            If input.FileExists Then
                Return {input}
            Else
                Return input.Split(","c)
            End If
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="assem"></param>
        ''' <param name="description">命令行的使用功能描述信息文本</param>
        ''' <param name="SYNOPSIS">命令行的使用语法</param>
        ''' <param name="write"></param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Sub AppSummary(assem As AssemblyInfo, description$, SYNOPSIS$, write As TextWriter)
            Call SDKManual.AppSummary(assem, description, SYNOPSIS, write)
        End Sub

        <Extension>
        Public Function ShellExec(cli As IIORedirectAbstract) As String
            Call cli.Run()
            Return cli.StandardOutput
        End Function

        <Extension>
        Public Function Print(args As CommandLine, Optional sep As Char = " "c, Optional leftMargin% = 0) As String
            Dim sb As New StringBuilder("ArgumentsOf: `" & args.Name & "`")
            Dim device As New StringWriter(sb)

            Call device.WriteLine()
            Call device.WriteLine(New String("-"c, args.Name.Length * 4))
            Call device.WriteLine()

            Call args _
                .ToArgumentVector _
                .Print(
                    device,
                    sep,
                    trilinearTable:=True,
                    leftMargin:=leftMargin)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' Parsing parameters from a specific tokens.
        ''' (从给定的词组之中解析出参数的结构)
        ''' </summary>
        ''' <param name="tokens">个数为偶数的，但是假若含有开关的时候，则可能为奇数了</param>
        ''' <param name="includeLogicals">返回来的列表之中是否包含有逻辑开关</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Extension>
        Public Function CreateParameterValues(tokens$(), includeLogicals As Boolean, Optional note$ = Nothing) As List(Of NamedValue(Of String))
            Dim list As New List(Of NamedValue(Of String))
            Dim key As String

            If tokens.IsNullOrEmpty Then
                Return list
            ElseIf tokens.Length = 1 Then
                If IsPossibleLogicFlag(tokens(Scan0)) AndAlso includeLogicals Then
                    list += New NamedValue(Of String) With {
                        .Name = tokens(Scan0),
                        .Value = CStr(True),
                        .Description = note
                    }
                Else
                    Return list
                End If
            End If

            ' 下面都是多余或者等于两个元素的情况
            ' 数目多于一个的
            For i As Integer = 0 To tokens.Length - 1
                Dim [next] As Integer = i + 1

                If [next] = tokens.Length Then
                    ' 这个元素是开关，已经到达最后则没有了，跳出循环
                    If IsPossibleLogicFlag(tokens(i)) AndAlso includeLogicals Then
                        list += New NamedValue(Of String)(tokens(i), True, note)
                    End If

                    Exit For
                End If

                Dim s As String = tokens([next])

                ' 当前的这个元素是开关，下一个也是开关开头，则本元素肯定是一个开关
                If tokens(i).ToLower <> "/@set" AndAlso IsPossibleLogicFlag(s) Then
                    If includeLogicals Then
                        list += New NamedValue(Of String)(tokens(i), True, note)
                    End If

                    Continue For
                Else
                    ' 下一个元素不是开关，则当前元素为一个参数名，则跳过下一个元素
                    key = tokens(i).ToLower
                    list += New NamedValue(Of String)(key, s, note)

                    i += 1
                End If
            Next

            Return list
        End Function

        ''' <summary>
        ''' Get all of the logical parameters from the input tokens.
        ''' (这个函数所生成的逻辑参数的名称全部都是小写形式的)
        ''' </summary>
        ''' <param name="args">要求第一个对象不能够是命令的名称</param>
        ''' <returns></returns>
        <Extension>
        Public Function GetLogicalFlags(args As IEnumerable(Of String), ByRef singleValue$) As String()
            Dim tokens$() = args.SafeQuery.ToArray

            If tokens.IsNullOrEmpty Then
                Return New String() {}
            ElseIf tokens.Length = 1 Then
                ' 只有一个元素，则肯定为开关
                Return {tokens(0).ToLower}
            End If

            Dim tkList As New List(Of String)

            For i As Integer = 0 To tokens.Length - 1 '数目多于一个的
                Dim next% = i + 1

                If [next] = tokens.Length Then
                    If IsPossibleLogicFlag(obj:=tokens(i)) Then
                        tkList += tokens(i)  '
                    End If

                    Exit For
                End If

                Dim s As String = tokens([next])

                If IsPossibleLogicFlag(obj:=s) Then  '当前的这个元素是开关，下一个也是开关开头，则本元素肯定是一个开关
                    If IsPossibleLogicFlag(obj:=tokens(i)) Then
                        tkList += tokens(i)
                    Else

                        If i = 0 Then
                            singleValue = tokens(i)
                        End If

                    End If
                Else  '下一个元素不是开关，则当前元素为一个参数名，则跳过下一个元素
                    i += 1
                End If

            Next

            Return (From s As String In tkList Select s.ToLower).ToArray
        End Function

        ''' <summary>
        ''' Try parsing the cli command string from the string value.(尝试着从文本行之中解析出命令行参数信息)
        ''' </summary>
        ''' <param name="args">The commandline arguments which is user inputs from the terminal.</param>
        ''' <param name="duplicatedAllows">Allow the duplicated command parameter argument name in the input, 
        ''' default is not allowed the duplication.(是否允许有重复名称的参数名出现，默认是不允许的)</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <ExportAPI("TryParse")>
        <Extension>
        Public Function TryParse(args As StringList,
                                 Optional duplicatedAllows As Boolean = False,
                                 Optional rawInput$ = Nothing) As CommandLine

            Dim tokens$() = args.SafeQuery.ToArray
            Dim singleValue$ = ""

            If tokens.Length = 0 Then
                Return New CommandLine
            Else
                tokens = tokens.fixWindowsNetworkDirectory.ToArray
            End If

            Dim bools$() = tokens _
                .Skip(1) _
                .GetLogicalFlags(singleValue)
            Dim cli As New CommandLine With {
                .Name = tokens(Scan0),
                .Tokens = tokens,
                .BoolFlags = bools,
                .cliCommandArgvs = Join(tokens)
            }

            cli.SingleValue = singleValue
            cli.cliCommandArgvs = rawInput

            If cli.Parameters.Length = 1 AndAlso
                String.IsNullOrEmpty(cli.SingleValue) Then

                cli.SingleValue = cli.Parameters(0)
            End If

            If tokens.Length > 1 Then
                cli.arguments = tokens.Skip(1).ToArray.CreateParameterValues(False)

                Dim Dk As String() = checkKeyDuplicated(cli.arguments)

                If Not duplicatedAllows AndAlso Not Dk.IsNullOrEmpty Then
                    Dim Key$ = String.Join(", ", Dk)
                    Dim msg$ = String.Format(KeyDuplicated, Key, String.Join(" ", tokens.Skip(1).ToArray))

                    Throw New Exception(msg)
                End If
            End If

            Return cli
        End Function

        Const KeyDuplicated As String = "The command line switch key ""{0}"" Is already been added! Here Is your input data:  CMD {1}."

        Private Function checkKeyDuplicated(source As IEnumerable(Of NamedValue(Of String))) As String()
            Dim LQuery = (From param As NamedValue(Of String)
                          In source
                          Select param.Name.ToLower
                          Group By ToLower Into Group).ToArray

            Return LinqAPI.Exec(Of String) _
 _
                () <= From group
                      In LQuery
                      Where group.Group.Count > 1
                      Select group.ToLower
        End Function

        ''' <summary>
        ''' Gets the commandline object for the current program.
        ''' </summary>
        ''' <returns></returns>
        <ExportAPI("args")>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Args() As CommandLine
            Return App.CommandLine
        End Function

        ''' <summary>
        ''' Try parsing the cli command string from the string value.
        ''' (尝试着从文本行之中解析出命令行参数信息，假若value里面有空格，则必须要将value添加双引号)
        ''' </summary>
        ''' <param name="CLI">The commandline arguments which is user inputs from the terminal.</param>
        ''' <param name="duplicateAllowed">Allow the duplicated command parameter argument name in the input, 
        ''' default is not allowed the duplication.(是否允许有重复名称的参数名出现，默认是不允许的)</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("TryParse")>
        Public Function TryParse(<Parameter("CLI", "The CLI arguments that inputs from the console by user.")> CLI$,
                                 <Parameter("Duplicates.Allowed")>
                                 Optional duplicateAllowed As Boolean = False) As CommandLine

            If String.IsNullOrEmpty(CLI) Then
                Return New CommandLine
            Else
#Const DEBUG = False
#If DEBUG Then
                Call CLI.__DEBUG_ECHO
#End If
            End If

            Dim args As CommandLine = CLITools _
                .GetTokens(CLI) _
                .TryParse(duplicateAllowed, rawInput:=CLI)

            Return args
        End Function

        ''' <summary>
        ''' Is this string tokens is a possible <see cref="Boolean"/> value flag
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        <ExportAPI("IsPossibleBoolFlag?")>
        Public Function IsPossibleLogicFlag(obj As String) As Boolean
            If obj.Contains(" ") Then
                Return False
            ElseIf IsNumeric(obj) Then
                Return False
            End If

            ' Linux上面全路径总是从/，即根目录开始的
            If obj.Count("/"c) > 1 Then
                Return False
            End If

            Return obj.StartsWith("-") OrElse obj.StartsWith("/")
        End Function

        ''' <summary>
        ''' ReGenerate the cli command line argument string text.(重新生成命令行字符串)
        ''' </summary>
        ''' <param name="tokens">
        ''' If the token value have a space character, then this function 
        ''' will be wrap that token with quot character automatically.
        ''' </param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("Join")>
        Public Function Join(tokens As IEnumerable(Of String)) As String
            If tokens Is Nothing Then
                Return ""
            Else
                Return tokens _
                    .Select(AddressOf makesureQuot) _
                    .JoinBy(" ")
            End If
        End Function

        Private Function makesureQuot(token As String) As String
            If InStr(token, " ") > 0 Then
                If token.First = """"c AndAlso token.Last = """"c Then
                    Return token
                Else
                    Return $"""{token}"""
                End If
            Else
                Return token
            End If
        End Function

        ''' <summary>
        ''' A regex expression string that use for split the commandline text.
        ''' (用于分析命令行字符串的正则表达式)
        ''' </summary>
        ''' <remarks></remarks>
        Public Const SPLIT_REGX_EXPRESSION As String = "\s+(?=(?:[^""]|""[^""]*"")*$)"

        ''' <summary>
        ''' Try parse the argument tokens which comes from the user input commandline string. 
        ''' (尝试从用户输入的命令行字符串之中解析出所有的参数)
        ''' </summary>
        ''' <param name="CLI"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("Tokens")>
        Public Function GetTokens(CLI As String) As String()
            If String.IsNullOrEmpty(CLI) Then
                Return New String() {""}
            Else
                CLI = CLI.Trim
            End If

            ' 由于在前面App位置已经将应用程序的路径去除了，所以这里已经不需要了，只需要直接解析即可

            'If Not Environment.OSVersion.Platform = PlatformID.Win32NT Then
            '    'LINUX下面的命令行会将程序集的完整路径也传递进来
            '    Dim l As Integer = Len(Application.ExecutablePath)
            '    CLI = Mid(CLI, l + 2).Trim

            '    If String.IsNullOrEmpty(CLI) Then  '在linux下面没有传递进来任何参数，则返回空集合
            '        Return New String() {""}
            '    End If
            'End If

            Dim tokens$() = Regex.Split(CLI, SPLIT_REGX_EXPRESSION)
            Dim argv As New List(Of String)

            tokens = tokens _
                .TakeWhile(Function(Token)
                               Return Not String.IsNullOrEmpty(Token.Trim)
                           End Function) _
                .ToArray

            For i As Integer = 0 To tokens.Length - 1
                Dim s As String = tokens(i)

                ' 消除单词单元中的双引号
                If s.First = ASCII.Quot AndAlso s.Last = ASCII.Quot Then
                    s = Mid(s, 2, Len(s) - 2)
                End If

                ' argv='dddddd'
                ' 键值对语法
                If s.Contains("="c) AndAlso Not s.IsURLPattern Then
                    If i > 0 AndAlso tokens(i - 1).TextEquals("/@set") Then
                        ' 在这里是环境变量，不需要进行解析
                        argv += s
                    Else
                        Call s.tupleParser(argv)
                    End If
                Else
                    argv += s
                End If
            Next

            Return argv
        End Function

        ''' <summary>
        ''' 只取第一个=符号出现的位置，结果会被添加进入<paramref name="argv"/>列表之中
        ''' </summary>
        ''' <param name="s$"></param>
        ''' <param name="argv"></param>
        <Extension>
        Private Sub tupleParser(s$, ByRef argv As List(Of String))
            Dim splitIndex% = -1

            For j As Integer = 0 To s.Length - 1
                Dim c = s(j)

                If c = "="c AndAlso splitIndex = -1 Then
                    ' 如果前一个字符是\转义，则不是键值对
                    If j > 1 AndAlso s(j - 1) <> "\"c Then
                        ' 这是第一个符号
                        splitIndex = j
                        Exit For
                    Else
                        splitIndex = -1
                    End If
                End If
            Next

            If splitIndex > -1 Then
                Dim name$ = s.Substring(0, splitIndex)
                Dim value = s.Substring(splitIndex + 1)

                argv += name
                argv += value
            Else
                argv += s
            End If
        End Sub

        ''' <summary>
        ''' 会对%进行替换的
        ''' </summary>
        Const TokenSplitRegex As String = "(?=(?:[^%]|%[^%]*%)*$)"

        ''' <summary>
        ''' 尝试从输入的语句之中解析出词法单元，注意，这个函数不是处理从操作系统所传递进入的命令行语句
        ''' </summary>
        ''' <param name="CommandLine"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("TryParse")>
        Public Function TryParse(CommandLine As String, TokenDelimited As String, InnerDelimited As Char) As String()
            If String.IsNullOrEmpty(CommandLine) Then
                Return New String() {""}
            End If

            Dim regxPattern$ = TokenDelimited & TokenSplitRegex.Replace("%"c, InnerDelimited)
            Dim tokens = Regex.Split(CommandLine, regxPattern)

            For i As Integer = 0 To tokens.Length - 1
                Dim s As String = tokens(i)

                ' 消除单词单元中的双引号
                If s.First = InnerDelimited AndAlso s.Last = InnerDelimited Then
                    tokens(i) = Mid(s, 2, Len(s) - 2)
                End If
            Next

            Return tokens
        End Function

        ''' <summary>
        ''' Creates command line object from a set obj <see cref="KeyValuePair(Of String, String)"/>
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="args"></param>
        ''' <param name="bFlags"></param>
        ''' <returns></returns>
        <ExportAPI("CreateObject")>
        Public Function CreateObject(name$, args As IEnumerable(Of ValueTuple), Optional bFlags As IEnumerable(Of String) = Nothing) As CommandLine
            Dim parameters As New List(Of NamedValue(Of String))
            Dim tokens As New List(Of String) From {name}

            For Each tuple As ValueTuple In args
                Dim key As String = tuple.Key.ToLower
                Dim param As New NamedValue(Of String)(key, tuple.Value)

                Call parameters.Add(param)
                Call tokens.AddRange(New String() {key, tuple.Value})
            Next

            Return New CommandLine With {
                .Name = name,
                .arguments = parameters,
                .Tokens = tokens.Join(bFlags).ToArray,
                .BoolFlags = bFlags.SafeQuery.ToArray
            }
        End Function

        ''' <summary>
        ''' Trim the CLI argument name its prefix symbols.
        ''' (修剪命令行参数名称的前置符号)
        ''' </summary>
        ''' <param name="argName"></param>
        ''' <returns></returns>
        <ExportAPI("Trim.Prefix.BoolFlag")>
        <Extension>
        Public Function TrimParamPrefix(argName$) As String
            If argName.StartsWith("--") Then
                Return Mid(argName, 3)
            ElseIf argName.StartsWith("-") OrElse argName.StartsWith("\") OrElse argName.StartsWith("/") Then
                Return Mid(argName, 2)
            Else
                Return argName
            End If
        End Function

        ''' <summary>
        ''' 请注意，这个是有方向性的，由于是依照参数1来进行比较的，
        ''' 假若args2里面的参数要多于第一个参数，但是第一个参数里
        ''' 面的所有参数值都可以被参数2完全比对得上的话，就认为二
        ''' 者是相等的
        ''' </summary>
        ''' <param name="args1"></param>
        ''' <param name="args2"></param>
        ''' <returns></returns>
        ''' 
        <ExportAPI("CLI.Equals")>
        Public Function Equals(args1 As CommandLine, args2 As CommandLine) As Boolean
            If Not String.Equals(args1.Name, args2.Name, StringComparison.OrdinalIgnoreCase) Then
                Return False
            End If

            For Each bFlag As String In args1.BoolFlags
                If Not args2.GetBoolean(bFlag) Then
                    Return False
                End If
            Next

            For Each arg As NamedValue(Of String) In args1.arguments
                Dim value2 As String = args2(arg.Name)

                If Not String.Equals(value2, arg.Value, StringComparison.OrdinalIgnoreCase) Then
                    Return False
                End If
            Next

            Return True
        End Function

        <Extension>
        Public Function SingleValueOrStdIn(args As CommandLine) As String
            If Not String.IsNullOrEmpty(args.SingleValue) Then
                Return args.SingleValue
            Else
                Dim reader As New StreamReader(Console.OpenStandardInput)
                Return reader.ReadToEnd
            End If
        End Function

        ''' <summary>
        ''' Gets the brief summary information of current cli command line object.
        ''' (获取当前的命令行对象的参数摘要信息)
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Extension>
        Public Function GetCommandsOverview(cli As CommandLine) As String
            Dim sb As New StringBuilder(vbCrLf, 1024)

            Call sb.AppendLine($"Commandline arguments overviews{vbCrLf}Command Name  --  ""{cli.Name}""")
            Call sb.AppendLine()
            Call sb.AppendLine("---------------------------------------------------------")
            Call sb.AppendLine()

            If cli.arguments.Count = 0 Then
                Return sb.AppendLine("No parameter was define in this commandline.").ToString
            End If

            Dim maxLenParameterName As Integer = Aggregate item As NamedValue(Of String)
                                                 In cli.arguments
                                                 Let str_len As Integer = Len(item.Name)
                                                 Into Max(str_len)

            For Each parameter As NamedValue(Of String) In cli.arguments
                Call sb.AppendLine($"  {parameter.Name}  {New String(" "c, maxLenParameterName - Len(parameter.Name))}= ""{parameter.Value}"";")
            Next

            Return sb.ToString
        End Function
    End Module
End Namespace
