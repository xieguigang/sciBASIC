#Region "Microsoft.VisualBasic::7a3e1a767693c9801bd4defc97d917df, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\Utility\ProgressBar\ShellProgressBar\ShellProgressBar.Example\Examples\FixedDurationExample.vb"

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

    '   Total Lines: 35
    '    Code Lines: 30
    ' Comment Lines: 0
    '   Blank Lines: 5
    '     File Size: 1.27 KB


    '     Class FixedDurationExample
    ' 
    '         Function: StartAsync
    ' 
    '         Sub: LongRunningTask
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Threading
Imports System.Threading.Tasks

Namespace ShellProgressBar.Example.Examples
    Public Class FixedDurationExample
        Inherits ExampleBase
        Protected Overrides Function StartAsync() As Task
            Dim options = New ProgressBarOptions With {
    .ForegroundColor = ConsoleColor.Yellow,
    .ForegroundColorDone = ConsoleColor.DarkGreen,
    .BackgroundColor = ConsoleColor.DarkGray,
    .BackgroundCharacter = "▓"c
}
            Dim wait = TimeSpan.FromSeconds(5)
            Using pbar = New FixedDurationBar(wait, "", options)
                Dim t = New Thread(Sub() LongRunningTask(pbar))
                t.Start()

                If Not pbar.CompletedHandle.WaitOne(wait) Then Console.Error.WriteLine($"{NameOf(FixedDurationBar)} did not signal {NameOf(FixedDurationBar.CompletedHandle)} after {wait}")

            End Using

            Return Task.CompletedTask
        End Function

        Private Shared Sub LongRunningTask(bar As FixedDurationBar)
            For i = 0 To 999999
                bar.Message = $"{i} events"
                If bar.IsCompleted Then Exit For
                Thread.Sleep(1)
            Next
        End Sub
    End Class
End Namespace
