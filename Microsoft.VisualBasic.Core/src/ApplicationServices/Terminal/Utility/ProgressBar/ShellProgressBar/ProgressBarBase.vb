#Region "Microsoft.VisualBasic::9e7550ea939f1318766c5aa328dcac9a, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\Utility\ProgressBar\ShellProgressBar\ProgressBarBase.vb"

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

    '   Total Lines: 176
    '    Code Lines: 141
    ' Comment Lines: 3
    '   Blank Lines: 32
    '     File Size: 6.49 KB


    ' 	Class ProgressBarBase
    ' 
    ' 	    Properties: Children, Collapse, CurrentTick, EndTime, EstimatedDuration
    '                  ForegroundColor, MaxTicks, Message, ObservedError, Options
    '                  Percentage
    ' 
    ' 	    Constructor: (+2 Overloads) Sub New
    ' 
    ' 	    Function: GetDurationString, Spawn, SpawnIndeterminate
    ' 
    ' 	    Sub: FinishTick, Grow, OnDone, (+4 Overloads) Tick
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Collections.Concurrent
Imports System.Text
Imports System.Threading

Namespace ApplicationServices.Terminal.ProgressBar.ShellProgressBar
	Public MustInherit Class ProgressBarBase ' Implements IProgressBar, IDisposable

		Private _EndTime As System.DateTime?
		Shared Sub New()
			Call System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance)
		End Sub

		Protected ReadOnly _startDate As System.DateTime = System.DateTime.Now
		Private _maxTicks As Integer
		Private _currentTick As Integer
		Private _message As String
		Private _estimatedDuration As System.TimeSpan

		Protected Sub New(maxTicks As Integer, message As String, options As ShellProgressBar.ProgressBarOptions)
			Me._maxTicks = System.Math.Max(0, maxTicks)
			Me._message = message
			Me.Options = If(options, ShellProgressBar.ProgressBarOptions.[Default])
		End Sub

		Friend ReadOnly Property Options As ShellProgressBar.ProgressBarOptions
		Friend ReadOnly Property Children As System.Collections.Concurrent.ConcurrentBag(Of ShellProgressBar.ChildProgressBar) = New System.Collections.Concurrent.ConcurrentBag(Of ShellProgressBar.ChildProgressBar)()

		Protected MustOverride Sub DisplayProgress()

		Protected Overridable Sub Grow(direction As ShellProgressBar.ProgressBarHeight)
		End Sub

		Protected Overridable Sub OnDone()
		End Sub

		Public Property EndTime As System.DateTime?
			Get
				Return _EndTime
			End Get
			Protected Set(value As System.DateTime?)
				_EndTime = value
			End Set
		End Property

		Public Property ObservedError As Boolean

		Private _dynamicForegroundColor As System.ConsoleColor? = Nothing
		Public Property ForegroundColor As System.ConsoleColor
			Get
				Dim realColor = If(Me._dynamicForegroundColor, Me.Options.ForegroundColor)
				If Me.ObservedError AndAlso Me.Options.ForegroundColorError.HasValue Then Return Me.Options.ForegroundColorError.Value

				Return If(Me.EndTime.HasValue, If(Me.Options.ForegroundColorDone, realColor), realColor)
			End Get
			Set(value As System.ConsoleColor)
				Me._dynamicForegroundColor = value
			End Set
		End Property

		Public ReadOnly Property CurrentTick As Integer
			Get
				Return Me._currentTick
			End Get
		End Property

		Public Property MaxTicks As Integer
			Get
				Return Me._maxTicks
			End Get
			Set(value As Integer)
				Call System.Threading.Interlocked.Exchange(Me._maxTicks, value)
				Me.DisplayProgress()
			End Set
		End Property

		Public Property Message As String
			Get
				Return Me._message
			End Get
			Set(value As String)
				Call System.Threading.Interlocked.Exchange(Me._message, value)
				Me.DisplayProgress()
			End Set
		End Property

		Public Property EstimatedDuration As System.TimeSpan
			Get
				Return Me._estimatedDuration
			End Get
			Set(value As System.TimeSpan)
				Me._estimatedDuration = value
				Me.DisplayProgress()
			End Set
		End Property

		Public ReadOnly Property Percentage As Double
			Get
				Dim lPercentage = System.Math.Max(0, System.Math.Min(100, (100.0 / Me._maxTicks) * Me._currentTick))
				' Gracefully handle if the percentage is NaN due to division by 0
				If Double.IsNaN(lPercentage) OrElse lPercentage < 0 Then lPercentage = 100
				Return lPercentage
			End Get
		End Property

		Public ReadOnly Property Collapse As Boolean
			Get
				Return Me.EndTime.HasValue AndAlso Me.Options.CollapseWhenFinished
			End Get
		End Property

		Public Function Spawn(maxTicks As Integer, message As String, Optional options As ShellProgressBar.ProgressBarOptions = Nothing) As ShellProgressBar.ChildProgressBar
			' if this bar collapses all child progressbar will collapse
			If options?.CollapseWhenFinished = False AndAlso Me.Options.CollapseWhenFinished Then options.CollapseWhenFinished = True

			Dim pbar = New ShellProgressBar.ChildProgressBar(maxTicks, message, AddressOf Me.DisplayProgress, AddressOf Me.WriteLine, AddressOf Me.WriteErrorLine, If(options, Me.Options), Sub(d) Me.Grow(d))
			Me.Children.Add(pbar)
			Me.DisplayProgress()
			Return pbar
		End Function

		Public Function SpawnIndeterminate(message As String, Optional options As ShellProgressBar.ProgressBarOptions = Nothing) As ShellProgressBar.IndeterminateChildProgressBar
			' if this bar collapses all child progressbar will collapse
			If options?.CollapseWhenFinished = False AndAlso Me.Options.CollapseWhenFinished Then options.CollapseWhenFinished = True

			Dim pbar = New ShellProgressBar.IndeterminateChildProgressBar(message, AddressOf Me.DisplayProgress, AddressOf Me.WriteLine, AddressOf Me.WriteErrorLine, If(options, Me.Options), Sub(d) Me.Grow(d))
			Me.Children.Add(pbar)
			Me.DisplayProgress()
			Return pbar
		End Function

		Public MustOverride Sub WriteLine(message As String)
		Public MustOverride Sub WriteErrorLine(message As String)


		Public Sub Tick(Optional message As String = Nothing)
			Call System.Threading.Interlocked.Increment(Me._currentTick)
			Me.FinishTick(message)
		End Sub

		Public Sub Tick(newTickCount As Integer, Optional message As String = Nothing)
			Call System.Threading.Interlocked.Exchange(Me._currentTick, newTickCount)
			Me.FinishTick(message)
		End Sub

		Public Sub Tick(estimatedDuration As System.TimeSpan, Optional message As String = Nothing)
			Call System.Threading.Interlocked.Increment(Me._currentTick)
			Me._estimatedDuration = estimatedDuration

			Me.FinishTick(message)
		End Sub
		Public Sub Tick(newTickCount As Integer, estimatedDuration As System.TimeSpan, Optional message As String = Nothing)
			Call System.Threading.Interlocked.Exchange(Me._currentTick, newTickCount)
			Me._estimatedDuration = estimatedDuration

			Me.FinishTick(message)
		End Sub

		Private Sub FinishTick(message As String)
			If Not Equals(message, Nothing) Then Call System.Threading.Interlocked.Exchange(Me._message, message)

			If Me._currentTick >= Me._maxTicks Then
				Me.EndTime = System.DateTime.Now
				Me.OnDone()
			End If
			Me.DisplayProgress()
		End Sub

		Protected Shared Function GetDurationString(duration As System.TimeSpan) As String
			If duration.Days > 0 Then
				Return $"{duration.Days}D {duration.Hours:00}:{duration.Minutes:00}:{duration.Seconds:00}"
			End If
			Return $"{duration.Hours:00}:{duration.Minutes:00}:{duration.Seconds:00}"
		End Function
	End Class
End Namespace
