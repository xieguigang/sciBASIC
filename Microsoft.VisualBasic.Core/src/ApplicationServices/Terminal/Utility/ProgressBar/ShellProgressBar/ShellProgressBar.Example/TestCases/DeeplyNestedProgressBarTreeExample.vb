#Region "Microsoft.VisualBasic::22760958142c72952b455f97724f330c, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\Utility\ProgressBar\ShellProgressBar\ShellProgressBar.Example\TestCases\DeeplyNestedProgressBarTreeExample.vb"

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

    '   Total Lines: 57
    '    Code Lines: 52
    ' Comment Lines: 0
    '   Blank Lines: 5
    '     File Size: 2.62 KB


    '     Class DeeplyNestedProgressBarTreeExample
    ' 
    '         Function: Start
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Runtime.InteropServices
Imports System.Threading
Imports System.Threading.Tasks

Namespace ShellProgressBar.Example.TestCases
    Public Class DeeplyNestedProgressBarTreeExample
        Implements IProgressBarExample
        Public Function Start(token As CancellationToken) As Task Implements IProgressBarExample.Start
            Dim random = New Random()

            Dim numberOfSteps = 7

            Dim overProgressOptions = New ProgressBarOptions With {
    .DenseProgressBar = True,
    .ProgressCharacter = "─"c,
    .BackgroundColor = ConsoleColor.DarkGray,
    .EnableTaskBarProgress = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
}

            Using pbar = New ProgressBar(numberOfSteps, "overall progress", overProgressOptions)
                Dim stepBarOptions = New ProgressBarOptions With {
    .DenseProgressBar = True,
    .ForegroundColor = ConsoleColor.Cyan,
    .ForegroundColorDone = ConsoleColor.DarkGreen,
    .ProgressCharacter = "─"c,
    .BackgroundColor = ConsoleColor.DarkGray,
    .CollapseWhenFinished = True
}
                Call Parallel.For(0, numberOfSteps, Sub(i)
                                                        Dim workBarOptions = New ProgressBarOptions With {
                                        .DenseProgressBar = True,
                                        .ForegroundColor = ConsoleColor.Yellow,
                                        .ProgressCharacter = "─"c,
                                        .BackgroundColor = ConsoleColor.DarkGray
                                    }
                                                        Dim childSteps = random.Next(1, 5)
														Using childProgress As ChildProgressBar = pbar.Spawn(childSteps, $"step {i} progress", stepBarOptions)
															Call Parallel.For(0, childSteps, Sub(ci)
																								 Dim childTicks = random.Next(50, 250)
																								 Using innerChildProgress = childProgress.Spawn(childTicks, $"step {i}::{ci} progress", workBarOptions)
																									 For r = 0 To childTicks - 1
																										 innerChildProgress.Tick()
																										 Program.BusyWait(50)
																									 Next
																								 End Using
																								 childProgress.Tick()
																							 End Sub)
														End Using

														pbar.Tick()
                                                    End Sub)
            End Using
            Return Task.FromResult(1)
        End Function
    End Class
End Namespace
