#Region "Microsoft.VisualBasic::dad6e59ced848f109d82e8b5022d3bed, Data_science\Visualization\Plots\BarPlot\RectangleStyles.vb"

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

    '   Total Lines: 99
    '    Code Lines: 79
    ' Comment Lines: 0
    '   Blank Lines: 20
    '     File Size: 3.38 KB


    '     Enum RectangleSides
    ' 
    '         Bottom, Left, Right, Top
    ' 
    '  
    ' 
    ' 
    ' 
    '     Delegate Sub
    ' 
    ' 
    '     Module RectangleStyles
    ' 
    '         Function: DefaultStyle, ModernStyle, RectangleBorderStyle, UnsealBottomPath, UnsealPath
    '                   UnsealTopPath
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports defaultStyle = Microsoft.VisualBasic.Language.Default.[Default](Of  Microsoft.VisualBasic.Data.ChartPlots.BarPlot.RectangleStyling)

Namespace BarPlot

    Public Enum RectangleSides As Byte
        ALL = 0
        Top
        Right
        Bottom
        Left
    End Enum

    Public Delegate Sub RectangleStyling(g As IGraphics, color As SolidBrush, layout As Rectangle, unsealSide As RectangleSides)

    Public Module RectangleStyles

        Public Function DefaultStyle() As defaultStyle
            Dim del As RectangleStyling =
                Sub(g, color, layout, unsealSide)
                    Call g.FillRectangle(color, layout)
                End Sub

            Return del
        End Function

        Public Function RectangleBorderStyle() As defaultStyle
            Dim del As RectangleStyling =
                Sub(g, color, layout, unsealSide)
                    Call g.FillRectangle(color, layout)
                    Call g.DrawRectangle(Pens.Black, layout)
                End Sub

            Return del
        End Function

        <Extension>
        Public Function UnsealTopPath(layout As Rectangle) As GraphicsPath
            Dim path As New GraphicsPath
            Dim a As New Point(layout.Left, layout.Top)
            Dim b As New Point(layout.Left, layout.Bottom)
            Dim c As New Point(layout.Right, layout.Bottom)
            Dim d As New Point(layout.Right, layout.Top)

            Call path.AddLine(a, b)
            Call path.AddLine(b, c)
            Call path.AddLine(c, d)

            Return path
        End Function

        <Extension>
        Public Function UnsealBottomPath(layout As Rectangle) As GraphicsPath
            Dim path As New GraphicsPath
            Dim a As New Point(layout.Left, layout.Bottom)
            Dim b As New Point(layout.Left, layout.Top)
            Dim c As New Point(layout.Right, layout.Top)
            Dim d As New Point(layout.Right, layout.Bottom)

            Call path.AddLine(a, b)
            Call path.AddLine(b, c)
            Call path.AddLine(c, d)

            Return path
        End Function

        <Extension>
        Public Function UnsealPath(layout As Rectangle, side As RectangleSides) As GraphicsPath
            Select Case side
                Case RectangleSides.Bottom
                    Return layout.UnsealBottomPath
                Case RectangleSides.Left
                Case RectangleSides.Right
                Case RectangleSides.Top
                    Return layout.UnsealTopPath
                Case Else

            End Select

            Throw New NotImplementedException
        End Function

        Public Function ModernStyle() As defaultStyle
            Dim del As RectangleStyling =
                Sub(g, color, layout, unsealSide)
                    Dim fillColor As New SolidBrush(color.Color.Alpha(200))
                    Dim path = layout.UnsealPath(unsealSide)

                    Call g.FillRectangle(fillColor, layout)
                    Call g.DrawPath(New Pen(color, 2), path)
                End Sub

            Return del
        End Function
    End Module
End Namespace
