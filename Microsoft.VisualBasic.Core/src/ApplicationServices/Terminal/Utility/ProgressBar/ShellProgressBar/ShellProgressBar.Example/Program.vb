#Region "Microsoft.VisualBasic::0661fec0179b0187dc08534262f50297, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\Utility\ProgressBar\ShellProgressBar\ShellProgressBar.Example\Program.vb"

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

    '   Total Lines: 92
    '    Code Lines: 82
    ' Comment Lines: 0
    '   Blank Lines: 10
    '     File Size: 3.42 KB


    '     Class Program
    ' 
    '         Sub: BusyWait, Main
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Collections.Generic
Imports System.Threading
Imports System.Threading.Tasks
Imports ShellProgressBar.Example.Examples
Imports ShellProgressBar.Example.TestCases

Namespace ShellProgressBar.Example
    Friend Class Program
        Private Shared ReadOnly TestCases As IList(Of IProgressBarExample) = New List(Of IProgressBarExample) From {
    New PersistMessageExample(),
    New FixedDurationExample(),
    New DeeplyNestedProgressBarTreeExample(),
    New NestedProgressBarPerStepProgress(),
    New DrawsOnlyOnTickExample(),
    New ThreadedTicksOverflowExample(),
    New TicksOverflowExample(),
    New NegativeMaxTicksExample(),
    New ZeroMaxTicksExample(),
    New LongRunningExample(),
    New NeverCompletesExample(),
    New UpdatesMaxTicksExample(),
    New NeverTicksExample(),
    New EstimatedDurationExample(),
    New IndeterminateProgressExample(),
    New IndeterminateChildrenNoCollapseExample(),
    New AlternateFinishedColorExample()
}

        Private Shared ReadOnly Examples As IList(Of IProgressBarExample) = New List(Of IProgressBarExample) From {
    New DontDisplayInRealTimeExample(),
    New StylingExample(),
    New ProgressBarOnBottomExample(),
    New ChildrenExample(),
    New ChildrenNoCollapseExample(),
    New IntegrationWithIProgressExample(),
    New IntegrationWithIProgressPercentageExample(),
    New MessageBeforeAndAfterExample(),
    New DeeplyNestedProgressBarTreeExample(),
    New EstimatedDurationExample(),
    New DownloadProgressExample(),
    New AlternateFinishedColorExample()
}

		Public Shared Sub Main(args As String())
			Dim cts = New CancellationTokenSource()
			AddHandler Console.CancelKeyPress, Sub(s, e) cts.Cancel()

			Call MainAsync(args, cts.Token).Wait()
		End Sub

		Private Shared Async Function MainAsync(args As String(), token As CancellationToken) As Task
            Dim command = If(args.Length > 0, args(0), "test")
            Select Case command
                Case "test"
                    Await RunTestCases(token)
                    Return
                Case "example"
                    Dim nth = If(args.Length > 1, Integer.Parse(args(1)), 0)
                    Await RunExample(nth, token)
                    Return
                Case Else
                    Await Console.Error.WriteLineAsync($"Unknown command:{command}")
                    Return
            End Select
        End Function

        Private Shared Async Function RunExample(nth As Integer, token As CancellationToken) As Task
            If nth > Examples.Count - 1 OrElse nth < 0 Then
                Await Console.Error.WriteLineAsync($"There are only {Examples.Count} examples, {nth} is not valid")
            End If

            Dim example = Examples(nth)

            Await example.Start(token)
        End Function

        Private Shared Async Function RunTestCases(token As CancellationToken) As Task
            Dim i = 0
			For Each example As IProgressBarExample In TestCases
				If i > 0 Then Console.Clear() 'not necessary but for demo/recording purposes.
				Await example.Start(token)
				i += 1
			Next
			Console.Write("Shown all examples!")
        End Function

        Public Shared Sub BusyWait(milliseconds As Integer)
            Thread.Sleep(milliseconds)
        End Sub
    End Class
End Namespace
