#Region "Microsoft.VisualBasic::405bee02ccfc2064ac4ca7ef10a96f08, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\Utility\ProgressBar\ShellProgressBar\ProgressBar.vb"

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

    '   Total Lines: 458
    '    Code Lines: 355 (77.51%)
    ' Comment Lines: 17 (3.71%)
    '    - Xml Docs: 35.29%
    ' 
    '   Blank Lines: 86 (18.78%)
    '     File Size: 21.25 KB


    '     Class ProgressBar
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: AsProgress, DefaultConsoleWrite, NewIndentation
    ' 
    '         Sub: CondensedProgressBar, DisplayProgress, Dispose, DrawBottomHalfPrefix, DrawChildren
    '              DrawTopHalfPrefix, EnsureMainProgressBarVisible, Grow, GrowDrawingAreaBasedOnChildren, OnTimerTick
    '              ProgressBarBottomHalf, ProgressBarTopHalf, ResetToBottom, UpdateProgress, WriteConsoleLine
    '              WriteErrorLine, WriteLine
    '         Structure Indentation
    ' 
    '             Properties: Glyph
    ' 
    '             Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Collections.Concurrent
Imports System.Collections.Generic
Imports System.Linq
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Threading
Imports stdNum = System.Math

Namespace ApplicationServices.Terminal.ProgressBar.ShellProgressBar

    ''' <summary>
    ''' Visualize (concurrent) progress in your console application
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/Mpdreamz/shellprogressbar
    ''' </remarks>
    Public Class ProgressBar
        Inherits ProgressBarBase
        Implements IDisposable

        Private Shared ReadOnly IsWindows As Boolean = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)

        Private ReadOnly _originalColor As ConsoleColor
        Private ReadOnly _writeMessageToConsole As Func(Of ConsoleOutLine, Integer)
        Private ReadOnly _originalWindowTop As Integer
        Private ReadOnly _originalWindowHeight As Integer
        Private ReadOnly _startedRedirected As Boolean
        Private _originalCursorTop As Integer
        Private _isDisposed As Integer

        Private _timer As Timer
        Private _visibleDescendants As Integer = 0
        Private ReadOnly _displayProgressEvent As AutoResetEvent
        Private ReadOnly _displayProgress As Task

        Public Sub New(maxTicks As Integer, message As String, color As ConsoleColor)
            Me.New(maxTicks, message, New ProgressBarOptions With {
                .ForegroundColor = color
            })
        End Sub

        Public Sub New(maxTicks As Integer, message As String, Optional options As ProgressBarOptions = Nothing)
            MyBase.New(maxTicks, message, options)

            _writeMessageToConsole = If(Me.Options.WriteQueuedMessage, AddressOf DefaultConsoleWrite)
            _startedRedirected = Console.IsOutputRedirected

            Try
                _originalCursorTop = Console.CursorTop
                _originalWindowTop = Console.WindowTop
                _originalWindowHeight = Console.WindowHeight + _originalWindowTop
                _originalColor = Console.ForegroundColor
            Catch
                _startedRedirected = True
            End Try

            If Not _startedRedirected Then Console.CursorVisible = False

            If Me.Options.EnableTaskBarProgress Then SetState(TaskbarStates.Normal)

            If Me.Options.DisplayTimeInRealTime Then
                _timer = New Timer(Sub(s) OnTimerTick(), Nothing, 500, 500) 'draw once
            Else
                _timer = New Timer(Sub(s)
                                       _timer.Dispose()
                                       DisplayProgress()
                                   End Sub, Nothing, 0, 1000)
            End If

            _displayProgressEvent = New AutoResetEvent(False)
            _displayProgress = Task.Run(Sub()
                                            While _isDisposed = 0
                                                If Not _displayProgressEvent.WaitOne(TimeSpan.FromSeconds(10)) Then Continue While
                                                If _isDisposed > 0 Then Return
                                                Try
                                                    UpdateProgress()
                                                Catch
                                                    'don't want to crash background thread
                                                End Try
                                            End While
                                        End Sub)
        End Sub

        Protected Overridable Sub OnTimerTick()
            DisplayProgress()
        End Sub

        Protected Overrides Sub Grow(direction As ProgressBarHeight)
            Select Case direction
                Case ProgressBarHeight.Increment
                    Interlocked.Increment(_visibleDescendants)
                Case ProgressBarHeight.Decrement
                    Interlocked.Decrement(_visibleDescendants)
            End Select
        End Sub

        Private Sub EnsureMainProgressBarVisible(Optional extraBars As Integer = 0)
            Dim pbarHeight = If(Options.DenseProgressBar, 1, 2)
            Dim neededPadding = stdNum.Min(_originalWindowHeight - pbarHeight, (1 + extraBars) * pbarHeight)
            Dim difference = _originalWindowHeight - _originalCursorTop
            Dim write = If(difference <= neededPadding, stdNum.Max(0, stdNum.Max(neededPadding, difference)), 0)

            Dim written = 0
            While written < write
                Console.WriteLine()
                written += 1
            End While
            If written = 0 Then Return

            Console.CursorTop = _originalWindowHeight - written
            _originalCursorTop = Console.CursorTop - 1
        End Sub

        Private Sub GrowDrawingAreaBasedOnChildren()
            EnsureMainProgressBarVisible(_visibleDescendants)
        End Sub

        Private Structure Indentation
            Public Sub New(color As ConsoleColor, lastChild As Boolean)
                ConsoleColor = color
                Me.LastChild = lastChild
            End Sub

            Public ReadOnly Property Glyph As String
                Get
                    Return If(Not LastChild, "├─", "└─")
                End Get
            End Property

            Public ReadOnly ConsoleColor As ConsoleColor
            Public ReadOnly LastChild As Boolean
        End Structure

        Private Shared Sub CondensedProgressBar(percentage As Double, message As String, progressCharacter As Char, progressBackgroundCharacter As Char?, backgroundColor As ConsoleColor?, indentation As Indentation(), progressBarOnTop As Boolean)
            Dim depth = indentation.Length
            Dim messageWidth = 30
            Dim maxCharacterWidth = Console.WindowWidth - depth * 2 + 2
            Dim truncatedMessage = Excerpt(message, messageWidth - 2) & " "
            Dim width = Console.WindowWidth - depth * 2 + 2 - truncatedMessage.Length

            If Not String.IsNullOrWhiteSpace(ProgressBarOptions.ProgressMessageEncodingName) Then
                width = width + message.Length - Encoding.GetEncoding(ProgressBarOptions.ProgressMessageEncodingName).GetBytes(message).Length
            End If

            Dim newWidth = CInt(width * percentage / 100.0R)
            Dim progBar = New String(progressCharacter, newWidth)
            DrawBottomHalfPrefix(indentation, depth)
            Console.Write(truncatedMessage)
            Console.Write(progBar)
            If backgroundColor.HasValue Then
                Console.ForegroundColor = backgroundColor.Value
                Console.Write(New String(If(progressBackgroundCharacter, progressCharacter), width - newWidth))
            Else
                Console.Write(New String(" "c, width - newWidth))
            End If

            Console.ForegroundColor = indentation(depth - 1).ConsoleColor
        End Sub


        Private Shared Sub ProgressBarBottomHalf(percentage As Double, startDate As Date, endDate As Date?, message As String, indentation As Indentation(), progressBarOnBottom As Boolean, showEstimatedDuration As Boolean, estimatedDuration As TimeSpan, disableBottomPercentage As Boolean, percentageFormat As String)
            Dim depth = indentation.Length
            Dim maxCharacterWidth = Console.WindowWidth - depth * 2 + 2
            Dim duration = If(endDate, Date.Now) - startDate
            Dim durationString = GetDurationString(duration)

            If showEstimatedDuration Then durationString += $" / {GetDurationString(estimatedDuration)}"

            Dim column1Width = Console.WindowWidth - durationString.Length - depth * 2 + 2
            Dim column2Width = durationString.Length

            If Not String.IsNullOrWhiteSpace(ProgressBarOptions.ProgressMessageEncodingName) Then
                column1Width = column1Width + message.Length - Encoding.GetEncoding(ProgressBarOptions.ProgressMessageEncodingName).GetBytes(message).Length
            End If

            If progressBarOnBottom Then
                DrawTopHalfPrefix(indentation, depth)
            Else
                DrawBottomHalfPrefix(indentation, depth)
            End If

            Dim format = $"{{0, -{column1Width}}}{{1,{column2Width}}}"
            Dim percentageFormatedString = String.Format(percentageFormat, percentage)
            Dim truncatedMessage = Excerpt(percentageFormatedString & message, column1Width)

            If disableBottomPercentage Then
                truncatedMessage = Excerpt(message, column1Width)
            End If

            Dim formatted = String.Format(format, truncatedMessage, durationString)
            Dim m = formatted & New String(" "c, stdNum.Max(0, maxCharacterWidth - formatted.Length))
            Console.Write(m)
        End Sub

        Private Shared Sub DrawBottomHalfPrefix(indentation As Indentation(), depth As Integer)
            For i = 1 To depth - 1
                Dim ind = indentation(i)
                Console.ForegroundColor = indentation(i - 1).ConsoleColor
                If Not ind.LastChild Then
                    Console.Write(If(i = depth - 1, ind.Glyph, "│ "))
                Else
                    Console.Write(If(i = depth - 1, ind.Glyph, "  "))
                End If
            Next

            Console.ForegroundColor = indentation(depth - 1).ConsoleColor
        End Sub

        Private Shared Sub ProgressBarTopHalf(percentage As Double, progressCharacter As Char, progressBackgroundCharacter As Char?, backgroundColor As ConsoleColor?, indentation As Indentation(), progressBarOnTop As Boolean)
            Dim depth = indentation.Length
            Dim width = Console.WindowWidth - depth * 2 + 2

            If progressBarOnTop Then
                DrawBottomHalfPrefix(indentation, depth)
            Else
                DrawTopHalfPrefix(indentation, depth)
            End If

            Dim newWidth = CInt(width * percentage / 100.0R)
            Dim progBar = New String(progressCharacter, newWidth)
            Console.Write(progBar)
            If backgroundColor.HasValue Then
                Console.ForegroundColor = backgroundColor.Value
                Console.Write(New String(If(progressBackgroundCharacter, progressCharacter), width - newWidth))
            Else
                Console.Write(New String(" "c, width - newWidth))
            End If

            Console.ForegroundColor = indentation(depth - 1).ConsoleColor
        End Sub

        Private Shared Sub DrawTopHalfPrefix(indentation As Indentation(), depth As Integer)
            For i = 1 To depth - 1
                Dim ind = indentation(i)
                Console.ForegroundColor = indentation(i - 1).ConsoleColor
                If ind.LastChild AndAlso i <> depth - 1 Then
                    Console.Write("  ")
                Else
                    Console.Write("│ ")
                End If
            Next

            Console.ForegroundColor = indentation(depth - 1).ConsoleColor
        End Sub

        Protected Overrides Sub DisplayProgress()
            _displayProgressEvent.Set()
        End Sub

        Private ReadOnly _stickyMessages As ConcurrentQueue(Of ConsoleOutLine) = New ConcurrentQueue(Of ConsoleOutLine)()

        Public Overrides Sub WriteLine(message As String) 'Implements IProgressBar.WriteLine
            _stickyMessages.Enqueue(New ConsoleOutLine(message))
            DisplayProgress()
        End Sub
        Public Overrides Sub WriteErrorLine(message As String) 'Implements IProgressBar.WriteErrorLine
            ObservedError = True
            _stickyMessages.Enqueue(New ConsoleOutLine(message, [error]:=True))
            DisplayProgress()
        End Sub

        Private Sub UpdateProgress()
            Dim mainPercentage = Percentage

            If Options.EnableTaskBarProgress Then SetValue(mainPercentage, 100)

            ' write queued console messages, displayprogress is signaled straight after but
            ' just in case make sure we never write more then 5 in a display progress tick
            Dim m As ConsoleOutLine = Nothing
            Dim i = 0

            While i < 5 AndAlso _stickyMessages.TryDequeue(m)
                WriteConsoleLine(m)
                i += 1
            End While

            If _startedRedirected Then Return

            Console.CursorVisible = False
            Console.ForegroundColor = ForegroundColor

            GrowDrawingAreaBasedOnChildren()
            Dim cursorTop = _originalCursorTop
            Dim indentation = {New Indentation(ForegroundColor, True)}
            Dim TopHalf As Action =
                Sub()
                    ShellProgressBar.ProgressBar.ProgressBarTopHalf(mainPercentage,
                Me.Options.ProgressCharacter,
                    Me.Options.BackgroundCharacter,
                    Me.Options.BackgroundColor,
                    indentation,
                    Me.Options.ProgressBarOnBottom
                )
                End Sub



            If Options.DenseProgressBar Then
                CondensedProgressBar(mainPercentage, Message, Options.ProgressCharacter, Options.BackgroundCharacter, Options.BackgroundColor, indentation, Options.ProgressBarOnBottom)
            ElseIf Options.ProgressBarOnBottom Then
                ProgressBarBottomHalf(mainPercentage, _startDate, Nothing, Message, indentation, Options.ProgressBarOnBottom, Options.ShowEstimatedDuration, EstimatedDuration, Options.DisableBottomPercentage, Options.PercentageFormat)
                Console.SetCursorPosition(0, Interlocked.Increment(cursorTop))
                TopHalf()
            Else
                TopHalf()
                Console.SetCursorPosition(0, Interlocked.Increment(cursorTop))
                ProgressBarBottomHalf(mainPercentage, _startDate, Nothing, Message, indentation, Options.ProgressBarOnBottom, Options.ShowEstimatedDuration, EstimatedDuration, Options.DisableBottomPercentage, Options.PercentageFormat)
            End If

            DrawChildren(Children, indentation, cursorTop, Options.PercentageFormat)

            ResetToBottom(cursorTop)

            Console.SetCursorPosition(0, _originalCursorTop)
            Console.ForegroundColor = _originalColor

            If Not mainPercentage >= 100 Then Return
            _timer?.Dispose()
            _timer = Nothing
        End Sub

        Private Sub WriteConsoleLine(m As ConsoleOutLine)
            Dim resetString = New String(" "c, Console.WindowWidth)
            Console.Write(resetString)
            Console.Write(Microsoft.VisualBasic.Constants.vbCr)
            Dim foreground = Console.ForegroundColor
            Dim background = Console.BackgroundColor
            Dim written = _writeMessageToConsole(m)
            Console.ForegroundColor = foreground
            Console.BackgroundColor = background
            _originalCursorTop += written
        End Sub

        Private Shared Function DefaultConsoleWrite(line As ConsoleOutLine) As Integer
            If line.Error Then
                Console.Error.WriteLine(line.Line)
            Else
                Console.WriteLine(line.Line)
            End If
            Return 1
        End Function

        Private Sub ResetToBottom(ByRef cursorTop As Integer)
            Dim resetString = New String(" "c, Console.WindowWidth)
            Dim windowHeight = _originalWindowHeight
            If cursorTop >= windowHeight - 1 Then Return
            Do
                Console.Write(resetString)
            Loop While Interlocked.Increment(cursorTop) < windowHeight - 1
        End Sub

        Private Shared Sub DrawChildren(children As IEnumerable(Of ChildProgressBar), indentation As Indentation(), ByRef cursorTop As Integer, percentageFormat As String)
            Dim view = children.Where(Function(c) Not c.Collapse).[Select](Function(c, i) New With {c, i
            }).ToList()
            If Not view.Any() Then Return

            Dim windowHeight = Console.WindowHeight
            Dim lastChild = view.Max(Function(t) t.i)
            For Each tuple In view
                'Dont bother drawing children that would fall off the screen
                If cursorTop >= windowHeight - 2 Then Return

                Dim child = tuple.c
                Dim currentIndentation = New Indentation(child.ForegroundColor, tuple.i = lastChild)
                Dim childIndentation = NewIndentation(indentation, currentIndentation)

                Dim percentage = child.Percentage
                Console.ForegroundColor = child.ForegroundColor

                Dim TopHalf As Action = Sub()
                                            ShellProgressBar.ProgressBar.ProgressBarTopHalf(percentage,
                        child.Options.ProgressCharacter,
                        child.Options.BackgroundCharacter,
                        child.Options.BackgroundColor,
                        childIndentation,
                        child.Options.ProgressBarOnBottom
                    )
                                        End Sub



                Console.SetCursorPosition(0, Interlocked.Increment(cursorTop))

                If child.Options.DenseProgressBar Then
                    CondensedProgressBar(percentage, child.Message, child.Options.ProgressCharacter, child.Options.BackgroundCharacter, child.Options.BackgroundColor, childIndentation, child.Options.ProgressBarOnBottom)
                ElseIf child.Options.ProgressBarOnBottom Then
                    ProgressBarBottomHalf(percentage, child.StartDate, child.EndTime, child.Message, childIndentation, child.Options.ProgressBarOnBottom, child.Options.ShowEstimatedDuration, child.EstimatedDuration, child.Options.DisableBottomPercentage, percentageFormat)
                    Console.SetCursorPosition(0, Interlocked.Increment(cursorTop))
                    TopHalf()
                Else
                    TopHalf()
                    Console.SetCursorPosition(0, Interlocked.Increment(cursorTop))
                    ProgressBarBottomHalf(percentage, child.StartDate, child.EndTime, child.Message, childIndentation, child.Options.ProgressBarOnBottom, child.Options.ShowEstimatedDuration, child.EstimatedDuration, child.Options.DisableBottomPercentage, percentageFormat)
                End If

                DrawChildren(child.Children, childIndentation, cursorTop, percentageFormat)
            Next
        End Sub

        Private Shared Function NewIndentation(array As Indentation(), append As Indentation) As Indentation()
            Dim result = New Indentation(array.Length + 1 - 1) {}
            System.Array.Copy(array, result, array.Length)
            result(array.Length) = append
            Return result
        End Function

        Public Sub Dispose() Implements IDisposable.Dispose
            If Interlocked.CompareExchange(_isDisposed, 1, 0) <> 0 Then Return

            _timer?.Dispose()
            _timer = Nothing

            ' make sure background task is stopped before we clean up
            _displayProgressEvent.Set()
            _displayProgress.Wait()

            ' update one last time - needed because background task might have
            ' been already in progress before Dispose was called and it might
            ' have been running for a very long time due to poor performance
            ' of System.Console
            UpdateProgress()

            'make sure we pop all pending messages
            Dim m As ConsoleOutLine = Nothing

            While _stickyMessages.TryDequeue(m)
                WriteConsoleLine(m)
            End While

            If EndTime Is Nothing Then EndTime = Date.Now

            If Options.EnableTaskBarProgress Then SetState(TaskbarStates.NoProgress)

            Try
                For Each c In Children
                    c.Dispose()
                Next

            Catch
            End Try

            Try
                Dim pbarHeight = If(Options.DenseProgressBar, 1, 2)
                Dim openDescendantsPadding = _visibleDescendants * pbarHeight
                Dim newCursorTop = stdNum.Min(_originalWindowHeight, _originalCursorTop + pbarHeight + openDescendantsPadding)
                Console.CursorVisible = True
                Console.SetCursorPosition(0, newCursorTop)
            Catch
                'This is bad and I should feel bad, but i rather eat pbar exceptions in production then causing false negatives
            End Try
        End Sub

        Public Function AsProgress(Of T)(Optional message As Func(Of T, String) = Nothing, Optional percentage As Func(Of T, Double?) = Nothing) As IProgress(Of T) 'Implements IProgressBar.AsProgress
            Return New Progress(Of T)(Me, message, percentage)
        End Function
    End Class
End Namespace
