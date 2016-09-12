Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.ComponentModel.DataStructures.SlideWindow
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical.Calculus

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
    Public Function Plot(c As IEnumerable(Of Serials), Optional size As Size = Nothing, Optional margin As Size = Nothing, Optional bg As String = "white") As Bitmap
        If size.IsEmpty Then
            size = New Size(4000, 3000)
        End If
        If margin.IsEmpty Then
            margin = New Size(100, 100)
        End If

        Dim array As Serials() = c.ToArray
        Dim bmp As New Bitmap(size.Width, size.Height)
        Dim bgColor As Color = bg.ToColor(onFailure:=Color.White)
        Dim mapper As New Scaling(array)

        Using g As Graphics = Graphics.FromImage(bmp)
            Dim rect As New Rectangle(New Point, size)

            Call g.FillRectangle(New SolidBrush(bgColor), rect)

            For Each line As Serials In mapper.ForEach(size, margin)
                Dim pts = line.pts.SlideWindows(2)
                Dim pen As New Pen(color:=line.color, width:=line.width) With {
                    .DashStyle = line.lineType
                }
                Dim br As New SolidBrush(line.color)
                Dim r As Single = line.PointSize

                For Each pt In pts
                    Dim a = pt.First
                    Dim b = pt.Last
                    Call g.DrawLine(pen, a, b)
                    Call g.FillPie(br, a.X, a.Y, r, r, 0, 360)
                    Call g.FillPie(br, b.X, b.Y, r, r, 0, 360)
                Next
            Next
        End Using

        Return bmp
    End Function

    <Extension>
    Public Function Plot(ode As ODE, Optional size As Size = Nothing, Optional margin As Size = Nothing, Optional bg As String = "white") As Bitmap
        Dim c = {
            New Serials With {
                .title = ode.df.ToString,
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