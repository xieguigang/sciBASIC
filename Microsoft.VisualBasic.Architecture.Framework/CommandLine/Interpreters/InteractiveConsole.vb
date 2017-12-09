#Region "Microsoft.VisualBasic::5ea45779bc526ad1eead690838d308fe, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\CommandLine\Interpreters\ActivityInst.vb"

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

Imports System.Reflection
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.UnixBash

Namespace CommandLine

    ''' <summary>
    ''' interactive console in ``/i`` mode, example:
    ''' 
    ''' ```bash
    ''' # This command will makes your CLI program enter 
    ''' # the interactive console mode.
    ''' # 
    ''' # type quit() for exit.
    ''' #
    ''' App /i
    ''' ```
    ''' </summary>
    ''' <remarks></remarks>
    Public NotInheritable Class InteractiveConsole : Inherits Interpreter

        ReadOnly variables As New Dictionary(Of String, String)

        Sub New(App As Type)
            Call MyBase.New(App)
        End Sub

        Public Function RunApp() As Integer
            Dim input As Value(Of String) = ""
            Dim cmd As CommandLine
            Dim ps1 As PS1 = PS1.Fedora12

            Call Console.Write(ps1.ToString)
            Call Console.Write(" ")

            Do While Not (input = Console.ReadLine).TextEquals("quit()")
                With input.Value
                    If Not .StringEmpty Then
                        cmd = CLITools.TryParse(.ref, False)

                        Select Case cmd.Name.ToLower

                            Case "ls"
                                ' list directory
                            Case "cd"
                                ' change directory
                            Case "cat"
                                ' display text file content
                            Case "help"
                                ' view commandline help 
                            Case Else

                                Call MyBase.Execute(args:=cmd)

                        End Select
                    End If
                End With

                Call Console.Write(ps1.ToString)
                Call Console.Write(" ")
            Loop

            Return 0
        End Function
    End Class
End Namespace
