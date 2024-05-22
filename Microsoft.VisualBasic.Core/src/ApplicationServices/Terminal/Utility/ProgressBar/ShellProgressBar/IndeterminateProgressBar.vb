#Region "Microsoft.VisualBasic::23f52e55e6458a0d475f3aa1073be8c3, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\Utility\ProgressBar\ShellProgressBar\IndeterminateProgressBar.vb"

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

    '   Total Lines: 45
    '    Code Lines: 36 (80.00%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 9 (20.00%)
    '     File Size: 1.57 KB


    '     Class IndeterminateProgressBar
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Sub: Finished, OnTimerTick
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Threading

Namespace ApplicationServices.Terminal.ProgressBar.ShellProgressBar
    Public Class IndeterminateProgressBar
        Inherits ProgressBar
        Private Const MaxTicksForIndeterminate As Integer = 20

        Public Sub New(message As String, color As ConsoleColor)
            Me.New(message, New ProgressBarOptions With {
    .ForegroundColor = color
})
        End Sub

        Public Sub New(message As String, Optional options As ProgressBarOptions = Nothing)
            MyBase.New(MaxTicksForIndeterminate, message, options)
            If options Is Nothing Then
                options = New ProgressBarOptions()
            End If

            options.DisableBottomPercentage = True
            options.DisplayTimeInRealTime = True

            If Not Me.Options.DisplayTimeInRealTime Then Throw New ArgumentException($"{NameOf(ProgressBarOptions)}.{NameOf(ProgressBarOptions.DisplayTimeInRealTime)} has to be true for {NameOf(FixedDurationBar)}", NameOf(options))
        End Sub

        Private _seenTicks As Long = 0

        Protected Overrides Sub OnTimerTick()
            Interlocked.Increment(_seenTicks)
            If _seenTicks = MaxTicksForIndeterminate - 1 Then
                Tick(0)
                Interlocked.Exchange(_seenTicks, 0)
            Else
                Tick()
            End If

            MyBase.OnTimerTick()
        End Sub

        Public Sub Finished()
            Tick(MaxTicksForIndeterminate)
        End Sub
    End Class
End Namespace
