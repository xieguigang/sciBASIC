Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging

Public Module Bubble

    ''' <summary>
    ''' <see cref="PointData.value"/>是Bubble的半径大小
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="size"></param>
    ''' <param name="margin"></param>
    ''' <param name="bg"></param>
    ''' <param name="legend"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Plot(data As IEnumerable(Of SerialData),
                         Optional size As Size = Nothing,
                         Optional margin As Size = Nothing,
                         Optional bg As String = "white",
                         Optional legend As Boolean = True,
                         Optional logR As Boolean = False) As Bitmap

        Return GraphicsPlots(
            size, margin, bg,
            Sub(g)
                Dim array As SerialData() = data.ToArray
                Dim mapper As New Scaling(array)
                Dim scale As Func(Of Double, Double) =
                    If(logR,
                    Function(r) Math.Log(r + 1) + 1,
                    Function(r) r)

                Call g.DrawAxis(size, margin, mapper, True)

                For Each s As SerialData In mapper.ForEach(size, margin)
                    Dim b As New SolidBrush(s.color)

                    For Each pt As PointData In s
                        Dim r As Double = scale(pt.value)
                        Dim p As New Point(pt.pt.X - r, pt.pt.Y - r)
                        Dim rect As New Rectangle(p, New Size(r * 2, r * 2))

                        Call g.FillPie(b, rect, 0, 360)
                    Next
                Next

                If legend Then
                    Call g.DrawLegend(Of SerialData)(
                        array,
                        Function(x) x.title,
                        Function(x) x.color,
                        margin.Height,
                        size.Width * 0.8,
                        New Font(FontFace.MicrosoftYaHei, 20))
                End If
            End Sub)
    End Function
End Module
