#Region "Microsoft.VisualBasic::1342c2e8f0012bfb8993cefd8d13a37c, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\Utility\ProgressBar\ShellProgressBar\IProgressBar.vb"

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

    '   Total Lines: 30
    '    Code Lines: 17 (56.67%)
    ' Comment Lines: 5 (16.67%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (26.67%)
    '     File Size: 1.27 KB


    '     Interface IProgressBar
    ' 
    '         Properties: CurrentTick, ForegroundColor, MaxTicks, Message, Percentage
    ' 
    '         Function: AsProgress, Spawn
    ' 
    '         Sub: (+2 Overloads) Tick, WriteErrorLine, WriteLine
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System

Namespace ApplicationServices.Terminal.ProgressBar.ShellProgressBar
    Public Interface IProgressBar
        Inherits IDisposable
        Function Spawn(maxTicks As Integer, message As String, Optional options As ProgressBarOptions = Nothing) As ChildProgressBar

        Sub Tick(Optional message As String = Nothing)
        Sub Tick(newTickCount As Integer, Optional message As String = Nothing)

        Property MaxTicks As Integer
        Property Message As String

        ReadOnly Property Percentage As Double
        ReadOnly Property CurrentTick As Integer

        Property ForegroundColor As ConsoleColor

        ''' <summary>
        ''' This writes a new line above the progress bar to <see cref="Console.Out"/>.
        ''' Use <see cref="Message"/> to update the message inside the progress bar
        ''' </summary>
        Sub WriteLine(message As String)

        ''' <summary> This writes a new line above the progress bar to <see cref="Console.Error"/></summary>
        Sub WriteErrorLine(message As String)

        Function AsProgress(Of T)(Optional message As Func(Of T, String) = Nothing, Optional percentage As Func(Of T, Double?) = Nothing) As IProgress(Of T)
    End Interface
End Namespace
