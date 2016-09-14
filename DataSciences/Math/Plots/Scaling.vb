Imports System.Drawing
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Public Class Scaling

    Public ReadOnly dx, dy As Single
    Public ReadOnly xmin, ymin As Single

    ReadOnly serials As SerialData()

    Public ReadOnly type As Type

    Sub New(array As SerialData())
        dx = Scaling(array, Function(p) p.X, xmin)
        dy = Scaling(array, Function(p) p.Y, ymin)
        serials = array
        type = GetType(Scatter)
    End Sub

    Sub New(hist As HistogramGroup)
        Dim h As List(Of Double) = hist.Samples.Select(Function(s) s.data).MatrixToList
        ymin = h.Min
        dy = h.Max - ymin
        type = GetType(Histogram)
    End Sub

    ''' <summary>
    ''' 返回的系列是已经被转换过的，直接使用来进行画图
    ''' </summary>
    ''' <returns></returns>
    Public Iterator Function ForEach(size As Size, margin As Size) As IEnumerable(Of SerialData)
        Dim bottom As Integer = size.Height - margin.Height
        Dim width As Integer = size.Width - margin.Width * 2
        Dim height As Integer = size.Height - margin.Height * 2

        For Each s In serials
            Dim pts = LinqAPI.Exec(Of PointF) <= From p As PointF
                                                 In s.pts
                                                 Let px As Single = margin.Width + width * (p.X - xmin) / dx
                                                 Let py As Single = bottom - height * (p.Y - ymin) / dy
                                                 Select New PointF(px, py)
            Yield New SerialData With {
                .color = s.color,
                .lineType = s.lineType,
                .PointSize = s.PointSize,
                .pts = pts,
                .title = s.title,
                .width = s.width
            }
        Next
    End Function

    Public Function XScaler(size As Size, margin As Size) As Func(Of Single, Single)
        Dim bottom As Integer = size.Height - margin.Height
        Dim width As Integer = size.Width - margin.Width * 2
        Dim height As Integer = size.Height - margin.Height * 2

        Return Function(x) margin.Width + width * (x - xmin) / dx
    End Function

    Public Function YScaler(size As Size, margin As Size) As Func(Of Single, Single)
        Dim bottom As Integer = size.Height - margin.Height
        Dim width As Integer = size.Width - margin.Width * 2
        Dim height As Integer = size.Height - margin.Height * 2

        Return Function(y) bottom - height * (y - ymin) / dy
    End Function

    ''' <summary>
    ''' 返回dx或者dy
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function Scaling(data As IEnumerable(Of SerialData), [get] As Func(Of PointF, Single), ByRef min As Single) As Single
        Dim array As Single() = data.Select(Function(s) s.pts).MatrixAsIterator.ToArray([get])
        Dim max = array.Max : min = array.Min
        Dim d As Single = max - min
        Return d
    End Function
End Class
