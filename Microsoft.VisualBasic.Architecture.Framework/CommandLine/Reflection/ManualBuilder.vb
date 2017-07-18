#Region "Microsoft.VisualBasic::6965f40f5bab7ad1a40af76e9c7b146c, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\CommandLine\Reflection\ManualBuilder.vb"

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
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection.EntryPoints
Imports Microsoft.VisualBasic.Text

Namespace CommandLine.Reflection

    ''' <summary>
    ''' 用来生成帮助信息
    ''' </summary>
    Module ManualBuilder

        ''' <summary>
        ''' Prints the formatted help information on the console.
        ''' </summary>
        ''' <param name="api"></param>
        ''' <returns></returns>
        <Extension>
        Public Function PrintHelp(api As APIEntryPoint) As Integer
            Dim infoLines = Paragraph.Split(api.Info, 90).ToArray

            Call Console.WriteLine($"Help for command '{api.Name}':")
            Call Console.WriteLine()
            Call Console.WriteLine($"  Information:  {infoLines.FirstOrDefault}")

            If infoLines.Length > 1 Then
                For Each line$ In infoLines.Skip(1)
                    Call Console.WriteLine($"                {line}")
                Next
            End If

            Call Console.Write($"  Usage:        ")

            Dim fore As ConsoleColor = Console.ForegroundColor

            Console.ForegroundColor = ConsoleColor.Cyan
            Call Console.Write(
                If(App.Platform = PlatformID.Unix OrElse
                App.Platform = PlatformID.MacOSX,
                App.ExecutablePath.TrimSuffix,
                App.ExecutablePath) & " ")
            Console.ForegroundColor = ConsoleColor.Green
            Call Console.WriteLine(api.Usage)
            Console.ForegroundColor = fore

            If String.IsNullOrEmpty(api.Example) Then
                Call Console.WriteLine($"  Example:      CLI usage example not found!")
            Else
                Call Console.WriteLine($"  Example:      {App.AssemblyName} {api.Example}")
            End If

            If Not api.Arguments.IsNullOrEmpty Then
                Call Console.WriteLine()
                Call Console.WriteLine("  Arguments:")
                Call Console.WriteLine("  ============================")
                Call Console.WriteLine()

                Dim maxLen As Integer = (From x In api.Arguments Select x.Name.Length + 2).Max
                Dim l As Integer

                For Each param As Argument In api.Arguments.Select(Function(x) x.Value)
                    fore = Console.ForegroundColor

                    If param.[Optional] Then
                        Call Console.Write("  [")
                        Console.ForegroundColor = ConsoleColor.Green
                        Call Console.Write(param.Name)
                        Console.ForegroundColor = fore
                        Call Console.Write("]")
                        l = param.Name.Length
                    Else
                        Call Console.Write("   " & param.Name)
                        l = param.Name.Length - 1
                    End If

                    Dim blank As String = New String(" "c, maxLen - l + 1)
                    Dim descriptLines = Paragraph.Split(param.Description, 120).ToArray

                    Call Console.Write(blank)
                    Call Console.WriteLine($"Description:  {descriptLines.FirstOrDefault}")

                    If descriptLines.Length > 1 Then
                        blank = New String(" "c, maxLen + 11)

                        For Each line In descriptLines.Skip(1)
                            Call Console.WriteLine(blank & "        " & line)
                        Next
                    End If

                    blank = New String(" "c, maxLen + 5)

                    ' Call Console.WriteLine()
                    Call Console.Write(blank)

                    blank = blank & "              "

                    If param.TokenType = CLITypes.Boolean Then
                        Call Console.WriteLine($"Example:      {param.Name}")
                        Call Console.Write(blank)
                        Call Console.WriteLine(boolFlag)
                    Else
                        Dim example$ = param.ExampleValue
                        Call Console.WriteLine($"Example:      {param.Name} {example}")
                        If param.Pipeline <> PipelineTypes.undefined Then
                            Call Console.WriteLine(blank & param.Pipeline.Description)
                        End If
                    End If

                    Call Console.WriteLine()
                Next
            End If

            Return 0
        End Function

        ''' <summary>
        ''' (bool flag does not require of argument value)
        ''' </summary>
        Public Const boolFlag$ = "(bool flag does not require of argument value)"

        <Extension>
        Public Function ExampleValue(arg As Argument) As String
            Dim example$ = arg.Example

            If String.IsNullOrEmpty(example) Then
                Select Case arg.TokenType
                    Case CLITypes.Double
                        example = "<float>"
                    Case CLITypes.Integer
                        example = "<int32>"
                    Case CLITypes.String
                        example = "<term_string>"
                    Case CLITypes.File
                        example = "<file/directory>"
                End Select
            Else
                example = example.CLIToken
            End If

            Return example
        End Function

        Const CLI$ = "(Microsoft.VisualBasic.CommandLine.CommandLine)"
        Const VBStyle_CLI = "(args As Microsoft.VisualBasic.CommandLine.CommandLine)"

        Public Function APIPrototype(declare$) As String
            Return [declare].Replace(CLI, VBStyle_CLI)
        End Function
    End Module
End Namespace
