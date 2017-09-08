#Region "Microsoft.VisualBasic::4c2852cc3f71b1c83f99798ca35a85c4, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\CommandLine\Reflection\ManualBuilder.vb"

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
Imports Microsoft.VisualBasic.CommandLine.Reflection.EntryPoints
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
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
            Call Console.WriteLine($"  '{api.Name}' - {infoLines.FirstOrDefault}")

            If infoLines.Length > 1 Then
                blank = New String(
                    " ",
                    2 + ' 两个前导空格
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
                Call Console.WriteLine($" Usage:")
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
                Call Console.WriteLine("  Arguments:")
                Call Console.WriteLine("  ============================")
                Call Console.WriteLine()

                Dim maxLen% = Aggregate x As NamedValue(Of Argument)
                              In api.Arguments
                              Let isOptional As String = If(x.Value.Optional, "(optional) ", "")
                              Let stringL = (isOptional & x.Name & " " & x.Value.Example)
                              Into Max(stringL.Length)
                Dim l%
                Dim s$
                Dim std_in As Boolean = False
                Dim std_out As Boolean = False
                Dim bool As Boolean = False

                For Each param As Argument In api.Arguments.Select(Function(x) x.Value)
                    If param.[Optional] Then
                        Dim fore = Console.ForegroundColor

                        Call Console.Write("  (")
                        Console.ForegroundColor = ConsoleColor.Green
                        Call Console.Write("optional")
                        Console.ForegroundColor = fore
                        Call Console.Write(") ")
                        Call Console.Write(param.Example)
                        l = ("(optional) " & param.Example).Length
                    Else
                        s = param.Example
                        Call Console.Write("   " & s)
                        l = s.Length - 1
                    End If

                    If param.TokenType = CLITypes.Boolean Then
                        bool = True
                    End If
                    If param.Pipeline = PipelineTypes.std_in Then
                        std_in = True
                    End If
                    If param.Pipeline = PipelineTypes.std_out Then
                        std_out = True
                    End If

                    ' 这里的blank调整的是命令开关名称与描述之间的字符间距
                    blank = New String(" "c, maxLen - l - 3)
                    infoLines$ = Paragraph.Split(param.Description, 120).ToArray

                    Call Console.Write(blank)
                    Call Console.WriteLine($"{infoLines.FirstOrDefault}")

                    If infoLines.Length > 1 Then
                        blank = New String(" "c, maxLen - 1)

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
                If bool Then
                    Call Console.WriteLine()
                    Call Console.WriteLine("  " & boolFlag)
                End If
            End If

            Return 0
        End Function

        ''' <summary>
        ''' (bool flag does not require of argument value)
        ''' </summary>
        Public Const boolFlag$ = "(bool flag does not require of argument value)"

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
                    example = "<file/directory>"
                Case Else
                    example = "unknown"
            End Select

            Return example
        End Function

        Const CLI$ = "(Microsoft.VisualBasic.CommandLine.CommandLine)"
        Const VBStyle_CLI = "(args As Microsoft.VisualBasic.CommandLine.CommandLine)"

        Public Function APIPrototype(declare$) As String
            Return [declare].Replace(CLI, VBStyle_CLI)
        End Function
    End Module
End Namespace
