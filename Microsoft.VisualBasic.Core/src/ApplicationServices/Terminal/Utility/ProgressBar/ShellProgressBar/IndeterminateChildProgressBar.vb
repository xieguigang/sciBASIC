#Region "Microsoft.VisualBasic::6a8691de4a332f834ee84e2e5f4c0452, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\Utility\ProgressBar\ShellProgressBar\IndeterminateChildProgressBar.vb"

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

    '   Total Lines: 46
    '    Code Lines: 40 (86.96%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 6 (13.04%)
    '     File Size: 1.72 KB


    '     Class IndeterminateChildProgressBar
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: Dispose, Finished, OnTimerTick
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Threading

Namespace ApplicationServices.Terminal.ProgressBar.ShellProgressBar
    Public Class IndeterminateChildProgressBar
        Inherits ChildProgressBar
        Private Const MaxTicksForIndeterminate As Integer = 20
        Private _timer As Timer
        Friend Sub New(message As String, scheduleDraw As Action, writeLine As Action(Of String), writeError As Action(Of String), Optional options As ProgressBarOptions = Nothing, Optional growth As Action(Of ProgressBarHeight) = Nothing)
            MyBase.New(MaxTicksForIndeterminate, message, scheduleDraw, writeLine, writeError, options, growth)
            If options Is Nothing Then
                options = New ProgressBarOptions()
            End If

            options.DisableBottomPercentage = True
            _timer = New Timer(Sub(s) OnTimerTick(), Nothing, 500, 500)
        End Sub

        Private _seenTicks As Long = 0

        Protected Sub OnTimerTick()
            Interlocked.Increment(_seenTicks)
            If _seenTicks = MaxTicksForIndeterminate - 1 Then
                Tick(0)
                Interlocked.Exchange(_seenTicks, 0)
            Else
                Tick()
            End If
            DisplayProgress()
        End Sub

        Public Sub Finished()
            _timer.Change(Timeout.Infinite, Timeout.Infinite)
            _timer.Dispose()
            Tick(MaxTicksForIndeterminate)
        End Sub

        Public Overloads Sub Dispose()
            If _timer IsNot Nothing Then _timer.Dispose()
            For Each c In Children
                c.Dispose()
            Next
            OnDone()
        End Sub
    End Class
End Namespace
