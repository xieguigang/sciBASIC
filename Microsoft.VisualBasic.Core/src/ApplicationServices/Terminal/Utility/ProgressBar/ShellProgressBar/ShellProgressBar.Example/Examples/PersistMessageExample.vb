#Region "Microsoft.VisualBasic::e9a0b0d5171b3614c2525d06838adccb, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\Utility\ProgressBar\ShellProgressBar\ShellProgressBar.Example\Examples\PersistMessageExample.vb"

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

    '   Total Lines: 52
    '    Code Lines: 48 (92.31%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 4 (7.69%)
    '     File Size: 2.41 KB


    '     Class PersistMessageExample
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
    Public Class PersistMessageExample
        Inherits ExampleBase
        Protected Overrides Function StartAsync() As Task
            Dim options = New ProgressBarOptions With {
    .ForegroundColor = ConsoleColor.Yellow,
    .ForegroundColorDone = ConsoleColor.DarkGreen,
    .ForegroundColorError = ConsoleColor.Red,
    .BackgroundColor = ConsoleColor.DarkGray,
    .BackgroundCharacter = "▓"c,
    .WriteQueuedMessage = Function(o)
                              Dim writer = If(o.Error, Console.Error, Console.Out)
                              Dim c = If(o.Error, ConsoleColor.DarkRed, ConsoleColor.Blue)
                              If o.Line.StartsWith("Report 500") Then
                                  Console.ForegroundColor = ConsoleColor.Yellow
                                  writer.WriteLine("Add an extra message, because why not")

                                  Console.ForegroundColor = c
                                  writer.WriteLine(o.Line)
                                  Return 2 'signal to the progressbar we wrote two messages
                              End If
                              Console.ForegroundColor = c
                              writer.WriteLine(o.Line)
                              Return 1
                          End Function
}
            Dim wait = TimeSpan.FromSeconds(6)
            Dim pbar = New FixedDurationBar(wait, "", options)
            Dim t = New Thread(Sub() LongRunningTask(pbar))
            t.Start()

            If Not pbar.CompletedHandle.WaitOne(wait.Subtract(TimeSpan.FromSeconds(.5))) Then
                pbar.WriteErrorLine($"{NameOf(FixedDurationBar)} did not signal {NameOf(FixedDurationBar.CompletedHandle)} after {wait}")
                pbar.Dispose()
            End If
            Return Task.CompletedTask
        End Function

        Private Shared Sub LongRunningTask(bar As FixedDurationBar)
            For i = 0 To 999999
                bar.Message = $"{i} events"
                If bar.IsCompleted OrElse bar.ObservedError Then Exit For
                If i Mod 500 = 0 Then bar.WriteLine($"Report {i} to console above the progressbar")
                Thread.Sleep(1)
            Next
        End Sub
    End Class
End Namespace
