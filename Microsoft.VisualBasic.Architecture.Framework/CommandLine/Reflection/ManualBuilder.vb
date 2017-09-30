#Region "Microsoft.VisualBasic::fb59cfa14345c14325eb5a03ff6ce746, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\CommandLine\Reflection\ManualBuilder.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2016 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.CommandLine.Reflection.EntryPoints
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Net.Protocols.ContentTypes
Imports Microsoft.VisualBasic.Text

Namespace CommandLine.Reflection

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
            Dim infoLines = Paragraph.Split(api.Info, 90).ToArray
            Dim blank$

            ' print API name and description
            Call Console.WriteLine()
            Call Console.WriteLine($"   '{api.Name}' - {infoLines.FirstOrDefault}")

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

                Console.ForegroundColor = ConsoleColor.Cyan

                Call Console.Write("  ")
                Call Console.Write(
                    If(App.Platform = PlatformID.Unix OrElse
                    App.Platform = PlatformID.MacOSX,
                    App.ExecutablePath.TrimSuffix,
                    App.ExecutablePath) & " ")

                Console.ForegroundColor = ConsoleColor.Green
                Call Console.WriteLine(api.Usage)
                Console.ForegroundColor = .ref

            End With

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

                ' 先输出必须的参数
                ' 之后为可选参数，但是可选参数会分为下面的顺序输出
                ' 1. 文件
                ' 2. 字符串
                ' 3. 数值
                ' 4. 整数
                ' 5. 逻辑值
                For Each arg In api.Arguments
                    With arg.Value

                        If .TokenType = CLITypes.Boolean Then
                            ' 逻辑值类型的只能够是可选类型
                            s = "(optional) (boolean)"
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

                    If s.Length > maxPrefix Then
                        maxPrefix = s.Length
                    End If
                Next

                ' 这里计算出来的是name usage的最大长度
                Dim maxLen% = Aggregate x As NamedValue(Of Argument)
                              In api.Arguments
                              Let stringL = x.Value.Example.Length
                              Into Max(stringL)
                Dim l%
                Dim helpOffset% = maxPrefix + maxLen
                Dim skipOptionalLine As Boolean = False

                ' 必须的参数放在前面，可选的参数都是在后面的位置
                For Each param As Argument In api.Arguments.Select(Function(x) x.Value)
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
                        l = "(optional) ".Length + s.Length
                    Else
                        s = param.Example
                        s = s
                        l = s.Length

                        Console.Write("  ")
                    End If

                    With param
                        If .Pipeline = PipelineTypes.std_in Then
                            s = "(*std_in)  " & s
                        ElseIf .Pipeline = PipelineTypes.std_out Then
                            s = "(*std_out) " & s
                        ElseIf .TokenType = CLITypes.Boolean Then
                            s = "(boolean)  " & s
                        End If

                        If Not .Pipeline = PipelineTypes.undefined OrElse .TokenType = CLITypes.Boolean Then
                            l += 11
                        End If
                    End With

                    Call Console.Write(s)

                    ' 这里的blank调整的是命令开关名称与描述之间的字符间距
                    blank = New String(" "c, helpOffset - l)
                    infoLines$ = Paragraph _
                        .Split(param.Description, 120) _
                        .ToArray

                    Call Console.Write(blank)
                    Call Console.WriteLine($"{infoLines.FirstOrDefault}")

                    If infoLines.Length > 1 Then
                        blank = New String(" "c, helpOffset + 2)

                        For Each line In infoLines.Skip(1)
                            Call Console.WriteLine(blank & line)
                        Next
                    End If
                Next

                If std_in OrElse std_out OrElse bool Then
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
                    .ToArray

                If allExts.Length > 0 Then
                    Call Console.WriteLine()

                    For Each ext As String In allExts
                        With ext.GetMIMEDescrib
                            Call Console.WriteLine($"  {ext}{vbTab}{vbTab}({ .MIMEType}) { .Name}")
                        End With
                    Next
                End If

                If bool Then
                    Call Console.WriteLine()
                    Call Console.WriteLine("  " & boolFlag)
                End If
            End If

            Return 0
        End Function

        <Extension>
        Public Function GetFileExtensions(arg As Argument) As String()
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
        Public Function ExampleValue(arg As Argument) As String
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
