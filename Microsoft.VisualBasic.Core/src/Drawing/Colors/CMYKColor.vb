#Region "Microsoft.VisualBasic::d75b6388da07421349e7203d4d9d561f, Microsoft.VisualBasic.Core\src\Drawing\Colors\CMYKColor.vb"

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

    '   Total Lines: 93
    '    Code Lines: 50 (53.76%)
    ' Comment Lines: 30 (32.26%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 13 (13.98%)
    '     File Size: 2.85 KB


    '     Class CMYKColor
    ' 
    '         Properties: C, K, M, Y
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: FromRGB, ToRGB, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices

Namespace Imaging

    ''' <summary>
    ''' The CMYK color model (also known as process color, or four color) 
    ''' is a subtractive color model, based on the CMY color model, used 
    ''' in color printing, and is also used to describe the printing process
    ''' itself. The abbreviation CMYK refers to the four ink plates used: 
    ''' cyan, magenta, yellow, and key (black).
    ''' </summary>
    ''' <remarks>
    ''' All color channel in this color class should be in value range of ``[0,1]``.
    ''' </remarks>
    Public Class CMYKColor

        ''' <summary>
        ''' cyan
        ''' </summary>
        ''' <returns></returns>
        Public Property C As Single
        ''' <summary>
        ''' magenta
        ''' </summary>
        ''' <returns></returns>
        Public Property M As Single
        ''' <summary>
        ''' yellow
        ''' </summary>
        ''' <returns></returns>
        Public Property Y As Single
        ''' <summary>
        ''' key (black)
        ''' </summary>
        ''' <returns></returns>
        Public Property K As Single

        Sub New()
        End Sub

        Sub New(c#, m#, y#, k#)
            Me.C = c
            Me.M = m
            Me.Y = y
            Me.K = k
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(c As Byte, m As Byte, y As Byte, k As Byte)
            Call Me.New(c / 255, m / 255, y / 255, k / 255)
        End Sub

        ''' <summary>
        ''' CMYK
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return $"CMYK({C}, {M}, {Y}, {K})"
        End Function

        Public Shared Function FromRGB(rgb As Color) As CMYKColor
            Dim R = rgb.R / 255
            Dim G = rgb.G / 255
            Dim B = rgb.B / 255
            Dim K = 1 - {R, G, B}.Max

            Return New CMYKColor With {
                .K = K,
                .C = (1 - R - K) / (1 - K),
                .M = (1 - G - K) / (1 - K),
                .Y = (1 - B - K) / (1 - K)
            }
        End Function

        Public Function ToRGB() As Color
            Dim R = 255 * (1 - C) * (1 - K)
            Dim G = 255 * (1 - M) * (1 - K)
            Dim B = 255 * (1 - Y) * (1 - K)
            Return Color.FromArgb(255, R, G, B)
        End Function

        Public Shared Narrowing Operator CType(color As CMYKColor) As Color
            Return color.ToRGB
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(color As Color) As CMYKColor
            Return FromRGB(color)
        End Operator

    End Class
End Namespace
