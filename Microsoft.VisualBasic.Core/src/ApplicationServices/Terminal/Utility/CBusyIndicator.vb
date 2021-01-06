#Region "Microsoft.VisualBasic::6438eb51b688ea913d00f1f902d80fb5, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\Utility\CBusyIndicator.vb"

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

    '     Class CBusyIndicator
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: [Stop], (+2 Overloads) Dispose, DoEvents, Start
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Threading
Imports Microsoft.VisualBasic.Parallel

Namespace ApplicationServices.Terminal.Utility

    ''' <summary>
    ''' The console BusyIndicator
    ''' </summary>
    Public Class CBusyIndicator : Implements IDisposable

        Dim _indicatorStyle As Char
        Dim _onRunningState As Boolean = False
        Dim _ticksCount As Integer

        Sub New(Optional indicatorStyle As Char = "."c, Optional start As Boolean = False, Optional ticks As Integer = -1)
            _indicatorStyle = indicatorStyle

            If start Then
                Call Me.Start(ticks)
            End If
        End Sub

        Private Sub DoEvents()
            Do While _onRunningState = True

                Call Thread.Sleep(1000)
                Call STDIO.Write(_indicatorStyle)

                If _ticksCount > 0 Then
                    _ticksCount -= 1
                Else
                    If _ticksCount <> -1 Then
                        _onRunningState = False
                    End If
                End If
            Loop
        End Sub

        ''' <summary>
        ''' 运行进度条
        ''' </summary>
        ''' <param name="Ticks">The total ticking counts of the indicator, Unit is [second].</param>
        ''' <remarks></remarks>
        Public Sub Start(Optional Ticks As Integer = -1)
            If _onRunningState = True Then
                Return
            End If

            _ticksCount = Ticks
            _onRunningState = True

            Call RunTask(AddressOf DoEvents)
        End Sub

        Public Sub [Stop]()
            _onRunningState = False
        End Sub

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    _onRunningState = False
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(      disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(      disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region

    End Class
End Namespace
