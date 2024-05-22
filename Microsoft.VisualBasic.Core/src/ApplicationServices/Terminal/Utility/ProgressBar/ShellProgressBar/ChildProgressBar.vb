#Region "Microsoft.VisualBasic::afe663eb552880c42ff837113604f0e9, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\Utility\ProgressBar\ShellProgressBar\ChildProgressBar.vb"

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

    '   Total Lines: 74
    '    Code Lines: 59 (79.73%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 15 (20.27%)
    '     File Size: 2.76 KB


    '     Class ChildProgressBar
    ' 
    '         Properties: StartDate
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: AsProgress
    ' 
    '         Sub: DisplayProgress, Dispose, Grow, OnDone, WriteErrorLine
    '              WriteLine
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ApplicationServices.Terminal.ProgressBar.ShellProgressBar

    Public Class ChildProgressBar : Inherits ProgressBarBase
        Implements IDisposable

        Private ReadOnly _scheduleDraw As Action
        Private ReadOnly _writeLine As Action(Of String)
        Private ReadOnly _writeError As Action(Of String)
        Private ReadOnly _growth As Action(Of ProgressBarHeight)

        Public ReadOnly Property StartDate As Date
            Get
                Return MyBase._startDate
            End Get
        End Property

        Protected Overrides Sub DisplayProgress()
            _scheduleDraw?.Invoke()
        End Sub

        Friend Sub New(maxTicks As Integer, message As String,
                       scheduleDraw As Action,
                       writeLine As Action(Of String),
                       writeError As Action(Of String),
                       Optional options As ProgressBarOptions = Nothing,
                       Optional growth As Action(Of ProgressBarHeight) = Nothing)

            MyBase.New(maxTicks, message, options)

            _scheduleDraw = scheduleDraw
            _writeLine = writeLine
            _writeError = writeError
            _growth = growth
            _growth?.Invoke(ProgressBarHeight.Increment)
        End Sub

        Protected Overrides Sub Grow(direction As ProgressBarHeight)
            _growth?.Invoke(direction)
        End Sub

        Private _calledDone As Boolean
        Private ReadOnly _callOnce As Object = New Object()
        Protected Overrides Sub OnDone()
            If _calledDone Then Return
            SyncLock _callOnce
                If _calledDone Then Return

                If EndTime Is Nothing Then EndTime = Date.Now

                If Options.CollapseWhenFinished Then _growth?.Invoke(ProgressBarHeight.Decrement)

                _calledDone = True
            End SyncLock
        End Sub

        Public Overrides Sub WriteLine(message As String) 'Implements IProgressBar.WriteLine
            _writeLine(message)
        End Sub
        Public Overrides Sub WriteErrorLine(message As String) 'Implements IProgressBar.WriteErrorLine
            _writeError(message)
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            For Each c In Children
                c.Dispose()
            Next
            OnDone()
        End Sub

        Public Function AsProgress(Of T)(Optional message As Func(Of T, String) = Nothing, Optional percentage As Func(Of T, Double?) = Nothing) As IProgress(Of T) 'Implements IProgressBar.AsProgress
            Return New Progress(Of T)(Me, message, percentage)
        End Function
    End Class
End Namespace
