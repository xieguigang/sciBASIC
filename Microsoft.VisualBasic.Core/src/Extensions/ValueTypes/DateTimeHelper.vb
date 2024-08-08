#Region "Microsoft.VisualBasic::d8044f13a2b278a3e29adc9d65224415, Microsoft.VisualBasic.Core\src\Extensions\ValueTypes\DateTimeHelper.vb"

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

    '   Total Lines: 228
    '    Code Lines: 139 (60.96%)
    ' Comment Lines: 51 (22.37%)
    '    - Xml Docs: 84.31%
    ' 
    '   Blank Lines: 38 (16.67%)
    '     File Size: 7.90 KB


    '     Enum TimeScales
    ' 
    '         Day, Hour, Millisecond, Minute, Month
    '         Second, Year
    ' 
    '  
    ' 
    ' 
    ' 
    '     Module DateTimeHelper
    ' 
    '         Properties: MonthList
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: DateSeq, FillDateZero, FromMilliseconds, FromUnixTimeStamp, GetMonthInteger
    '                   IsEmpty, ReadableElapsedTime, ToDate, UnixTimeStamp, UnixTimeStampMillis
    '                   YYMMDD
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language.C
Imports std = System.Math

Namespace ValueTypes

    Public Enum TimeScales As Integer
        Millisecond
        Second
        Minute
        Hour
        Day
        Month
        Year
    End Enum

    Public Module DateTimeHelper

        ''' <summary>
        ''' List of month names and its <see cref="Integer"/> value in a year
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property MonthList As Dictionary(Of String, Integer)

        Sub New()
            MonthList = New Dictionary(Of String, Integer)

            MonthList.Add("January", 1)
            MonthList.Add("Jan", 1)

            MonthList.Add("February", 2)
            MonthList.Add("Feb", 2)

            MonthList.Add("March", 3)
            MonthList.Add("Mar", 3)

            MonthList.Add("April", 4)
            MonthList.Add("Apr", 4)

            MonthList.Add("May", 5)

            MonthList.Add("June", 6)
            MonthList.Add("Jun", 6)

            MonthList.Add("July", 7)
            MonthList.Add("Jul", 7)

            MonthList.Add("August", 8)
            MonthList.Add("Aug", 8)

            MonthList.Add("September", 9)
            MonthList.Add("Sep", 9)

            MonthList.Add("October", 10)
            MonthList.Add("Oct", 10)

            MonthList.Add("November", 11)
            MonthList.Add("Nov", 11)

            MonthList.Add("December", 12)
            MonthList.Add("Dec", 12)
        End Sub

        ''' <summary>
        ''' 从全称或者简称解析出月份的数字
        ''' </summary>
        ''' <param name="mon">大小写不敏感</param>
        ''' <returns></returns>
        Public Function GetMonthInteger(mon As String) As Integer
            If Not MonthList.ContainsKey(mon) Then
                For Each k As String In MonthList.Keys
                    If String.Equals(mon, k, StringComparison.OrdinalIgnoreCase) Then
                        Return MonthList(k)
                    End If
                Next

                Return -1
            Else
                Return MonthList(mon)
            End If
        End Function

        ''' <summary>
        ''' 00
        ''' </summary>
        ''' <param name="d"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function FillDateZero(d As Integer) As String
            Return If(d >= 10, d, "0" & d)
        End Function

        ''' <summary>
        ''' 枚举出在<paramref name="start"/>到<paramref name="ends"/>这个时间窗里面的所有日期，单位为天
        ''' </summary>
        ''' <param name="start"></param>
        ''' <param name="ends"></param>
        ''' <returns>返回值里面包含有起始和结束的日期</returns>
        Public Iterator Function DateSeq(start As Date, ends As Date) As IEnumerable(Of Date)
            Do While start <= ends
                ' Yield $"{start.Year}-{If(start.Month < 10, "0" & start.Month, start.Month)}-{If(start.Day < 10, "0" & start.Day, start.Day)}"
                Yield start
                start = start.AddDays(1)
            Loop
        End Function

        ''' <summary>
        ''' yyyy-mm-dd
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        <Extension>
        Public Function YYMMDD(x As Date) As String
            Dim mm$ = If(x.Month < 10, "0" & x.Month, x.Month)
            Dim dd$ = If(x.Day < 10, "0" & x.Day, x.Day)
            Return $"{x.Year}-{mm}-{dd}"
        End Function

        ''' <summary>
        ''' Convert <see cref="DateTime"/> to unix time stamp
        ''' </summary>
        ''' <param name="time"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function UnixTimeStamp(time As DateTime) As Double
            Static ZERO As New DateTime(1970, 1, 1, 0, 0, 0)
            Return (time.ToUniversalTime - ZERO).TotalSeconds
        End Function

        Public Function UnixTimeStampMillis(time As DateTime) As Double
            Static ZERO As New DateTime(1970, 1, 1, 0, 0, 0)
            Return (time.ToUniversalTime - ZERO).TotalMilliseconds
        End Function

        ''' <summary>
        ''' 将Unix时间戳转换为可读的日期
        ''' </summary>
        ''' <param name="unixDateTime"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function FromUnixTimeStamp(unixDateTime As Long) As Date
#If NET_48 = 1 Or netcore5 = 1 Then
            Return DateTimeOffset _
                .FromUnixTimeSeconds(unixDateTime) _
                .DateTime _
                .ToLocalTime()
#Else
            ' Unix timestamp Is seconds past epoch
            Dim dtDateTime As New DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
            dtDateTime = dtDateTime.AddSeconds(unixDateTime).ToLocalTime()
            Return dtDateTime
#End If
        End Function

        Public Function FromMilliseconds(milliseconds As Long) As Date
            Dim start As New DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            Dim [date] = start.AddMilliseconds(milliseconds).ToLocalTime()

            Return [date]
        End Function

        Const ZeroDate1$ = "0001-01-01, 00:00:00"
        Const ZeroDate2$ = "0000-00-00, 00:00:00"
        ''' <summary>
        ''' 对于unix timestamp而言，这个日期是零
        ''' </summary>
        Const ZeroDate3$ = "1970-01-01, 08:00:00"

        <Extension>
        Public Function IsEmpty(time As Date, Optional unixTimestamp As Boolean = False) As Boolean
            Dim ts = time.FormatTime()

            If ts = ZeroDate1 OrElse ts = ZeroDate2 Then
                Return True
            ElseIf unixTimestamp AndAlso ts = ZeroDate3 Then
                Return True
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="microtime">xxx ms</param>
        ''' <param name="format"></param>
        ''' <param name="round"></param>
        ''' <returns></returns>
        Public Function ReadableElapsedTime(microtime&, Optional format$ = "%.3f%s", Optional round% = 3) As String
            Dim unit$
            Dim time!

            If microtime >= 1000 Then
                unit = "s"
                time = std.Round(microtime / 1000, round)

                If time >= 60 Then
                    unit = "min"
                    time = std.Round(time / 60, round)
                End If

                format = sprintf(format, time, unit)
            Else
                unit = "ms"
                time = microtime
                format = sprintf("%s%s", time, unit)
            End If

            Return format
        End Function

        ' var s = "1364835180000-0700"
        ' alert(toDate(s)); // Tue Apr 02 2013 09:53:00 GMT+1000 (EST)

        ''' <summary>
        ''' Parse date string in json value format.
        ''' </summary>
        ''' <param name="s">Where s Is a time value with offset</param>
        ''' <returns></returns>
        Public Function ToDate(s As String) As Date
            ' Include factor to convert mins to ms in sign
            Dim sign = If(s.IndexOf("-") > -1, 60000.0, -60000.0)
            Dim twoParts = s.StringSplit("[\+\-]")
            Dim l As Integer = twoParts(1).Length
            ' Convert offset in milliseconds
            Dim offset = sign * twoParts(1).Substring(l - 2, 2) + sign * twoParts(1).Substring(l - 4, 2) * 60

            ' Add offset to time value to get UTC And create date object 
            Return New Date(Long.Parse(twoParts(0)) + offset)
        End Function
    End Module
End Namespace
