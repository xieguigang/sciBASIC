' Description: ProgressBar for Console Applications, with advanced features.
' Project site: https://github.com/iluvadev/ConsoleProgressBar
' Issues: https://github.com/iluvadev/ConsoleProgressBar/issues
' License (MIT): https://github.com/iluvadev/ConsoleProgressBar/blob/main/LICENSE
'
' Copyright (c) 2021, iluvadev, and released under MIT License.
'

Imports iluvadev.ConsoleProgressBar.Extensions
Imports System

Namespace iluvadev.ConsoleProgressBar
    Public Partial Class Text
        ''' <summary>
        ''' Definition for the Texts in a ProgressBar
        ''' </summary>
        Public Class TextBody
            ''' <summary>
            ''' Text in Body definition when ProgressBar is "Processing"
            ''' </summary>
            Public ReadOnly Property Processing As Element(Of String) = New Element(Of String)()

            ''' <summary>
            ''' Text in Body definition when ProgressBar is "Paused"
            ''' </summary>
            Public ReadOnly Property Paused As Element(Of String) = New Element(Of String)()

            ''' <summary>
            ''' Text in Body definition when ProgressBar is "Done"
            ''' </summary>
            Public ReadOnly Property Done As Element(Of String) = New Element(Of String)()

            ''' <summary>
            ''' Sets the Body Text visibility
            ''' </summary>
            ''' <param name="show"></param>
            ''' <returns></returns>
            Public Function SetVisible(show As Boolean) As TextBody
                Return SetVisible(Function(pb) show)
            End Function

            ''' <summary>
            ''' Sets the Body Text visibility
            ''' </summary>
            ''' <param name="showGetter"></param>
            ''' <returns></returns>
            Public Function SetVisible(showGetter As Func(Of ProgressBar, Boolean)) As TextBody
                Processing.SetVisible(showGetter)
                Paused.SetVisible(showGetter)
                Done.SetVisible(showGetter)
                Return Me
            End Function

            ''' <summary>
            ''' Sets the Body Text definition in all ProgressBar states ("Processing", "Paused", "Done")
            ''' </summary>
            ''' <param name="value"></param>
            ''' <returns></returns>
            Public Function SetValue(value As String) As TextBody
                Return SetValue(Function(pb) value)
            End Function

            ''' <summary>
            ''' Sets the Body Text definition in all ProgressBar states ("Processing", "Paused", "Done")
            ''' </summary>
            ''' <param name="valueGetter"></param>
            ''' <returns></returns>
            Public Function SetValue(valueGetter As Func(Of ProgressBar, String)) As TextBody
                Processing.SetValue(valueGetter)
                Paused.SetValue(valueGetter)
                Done.SetValue(valueGetter)
                Return Me
            End Function

            ''' <summary>
            ''' Sets the Body Text Foreground Color
            ''' </summary>
            ''' <param name="foregroundColor"></param>
            ''' <returns></returns>
            Public Function SetForegroundColor(foregroundColor As ConsoleColor) As TextBody
                Return SetForegroundColor(Function(pb) foregroundColor)
            End Function

            ''' <summary>
            ''' Sets the Body Text Foreground Color
            ''' </summary>
            ''' <param name="foregroundColorGetter"></param>
            ''' <returns></returns>
            Public Function SetForegroundColor(foregroundColorGetter As Func(Of ProgressBar, ConsoleColor)) As TextBody
                Processing.SetForegroundColor(foregroundColorGetter)
                Paused.SetForegroundColor(foregroundColorGetter)
                Done.SetForegroundColor(foregroundColorGetter)
                Return Me
            End Function

            ''' <summary>
            ''' Sets the Body Text Background Color
            ''' </summary>
            ''' <param name="backgroundColor"></param>
            ''' <returns></returns>
            Public Function SetBackgroundColor(backgroundColor As ConsoleColor) As TextBody
                Return SetBackgroundColor(Function(pb) backgroundColor)
            End Function

            ''' <summary>
            ''' Sets the Body Text Background Color
            ''' </summary>
            ''' <param name="backgroundColorGetter"></param>
            ''' <returns></returns>
            Public Function SetBackgroundColor(backgroundColorGetter As Func(Of ProgressBar, ConsoleColor)) As TextBody
                Processing.SetBackgroundColor(backgroundColorGetter)
                Paused.SetBackgroundColor(backgroundColorGetter)
                Done.SetBackgroundColor(backgroundColorGetter)
                Return Me
            End Function

            ''' <summary>
            ''' Ctor
            ''' </summary>
            Public Sub New()
                Processing.SetValue(Function(pb) If(pb.HasProgress, $"{pb.Value} of {pb.Maximum} in {pb.TimeProcessing.ToStringWithAllHours()}, remaining: {pb.TimeRemaining.ToStringAsSumarizedRemainingText()}", $"Processing... ({pb.Value} in {pb.TimeProcessing.ToStringWithAllHours()})")).SetForegroundColor(ConsoleColor.Cyan)

                Paused.SetValue(Function(pb) If(pb.HasProgress, $"Paused... Running time: {pb.TimeProcessing.ToStringWithAllHours()}", $"{pb.Value} of {pb.Maximum} in {pb.TimeProcessing.ToStringWithAllHours()} (paused)")).SetForegroundColor(ConsoleColor.DarkCyan)

                Done.SetValue("Done!").SetForegroundColor(ConsoleColor.DarkYellow)
            End Sub

            ''' <summary>
            ''' Gets the current Text Body definition by the ProgressBar context ("Processing", "Paused" or "Done")
            ''' </summary>
            ''' <param name="progressBar"></param>
            ''' <returns></returns>
            Public Function GetCurrentText(progressBar As ProgressBar) As Element(Of String)
                If progressBar Is Nothing Then
                    Return Nothing
                ElseIf progressBar.IsPaused Then
                    Return Paused
                ElseIf progressBar.IsDone Then
                    Return Done
                ElseIf progressBar.IsStarted Then
                    Return Processing
                Else
                    Return Nothing
                End If
            End Function
        End Class

    End Class
End Namespace
