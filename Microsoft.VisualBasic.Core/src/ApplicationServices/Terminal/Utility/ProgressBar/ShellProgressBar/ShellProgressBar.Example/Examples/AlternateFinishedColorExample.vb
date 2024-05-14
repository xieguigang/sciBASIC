#Region "Microsoft.VisualBasic::f1b561eacee5b3ca179344ef4286cc5c, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\Utility\ProgressBar\ShellProgressBar\ShellProgressBar.Example\Examples\AlternateFinishedColorExample.vb"

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

    '   Total Lines: 30
    '    Code Lines: 25
    ' Comment Lines: 1
    '   Blank Lines: 4
    '     File Size: 1.16 KB


    '     Class AlternateFinishedColorExample
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Threading.Tasks

Namespace ShellProgressBar.Example.Examples
    Public Class AlternateFinishedColorExample
        Inherits ExampleBase
        Protected Overrides Async Function StartAsync() As Task
            Dim options = New ProgressBarOptions With {
    .ForegroundColor = ConsoleColor.Yellow,
    .ForegroundColorDone = ConsoleColor.DarkGreen,
    .ForegroundColorError = ConsoleColor.Red,
    .BackgroundColor = ConsoleColor.DarkGray,
    .BackgroundCharacter = "▓"c
}

            Dim pbar = New ProgressBar(100, "100 ticks", options)
            Await Task.Run(Sub()
                               For i = 0 To 9
                                   Call Task.Delay(10).Wait()
                                   pbar.Tick($"Step {i}")
                               Next
                               pbar.WriteErrorLine("The task ran into an issue!")
                               ' OR pbar.ObservedError = true;
                           End Sub)
            pbar.Message = "Indicate the task is done, but the status is not Green."
        End Function


    End Class
End Namespace
