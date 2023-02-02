Imports System

Namespace ApplicationServices.Terminal.ProgressBar.ShellProgressBar
    Public Interface IProgressBar
        Inherits IDisposable
        Function Spawn(maxTicks As Integer, message As String, Optional options As ProgressBarOptions = Nothing) As ChildProgressBar

        Sub Tick(Optional message As String = Nothing)
        Sub Tick(newTickCount As Integer, Optional message As String = Nothing)

        Property MaxTicks As Integer
        Property Message As String

        ReadOnly Property Percentage As Double
        ReadOnly Property CurrentTick As Integer

        Property ForegroundColor As ConsoleColor

        ''' <summary>
        ''' This writes a new line above the progress bar to <see cref="Console.Out"/>.
        ''' Use <see cref="Message"/> to update the message inside the progress bar
        ''' </summary>
        Sub WriteLine(message As String)

        ''' <summary> This writes a new line above the progress bar to <see cref="Console.Error"/></summary>
        Sub WriteErrorLine(message As String)

        Function AsProgress(Of T)(Optional message As Func(Of T, String) = Nothing, Optional percentage As Func(Of T, Double?) = Nothing) As IProgress(Of T)
    End Interface
End Namespace
