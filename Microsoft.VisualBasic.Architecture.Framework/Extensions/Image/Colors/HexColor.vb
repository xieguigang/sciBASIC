#Region "Microsoft.VisualBasic::a32ae8c0021ded70c151d20d34690584, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Extensions\Image\Colors\HexColor.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Drawing
Imports System.Globalization
Imports System.Runtime.CompilerServices

Namespace Imaging

    ''' <summary>
    ''' Convert hex color string to RGB color
    ''' </summary>
    ''' <remarks>http://stackoverflow.com/questions/13356486/convert-hex-color-string-to-rgb-color</remarks>
    Public Module HexColor

        Public Function ConvertToRbg(ByVal HexColor As String) As Color
            Dim Red As String
            Dim Green As String
            Dim Blue As String
            HexColor = Replace(HexColor, "#", "")
            Red = Val("&H" & Mid(HexColor, 1, 2))
            Green = Val("&H" & Mid(HexColor, 3, 2))
            Blue = Val("&H" & Mid(HexColor, 5, 2))
            Return Color.FromArgb(Red, Green, Blue)
        End Function

        Public Function HexToColor(ByVal hexColor As String) As Color
            If hexColor.IndexOf("#"c) <> -1 Then
                hexColor = hexColor.Replace("#", "")
            End If
            Dim red As Integer = 0
            Dim green As Integer = 0
            Dim blue As Integer = 0
            If hexColor.Length = 6 Then
                red = Integer.Parse(hexColor.Substring(0, 2), NumberStyles.AllowHexSpecifier)
                green = Integer.Parse(hexColor.Substring(2, 2), NumberStyles.AllowHexSpecifier)
                blue = Integer.Parse(hexColor.Substring(4, 2), NumberStyles.AllowHexSpecifier)
            ElseIf hexColor.Length = 3 Then
                red = Integer.Parse(hexColor(0).ToString() + hexColor(0).ToString(), NumberStyles.AllowHexSpecifier)
                green = Integer.Parse(hexColor(1).ToString() + hexColor(1).ToString(), NumberStyles.AllowHexSpecifier)
                blue = Integer.Parse(hexColor(2).ToString() + hexColor(2).ToString(), NumberStyles.AllowHexSpecifier)
            End If
            Return Color.FromArgb(red, green, blue)
        End Function

        Public Function hexToRbgNew(ByVal Hex As String) As Color
            Hex = Replace(Hex, "#", "")
            Dim red As String = "&H" & Hex.Substring(0, 2)
            Hex = Replace(Hex, red, "", , 1)
            Dim green As String = "&H" & Hex.Substring(0, 2)
            Hex = Replace(Hex, green, "", , 1)
            Dim blue As String = "&H" & Hex.Substring(0, 2)
            Hex = Replace(Hex, blue, "", , 1)
            Return Color.FromArgb(red, green, blue)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="c">Example: ``#ffffff``</param>
        ''' <returns></returns>
        Public Function OLE(c As String) As Color
            c = Replace(c, "#", "")
            c = "&H" & c
            ColorTranslator.FromOle(c)
        End Function
    End Module
End Namespace
