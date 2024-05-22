#Region "Microsoft.VisualBasic::4cc4589722b2fc97853cf671d487d60d, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\Utility\ProgressBar\ShellProgressBar\ShellProgressBar.Example\TestCases\NestedProgressBarPerStepProgress.vb"

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

    '   Total Lines: 58
    '    Code Lines: 49 (84.48%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 9 (15.52%)
    '     File Size: 2.19 KB


    '     Class NestedProgressBarPerStepProgress
    ' 
    '         Function: Start
    ' 
    '         Sub: InnerInnerProgressBars, InnerProgressBars
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks

Namespace ShellProgressBar.Example.TestCases
    Public Class NestedProgressBarPerStepProgress
        Implements IProgressBarExample
        Public Function Start(token As CancellationToken) As Task Implements IProgressBarExample.Start
            Dim outerTicks = 10
            Using pbar = New ProgressBar(outerTicks, "outer progress", ConsoleColor.Cyan)
                For i = 0 To outerTicks - 1
                    InnerProgressBars(pbar)
                    pbar.Tick()
                Next
            End Using
            Return Task.FromResult(1)
        End Function

        Private Shared Sub InnerProgressBars(pbar As ProgressBar)
            Dim lInnerProgressBars = Enumerable.Range(0, New Random().Next(2, 6)).[Select](Function(s) pbar.Spawn(New Random().Next(2, 5), $"inner bar {s}")).ToList()

            Dim maxTicks = lInnerProgressBars.Max(Function(p) p.MaxTicks)

            For ii = 0 To maxTicks - 1
                For Each p In lInnerProgressBars
                    InnerInnerProgressBars(p)
                    p.Tick()
                Next


                Thread.Sleep(4)
            Next
            For Each p In lInnerProgressBars
                p.Dispose()
            Next
        End Sub

        Private Shared Sub InnerInnerProgressBars(pbar As ChildProgressBar)
            Dim progressBarOption = New ProgressBarOptions With {
                .ForegroundColor = ConsoleColor.Yellow
            }
            Dim innerProgressBars = Enumerable.Range(0, New Random().Next(1, 3)).[Select](Function(s) pbar.Spawn(New Random().Next(5, 10), $"inner bar {s}", progressBarOption)).ToList()
            If Not innerProgressBars.Any() Then Return

            Dim maxTicks = innerProgressBars.Max(Function(p) p.MaxTicks)

            For ii = 0 To maxTicks - 1
                For Each p In innerProgressBars
                    p.Tick()
                Next
            Next
            For Each p In innerProgressBars
                p.Dispose()
            Next
        End Sub
    End Class
End Namespace
