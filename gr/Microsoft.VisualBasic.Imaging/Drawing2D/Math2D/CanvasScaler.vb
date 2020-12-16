Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Namespace Drawing2D.Math2D

    Public Module CanvasScaler

        <Extension>
        Public Function AutoScaler(boundary As RectangleF, frameSize As SizeF, padding As Padding) As SizeF
            With boundary
                Dim w = (frameSize.Width - padding.Horizontal) / .Width
                Dim h = (frameSize.Height - padding.Vertical) / .Height

                Return New SizeF(w, h)
            End With
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function AutoScaler(shape As IEnumerable(Of PointF), frameSize As SizeF, padding As Padding) As SizeF
            Return shape.GetBounds.AutoScaler(frameSize, padding)
        End Function

        <Extension>
        Public Function ScalePoints(polygon As PointF(), frameSize As SizeF, padding As Padding,
                                    Optional ByRef scaleFactor As SizeF = Nothing,
                                    Optional ByRef centraOffset As PointF = Nothing) As PointF()

            ' 1. 首先计算出边界
            Dim boundary As RectangleF = polygon.GetBounds
            ' 2. 计算出缩放的因子大小
            Dim factor As SizeF = boundary.AutoScaler(frameSize, padding)
            Dim scales As PointF() = polygon.Enlarge((CDbl(factor.Width), CDbl(factor.Height)))
            ' 4. 计算出中心点平移的偏移值
            Dim plotSize As New Size With {
                .Width = CInt(frameSize.Width - padding.Horizontal),
                .Height = CInt(frameSize.Height - padding.Vertical)
            }
            Dim offset As PointF = scales _
                .CentralOffset(plotSize) _
                .OffSet2D(New PointF(padding.Left, padding.Top))

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