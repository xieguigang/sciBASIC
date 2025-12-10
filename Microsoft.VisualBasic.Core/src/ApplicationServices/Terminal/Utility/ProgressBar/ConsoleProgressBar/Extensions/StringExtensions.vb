#Region "Microsoft.VisualBasic::11e33c5d159fa82eaea365132998ef67, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\Utility\ProgressBar\ConsoleProgressBar\Extensions\StringExtensions.vb"

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

    '   Total Lines: 60
    '    Code Lines: 28 (46.67%)
    ' Comment Lines: 26 (43.33%)
    '    - Xml Docs: 73.08%
    ' 
    '   Blank Lines: 6 (10.00%)
    '     File Size: 2.61 KB


    '     Module StringExtensions
    ' 
    '         Function: AdaptToConsole, AdaptToMaxWidth
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
Imports std = System.Math

Namespace ApplicationServices.Terminal.ProgressBar.ConsoleProgressBar.Extensions
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
                maxWidth *= std.DivRem(value.Length, maxWidth, Nothing) + 1
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
