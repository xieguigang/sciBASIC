Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataStructures.SlideWindow
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical.diffEq
Imports Microsoft.VisualBasic.Serialization.JSON

Public Module Scatter

    ''' <summary>
    ''' 绘图函数
    ''' </summary>
    ''' <param name="c"></param>
    ''' <param name="size"></param>
    ''' <param name="margin"></param>
    ''' <param name="bg"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Plot(c As IEnumerable(Of Serials),
                         Optional size As Size = Nothing,
                         Optional margin As Size = Nothing,
                         Optional bg As String = "white",
                         Optional showGrid As Boolean = True) As Bitmap

        Return GraphicsPlots(
            size, margin, bg,
            Sub(g)
                Dim mapper As New Scaling(c.ToArray)

                Call g.DrawAxis(size, margin, mapper, showGrid)

                For Each line As Serials In mapper.ForEach(size, margin)
                    Dim pts = line.pts.SlideWindows(2)
                    Dim pen As New Pen(color:=line.color, width:=line.width) With {
                        .DashStyle = line.lineType
                    }
                    Dim br As New SolidBrush(line.color)
                    Dim d = line.PointSize
                    Dim r As Single = line.PointSize / 2

                    For Each pt In pts
                        Dim a = pt.First
                        Dim b = pt.Last
                        Call g.DrawLine(pen, a, b)
                        Call g.FillPie(br, a.X - r, a.Y - r, d, d, 0, 360)
                        Call g.FillPie(br, b.X - r, b.Y - r, d, d, 0, 360)
                    Next
                Next
            End Sub)
    End Function

    <Extension>
    Public Function Plot(ode As ODE, Optional size As Size = Nothing, Optional margin As Size = Nothing, Optional bg As String = "white") As Bitmap
        Dim c = {
            New Serials With {
                .title = ode.df.ToString,
                .color = Color.DarkCyan,
                .lineType = DashStyle.Dash,
                .PointSize = 50,
                .width = 5,
                .pts = LinqAPI.Exec(Of PointF) <= From x As SeqValue(Of Double)
                                                  In ode.x.SeqIterator
                                                  Select New PointF(CSng(x.obj), CSng(ode.y(x.i)))
            }
        }
        Return c.Plot(size, margin, bg)
    End Function
End Module

Public Class Serials

    Public pts As PointF()
    Public lineType As DashStyle = DashStyle.Solid
    Public title As String
    ''' <summary>
    ''' 点的半径大小
    ''' </summary>
    Public PointSize As Single = 1
    Public color As Color = Color.Black
    Public width As Single = 1

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class