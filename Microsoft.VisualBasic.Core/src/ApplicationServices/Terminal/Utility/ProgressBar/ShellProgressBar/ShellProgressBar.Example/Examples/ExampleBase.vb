#Region "Microsoft.VisualBasic::01148a853b065b58d06add977936a047, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\Utility\ProgressBar\ShellProgressBar\ShellProgressBar.Example\Examples\ExampleBase.vb"

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

    '   Total Lines: 32
    '    Code Lines: 26
    ' Comment Lines: 0
    '   Blank Lines: 6
    '     File Size: 1.28 KB


    '     Class ExampleBase
    ' 
    '         Properties: RequestToQuit
    ' 
    '         Sub: TickToCompletion
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Threading
Imports System.Threading.Tasks

Namespace ShellProgressBar.Example.Examples
    Public MustInherit Class ExampleBase
        Implements IProgressBarExample
        Private Property RequestToQuit As Boolean

        Protected Sub TickToCompletion(pbar As IProgressBar, ticks As Integer, Optional sleep As Integer = 1750, Optional childAction As Action(Of Integer) = Nothing)
            Dim initialMessage = pbar.Message
            Dim i = 0

            While i < ticks AndAlso Not RequestToQuit
                pbar.Message = $"Start {i + 1} of {ticks} {Console.CursorTop}/{Console.WindowHeight}: {initialMessage}"
                childAction?.Invoke(i)
                Thread.Sleep(sleep)
                pbar.Tick($"End {i + 1} of {ticks} {Console.CursorTop}/{Console.WindowHeight}: {initialMessage}")
                i += 1
            End While
        End Sub

        Public Async Function Start(token As CancellationToken) As Task Implements IProgressBarExample.Start
            RequestToQuit = False
            token.Register(Sub() RequestToQuit = True)

            Await StartAsync()
        End Function

        Protected MustOverride Function StartAsync() As Task
    End Class
End Namespace
