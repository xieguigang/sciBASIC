#Region "Microsoft.VisualBasic::c36dcec78290e972fa2362d0ba847402, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\xConsole\Spinner.vb"

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

    '   Total Lines: 114
    '    Code Lines: 65 (57.02%)
    ' Comment Lines: 30 (26.32%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 19 (16.67%)
    '     File Size: 4.13 KB


    '     Class Spinner
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: Turn
    ' 
    '         Sub: Break, Run, RunTask
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Threading

Namespace ApplicationServices.Terminal.xConsole

#Region "SPINNER ⌛"

    ''' <summary>
    ''' A list of spinners for your console ❤
    ''' </summary>
    Public Class Spinner
        Private counter As Integer = 0
        Private c As Char()

        ''' <summary>
        ''' List of available spinners (you can add new)
        ''' </summary>
        Public Spinners As New List(Of Char())() From {
                New Char() {"-"c, "\"c, "|"c, "/"c},
                New Char() {"▄"c, "■"c, "▀"c, "■"c},
                New Char() {"╔"c, "╗"c, "╝"c, "╚"c},
                New Char() {"."c, "·"c, "`"c, "·"c},
                New Char() {"1"c, "2"c, "3"c, "4"c, "5"c, "6"c, "7"c, "8"c, "9"c, "0"c}
            }

        ''' <summary>
        ''' looplooplooplooplooplooplooplooplooploop[...]
        ''' </summary>
        Private inLoop As Boolean = True

        ''' <summary>
        ''' The base string for spinning. {0} will display the spinner. COLOR is SUPPORTED! 🆒
        ''' </summary>
        Public SpinText As String = "{0} ^gLoading^!..."

        ''' <summary>
        ''' Initialize the spinner
        ''' </summary>
        ''' <param name="i">Index of the spinner to use</param>
        ''' <param name="txt">Base string. `{0} show the spinner`</param>
        Public Sub New(Optional i As Integer = 0, Optional txt As String = Nothing)
            If Not String.IsNullOrWhiteSpace(txt) Then
                SpinText = txt
            End If
            c = If((Spinners.Count > i), Spinners(i), Spinners(0))
        End Sub

        ''' <summary>
        ''' Initialize a custom spinner
        ''' </summary>
        ''' <param name="spinner">Set a custom spinner, no size limit.</param>
        Public Sub New(spinner As Char())
            c = spinner
        End Sub

        ''' <summary>
        ''' Breaks the spinner
        ''' </summary>
        Public Sub Break()
            inLoop = False
        End Sub

        Private StringSize As Integer = 0

        ''' <summary>
        ''' Turn the spin!
        ''' </summary>
        ''' <param name="time">Waiting time. Default 130 ms</param>
        ''' <returns>False if it has been stopped</returns>
        ''' <example>while(spinner.Turn());</example>
        Public Function Turn(Optional time As Integer = 130) As Boolean
            Dim [loop] = inLoop

            If [loop] Then
                counter += 1
                Dim wr As String = String.Format(SpinText, c(counter Mod c.Length), "wtf?")
                My.InnerQueue.AddToQueue(Sub()
                                             StringSize = xConsole.Print(wr)
                                             Dim left As Integer = Console.CursorLeft - StringSize
                                             If left < 0 Then
                                                 left = 0
                                             End If
                                             Console.SetCursorPosition(left, Console.CursorTop)
                                         End Sub)

                Thread.Sleep(time)
            Else
                My.InnerQueue.AddToQueue(Sub()
                                             Dim pos As New Point(Console.CursorLeft, Console.CursorTop)
                                             Console.Write(New String(" "c, StringSize))
                                             Console.SetCursorPosition(pos.X, pos.Y)
                                         End Sub)
            End If

            Console.CursorVisible = Not [loop]

            Return [loop]
        End Function

        Public Sub Run(Optional speed As Integer = 130)
            Do While inLoop
                Call Turn(speed)
                Call Thread.Sleep(10)
            Loop
        End Sub

        Public Sub RunTask(Optional speed As Integer = 130)
            Call Parallel.RunTask(Sub() Call Run(speed))
        End Sub
    End Class

#End Region

End Namespace
