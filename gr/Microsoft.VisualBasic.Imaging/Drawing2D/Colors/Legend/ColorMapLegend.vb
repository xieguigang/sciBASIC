﻿#Region "Microsoft.VisualBasic::546757cb2c6d38e9f37e44bcc66d0bad, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Colors\Legend\ColorMapLegend.vb"

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

    '   Total Lines: 92
    '    Code Lines: 70 (76.09%)
    ' Comment Lines: 11 (11.96%)
    '    - Xml Docs: 81.82%
    ' 
    '   Blank Lines: 11 (11.96%)
    '     File Size: 3.44 KB


    '     Class ColorMapLegend
    ' 
    '         Properties: designer, foreColor, format, legendOffsetLeft, maxWidth
    '                     noblank, ruleOffset, tickAxisStroke, tickFont, ticks
    '                     title, titleFont, unmapColor
    ' 
    '         Constructor: (+4 Overloads) Sub New
    ' 
    '         Function: Draw, ScaleColors, ToString
    ' 
    '         Sub: Draw
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Driver

Namespace Drawing2D.Colors

    ''' <summary>
    ''' a continues numeric scaler legend bar
    ''' </summary>
    Public Class ColorMapLegend

        Public ReadOnly Property designer As SolidBrush()
        Public Property title As String
        Public Property titleFont As Font
        Public Property ticks As Double()
        Public Property tickFont As Font
        Public Property tickAxisStroke As Pen
        Public Property unmapColor As String = Nothing
        Public Property ruleOffset As Single = 10
        Public Property format As String = "F2"
        Public Property legendOffsetLeft As Single = -99999
        Public Property noblank As Boolean = True
        Public Property foreColor As Color = Color.Black
        Public Property maxWidth As Single = 0.25

        Sub New()
        End Sub

        Sub New(palette As String, Optional mapLevels As Integer = 30)
            designer = GetColors(palette, mapLevels) _
                .Select(Function(c) New SolidBrush(c)) _
                .ToArray
        End Sub

        Sub New(palette As String(), Optional mapLevels As Integer = 30)
            designer = CubicSpline(palette.Select(Function(c) c.TranslateColor), mapLevels) _
                .Select(Function(c) New SolidBrush(c)) _
                .ToArray
        End Sub

        Sub New(palette As IEnumerable(Of SolidBrush))
            designer = palette.ToArray
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="layout">
        ''' the plot location and the rectangle region
        ''' </param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Draw(ByRef g As IGraphics, layout As Rectangle)
            Call g.ColorMapLegend(
                layout:=layout,
                designer:=designer,
                ticks:=ticks,
                titleFont:=titleFont,
                title:=title,
                tickFont:=tickFont,
                tickAxisStroke:=tickAxisStroke,
                unmapColor:=unmapColor,
                ruleOffset:=ruleOffset,
                format:=format,
                legendOffsetLeft:=legendOffsetLeft,
                noLeftBlank:=noblank,
                foreColor:=foreColor.ToHtmlColor,
                maxWidth:=maxWidth
            )
        End Sub

        Public Function Draw(size As Size) As Image
            Using g As IGraphics = Driver.CreateGraphicsDevice(size, fill:=NameOf(Color.Transparent), driver:=Driver.Drivers.GDI)
                Call Draw(g, New Rectangle With {.X = 0, .Y = 0, .Width = size.Width, .Height = size.Height})
                Return DirectCast(g, GdiRasterGraphics).ImageResource
            End Using
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ScaleColors(n As Integer) As Color()
            Return Drawing2D.Colors _
                .Designer _
                .CubicSpline(designer.Select(Function(b) b.Color), n)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return title
        End Function
    End Class
End Namespace
