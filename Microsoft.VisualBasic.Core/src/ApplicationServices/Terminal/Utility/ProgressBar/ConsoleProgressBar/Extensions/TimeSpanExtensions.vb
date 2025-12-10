#Region "Microsoft.VisualBasic::8632ad96cf6afb127bb85c9a5aed68c8, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\Utility\ProgressBar\ConsoleProgressBar\Extensions\TimeSpanExtensions.vb"

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

    '   Total Lines: 80
    '    Code Lines: 42 (52.50%)
    ' Comment Lines: 32 (40.00%)
    '    - Xml Docs: 78.12%
    ' 
    '   Blank Lines: 6 (7.50%)
    '     File Size: 3.54 KB


    '     Module TimeSpanExtensions
    ' 
    '         Function: (+2 Overloads) ToStringAsSumarizedRemainingText, (+2 Overloads) ToStringWithAllHours
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

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports std = System.Math

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

            If (units = Convert.ToInt32(std.Round(ts.TotalDays))) > 1 Then
                Return $"{units} days"
            ElseIf Convert.ToInt32(std.Floor(ts.TotalDays)) = 1 Then
                Return $"a day"
            ElseIf (units = Convert.ToInt32(std.Round(ts.TotalHours))) > 1 Then
                Return $"{units} hours"
            ElseIf Convert.ToInt32(std.Floor(ts.TotalHours)) = 1 Then
                Return $"an hour"
            ElseIf (units = Convert.ToInt32(std.Round(ts.TotalMinutes))) > 1 Then
                Return $"{units} minutes"
            ElseIf Convert.ToInt32(std.Floor(ts.TotalMinutes)) = 1 Then
                Return $"a minute"
            ElseIf (units = Convert.ToInt32(std.Round(ts.TotalSeconds))) > 1 Then
                Return $"{units} seconds"
            ElseIf Convert.ToInt32(std.Floor(ts.TotalSeconds)) = 1 Then
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
