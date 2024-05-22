#Region "Microsoft.VisualBasic::c1bb407a8f47baa7e7b319f4402731b7, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\Utility\ProgressBar\ShellProgressBar\ShellProgressBar.Example\Examples\IndeterminateChildrenNoCollapse.vb"

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

    '   Total Lines: 33
    '    Code Lines: 32 (96.97%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 1 (3.03%)
    '     File Size: 1.32 KB


    '     Class IndeterminateChildrenNoCollapseExample
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Threading.Tasks

Namespace ShellProgressBar.Example.Examples
    Public Class IndeterminateChildrenNoCollapseExample
        Inherits ExampleBase
        Protected Overrides Async Function StartAsync() As Task
            Const totalChildren = 2
            Dim random As Random = New Random()
            Dim options = New ProgressBarOptions With {
    .ForegroundColor = ConsoleColor.Yellow,
    .BackgroundColor = ConsoleColor.DarkGray,
    .ProgressCharacter = "─"c
}
            Dim childOptions = New ProgressBarOptions With {
    .ForegroundColor = ConsoleColor.Green,
    .BackgroundColor = ConsoleColor.DarkGray,
    .ProgressCharacter = "─"c,
    .CollapseWhenFinished = False
}
            Using pbar = New ProgressBar(totalChildren, "main progressbar", options)
                For i = 1 To totalChildren
                    pbar.Message = $"Start {i} of {totalChildren}: main progressbar"
                    Using child = pbar.SpawnIndeterminate("child action " & i, childOptions)
                        Await Task.Delay(1000 * random.Next(5, 15))
                        child.Finished()
                    End Using
                    pbar.Tick()
                Next
            End Using
        End Function
    End Class
End Namespace
