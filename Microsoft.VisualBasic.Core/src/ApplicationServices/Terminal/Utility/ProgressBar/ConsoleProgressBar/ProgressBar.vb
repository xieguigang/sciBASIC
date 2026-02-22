#Region "Microsoft.VisualBasic::d6540046021c907f04f4511532959096, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\Utility\ProgressBar\ConsoleProgressBar\ProgressBar.vb"

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

'   Total Lines: 547
'    Code Lines: 319 (58.32%)
' Comment Lines: 161 (29.43%)
'    - Xml Docs: 78.88%
' 
'   Blank Lines: 67 (12.25%)
'     File Size: 21.99 KB


'     Class ProgressBar
' 
'         Properties: [Step], CancelThread, Delay, ElementName, FixedInBottom
'                     HasProgress, IsDone, IsPaused, IsStarted, Layout
'                     MarqueeIncrement, MarqueePosition, Maximum, Percentage, ProgressStopwatch
'                     Tag, Text, TicksCompletedElements, TicksPerElement, TicksRemaining
'                     TimePerElement, TimeProcessing, TimeRemaining, Value
' 
'         Constructor: (+1 Overloads) Sub New
' 
'         Function: GetRenderActionsForProgressBarAndText
' 
'         Sub: [Resume], Dispose, Pause, (+2 Overloads) PerformStep, Render
'              SetValue, Start, ThreadAction, Unrender, UpdateMarqueePosition
'              WaitForExit, (+4 Overloads) WriteLine
' 
' 
' /********************************************************************************/

#End Region

' Description: ProgressBar for Console Applications, with advanced features.
' Project site: https://github.com/iluvadev/ConsoleProgressBar
' Issues: https://github.com/iluvadev/ConsoleProgressBar/issues
' License (MIT): https://github.com/iluvadev/ConsoleProgressBar/blob/main/LICENSE
'
' Copyright (c) 2021, iluvadev, and released under MIT License.
'

Imports System.Drawing
Imports System.Threading
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar.ConsoleProgressBar.Extensions
Imports std = System.Math

Namespace ApplicationServices.Terminal.ProgressBar.ConsoleProgressBar

    ''' <summary>
    ''' A ProgressBar for Console
    ''' </summary>
    Public Class ProgressBar : Implements IDisposable

        ''' <summary>
        ''' True if ProgressBar is Started
        ''' </summary>
        Dim _IsStarted As Boolean
        ''' <summary>
        ''' True if ProgressBar is Paused
        ''' </summary>
        Dim _IsPaused As Boolean

        Private _Layout As Layout = Nothing

        ''' <summary>
        ''' Layout of the ProgressBar
        ''' </summary>
        Public Property Layout As Layout
            Get
                _Layout = If(_Layout, New Layout)
                Return _Layout
            End Get
            Set(value As Layout)
                _Layout = value
            End Set
        End Property

        Private _Text As Text = Nothing
        ''' <summary>
        ''' Text definitions for the ProgressBar
        ''' </summary>
        Public Property Text As Text
            Get
                _Text = If(_Text, New Text)
                Return _Text
            End Get
            Set(value As Text)
                _Text = value
            End Set
        End Property

        ''' <summary>
        ''' Tag object
        ''' </summary>
        Public Property Tag As Object

        ''' <summary>
        ''' The Maximum value
        ''' Default = 100
        ''' </summary>
        Public Property Maximum As Integer? = 100

        Private _Value As Integer = 0
        ''' <summary>
        ''' The current Value
        ''' If Value is greater than Maximum, then updates Maximum value
        ''' </summary>
        Public Property Value As Integer
            Get
                Return _Value
            End Get
            Set(value As Integer)
                SetValue(value)
            End Set
        End Property

        ''' <summary>
        ''' Percentage of progress
        ''' </summary>
        Public ReadOnly Property Percentage As Integer?
            Get
                Return If(Maximum.HasValue, If(Maximum.Value <> 0, Value * 100 / Maximum.Value, 100), Nothing)
            End Get
        End Property

        ''' <summary>
        ''' Indicates if the ProgressBar has Progress defined (Maximum defined)
        ''' </summary>
        Public ReadOnly Property HasProgress As Boolean
            Get
                Return Maximum.HasValue
            End Get
        End Property

        ''' <summary>
        ''' The amount by which to increment the ProgressBar with each call to the PerformStep() method.
        ''' Default = 1
        ''' </summary>
        Public Property [Step] As Integer = 1

        ''' <summary>
        ''' The Name of the Curent Element
        ''' </summary>
        Public Property ElementName As String


        Private _FixedInBottom As Boolean = False
        ''' <summary>
        ''' True to Print the ProgressBar always in last Console Line
        ''' False to Print the ProgressBar fixed in Console (Current position at Starting)
        ''' You can Write at Console and ProgressBar will always be below your lines
        ''' Default = true
        ''' </summary>
        Public Property FixedInBottom As Boolean
            Get
                Return _FixedInBottom
            End Get
            Set(value As Boolean)
                If _FixedInBottom <> value Then
                    Unrender()
                    _FixedInBottom = value
                    Render()
                End If
            End Set
        End Property

        ''' <summary>
        ''' Delay for repaint and recalculate all ProgressBar
        ''' Default = 75
        ''' </summary>
        Public Property Delay As Integer = 75

        Public Property IsStarted As Boolean
            Get
                Return _IsStarted
            End Get
            Private Set(value As Boolean)
                _IsStarted = value
            End Set
        End Property

        Public Property IsPaused As Boolean
            Get
                Return _IsPaused
            End Get
            Private Set(value As Boolean)
                _IsPaused = value
            End Set
        End Property

        ''' <summary>
        ''' True if ProgresBar is Done: when disposing or Progress is finished
        ''' </summary>
        Public ReadOnly Property IsDone As Boolean
            Get
                Return CancelThread OrElse HasProgress AndAlso Value = Maximum
            End Get
        End Property

        ''' <summary>
        ''' Processing time (time paused excluded)
        ''' </summary>
        Public ReadOnly Property TimeProcessing As TimeSpan
            Get
                Return ProgressStopwatch.Elapsed
            End Get
        End Property

        ''' <summary>
        ''' Processing time per element (median)
        ''' </summary>
        Public ReadOnly Property TimePerElement As TimeSpan?
            Get
                Return If(TicksPerElement.HasValue, New TimeSpan(TicksPerElement.Value), CType(Nothing, TimeSpan?))
            End Get
        End Property

        ''' <summary>
        ''' Estimated time finish (to Value = Maximum)
        ''' </summary>
        Public ReadOnly Property TimeRemaining As TimeSpan?
            Get
                Return If(TicksRemaining.HasValue, New TimeSpan(TicksRemaining.Value), CType(Nothing, TimeSpan?))
            End Get
        End Property

        ''' <summary>
        ''' A Lock for Writing to Console
        ''' </summary>
        Public Shared ReadOnly ConsoleWriterLock As Object = New Object()

        Friend Property MarqueePosition As Integer = -1
        Private Property MarqueeIncrement As Integer = 1

        Private Property CancelThread As Boolean

        Private Property ProgressStopwatch As Stopwatch
        Private Property TicksCompletedElements As Long?
        Private ReadOnly Property TicksPerElement As Long?
            Get
                Return If(TicksCompletedElements.HasValue AndAlso Value > 0, TicksCompletedElements.Value / Value, Nothing)
            End Get
        End Property
        Private ReadOnly Property TicksRemaining As Long?
            Get
                If Not Maximum.HasValue OrElse Not TicksPerElement.HasValue OrElse Not TicksCompletedElements.HasValue Then Return Nothing
                Dim currentTicks = ProgressStopwatch.ElapsedTicks
                Dim currentElementTicks = currentTicks - TicksCompletedElements.Value
                Dim elementTicks As Long = If(currentElementTicks <= TicksPerElement.Value, TicksPerElement.Value, currentTicks / (Value + 1))
                Dim totalTicks = elementTicks * Maximum.Value
                Return std.Max(totalTicks - currentTicks, 0)
            End Get
        End Property

        Private _ConsoleRow As Integer = -1
        Private _NumberLastLinesWritten As Integer = -1

        Dim threadExit As Boolean = False
        Dim cursorPos As Point?

        ''' <summary>
        ''' Creates an instance of ConsoleProgressBar
        ''' </summary>
        ''' <param name="initialPosition">Initial position of the ProgressBar</param>
        ''' <param name="autoStart">True if ProgressBar starts automatically</param>
        Public Sub New(Optional initialPosition As Integer? = Nothing, Optional autoStart As Boolean = True)
            ProgressStopwatch = New Stopwatch()
            If initialPosition.HasValue Then _ConsoleRow = initialPosition.Value
            If autoStart Then Start()
        End Sub

        ''' <summary>
        ''' Starts the ProgressBar
        ''' </summary>
        Public Sub Start()
            If IsStarted Then
                [Resume]()
            Else
                Call (New Thread(AddressOf ThreadAction) With {
                        .IsBackground = True
                    }).Start()
            End If
        End Sub

        Private Sub ThreadAction()
            ProgressStopwatch.Start()
            IsStarted = True
            threadExit = False
            While App.Running AndAlso Not CancelThread
                If Not IsPaused Then
                    Try
                        UpdateMarqueePosition()
                        Render()
                        Call Task.Delay(Delay).Wait()
                    Catch
                    End Try
                End If
            End While
            threadExit = True
        End Sub

        ''' <summary>
        ''' wait for <see cref="ThreadAction()"/> exit
        ''' </summary>
        Private Sub WaitForExit()
            Do While App.Running AndAlso Not threadExit
                Call Thread.Sleep(10)
            Loop
        End Sub

        Public Sub SetLocation(x As Integer, y As Integer)
            cursorPos = New Point(x, y)
        End Sub

        ''' <summary>
        ''' Pauses the ProgressBar
        ''' </summary>
        Public Sub Pause()
            IsPaused = True
            ProgressStopwatch.Stop()
            Render()
        End Sub

        ''' <summary>
        ''' Resume the ProgresBar
        ''' </summary>
        Public Sub [Resume]()
            ProgressStopwatch.Start()
            IsPaused = False
            Render()
        End Sub

        ''' <summary>
        ''' Assigns the current Value, and optionally current ElementName and Tag
        ''' If Value is greater than Maximum, updates Maximum as Value
        ''' </summary>
        ''' <param name="value"></param>
        ''' <param name="elementName"></param>
        ''' <param name="tag"></param>
        Public Sub SetValue(value As Integer, Optional elementName As String = Nothing, Optional tag As Object = Nothing)
            If value > Maximum Then Maximum = value
            _Value = value
            TicksCompletedElements = If(value > 0, ProgressStopwatch.ElapsedTicks, CType(Nothing, Long?))

            If Not elementName Is Nothing Then Me.ElementName = elementName
            If tag IsNot Nothing Then Me.Tag = tag
        End Sub

        ''' <summary>
        ''' Advances the current position of the progress bar by the amount of the Step property
        ''' </summary>
        ''' <param name="elementName">The name of the new Element</param>
        ''' <param name="tag"></param>
        Public Sub PerformStep(Optional elementName As String = Nothing, Optional tag As Object = Nothing)
            PerformStep([Step], elementName, tag)
        End Sub

        ''' <summary>
        ''' Advances the current position of the progress bar by the amount of the Step property
        ''' </summary>
        ''' <param name="step">Step to perform</param>
        ''' <param name="elementName">The name of the new Element</param>
        ''' <param name="tag"></param>
        Public Sub PerformStep([step] As Integer, Optional elementName As String = Nothing, Optional tag As Object = Nothing)
            If Not elementName Is Nothing Then Me.ElementName = elementName
            If tag IsNot Nothing Then Me.Tag = tag
            Value += [step]
        End Sub
        ''' <summary>
        ''' WriteLine in Console when ProgressBar is running
        ''' </summary>
        Public Sub WriteLine()
            WriteLine("", Nothing, Nothing, True)
        End Sub
        ''' <summary>
        ''' WriteLine in Console when ProgressBar is running
        ''' </summary>
        ''' <param name="value"></param>
        Public Sub WriteLine(value As String)
            WriteLine(value, Nothing, Nothing, True)
        End Sub
        ''' <summary>
        ''' WriteLine in Console when ProgressBar is running
        ''' </summary>
        ''' <param name="value"></param>
        ''' <param name="truncateToOneLine"></param>
        Public Sub WriteLine(value As String, truncateToOneLine As Boolean)
            WriteLine(value, Nothing, Nothing, truncateToOneLine)
        End Sub
        ''' <summary>
        ''' WriteLine in Console when ProgressBar is running
        ''' </summary>
        ''' <param name="value"></param>
        ''' <param name="foregroundColor"></param>
        ''' <param name="backgroundColor"></param>
        ''' <param name="truncateToOneLine"></param>
        Public Sub WriteLine(value As String, Optional foregroundColor As ConsoleColor? = Nothing, Optional backgroundColor As ConsoleColor? = Nothing, Optional truncateToOneLine As Boolean = True)
            Dim actions As New List(Of Action)()

            If foregroundColor.HasValue Then actions.Add(Sub() Console.ForegroundColor = foregroundColor.Value)
            If backgroundColor.HasValue Then actions.Add(Sub() Console.BackgroundColor = backgroundColor.Value)
            actions.Add(Sub() Console.Write(value.AdaptToConsole(Not truncateToOneLine) & Environment.NewLine))

            SyncLock ConsoleWriterLock
                If foregroundColor.HasValue Then
                    Dim oldColor = Console.ForegroundColor
                    actions.Add(Sub() Console.ForegroundColor = oldColor)
                End If
                If backgroundColor.HasValue Then
                    Dim oldColor = Console.BackgroundColor
                    actions.Add(Sub() Console.BackgroundColor = oldColor)
                End If
                actions.ForEach(Sub(a) a.Invoke())
            End SyncLock

            'If FixedInBottom and we written over the ProgressBar -> Print it again
            If FixedInBottom AndAlso _NumberLastLinesWritten > 0 AndAlso Console.CursorTop >= _ConsoleRow Then Render()
        End Sub

        Private Function GetRenderLocation() As Point
            If cursorPos Is Nothing Then
                Return New Point(Console.CursorLeft, Console.CursorTop)
            Else
                Return cursorPos
            End If
        End Function

        ''' <summary>
        ''' Renders in Console the ProgressBar
        ''' </summary>
        Public Sub Render()
            Dim actionsProgressBar As List(Of Action) = GetRenderActionsForProgressBarAndText()
            Dim emptyLine As String = " ".AdaptToConsole()

            'Lock Write to console
            SyncLock ConsoleWriterLock
                Dim location = GetRenderLocation()
                Dim oldCursorLeft = location.X
                Dim oldCursorTop = location.Y
                Dim oldCursorVisible = Console.CursorVisible
                Dim oldForegroundColor = Console.ForegroundColor
                Dim oldBackgroundColor = Console.BackgroundColor

                'Hide Cursor
                Console.CursorVisible = False

                'Position
                If FixedInBottom Then
                    If _ConsoleRow < Console.WindowHeight - _NumberLastLinesWritten Then
                        Dim newConsoleRow = Console.WindowHeight - _NumberLastLinesWritten
                        If _NumberLastLinesWritten > 0 AndAlso _ConsoleRow >= 0 Then
                            'Clear old ProgressBar
                            Console.SetCursorPosition(0, _ConsoleRow)
                            For i = _ConsoleRow To newConsoleRow - 1
                                Console.WriteLine(emptyLine)
                            Next
                        End If
                        _ConsoleRow = newConsoleRow
                    End If
                    'if (oldCursorTop >= _ConsoleRow)
                    '{
                    '    //oldCursorTop is near or over: Keep 2 empty lines between Text and ProgressBar (avoid flickering)
                    '    Console.SetCursorPosition(0, oldCursorTop);
                    '    Console.WriteLine(emptyLine);
                    '    Console.WriteLine(emptyLine);
                    '    _ConsoleRow = oldCursorTop + 2;
                    '}

                    Dim scrollMargin As Integer = std.Max((Console.WindowHeight - _NumberLastLinesWritten) / 3, 2)
                    If _ConsoleRow - oldCursorTop <= scrollMargin / 2 Then
                        'oldCursorTop is near or over: Keep a margin between Text and ProgressBar (avoid flickering)
                        Console.SetCursorPosition(0, oldCursorTop)
                        For i = oldCursorTop To _ConsoleRow + _NumberLastLinesWritten - 1
                            Console.WriteLine(emptyLine)
                        Next
                        _ConsoleRow = oldCursorTop + scrollMargin
                    End If
                ElseIf _ConsoleRow < 0 Then
                    _ConsoleRow = oldCursorTop
                End If
                Console.SetCursorPosition(0, _ConsoleRow)

                'ProgressBar and Text
                actionsProgressBar.ForEach(Sub(a) a.Invoke())

                ' Restore Cursor Position, Colors and Cursor visible
                Console.SetCursorPosition(oldCursorLeft, oldCursorTop)
                Console.ForegroundColor = oldForegroundColor
                Console.BackgroundColor = oldBackgroundColor
                If oldCursorVisible Then Console.CursorVisible = oldCursorVisible
            End SyncLock
        End Sub

        ''' <summary>
        ''' Unrenders (remove) from Console last ProgressBar printed
        ''' </summary>
        Public Sub Unrender()
            If _ConsoleRow < 0 OrElse _NumberLastLinesWritten <= 0 Then Return

            Dim emptyLine As String = " ".AdaptToConsole()

            'Lock Write to console
            SyncLock ConsoleWriterLock
                Dim location = GetRenderLocation()
                Dim oldCursorLeft = location.X
                Dim oldCursorTop = location.Y
                Dim oldCursorVisible = Console.CursorVisible

                'Hide Cursor
                Console.CursorVisible = False

                Dim initialRow = _ConsoleRow
                If FixedInBottom Then initialRow = std.Max(_ConsoleRow, oldCursorTop)

                'Position
                Console.SetCursorPosition(0, initialRow)

                'Remove lines
                For i = initialRow To _ConsoleRow + _NumberLastLinesWritten - 1
                    Console.WriteLine(emptyLine)
                Next

                ' Restore Cursor Position and Cursor Visible
                Console.SetCursorPosition(oldCursorLeft, oldCursorTop)
                If oldCursorVisible Then Console.CursorVisible = oldCursorVisible
            End SyncLock
        End Sub

        Private Function GetRenderActionsForProgressBarAndText() As List(Of Action)
            Dim oldLinesWritten = _NumberLastLinesWritten
            Dim emptyLine As String = " ".AdaptToConsole()

            ' ProgressBar
            Dim list = Layout.GetRenderActions(Me)

            ' Text in same line
            Dim maxTextLenght = Console.BufferWidth - Layout.ProgressBarWidth
            If maxTextLenght >= 10 Then 'Text will be printed if there are 10 chars or more
                Dim text = Me.Text.Body.GetCurrentText(Me)
                If text IsNot Nothing Then list.AddRange(text.GetRenderActions(Me, Function(s) (" " & s).AdaptToMaxWidth(maxTextLenght)))
            End If
            list.Add(Sub() Console.Write(Environment.NewLine))
            _NumberLastLinesWritten = 1

            ' Descriptions
            Dim descriptionList = Text.Description.GetCurrentDefinitionList(Me)?.List?.Where(Function(d) d IsNot Nothing AndAlso d.GetVisible(Me))
            If descriptionList IsNot Nothing AndAlso descriptionList.Any() Then
                Dim indentationLen = If(Text.Description.Indentation.GetValue(Me)?.Length, 0)
                Dim maxDescLenght = Console.BufferWidth - indentationLen
                For Each description As Element(Of String) In descriptionList
                    If indentationLen > 0 Then list.AddRange(Text.Description.Indentation.GetRenderActions(Me))
                    list.AddRange(description.GetRenderActions(Me, Function(s) s.AdaptToMaxWidth(maxDescLenght) & Environment.NewLine))
                    _NumberLastLinesWritten += 1
                Next
            End If

            ' Clear old lines
            If oldLinesWritten > _NumberLastLinesWritten Then
                For i = 0 To oldLinesWritten - _NumberLastLinesWritten - 1
                    list.Add(Sub() Console.WriteLine(emptyLine))
                Next
            End If
            Return list
        End Function

        Private Sub UpdateMarqueePosition()
            Dim newProgressPosition = MarqueePosition + MarqueeIncrement
            If newProgressPosition < 0 OrElse newProgressPosition >= Layout.GetInnerWidth(Me) Then
                MarqueeIncrement *= -1
            End If

            MarqueePosition += MarqueeIncrement
        End Sub

        ''' <summary>
        ''' Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        ''' </summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            CancelThread = True
            Layout.Marquee.SetVisible(False)
            'UpdateRemainingTime();
            Render()
            If FixedInBottom AndAlso _NumberLastLinesWritten > 0 AndAlso _ConsoleRow >= 0 Then
                Console.CursorTop = _ConsoleRow + _NumberLastLinesWritten
            End If

            If ProgressStopwatch.IsRunning Then
                ProgressStopwatch.Stop()
            End If
            ProgressStopwatch.Reset()

            ' wait for progress bar finished the render task
            Call WaitForExit()
        End Sub
    End Class
End Namespace
