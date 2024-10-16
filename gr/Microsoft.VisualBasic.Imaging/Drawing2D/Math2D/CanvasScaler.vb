#Region "Microsoft.VisualBasic::016c7364e32dfe4195ab62264e1c7ef2, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Math2D\CanvasScaler.vb"

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

    '   Total Lines: 81
    '    Code Lines: 48 (59.26%)
    ' Comment Lines: 23 (28.40%)
    '    - Xml Docs: 73.91%
    ' 
    '   Blank Lines: 10 (12.35%)
    '     File Size: 3.21 KB


    '     Module CanvasScaler
    ' 
    '         Function: (+2 Overloads) AutoScaler, ScalePoints
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.MIME.Html.CSS

Namespace Drawing2D.Math2D

    ''' <summary>
    ''' the scaler of the canvas size and rectangle
    ''' </summary>
    Public Module CanvasScaler

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="boundary"></param>
        ''' <param name="frame"></param>
        ''' <param name="padding"></param>
        ''' <returns></returns>
        <Extension>
        Public Function AutoScaler(boundary As RectangleF, frame As CSSEnvirnment, padding As Padding) As SizeF
            With boundary
                Dim frameSize = frame.canvas
                Dim w = (frameSize.Width - padding.Horizontal(frame)) / .Width
                Dim h = (frameSize.Height - padding.Vertical(frame)) / .Height

                Return New SizeF(w, h)
            End With
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function AutoScaler(shape As IEnumerable(Of PointF), frameSize As SizeF, padding As Padding) As SizeF
            Return shape.GetBounds.AutoScaler(New CSSEnvirnment(frameSize), padding)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="polygon"></param>
        ''' <param name="frameSize">the canvas drawing size</param>
        ''' <param name="padding"></param>
        ''' <param name="scaleFactor"></param>
        ''' <param name="centraOffset"></param>
        ''' <returns></returns>
        <Extension>
        Public Function ScalePoints(polygon As PointF(), frameSize As SizeF, padding As Padding,
                                    Optional ByRef scaleFactor As SizeF = Nothing,
                                    Optional ByRef centraOffset As PointF = Nothing) As PointF()

            ' 1. 首先计算出边界
            Dim boundary As RectangleF = polygon.GetBounds
            Dim frame As New CSSEnvirnment(frameSize)
            ' 2. 计算出缩放的因子大小
            Dim factor As SizeF = boundary.AutoScaler(frame, padding)
            Dim scales As PointF() = polygon.Enlarge((CDbl(factor.Width), CDbl(factor.Height)))
            ' 4. 计算出中心点平移的偏移值
            Dim plotSize As New Size With {
                .Width = CInt(frameSize.Width - padding.Horizontal(frame)),
                .Height = CInt(frameSize.Height - padding.Vertical(frame))
            }
            Dim paddingTopleft As New PointF(
                frame.GetWidth(padding.Left),
                frame.GetHeight(padding.Top)
            )
            Dim offset As PointF = scales _
                .CentralOffset(plotSize) _
                .OffSet2D(paddingTopleft)

            ' 5. 执行中心点平移
            For i As Integer = 0 To polygon.Length - 1
                polygon(i) = scales(i).OffSet2D(offset)
            Next

            scaleFactor = factor
            centraOffset = offset

            Return polygon
        End Function
    End Module
End Namespace
