#Region "Microsoft.VisualBasic::93392fb120df341bb59d6c0fab4ba0a7, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\Utility\ProgressBar\ShellProgressBar\ShellProgressBar.Example\Examples\StylingExample.vb"

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

    '   Total Lines: 23
    '    Code Lines: 21
    ' Comment Lines: 0
    '   Blank Lines: 2
    '     File Size: 920 B


    '     Class StylingExample
    ' 
    '         Function: StartAsync
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Threading.Tasks

Namespace ShellProgressBar.Example.Examples
    Public Class StylingExample
        Inherits ExampleBase
        Protected Overrides Function StartAsync() As Task
            Const totalTicks = 10
            Dim options = New ProgressBarOptions With {
    .ForegroundColor = ConsoleColor.Yellow,
    .ForegroundColorDone = ConsoleColor.DarkGreen,
    .BackgroundColor = ConsoleColor.DarkGray,
    .BackgroundCharacter = "▓"c
}
            Dim pbar = New ProgressBar(totalTicks, "showing off styling", options)
            TickToCompletion(pbar, totalTicks, sleep:=500, Sub(i)
                                                               If i > 5 Then pbar.ForegroundColor = ConsoleColor.Red
                                                           End Sub)

            Return Task.CompletedTask
        End Function
    End Class
End Namespace
