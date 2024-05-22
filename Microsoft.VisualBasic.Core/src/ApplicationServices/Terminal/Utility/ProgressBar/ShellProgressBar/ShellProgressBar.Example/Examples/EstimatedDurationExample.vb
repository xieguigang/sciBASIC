#Region "Microsoft.VisualBasic::87836bd526db82086f3f9f310dfc9caa, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\Utility\ProgressBar\ShellProgressBar\ShellProgressBar.Example\Examples\EstimatedDurationExample.vb"

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

    '   Total Lines: 31
    '    Code Lines: 26 (83.87%)
    ' Comment Lines: 1 (3.23%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 4 (12.90%)
    '     File Size: 1.28 KB


    '     Class EstimatedDurationExample
    ' 
    '         Function: StartAsync
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Threading
Imports System.Threading.Tasks

Namespace ShellProgressBar.Example.Examples
    Public Class EstimatedDurationExample
        Inherits ExampleBase
        Protected Overrides Function StartAsync() As Task
            Const totalTicks = 10
            Dim options = New ProgressBarOptions With {
    .ProgressCharacter = "─"c,
    .ShowEstimatedDuration = True
}
            Using pbar = New ProgressBar(totalTicks, "you can set the estimated duration too", options)
                pbar.EstimatedDuration = TimeSpan.FromMilliseconds(totalTicks * 500)

                Dim initialMessage = pbar.Message
                For i = 0 To totalTicks - 1
                    pbar.Message = $"Start {i + 1} of {totalTicks}: {initialMessage}"
                    Thread.Sleep(500)

                    ' Simulate changing estimated durations while progress increases
                    Dim estimatedDuration = TimeSpan.FromMilliseconds(500 * totalTicks) + TimeSpan.FromMilliseconds(300 * i)
                    pbar.Tick(estimatedDuration, $"End {i + 1} of {totalTicks}: {initialMessage}")
                Next
            End Using

            Return Task.CompletedTask
        End Function
    End Class
End Namespace
