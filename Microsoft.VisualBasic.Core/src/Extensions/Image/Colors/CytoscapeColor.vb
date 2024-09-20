#Region "Microsoft.VisualBasic::4961155af412b3ed26de1bfe5e85b8cf, Microsoft.VisualBasic.Core\src\Extensions\Image\Colors\CytoscapeColor.vb"

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

    '   Total Lines: 137
    '    Code Lines: 95 (69.34%)
    ' Comment Lines: 16 (11.68%)
    '    - Xml Docs: 68.75%
    ' 
    '   Blank Lines: 26 (18.98%)
    '     File Size: 4.39 KB


    '     Module HexColor
    ' 
    '         Function: DeciamlToHexadeciaml, GetNumberFromNotation, HexadecimaltoDecimal, HexToARGB, IsHexadecimal
    '                   IsNumber, ToHtmlColor
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports std = System.Math

Namespace Imaging

    Partial Module HexColor

#Region "CONVERSION FROM DECIMAL TO HEXADECIMAL AND VICE VERSA"

        Public Function HexadecimaltoDecimal(hexadecimal As String) As Integer
            Dim result As Integer = 0

            For i As Integer = 0 To hexadecimal.Length - 1
                result += Convert.ToInt32(
                    GetNumberFromNotation(hexadecimal(i)) * std.Pow(16, hexadecimal.Length - (i + 1)))
            Next

            Return Convert.ToInt32(result)
        End Function

        Private Function DeciamlToHexadeciaml(number As Integer) As String
            Dim hexvalues As String() = {
                "0", "1", "2", "3", "4", "5",
                "6", "7", "8", "9", "A", "B",
                "C", "D", "E", "F"
            }
            Dim result As String = "", final As String = ""
            Dim [rem] As Integer = 0, div As Integer = 0

            While True
                [rem] = (number Mod 16)
                result &= hexvalues([rem]).ToString()

                If number < 16 Then
                    Exit While
                End If

                number = (number \ 16)
            End While

            For i As Integer = (result.Length - 1) To 0 Step -1
                final &= result(i)
            Next

            Return final
        End Function

#End Region

        ''' <summary>
        ''' To HTML color
        ''' </summary>
        ''' <param name="color"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 2020-12-06
        ''' 
        ''' alpha value in html color is not supported, you will lost the 
        ''' alpha value from this function when your color value is 
        ''' translate to html color string.
        ''' </remarks>
        ''' 
        <DebuggerStepThrough>
        <Extension>
        Public Function ToHtmlColor(color As Color) As String
            'Dim RGBValue As Integer = color.ToArgb
            'Dim HexValue = DeciamlToHexadeciaml(RGBValue)
            'Return HexValue
            Dim colorString$ = String.Format("#{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B)
            Return colorString
        End Function

#Region "CHECKING TO FORMAT OF THE INPUT THAT WHETHER IT IS IN THE CORRECT FORMAT (DECIAML/HEXADECIMAL)"

        Private Function IsNumber(number As String) As Boolean
            If number.Length = 0 Then
                Return False
            End If

            For i As Integer = 0 To number.Length - 1
                If Not ([Char].IsDigit(number(i))) Then
                    Return False
                End If
            Next

            Return True
        End Function

        Private Function IsHexadecimal(hexadecimal As String) As Boolean
            If hexadecimal.Length = 0 Then
                Return False
            End If

            For i As Integer = 0 To hexadecimal.Length - 1
                If Not (([Char].IsDigit(hexadecimal(i))) OrElse
                    (hexadecimal(i) = "A"c) OrElse
                    (hexadecimal(i) = "B"c) OrElse
                    (hexadecimal(i) = "C"c) OrElse
                    (hexadecimal(i) = "D"c) OrElse
                    (hexadecimal(i) = "E"c) OrElse
                    (hexadecimal(i) = "F"c)) Then

                    Return False
                End If
            Next

            Return True
        End Function

#End Region

        Private Function GetNumberFromNotation(c As Char) As Integer
            If c = "A"c Then
                Return 10
            ElseIf c = "B"c Then
                Return 11
            ElseIf c = "C"c Then
                Return 12
            ElseIf c = "D"c Then
                Return 13
            ElseIf c = "E"c Then
                Return 14
            ElseIf c = "F"c Then
                Return 15
            End If

            Return Convert.ToInt32(c.ToString())
        End Function

        Public Function HexToARGB(HexValue As String, alpha As Integer) As Color
            Dim r As Color = Color.FromArgb(HexadecimaltoDecimal(HexValue))
            r = Color.FromArgb(alpha, r)
            Return r
        End Function
    End Module
End Namespace
