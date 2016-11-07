#Region "Microsoft.VisualBasic::8dfb71f2545a0af0e708bfaf8250202e, ..\visualbasic_App\gr\Microsoft.VisualBasic.Imaging\Drawing2D\g.vb"

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
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging

Namespace Drawing2D

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="g">GDI+设备</param>
    ''' <param name="grct">绘图区域的大小</param>
    Public Delegate Sub IPlot(ByRef g As Graphics, grct As GraphicsRegion)

    ''' <summary>
    ''' Data plots graphics engine common abstract.
    ''' </summary>
    Public Module g

        ''' <summary>
        ''' Data plots graphics engine.
        ''' </summary>
        ''' <param name="size"></param>
        ''' <param name="margin"></param>
        ''' <param name="bg"></param>
        ''' <param name="plotAPI"></param>
        ''' <returns></returns>
        Public Function GraphicsPlots(ByRef size As Size, ByRef margin As Size, bg$, plotAPI As IPlot) As Bitmap
            If size.IsEmpty Then
                size = New Size(4300, 2000)
            End If
            If margin.IsEmpty Then
                margin = New Size(100, 100)
            End If

            Dim bmp As New Bitmap(size.Width, size.Height)
            Dim bgColor As Color = bg.ToColor(onFailure:=Color.White)

            Using g As Graphics = Graphics.FromImage(bmp)
                Dim rect As New Rectangle(New Point, size)

                g.FillRectangle(New SolidBrush(bgColor), rect)
                g.CompositingQuality = CompositingQuality.HighQuality

                Call plotAPI(g, New GraphicsRegion With {
                     .Size = size,
                     .Margin = margin
                })
            End Using

            Return bmp
        End Function

        ''' <summary>
        ''' Data plots graphics engine.
        ''' </summary>
        ''' <param name="size"></param>
        ''' <param name="margin"></param>
        ''' <param name="bg"></param>
        ''' <param name="plot"></param>
        ''' <returns></returns>
        Public Function GraphicsPlots(ByRef size As Size, ByRef margin As Size, bg$, plot As Action(Of Graphics)) As Bitmap
            Return GraphicsPlots(size, margin, bg, Sub(ByRef g, rect) Call plot(g))
        End Function
    End Module
End Namespace
