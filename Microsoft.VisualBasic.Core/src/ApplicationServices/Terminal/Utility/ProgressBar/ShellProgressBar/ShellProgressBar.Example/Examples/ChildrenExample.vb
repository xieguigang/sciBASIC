#Region "Microsoft.VisualBasic::031c3223a5c48577c4133c0c1229d77a, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\Utility\ProgressBar\ShellProgressBar\ShellProgressBar.Example\Examples\ChildrenExample.vb"

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

    '   Total Lines: 27
    '    Code Lines: 26
    ' Comment Lines: 0
    '   Blank Lines: 1
    '     File Size: 1.22 KB


    '     Class ChildrenExample
    ' 
    '         Function: StartAsync
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Threading.Tasks

Namespace ShellProgressBar.Example.Examples
    Public Class ChildrenExample
        Inherits ExampleBase
        Protected Overrides Function StartAsync() As Task
            Const totalTicks = 10
            Dim options = New ProgressBarOptions With {
    .ForegroundColor = ConsoleColor.Yellow,
    .BackgroundColor = ConsoleColor.DarkGray,
    .ProgressCharacter = "─"c
}
            Dim childOptions = New ProgressBarOptions With {
    .ForegroundColor = ConsoleColor.Green,
    .BackgroundColor = ConsoleColor.DarkGray,
    .ProgressCharacter = "─"c
}
            Dim pbar = New ProgressBar(totalTicks, "main progressbar", options)
            TickToCompletion(pbar, totalTicks, sleep:=10, childAction:=Sub(i)
                                                                           Dim child = pbar.Spawn(totalTicks, "child actions", childOptions)
                                                                           TickToCompletion(child, totalTicks, sleep:=100)
                                                                       End Sub)
            Return Task.CompletedTask
        End Function
    End Class
End Namespace
