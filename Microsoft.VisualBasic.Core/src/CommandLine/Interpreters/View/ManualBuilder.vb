#Region "Microsoft.VisualBasic::90ddbd25ce395f3f951d74cf387aade1, Microsoft.VisualBasic.Core\src\CommandLine\Interpreters\View\ManualBuilder.vb"

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

    '   Total Lines: 388
    '    Code Lines: 282 (72.68%)
    ' Comment Lines: 40 (10.31%)
    '    - Xml Docs: 30.00%
    ' 
    '   Blank Lines: 66 (17.01%)
    '     File Size: 15.95 KB


    '     Module ManualBuilder
    ' 
    '         Function: APIPrototype, ExampleValue, GetFileExtensions, PrintHelp
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.ApplicationServices.Terminal
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.CommandLine.Reflection.EntryPoints
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text

Namespace CommandLine.ManView

    ''' <summary>
    ''' 用来生成帮助信息
    ''' </summary>
    Module ManualBuilder

        ''' <summary>
        ''' Prints the formatted help information on the console.
        ''' (用于生成打印在终端上面的命令行帮助信息)
        ''' </summary>
        ''' <param name="api"></param>
        ''' <returns></returns>
        <Extension> Public Function PrintHelp(api As APIEntryPoint) As Integer
            ' 因为在编写帮助信息的时候可能会有多行字符串，则在vb源代码里面会出现前导的空格，
            ' 所以在这里需要将每一行的前导空格删除掉， 否则会破坏掉输出的文本对齐格式。
            Dim infoLines = api.Info _
                .LineTokens _
                .Select(Function(s) s.Trim(" "c, ASCII.TAB)) _
                .JoinBy(vbCrLf) _
                .SplitParagraph(90) _
                .ToArray
            Dim blank$

            If infoLines.IsNullOrEmpty Then
                infoLines = {"Description not available..."}
            End If

            ' print API name and description
            Call Console.WriteLine()
            Call Console.Write("   ")
            Call My.Log4VB.Println($"'{api.Name}' - {infoLines.FirstOrDefault}", ConsoleColor.Yellow, ConsoleColor.DarkBlue)
            Call VBDebugger.WaitOutput()

            If infoLines.Length > 1 Then
                blank = New String(
                    " ",
                    3 + ' 三个前导空格
                    2 + ' 两个命令行名称左右的单引号
                    3 + ' 空格-空格
                    api.Name.Length)

                For Each line$ In infoLines.Skip(1)
                    Call Console.WriteLine($"{blank}{line}")
                Next
            End If

            ' print usage
            With Console.ForegroundColor

                Call Console.WriteLine()
                Call Console.WriteLine($"Usage:")
                Call Console.WriteLine()

                Dim AppPath$

                If App.Platform = PlatformID.Unix OrElse App.Platform = PlatformID.MacOSX Then
                    AppPath = App.ExecutablePath.TrimSuffix
                Else
                    AppPath = App.ExecutablePath
                End If

                Call Console.Write("  ")
                Call My.Log4VB.Print(AppPath, ConsoleColor.DarkCyan)
                Call Console.Write(" ")
                Call My.Log4VB.Println(api.Usage, ConsoleColor.Green)

            End With

            Dim outputMarks = api.EntryPoint _
                .GetCustomAttributes(True) _
                .Where(Function(a) a.GetType Is GetType(OutputAttribute)) _
                .Select(Function(out) DirectCast(out, OutputAttribute)) _
                .ToArray

            If Not outputMarks.IsNullOrEmpty Then
                Call Console.WriteLine()
                Call Console.WriteLine("  This command produce these data files:")
                Call Console.WriteLine("  ====================================================")
                Call Console.WriteLine()

                Call outputMarks _
                    .Select(Function(o)
                                Dim desc = o.extension.GetMIMEDescrib
                                Return {desc.FileExt, desc.MIMEType, desc.Name, o.result.FullName}
                            End Function) _
                    .PrintTable(App.StdOut)
            End If

            If Not api.Arguments.IsNullOrEmpty Then
                Call Console.WriteLine()
                Call Console.WriteLine("  Command with arguments:")
                Call Console.WriteLine("  ====================================================")
                Call Console.WriteLine()

                ' 先计算出可能的最长的前导字符串的组合
                Dim maxPrefix% = -999
                Dim s$
                Dim std_in As Boolean = False
                Dim std_out As Boolean = False
                Dim bool As Boolean = False
                Dim haveOptional As Boolean = False
                Dim boolSeperator As Boolean = False
                Dim stringL$()

                ' 先输出必须的参数
                ' 之后为可选参数，但是可选参数会分为下面的顺序输出
                ' 1. 文件
                ' 2. 字符串
                ' 3. 数值
                ' 4. 整数
                ' 5. 逻辑值
                stringL = api.Arguments _
                    .Select(Function(arg)
                                With arg.Value

                                    If .TokenType = CLITypes.Boolean Then
                                        ' 逻辑值类型的只能够是可选类型
                                        ' 逻辑开关不计算在内
                                        ' s = "(optional) (boolean)"
                                        s = ""
                                        bool = True
                                    Else

                                        If .Pipeline = PipelineTypes.std_in Then
                                            s = "(*std_in)"
                                            std_in = True
                                        ElseIf .Pipeline = PipelineTypes.std_out Then
                                            s = "(*std_out)"
                                            std_out = True
                                        Else
                                            s = ""
                                        End If

                                        If .Optional Then
                                            s &= "(optional)"
                                            haveOptional = True
                                        End If
                                    End If
                                End With

                                Return s
                            End Function) _
                    .ToArray

                ' println("\n%s", stringL.MaxLengthString)

                ' 计算出诸如像(optional) (*std_in) (*std_out) (optional) (boolean)这类开关类型前导的
                ' 最大长度
                maxPrefix = stringL.MaxLengthString.Length

                ' 这里计算出来的是name usage的最大长度
                stringL$ = api.Arguments _
                    .Select(Function(x) x.Value.Example) _
                    .ToArray

                ' println("\n%s", stringL.MaxLengthString)

                Dim l%
                Dim maxLen% = stringL _
                    .MaxLengthString _
                    .Length

                ' Call stringL.MaxLengthString.debug

                ' 加上开关名字的最大长度就是前面的开关说明部分的最大字符串长度
                ' 后面的description帮助信息的偏移量都是依据这个值计算出来的
                Dim helpOffset% = maxPrefix + maxLen
                Dim skipOptionalLine As Boolean = False

                ' 必须的参数放在前面，可选的参数都是在后面的位置
                For Each param As ArgumentAttribute In api.Arguments.Select(Function(a) a.Value)
                    If param.TokenType = CLITypes.Boolean AndAlso Not boolSeperator Then
                        boolSeperator = True

                        Call Console.WriteLine()
                        Call Console.WriteLine("  Options:")
                        Call Console.WriteLine()
                    End If

                    ' Dim l% 这个参数就是当前的这个命令的前半部的标识符部分的字符串长度
                    ' helpOffset%的值减去当前的长度l，即可得到当前的命令的help info的
                    ' 偏移量

                    If param.[Optional] Then
                        Dim fore = Console.ForegroundColor

                        If Not skipOptionalLine Then
                            skipOptionalLine = True
                            Call Console.WriteLine()
                        End If

                        Call Console.Write("  (")
                        Console.ForegroundColor = ConsoleColor.Green
                        Call Console.Write("optional")
                        Console.ForegroundColor = fore
                        Call Console.Write(") ")

                        s = param.Example
                    Else
                        s = param.Example
                        Console.Write("  ")
                    End If

                    l = s.Length

                    With param
                        If .Pipeline = PipelineTypes.std_in Then
                            s = "(*std_in)  " & s
                        ElseIf .Pipeline = PipelineTypes.std_out Then
                            s = "(*std_out) " & s
                        ElseIf .TokenType = CLITypes.Boolean Then
                            s = "(boolean)  " & s
                        Else
                            If Not .Optional Then
                                s = New String(" "c, maxPrefix + 1) & s
                            End If
                        End If
                    End With

                    Call Console.Write(s)

                    If param.TokenType = CLITypes.Boolean Then
                        l += 11
                    ElseIf param.Pipeline = PipelineTypes.std_out Then
                        l += 11
                    End If

                    ' 这里的blank调整的是命令开关名称与描述之间的字符间距
                    l = helpOffset - l + 2
                    blank = If(l > 0, New String(" "c, l), "  ")
                    infoLines$ = param.Description _
                        .LineTokens _
                        .Select(Function(str) str.Trim(" "c, ASCII.TAB)) _
                        .JoinBy(vbCrLf) _
                        .SplitParagraph(120) _
                        .ToArray

                    Call Console.Write(blank)
                    Call Console.WriteLine($"{infoLines.FirstOrDefault}")

                    If infoLines.Length > 1 Then
                        Dim d% = 0

                        If param.Optional Then
                            d = 13
                        End If

                        blank = New String(" "c, helpOffset + d + 2)

                        For Each line In infoLines.Skip(1)
                            Call Console.WriteLine(blank & line)
                        Next
                    End If
                Next

                If std_in OrElse std_out OrElse bool Then
                    Call Console.WriteLine()
                    Call Console.WriteLine()
                    Call Console.WriteLine()
                    Call Console.WriteLine("  [Annotations]")
                    Call Console.WriteLine("  " & New String("-"c, 52))
                    Call Console.WriteLine()
                End If

                If std_in Then
                    If std_out Then
                        Call Console.WriteLine("  *std_in:  " & PipelineTypes.std_in.Description)
                    Else
                        Call Console.WriteLine("  *std_in: " & PipelineTypes.std_in.Description)
                    End If
                End If
                If std_out Then
                    Call Console.WriteLine("  *std_out: " & PipelineTypes.std_out.Description)
                End If

                Dim allExts = api.Arguments _
                    .Select(Function(arg) arg.Value.GetFileExtensions) _
                    .IteratesALL _
                    .Distinct _
                    .OrderBy(Function(ext) ext) _
                    .ToArray

                If allExts.Length > 0 Then
                    Call Console.WriteLine()

                    Dim allContentTypes = allExts _
                        .Select(Function(ext)
                                    Return New With {
                                        Key .ext = ext,
                                            .type = ext.GetMIMEDescrib
                                    }
                                End Function) _
                        .ToArray
                    Dim table$()() = allContentTypes _
                        .Select(Function(content)
                                    With content.Type
                                        Return {"  " & content.ext & "  ", $"({ .MIMEType})  ", .Name}
                                    End With
                                End Function) _
                        .ToArray

                    Call table.Print(New StreamWriter(Console.OpenStandardOutput))
                End If

                If bool Then
                    Call Console.WriteLine()
                    Call Console.WriteLine("  " & boolFlag)
                End If
            End If

            Return 0
        End Function

        <Extension>
        Public Function GetFileExtensions(arg As ArgumentAttribute) As String()
            If arg.TokenType = CLITypes.File AndAlso Not arg.Extensions.StringEmpty Then
                Dim extensions$() = arg _
                    .Extensions _
                    .Split(","c) _
                    .Select(AddressOf Trim) _
                    .Select(Function(s)
                                If InStr(s, "*.") = 1 Then
                                    Return s
                                Else
                                    Return $"*.{s}"
                                End If
                            End Function) _
                    .ToArray

                Return extensions
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' (boolean flag does not require of argument value)
        ''' </summary>
        Public Const boolFlag$ = "(boolean flag does not require of argument value)"

        <Extension>
        Public Function ExampleValue(arg As ArgumentAttribute) As String
            Dim example$

            Select Case arg.TokenType

                Case CLITypes.Double
                    example = "<float>"
                Case CLITypes.Integer
                    example = "<int32>"
                Case CLITypes.String
                    example = "<term_string>"
                Case CLITypes.File

                    With arg.GetFileExtensions
                        If .IsNullOrEmpty Then
                            example = "<file/directory>"
                        Else
                            example = $"<file, { .JoinBy(", ")}>"
                        End If
                    End With

                Case Else
                    example = "unknown"
            End Select

            Return example
        End Function

        Const CLI$ = "(Microsoft.VisualBasic.CommandLine.CommandLine)"
        Const VBStyle_CLI = "(args As Microsoft.VisualBasic.CommandLine.CommandLine)"

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function APIPrototype(declare$) As String
            Return [declare].Replace(CLI, VBStyle_CLI)
        End Function
    End Module
End Namespace
