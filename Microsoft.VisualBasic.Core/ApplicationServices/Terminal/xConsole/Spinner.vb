Imports System.Drawing
Imports System.Threading

Namespace Terminal.xConsole

#Region "SPINNER ⌛"

    ''' <summary>
    ''' A list of spinners for your console ❤
    ''' </summary>
    Public Class Spinner
        Private counter As Integer = 0
        Private c As Char()

        ''' <summary>
        ''' List of available spinners (you can add new)
        ''' </summary>
        Public Spinners As New List(Of Char())() From {
                New Char() {"-"c, "\"c, "|"c, "/"c},
                New Char() {"▄"c, "■"c, "▀"c, "■"c},
                New Char() {"╔"c, "╗"c, "╝"c, "╚"c},
                New Char() {"."c, "·"c, "`"c, "·"c},
                New Char() {"1"c, "2"c, "3"c, "4"c, "5"c, "6"c, "7"c, "8"c, "9"c, "0"c}
            }

        ''' <summary>
        ''' looplooplooplooplooplooplooplooplooploop[...]
        ''' </summary>
        Private inLoop As Boolean = True

        ''' <summary>
        ''' The base string for spinning. {0} will display the spinner. COLOR is SUPPORTED! 🆒
        ''' </summary>
        Public SpinText As String = "{0} ^gLoading^!..."

        ''' <summary>
        ''' Initialize the spinner
        ''' </summary>
        ''' <param name="i">Index of the spinner to use</param>
        ''' <param name="txt">Base string. `{0} show the spinner`</param>
        Public Sub New(Optional i As Integer = 0, Optional txt As String = Nothing)
            If Not String.IsNullOrWhiteSpace(txt) Then
                SpinText = txt
            End If
            c = If((Spinners.Count > i), Spinners(i), Spinners(0))
        End Sub

        ''' <summary>
        ''' Initialize a custom spinner
        ''' </summary>
        ''' <param name="spinner">Set a custom spinner, no size limit.</param>
        Public Sub New(spinner As Char())
            c = spinner
        End Sub

        ''' <summary>
        ''' Breaks the spinner
        ''' </summary>
        Public Sub Break()
            inLoop = False
        End Sub

        Private StringSize As Integer = 0

        ''' <summary>
        ''' Turn the spin!
        ''' </summary>
        ''' <param name="time">Waiting time. Default 130 ms</param>
        ''' <returns>False if it has been stopped</returns>
        ''' <example>while(spinner.Turn());</example>
        Public Function Turn(Optional time As Integer = 130) As Boolean
            Dim [loop] = inLoop

            If [loop] Then
                counter += 1
                Dim wr As String = String.Format(SpinText, c(counter Mod c.Length), "wtf?")
                My.InnerQueue.AddToQueue(Sub()
                                             StringSize = xConsole.Print(wr)
                                             Dim left As Integer = Console.CursorLeft - StringSize
                                             If left < 0 Then
                                                 left = 0
                                             End If
                                             Console.SetCursorPosition(left, Console.CursorTop)
                                         End Sub)

                Thread.Sleep(time)
            Else
                My.InnerQueue.AddToQueue(Sub()
                                             Dim pos As New Point(Console.CursorLeft, Console.CursorTop)
                                             Console.Write(New String(" "c, StringSize))
                                             Console.SetCursorPosition(pos.X, pos.Y)
                                         End Sub)
            End If

            Console.CursorVisible = Not [loop]

            Return [loop]
        End Function

        Public Sub Run(Optional speed As Integer = 130)
            Do While inLoop
                Call Turn(speed)
                Call Thread.Sleep(10)
            Loop
        End Sub

        Public Sub RunTask(Optional speed As Integer = 130)
            Call Parallel.RunTask(Sub() Call Run(speed))
        End Sub
    End Class

#End Region
End Namespace