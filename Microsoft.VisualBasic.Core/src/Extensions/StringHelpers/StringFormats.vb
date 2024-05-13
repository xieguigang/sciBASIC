#Region "Microsoft.VisualBasic::49360e95b31c16054a62ca4622a87b6d, G:/GCModeller/src/runtime/sciBASIC#/Microsoft.VisualBasic.Core/src//Extensions/StringHelpers/StringFormats.vb"

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

    '   Total Lines: 125
    '    Code Lines: 83
    ' Comment Lines: 26
    '   Blank Lines: 16
    '     File Size: 4.49 KB


    ' Module StringFormats
    ' 
    '     Function: FormatTime, (+2 Overloads) Lanudry, (+2 Overloads) ReadableElapsedTime
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Terminal
Imports Microsoft.VisualBasic.Language.C
Imports std = System.Math

Public Module StringFormats

    Public Function nsize(x As Double) As String
        If x <= 0 Then
            Return "0"
        ElseIf x.IsNaNImaginary Then
            Return "n/a"
        ElseIf x < 500 Then
            Return CInt(x).ToString
        Else
            Return CInt(x / 1000).ToString & "K"
        End If
    End Function

    ''' <summary>
    ''' 对bytes数值进行格式自动优化显示
    ''' </summary>
    ''' <param name="bytes"></param>
    ''' <returns>经过自动格式优化过后的大小显示字符串</returns>
    Public Function Lanudry(bytes As Double) As String
        If bytes <= 0 Then
            Return "0 B"
        ElseIf bytes.IsNaNImaginary Then
            Return "n/a KB"
        ElseIf bytes < 1024 Then
            Return $"{CInt(bytes)} B"
        End If

        Dim symbols = {"B", "KB", "MB", "GB", "TB"}
        Dim exp = std.Floor(std.Log(bytes) / std.Log(1000))
        Dim symbol = symbols(exp)
        Dim val = (bytes / (1000 ^ std.Floor(exp)))

        Return sprintf($"%.2f %s", val, symbol)
    End Function

    ''' <summary>
    ''' ``days, hh:mm:ss.ms``
    ''' </summary>
    ''' <param name="t"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function FormatTime(t As TimeSpan,
                               Optional showMs As Boolean = True,
                               Optional showDays As Boolean = False) As String
        With t
            Dim dhms As String

            If showDays Then
                dhms = $"{ZeroFill(.Days, 2)} days, {ZeroFill(.Hours, 2)}:{ZeroFill(.Minutes, 2)}:{ZeroFill(.Seconds, 2)}"
            Else
                dhms = $"{ZeroFill(std.Ceiling(.TotalHours) - 1, 2)}:{ZeroFill(.Minutes, 2)}:{ZeroFill(.Seconds, 2)}"
            End If

            If showMs Then
                Return $"{dhms}.{ZeroFill(.Milliseconds, 3)}"
            Else
                Return dhms
            End If
        End With
    End Function

    <Extension>
    Public Function Lanudry(timespan As TimeSpan, Optional showMs As Boolean = True) As String
        If timespan < TimeSpan.FromSeconds(1) Then
            Return $"{timespan.TotalMilliseconds} ms"
        ElseIf timespan < TimeSpan.FromMinutes(1) Then
            Return $"{timespan.TotalSeconds.ToString("F1")} seconds"
        ElseIf timespan < TimeSpan.FromHours(1) Then
            Return $"{timespan.TotalMinutes.ToString("F2")} min"
        ElseIf timespan < TimeSpan.FromDays(1) Then
            Return $"{timespan.TotalHours.ToString("F2")} hours"
        Else
            Return timespan.FormatTime(showMs)
        End If
    End Function

    ''' <summary>
    ''' convert the ms value to human readable string
    ''' </summary>
    ''' <param name="span"><see cref="TimeSpan.TotalMilliseconds"/></param>
    ''' <param name="format"></param>
    ''' <param name="round"></param>
    ''' <returns>human readable time string, example as: 3.6s, 45min or 1.99h</returns>
    ''' 
    <Extension>
    Public Function ReadableElapsedTime(span As TimeSpan, Optional format$ = "%.3f%s", Optional round% = 3) As String
        Return ReadableElapsedTime(span.TotalMilliseconds, format, round)
    End Function

    ''' <summary>
    ''' convert the ms value to human readable string
    ''' </summary>
    ''' <param name="microtime"><see cref="TimeSpan.TotalMilliseconds"/></param>
    ''' <param name="format"></param>
    ''' <param name="round"></param>
    ''' <returns>human readable time string, example as: 3.6s, 45min or 1.99h</returns>
    Public Function ReadableElapsedTime(microtime&, Optional format$ = "%.3f%s", Optional round% = 3) As String
        Dim unit$
        Dim time!

        If microtime >= 1000 Then
            unit = "s"
            time = std.Round(microtime / 1000, round)

            If time >= 60 Then
                unit = "min"
                time = std.Round(time / 60, round)

                If time >= 60 Then
                    unit = "h"
                    time = std.Round(time / 60, round)

                    If time >= 24 Then
                        unit = "days"
                        time = std.Round(time / 24, round)
                    End If
                End If
            End If

            format = sprintf(format, time, unit)
        Else
            unit = "ms"
            time = microtime
            format = sprintf("%s%s", time, unit)
        End If

        Return format
    End Function
End Module
