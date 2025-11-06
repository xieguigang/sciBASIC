' Description: ProgressBar for Console Applications, with advanced features.
' Project site: https://github.com/iluvadev/ConsoleProgressBar
' Issues: https://github.com/iluvadev/ConsoleProgressBar/issues
' License (MIT): https://github.com/iluvadev/ConsoleProgressBar/blob/main/LICENSE
'
' Copyright (c) 2021, iluvadev, and released under MIT License.
'

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language

Namespace ApplicationServices.Terminal.ProgressBar.ConsoleProgressBar.Extensions
    ''' <summary>
    ''' TimeSpan extensions
    ''' </summary>
    Public Module TimeSpanExtensions
        ''' <summary>
        ''' Gets a textual Sumarized for remaining time: X days, or Y hours, or Z minutes, etc.
        ''' </summary>
        ''' <param name="ts"></param>
        ''' <returns></returns>
        <Extension()>
        Public Function ToStringAsSumarizedRemainingText(ts As TimeSpan?) As String
            Return If(ts.HasValue, ts.Value.ToStringAsSumarizedRemainingText, "unknown")
        End Function

        ''' <summary>
        ''' Gets a textual Sumarized for remaining time: X days, or Y hours, or Z minutes, etc.
        ''' </summary>
        ''' <param name="ts"></param>
        ''' <returns></returns>
        <Extension()>
        Public Function ToStringAsSumarizedRemainingText(ts As TimeSpan) As String
            Dim units As Value(Of Integer) = 0

            If (units = Convert.ToInt32(Math.Round(ts.TotalDays))) > 1 Then
                Return $"{units} days"
            ElseIf Convert.ToInt32(Math.Floor(ts.TotalDays)) = 1 Then
                Return $"a day"
            ElseIf (units = Convert.ToInt32(Math.Round(ts.TotalHours))) > 1 Then
                Return $"{units} hours"
            ElseIf Convert.ToInt32(Math.Floor(ts.TotalHours)) = 1 Then
                Return $"an hour"
            ElseIf (units = Convert.ToInt32(Math.Round(ts.TotalMinutes))) > 1 Then
                Return $"{units} minutes"
            ElseIf Convert.ToInt32(Math.Floor(ts.TotalMinutes)) = 1 Then
                Return $"a minute"
            ElseIf (units = Convert.ToInt32(Math.Round(ts.TotalSeconds))) > 1 Then
                Return $"{units} seconds"
            ElseIf Convert.ToInt32(Math.Floor(ts.TotalSeconds)) = 1 Then
                Return $"a second"
            Else
                Return "a moment"
            End If
        End Function

        ''' <summary>
        ''' Converts a TimeSpan to String, showing all hours
        ''' </summary>
        ''' <param name="ts"></param>
        ''' <param name="includeMilliseconds"></param>
        ''' <returns></returns>
        <Extension()>
        Public Function ToStringWithAllHours(ts As TimeSpan?, Optional includeMilliseconds As Boolean = True) As String
            Return If(ts.HasValue, ts.Value.ToStringWithAllHours(includeMilliseconds), "unknown")
        End Function

        ''' <summary>
        ''' Converts a TimeSpan to String, showing all hours
        ''' </summary>
        ''' <param name="ts"></param>
        ''' <param name="includeMilliseconds"></param>
        ''' <returns></returns>
        <Extension()>
        Public Function ToStringWithAllHours(ts As TimeSpan, Optional includeMilliseconds As Boolean = True) As String
            Return If(includeMilliseconds, $"{ts.TotalHours:F0}{ts:\:mm\:ss\.fff}", $"{ts.TotalHours:F0}{ts:\:mm\:ss}")
        End Function
    End Module
End Namespace
