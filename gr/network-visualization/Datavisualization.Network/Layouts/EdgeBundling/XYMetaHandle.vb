Imports System.Drawing

Namespace Layouts.EdgeBundling

    ''' <summary>
    ''' 相对于<see cref="Handle"/>模型，这个矢量模型则是单纯的以xy偏移比例来进行矢量比例缩放
    ''' </summary>
    Public Class XYMetaHandle

        Public Property xoffsetscale As Double
        Public Property yoffsetscale As Double

        ''' <summary>
        ''' 将当前的这个矢量描述转换为实际的点位置
        ''' </summary>
        ''' <param name="sx#"></param>
        ''' <param name="sy#"></param>
        ''' <param name="tx#"></param>
        ''' <param name="ty#"></param>
        ''' <returns></returns>
        Public Function GetPoint(sx#, sy#, tx#, ty#) As PointF
            Dim dx = (tx - sx) * xoffsetscale
            Dim dy = (ty - sy) * yoffsetscale

            Return New PointF(sx + dx, sy + dy)
        End Function

        Public Shared Function CreateVector(ps As PointF, pt As PointF, hx!, hy!) As XYMetaHandle
            Dim dx = pt.X - ps.X
            Dim dy = pt.Y - ps.Y
            Dim offsetX = hx - ps.X
            Dim offsetY = hy - ps.Y

            Return New XYMetaHandle With {
                .xoffsetscale = offsetX / dx,
                .yoffsetscale = offsetY / dy
            }
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="ps"></param>
        ''' <param name="pt"></param>
        ''' <param name="handle">当前的这个需要进行矢量化描述的未知点坐标数据</param>
        ''' <returns></returns>
        Public Shared Function CreateVector(ps As PointF, pt As PointF, [handle] As PointF) As XYMetaHandle
            Return CreateVector(ps, pt, handle.X, handle.Y)
        End Function

        Public Overrides Function ToString() As String
            Return $"{xoffsetscale},{yoffsetscale}"
        End Function
    End Class
End Namespace