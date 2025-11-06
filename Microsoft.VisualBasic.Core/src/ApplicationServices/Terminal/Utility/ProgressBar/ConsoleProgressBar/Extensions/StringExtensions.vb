' Description: ProgressBar for Console Applications, with advanced features.
' Project site: https://github.com/iluvadev/ConsoleProgressBar
' Issues: https://github.com/iluvadev/ConsoleProgressBar/issues
' License (MIT): https://github.com/iluvadev/ConsoleProgressBar/blob/main/LICENSE
'
' Copyright (c) 2021, iluvadev, and released under MIT License.
'

Imports System
Imports System.Runtime.CompilerServices

Namespace iluvadev.ConsoleProgressBar.Extensions
    ''' <summary>
    ''' Extensions for String
    ''' </summary>
    Public Module StringExtensions
        ''' <summary>
        ''' Returns a string that occupy all console line/s
        ''' </summary>
        ''' <param name="value">The string to write in console</param>
        ''' <param name="allowMultipleLines">To allow print the string in muliple lines or only in one:
        '''     True: The text can be represented in more than one Console line (fill spaces to the end of last line)
        '''     False: The text must be represented in only ONE line (truncate to fit or fill spaces to the end of line)
        ''' </param>
        ''' <returns></returns>
        <Extension()>
        Public Function AdaptToConsole(value As String, Optional allowMultipleLines As Boolean = True) As String
            Dim maxWidth = Console.BufferWidth

            If allowMultipleLines Then
                maxWidth *= Math.DivRem(value.Length, maxWidth, Nothing) + 1
            End If

            Return value.AdaptToMaxWidth(maxWidth)
        End Function

        ''' <summary>
        ''' Returns a string with exactly maxChars: Truncates string value or fill with spaces to fits exact length
        ''' </summary>
        ''' <param name="value"></param>
        ''' <param name="maxWidth"></param>
        ''' <param name="append">Text appended when it is truncated. Default: "..."</param>
        ''' <returns></returns>
        <Extension()>
        Public Function AdaptToMaxWidth(value As String, maxWidth As Integer, Optional append As String = "...") As String
            value = If(value Is Nothing, "", value)
            Dim len = value.Length

            If maxWidth <= 0 Then
                Return ""
            ElseIf len = maxWidth Then
                Return value
            ElseIf len < maxWidth Then
                Return value.PadRight(maxWidth)
            Else
                Return value.Substring(0, maxWidth - append.Length) & append
            End If
        End Function
    End Module
End Namespace
