#Region "Microsoft.VisualBasic::7ec09874cf02fbbc46c97325b89403f3, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drawing2D\g.vb"

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
Imports System.Drawing.Text
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Net.Http

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

        Public Const DefaultPadding$ = "padding:100px 100px 100px 100px;"
        Public Const ZeroPadding$ = "padding: 0px 0px 0px 0px;"

        ''' <summary>
        ''' Data plots graphics engine. Default: <paramref name="size"/>:=(4300, 2000), <paramref name="padding"/>:=(100,100,100,100)
        ''' </summary>
        ''' <param name="size"></param>
        ''' <param name="padding"></param>
        ''' <param name="bg"></param>
        ''' <param name="plotAPI"></param>
        ''' <returns></returns>
        Public Function GraphicsPlots(ByRef size As Size, ByRef padding As Padding, bg$, plotAPI As IPlot) As Bitmap
            If size.IsEmpty Then
                size = New Size(4300, 2000)
            End If
            If padding.IsEmpty Then
                padding = New Padding(100)
            End If

            Dim bmp As New Bitmap(size.Width, size.Height)

            Using g As Graphics = Graphics.FromImage(bmp)
                Dim rect As New Rectangle(New Point, size)

                g.FillBg(bg$, rect)
                g.CompositingQuality = CompositingQuality.HighQuality
                g.CompositingMode = CompositingMode.SourceOver
                g.InterpolationMode = InterpolationMode.HighQualityBicubic
                g.PixelOffsetMode = PixelOffsetMode.HighQuality
                g.SmoothingMode = SmoothingMode.HighQuality
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit

                Call plotAPI(g, New GraphicsRegion With {
                     .Size = size,
                     .Padding = padding
                })
            End Using

            Return bmp
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="bg$">
        ''' 1. 可能为颜色表达式
        ''' 2. 可能为图片的路径
        ''' 3. 可能为base64图片字符串
        ''' </param>
        <Extension>
        Public Sub FillBg(ByRef g As Graphics, bg$, rect As Rectangle)
            Dim bgColor As Color = bg.ToColor(onFailure:=Nothing)

            If Not bgColor.IsEmpty Then
                Call g.FillRectangle(New SolidBrush(bgColor), rect)
            Else
                Dim res As Image

                If bg.FileExists Then
                    res = LoadImage(path:=bg$)
                Else
                    res = Base64Codec.GetImage(bg$)
                End If

                Call g.DrawImage(res, rect)
            End If
        End Sub

        ''' <summary>
        ''' Data plots graphics engine.
        ''' </summary>
        ''' <param name="size"></param>
        ''' <param name="bg"></param>
        ''' <param name="plot"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function GraphicsPlots(plot As Action(Of Graphics), ByRef size As Size, ByRef padding As Padding, bg$) As Bitmap
            Return GraphicsPlots(size, padding, bg, Sub(ByRef g, rect) Call plot(g))
        End Function

        Public Function Allocate(Optional size As Size = Nothing, Optional padding$ = DefaultPadding, Optional bg$ = "white") As InternalCanvas
            Return New InternalCanvas With {
                .size = size,
                .bg = bg,
                .padding = padding
            }
        End Function

        ''' <summary>
        ''' 可以借助这个画布对象创建多图层的绘图操作
        ''' </summary>
        Public Class InternalCanvas

            Dim plots As New List(Of IPlot)

            Public Property size As Size
            Public Property padding As Padding
            Public Property bg As String

            Public Function InvokePlot() As Bitmap
                Return GraphicsPlots(
                    size, padding, bg,
                    Sub(ByRef g, rect)

                        For Each plot As IPlot In plots
                            Call plot(g, rect)
                        Next
                    End Sub)
            End Function

            Public Shared Operator +(g As InternalCanvas, plot As IPlot) As InternalCanvas
                g.plots += plot
                Return g
            End Operator

            Public Shared Operator +(g As InternalCanvas, plot As IPlot()) As InternalCanvas
                g.plots += plot
                Return g
            End Operator

            Public Shared Narrowing Operator CType(g As InternalCanvas) As Bitmap
                Return g.InvokePlot
            End Operator

            ''' <summary>
            ''' canvas invoke this plot.
            ''' </summary>
            ''' <param name="g"></param>
            ''' <param name="plot"></param>
            ''' <returns></returns>
            Public Shared Operator <=(g As InternalCanvas, plot As IPlot) As Bitmap
                Dim size As Size = g.size
                Dim margin = g.padding
                Dim bg As String = g.bg

                Return GraphicsPlots(size, margin, bg, plot)
            End Operator

            Public Shared Operator >=(g As InternalCanvas, plot As IPlot) As Bitmap
                Throw New NotSupportedException
            End Operator
        End Class
    End Module
End Namespace
