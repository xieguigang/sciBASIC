Imports System.Drawing
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Public Class Scaling

    ReadOnly dx, dy As Single
    ReadOnly xmin, ymin As Single
    ReadOnly serials As Serials()

    Sub New(array As Serials())
        dx = Scaling(array, Function(p) p.X, xmin)
        dy = Scaling(array, Function(p) p.Y, ymin)
        serials = array
    End Sub

    ''' <summary>
    ''' 返回的系列是已经被转换过的，直接使用来进行画图
    ''' </summary>
    ''' <returns></returns>
    Public Iterator Function ForEach(size As Size, margin As Size) As IEnumerable(Of Serials)
        Dim bottom As Integer = size.Height - margin.Height
        Dim width As Integer = size.Width - margin.Width * 2
        Dim height As Integer = size.Height - margin.Height * 2

        For Each s In serials
            Dim pts = LinqAPI.Exec(Of PointF) <= From p As PointF
                                                 In s.pts
                                                 Let px As Single = margin.Width + width * (p.X - xmin) / dx
                                                 Let py As Single = bottom - height * (p.Y - ymin) / dy
                                                 Select New PointF(px, py)
            Yield New Serials With {
                .color = s.color,
                .lineType = s.lineType,
                .PointSize = s.PointSize,
                .pts = pts,
                .title = s.title,
                .width = s.width
            }
        Next
    End Function

    ''' <summary>
    ''' 返回dx或者dy
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function Scaling(data As IEnumerable(Of Serials), [get] As Func(Of PointF, Single), ByRef min As Single) As Single
        Dim array As Single() = data.Select(Function(s) s.pts).MatrixAsIterator.ToArray([get])
        Dim max = array.Max : min = array.Min
        Dim d As Single = max - min
        Return d
    End Function
End Class
