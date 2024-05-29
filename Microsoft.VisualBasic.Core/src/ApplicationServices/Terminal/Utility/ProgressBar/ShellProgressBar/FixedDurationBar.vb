#Region "Microsoft.VisualBasic::7d245fc1d2c6bf5b23d753b87d1f16db, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\Utility\ProgressBar\ShellProgressBar\FixedDurationBar.vb"

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

'   Total Lines: 51
'    Code Lines: 44 (86.27%)
' Comment Lines: 0 (0.00%)
'    - Xml Docs: 0.00%
' 
'   Blank Lines: 7 (13.73%)
'     File Size: 1.54 KB


' 	Class FixedDurationBar
' 
' 	    Properties: CompletedHandle, IsCompleted
' 
' 	    Constructor: (+2 Overloads) Sub New
' 	    Sub: OnDone, OnTimerTick
' 
' 
' /********************************************************************************/

#End Region

Imports System.Threading
Imports std = System.Math

Namespace ApplicationServices.Terminal.ProgressBar.ShellProgressBar
    Public Class FixedDurationBar
        Inherits ProgressBar
        Private _IsCompleted As Boolean

        Public Property IsCompleted As Boolean
            Get
                Return _IsCompleted
            End Get
            Private Set(value As Boolean)
                _IsCompleted = value
            End Set
        End Property

        Private ReadOnly _completedHandle As ManualResetEvent = New ManualResetEvent(False)
        Public ReadOnly Property CompletedHandle As WaitHandle
            Get
                Return _completedHandle
            End Get
        End Property

        Public Sub New(duration As TimeSpan, message As String, color As ConsoleColor)
            Me.New(duration, message, New ProgressBarOptions With {
                .ForegroundColor = color
            })
        End Sub

        Public Sub New(duration As TimeSpan, message As String, Optional options As ProgressBarOptions = Nothing)
            MyBase.New(CInt(std.Ceiling(duration.TotalSeconds)) * 2, message, options)
            If Not Me.Options.DisplayTimeInRealTime Then
				Throw New ArgumentException($"{NameOf(ProgressBarOptions)}.{NameOf(ProgressBarOptions.DisplayTimeInRealTime)} has to be true for {NameOf(FixedDurationBar)}", NameOf(options))
			End If
		End Sub

		Private _seenTicks As Long = 0
		Protected Overrides Sub OnTimerTick()
			Interlocked.Increment(_seenTicks)
			Tick()
			MyBase.OnTimerTick()
		End Sub

		Protected Overrides Sub OnDone()
			IsCompleted = True
			_completedHandle.Set()
		End Sub
	End Class
End Namespace
