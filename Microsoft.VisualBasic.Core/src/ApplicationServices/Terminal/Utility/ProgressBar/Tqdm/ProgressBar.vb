Imports System.Text
Imports std = System.Math

Namespace ApplicationServices.Terminal.ProgressBar.Tqdm

    ''' <summary>
    ''' The ProgressBar class offers a customizable console progress bar for tracking and displaying the progress of iterative tasks. 
    ''' It features various themes, real-time updates, and supports both simple and exponential moving average rate calculations.
    ''' </summary>
    Public Class ProgressBar

        ' Parameterized configuration
        Private ReadOnly _useExponentialMovingAverage As Boolean
        Private ReadOnly _alpha As Double
        Private ReadOnly _width As Integer
        Private _printsPerSecond As Integer
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
        Private _lastSeconds As Double

        Public ReadOnly Property ElapsedSeconds As Double
            Get
                Return _stopWatch.Elapsed.TotalSeconds
            End Get
        End Property

        Public Property UpdateDynamicConfigs As Boolean = True
        Public Property FormatTaskCounter As Func(Of Integer, String)

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
            _period = 1

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
            ' config these options will force the progress bar show the 100% progress
            _printsPerSecond = 1
            _period = 1

            Call Progress(_total, _total)
            Call VBDebugger.EchoLine("")
            Call VBDebugger.WaitOutput()
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
            If Not UpdateDynamicConfigs Then
                ' make updates in constant period 
                If ElapsedSeconds - _lastSeconds < _printsPerSecond Then
                    Return
                Else
                    _lastSeconds = ElapsedSeconds
                End If
            End If

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
                If UpdateDynamicConfigs Then
                    _period = std.Min(std.Max(1, CInt(current / elapsed / _printsPerSecond)), 500000)
                End If

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

            If _useColor Then
                Call sb.Append(ChrW(27) & "[34m")
            End If
            If FormatTaskCounter Is Nothing Then
                Call sb.Append($"[ {current:N0} / {_total:N0} | ")
            Else
                Call sb.Append($"[ {_FormatTaskCounter(current)} / {_FormatTaskCounter(_total)} | ")
            End If

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

End Namespace