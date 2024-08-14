#Region "Microsoft.VisualBasic::a2823af21793b0616418224d94b6ff5d, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\Utility\ProgressBar\Tqdm.vb"

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

'   Total Lines: 426
'    Code Lines: 227 (53.29%)
' Comment Lines: 144 (33.80%)
'    - Xml Docs: 75.00%
' 
'   Blank Lines: 55 (12.91%)
'     File Size: 20.98 KB


'     Module Tqdm
' 
'         Function: InternalWrap, Range, (+4 Overloads) Wrap
'         Class ProgressBar
' 
'             Constructor: (+1 Overloads) Sub New
'             Sub: [Step], Finish, PrintLine, (+2 Overloads) Progress, Reset
'                  SetLabel, SetTheme, SetThemeAscii, SetThemeBasic, SetThemePython
' 
' 
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Unit
Imports std = System.Math

Namespace ApplicationServices.Terminal.ProgressBar

    ''' <summary>
    ''' The Tqdm class offers utility functions to wrap collections and enumerables with a ProgressBar, 
    ''' providing a simple and effective way to track and display progress in console applications for various iterative operations.
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/shaltielshmid/TqdmSharp
    ''' </remarks>
    Public Module Tqdm

        ''' <summary>
        ''' The ProgressBar class offers a customizable console progress bar for tracking and displaying the progress of iterative tasks. 
        ''' It features various themes, real-time updates, and supports both simple and exponential moving average rate calculations.
        ''' </summary>
        Public Class ProgressBar
            ' Parameterized configuration
            Private ReadOnly _useExponentialMovingAverage As Boolean
            Private ReadOnly _alpha As Double
            Private ReadOnly _width As Integer
            Private ReadOnly _printsPerSecond As Integer
            Private ReadOnly _useColor As Boolean
            ' Total is set initially, but can be dynamically updated with each step
            Private _total As Integer
            Private _current As Integer

            ' State
            Private ReadOnly _stopWatch As Stopwatch
            Private ReadOnly _timeDeque As List(Of Double)
            Private ReadOnly _iterDeque As List(Of Integer)
            Private _startTime As Date
            Private _prevTime As Date
            Private _nUpdates As Integer = 0

            ' Dynamic configuration that updates as progress applies
            Private _period As Integer = 1
            Private _smoothCount As Integer = 50
            Private _prevIterations As Integer = 0
            Private _prevLength As Integer = 0

            ' Theme and output
            Private ReadOnly _rightPad As String = "|"
            Private _themeBars As Char()
            Private _label As String = ""

            Public ReadOnly Property ElapsedSeconds As Double
                Get
                    Return _stopWatch.Elapsed.TotalSeconds
                End Get
            End Property

            ''' <summary>
            ''' Initializes a new instance of the ProgressBar class.
            ''' </summary>
            ''' <param name="useExpMovingAvg">Whether to use exponential moving average for rate calculation.</param>
            ''' <param name="alpha">The smoothing factor for exponential moving average.</param>
            ''' <param name="total">The total number of iterations expected.</param>
            ''' <param name="width">The width of the progress bar in characters.</param>
            ''' <param name="printsPerSecond">The estimated number of updates to the progress bar per second.</param>
            ''' <param name="useColor">Indicates whether to use colored output for the progress bar.</param>
            ''' <remarks>The prints per second is not an absolute number, and gets constantly tuned as the process progresses.</remarks>
            Public Sub New(Optional useExpMovingAvg As Boolean = True,
                           Optional alpha As Double = 0.1,
                           Optional total As Integer = -1,
                           Optional width As Integer = 40,
                           Optional printsPerSecond As Integer = 10,
                           Optional useColor As Boolean = False)

                ' Update the state
                _stopWatch = Stopwatch.StartNew()
                _startTime = Date.Now
                _prevTime = Date.Now
                _timeDeque = New List(Of Double)()
                _iterDeque = New List(Of Integer)()

                _useExponentialMovingAverage = useExpMovingAvg
                _alpha = alpha
                _total = total
                _current = -1
                _width = width
                _printsPerSecond = printsPerSecond
                _useColor = useColor

                SetThemeAscii()

                If _themeBars Is Nothing Then
                    Throw New NullReferenceException("create progress bar theme data error!")
                End If
            End Sub

            ''' <summary>
            ''' Resets the progress bar to its initial state.
            ''' </summary>
            Public Sub Reset()
                ' Reset counters
                _stopWatch.Restart()
                _startTime = Date.Now
                _prevTime = Date.Now
                _prevIterations = 0

                ' Clear histories
                _timeDeque.Clear()
                _iterDeque.Clear()

                ' Reset config
                _period = 1
                _nUpdates = 0
                _current = -1
                _total = 0
                _label = ""
                _prevLength = 0
            End Sub

            ''' <summary>
            ''' Sets a label that appears at the end of the progress bar.
            ''' </summary>
            ''' <param name="text">The label text.</param>
            Public Sub SetLabel(text As String)
                _label = text
            End Sub

            ''' <summary>
            ''' Sets the progress bar theme to basic characters of spaces + '#'.
            ''' </summary>
            Public Sub SetThemeBasic()
                _themeBars = {" "c, " "c, " "c, " "c, " "c, " "c, " "c, " "c, "#"c}
            End Sub

            Public Const FullChar As Char = "█"c

            ''' <summary>
            ''' Sets the progress bar theme to ASCII characters.
            ''' </summary>
            Public Sub SetThemeAscii()
                _themeBars = {" "c, "."c, ":"c, "-"c, "="c, "≡"c, "#"c, "█"c, FullChar}
            End Sub

            ''' <summary>
            ''' Sets the progress bar theme to Python-style characters.
            ''' </summary>
            Public Sub SetThemePython()
                _themeBars = Enumerable.Range(&H2587, &H258F - &H2587).Reverse().Select(Function(i) ChrW(i)).Prepend(" "c).ToArray()
            End Sub

            ''' <summary>
            ''' Sets the progress bar theme using a custom array of characters.
            ''' </summary>
            ''' <param name="bars">An array of characters to use in the progress bar. Must be exactly 9 characters.</param>
            Public Sub SetTheme(bars As Char())
                If bars.Length <> 9 Then
                    Throw New ArgumentException("Must contain exactly 9 characters.", NameOf(bars))
                Else
                    _themeBars = bars
                End If
            End Sub

            ''' <summary>
            ''' Finalizes the progress bar display.
            ''' </summary>
            Public Sub Finish()
                Progress(_total, _total)
                VBDebugger.EchoLine("")
            End Sub

            ''' <summary>
            ''' Updates the progress bar with the current progress and total.
            ''' </summary>
            ''' <param name="current">The current progress.</param>
            ''' <param name="total">The total number of iterations expected. This will update the internal total counter given in the constructor.</param>
            Public Sub Progress(current As Integer, total As Integer)
                _total = total
                Progress(current)
            End Sub

            ''' <summary>
            ''' Updates the progress bar with the current progress.
            ''' </summary>
            ''' <param name="current">The current progress.</param>
            Public Sub Progress(current As Integer)
                _current = current
                ' _period is a number which is contantly tuned based on how often there are updates,
                ' to try and update the screen ~N times a second (parameter)
                If current Mod _period <> 0 Then Return

                _nUpdates += 1

                ' Figure out our current state - how long passed and how many iterations since the last update
                Dim elapsed = _stopWatch.Elapsed.TotalSeconds
                Dim iterations = current - _prevIterations
                _prevIterations = current

                Dim now = Date.Now
                Dim timeDiff = (now - _prevTime).TotalSeconds
                _prevTime = now

                ' In order to make the timing smoother, we average over the last N items (adjusted on the go)
                ' If we passed that number, just pop the top item
                If _timeDeque.Count >= _smoothCount Then _timeDeque.RemoveAt(0)
                If _iterDeque.Count >= _smoothCount Then _iterDeque.RemoveAt(0)
                _timeDeque.Add(timeDiff)
                _iterDeque.Add(iterations)

                ' Calculate the average rate of progress either as a simple progress / time or as an exponential
                ' moving average, with alpha as a parameter in the constructor. 
                Dim avgRate As Double
                If _useExponentialMovingAverage Then
                    avgRate = _iterDeque(0) / _timeDeque(0)
                    For i = 1 To _iterDeque.Count - 1
                        Dim r = _iterDeque(i) / _timeDeque(i)
                        avgRate = _alpha * r + (1 - _alpha) * avgRate
                    Next
                Else
                    Dim totalTime As Double = _timeDeque.Sum()
                    Dim totalIters As Integer = _iterDeque.Sum()
                    avgRate = totalIters / totalTime
                End If

                ' Auto-tune the period to try and only print N times a second.
                If _nUpdates > 10 Then
                    _period = std.Min(std.Max(1, CInt(current / elapsed / _printsPerSecond)), 500000)
                    _smoothCount = 25 * 3
                End If

                ' Calculate how much time remains and what percentage are we along
                Dim remaining = (_total - current) / avgRate
                Dim percent = current * 100.0 / _total

                ' If this will be the last update, then print a completed progress bar. 
                If _total - current <= _period Then
                    percent = 100
                    current = _total
                    remaining = 0
                End If

                ' Count what percentage of the bar we are printing. 
                ' Keep both a double and an int so we can calculate the relative remainder. 
                Dim fills = If(current = 0, 0, current / _total * _width)
                Dim ifills As Integer = fills

                ' Store the beginning of the line, so we can move back there for the next print
                Dim curCursorTop = Console.CursorTop

                ' Build our output string
                ' Start by typing "backspace" over the previous print, and then add a \r in case anything was added. 
                Dim sb = New StringBuilder()
                Dim offset As Integer

                ' Append the total number of filled bars
                If _useColor Then sb.Append(ChrW(27) & "[32m ")
                sb.Append(New String(_themeBars(8), ifills))
                ' If we aren't at the end, append the partial bar
                If current <> _total Then
                    offset = 8 * (fills - ifills)
                    If offset < 0 Then
                        offset = 0
                    End If
                    sb.Append(_themeBars(offset))
                End If

                ' Append the filler spaces and the padding on the right
                sb.Append(New String(_themeBars(0), _width - ifills))
                sb.Append(_rightPad)
                If _useColor Then sb.Append(ChrW(27) & "[1m" & ChrW(27) & "[31m")

                ' Print the percent with 1 fixed point, followed by the current / total numbers, and the elapsed/remaining time
                sb.Append($" {percent:F1}% ")
                If _useColor Then sb.Append(ChrW(27) & "[34m")
                sb.Append($"[ {current:N0} / {_total:N0} | ")
                sb.Append($"{StringFormats.ReadableElapsedTime(elapsed * 1000)} < {StringFormats.ReadableElapsedTime(remaining * 1000)} ] ")

                ' Finally, if there is one, print the label
                sb.Append(_label)
                sb.Append(" "c)
                If _useColor Then
                    sb.Append(ChrW(27) & "[0m" & ChrW(27) & "[32m" & ChrW(27) & "[0m ")
                End If

                VBDebugger.Echo(sb.ToString() & New String(" "c, std.Max(0, _prevLength - sb.Length)))

                Try
                    ' if (curCursorTop > 0) {
                    ' Move the cursor position back
                    ' }
                    Console.SetCursorPosition(0, curCursorTop)
                Catch
                End Try

                ' Store the length of the string so that we can clear it later
                _prevLength = sb.Length
            End Sub

            ''' <summary>
            ''' Advances the progress bar by one step, automatically updating the progress count internally.
            ''' This method is useful for scenarios where the progress is incremented in a regular manner
            ''' and eliminates the need for external progress tracking.
            ''' </summary>
            Public Sub [Step]()
                Progress(_current + 1)
            End Sub

            ''' <summary>
            ''' Clears the previous console line and prints a new line, ensuring subsequent prints appear on the next line.
            ''' </summary>
            ''' <param name="text">The text to be printed on the new line.</param>
            Public Sub PrintLine(text As String)
                ' Clear the previous line by resetting the cursor position and overwriting with spaces
                Console.Write(New String(" "c, std.Min(_prevLength, Console.BufferWidth - 1))) ' Clear the buffer
                Console.CursorLeft = 0

                ' Print the new line of text
                Console.WriteLine(text)
                _prevLength = 0
            End Sub
        End Class

        ''' <summary>
        ''' Wraps a collection with a progress bar for iteration, providing visual feedback on progress.
        ''' </summary>
        ''' <param name="collection">The collection to iterate over.</param>
        ''' <param name="width">The width of the progress bar.</param>
        ''' <param name="printsPerSecond">The update frequency of the progress bar.</param>
        ''' <param name="useColor">Indicates whether to use colored output for the progress bar.</param>
        ''' <returns>An enumerable that iterates over the collection with progress tracking.</returns> 
        Public Function Wrap(Of T)(collection As ICollection(Of T),
                                   Optional width As Integer = 40,
                                   Optional printsPerSecond As Integer = 10,
                                   Optional useColor As Boolean = False) As IEnumerable(Of T)

            Dim __ As ProgressBar = Nothing
            Return Wrap(collection, __, width, printsPerSecond, useColor)
        End Function

        ''' <summary>
        ''' Wraps an enumerable with a specified total count with a progress bar for iteration, providing visual feedback on progress.
        ''' </summary>
        ''' <param name="enumerable">The enumerable to iterate over.</param>
        ''' <param name="total">The total number of expected items in the enumerable.</param>
        ''' <param name="width">The width of the progress bar.</param>
        ''' <param name="printsPerSecond">The update frequency of the progress bar.</param>
        ''' <param name="useColor">Indicates whether to use colored output for the progress bar.</param>
        ''' <returns>An enumerable that iterates over the collection with progress tracking.</returns>
        Public Function Wrap(Of T)(enumerable As IEnumerable(Of T), total As Integer,
                                   Optional width As Integer = 40,
                                   Optional printsPerSecond As Integer = 10,
                                   Optional useColor As Boolean = False) As IEnumerable(Of T)

            Dim __ As ProgressBar = Nothing
            Return Wrap(enumerable, total, __, width, printsPerSecond, useColor)
        End Function

        ''' <summary>
        ''' Wraps a collection with a progress bar for iteration and provides the progress bar instance for external control (like custom labels).
        ''' </summary>
        ''' <param name="collection">The collection to iterate over.</param>
        ''' <param name="bar">The progress bar instance used for tracking.</param>
        ''' <param name="width">The width of the progress bar.</param>
        ''' <param name="printsPerSecond">The update frequency of the progress bar.</param>
        ''' <param name="useColor">Indicates whether to use colored output for the progress bar.</param>
        ''' <returns>An enumerable that iterates over the collection with progress tracking.</returns>
        ''' 
        <Extension>
        Public Function Wrap(Of T)(collection As ICollection(Of T), <Out> ByRef bar As ProgressBar,
                                   Optional width As Integer = 40,
                                   Optional printsPerSecond As Integer = 10,
                                   Optional useColor As Boolean = False) As IEnumerable(Of T)

            If collection Is Nothing Then
                Return New T() {}
            Else
                Return Wrap(collection, collection.Count, bar, width, printsPerSecond, useColor)
            End If
        End Function

        ''' <summary>
        ''' Wraps an enumerable with a specified total count with a progress bar for iteration and provides the progress bar instance for external control (like custom labels).
        ''' </summary>
        ''' <param name="enumerable">The enumerable to iterate over.</param>
        ''' <param name="total">The total number of items in the enumerable.</param>
        ''' <param name="bar">The progress bar instance used for tracking.</param>
        ''' <param name="width">The width of the progress bar.</param>
        ''' <param name="printsPerSecond">The update frequency of the progress bar.</param>
        ''' <param name="useColor">Indicates whether to use colored output for the progress bar.</param>
        ''' <returns>An enumerable that iterates over the collection with progress tracking.</returns>
        Public Function Wrap(Of T)(enumerable As IEnumerable(Of T), total As Integer, <Out> ByRef bar As ProgressBar,
                                   Optional width As Integer = 40,
                                   Optional printsPerSecond As Integer = 10,
                                   Optional useColor As Boolean = False) As IEnumerable(Of T)

            bar = New ProgressBar(total:=total, width:=width, printsPerSecond:=printsPerSecond, useColor:=useColor)
            Return InternalWrap(enumerable, total, bar)
        End Function

        Private Iterator Function InternalWrap(Of T)(enumerable As IEnumerable(Of T), total As Integer, bar As ProgressBar) As IEnumerable(Of T)
            Dim count = 0
            For Each item In enumerable
                bar.Progress(count, total)
                count += 1
                Yield item
            Next
            bar.Finish()
        End Function

        Public Delegate Function RequestStreamProgressLocation(Of T)(ByRef getOffset As Long, bar As ProgressBar) As T

        Public Iterator Function WrapStreamReader(Of T)(bytesOfStream As Long, request As RequestStreamProgressLocation(Of T)) As IEnumerable(Of T)
            Dim offset As Long = Scan0
            Dim page_unit As ByteSize = ByteSize.B

            If bytesOfStream > 2 * ByteSize.GB Then
                bytesOfStream /= ByteSize.KB
                page_unit = "KB"
            ElseIf bytesOfStream > 2 * 1024 * ByteSize.GB Then
                bytesOfStream /= ByteSize.MB
                page_unit = "MB"
            End If

            Dim bar As New ProgressBar(total:=bytesOfStream)

            Do While offset < bytesOfStream
                bar.Progress(offset / page_unit, bytesOfStream)
                bar.SetLabel(StringFormats.Lanudry(offset / (bar.ElapsedSeconds + 1)) & "/s")

                Yield request(offset, bar)
            Loop

            Call bar.Finish()
        End Function

        ''' <summary>
        ''' Generates a sequence of integral numbers within a specified range.
        ''' </summary>
        ''' <param name="start">The value of the first integer in the sequence.</param>
        ''' <param name="count">The number of sequential integers to generate.</param>
        ''' <param name="bar"></param>
        ''' <param name="width"></param>
        ''' <param name="printsPerSecond"></param>
        ''' <param name="useColor"></param>
        ''' <returns>a range of sequential integral numbers.</returns>
        Public Function Range(start As Integer, count As Integer,
                              <Out>
                              Optional ByRef bar As ProgressBar = Nothing,
                              Optional width As Integer = 40,
                              Optional printsPerSecond As Integer = 10,
                              Optional useColor As Boolean = False) As IEnumerable(Of Integer)

            Return Enumerable.Range(start, count).ToArray.Wrap(bar,
                width:=width,
                printsPerSecond:=printsPerSecond,
                useColor:=useColor
            )
        End Function
    End Module
End Namespace
