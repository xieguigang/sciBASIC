#Region "Microsoft.VisualBasic::16ebe1a3f6e49a08abb0f9f9fdd3b180, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\Utility\ProgressBar\ShellProgressBar\ShellProgressBar.Example\Examples\IndeterminateProgressExample.vb"

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

    '   Total Lines: 29
    '    Code Lines: 26
    ' Comment Lines: 0
    '   Blank Lines: 3
    '     File Size: 1.14 KB


    '     Class IndeterminateProgressExample
    ' 
    '         Function: StartAsync
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Threading.Tasks

Namespace ShellProgressBar.Example.Examples
    Public Class IndeterminateProgressExample
        Inherits ExampleBase
        Protected Overrides Function StartAsync() As Task
            Dim options = New ProgressBarOptions With {
    .ForegroundColor = ConsoleColor.Yellow,
    .ForegroundColorDone = ConsoleColor.DarkGreen,
    .BackgroundColor = ConsoleColor.DarkGray,
    .BackgroundCharacter = "▓"c
}

            Using pbar = New IndeterminateProgressBar("Indeterminate", options)
                Call Task.Run(Sub()
                                  For i = 0 To 999
                                      pbar.Message = $"The progress is beating to its own drum (indeterminate) {i}"
                                      Call Task.Delay(10).Wait()
                                  Next
                              End Sub).Wait()
                pbar.Finished()
                pbar.Message = "Finished! Moving on to the next in 5 seconds."
            End Using

            Return Task.CompletedTask
        End Function
    End Class
End Namespace
