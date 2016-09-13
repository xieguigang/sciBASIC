Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical.diffEq
Imports Microsoft.VisualBasic.Serialization.JSON

Public Module Histogram

    <Extension>
    Public Function Plot(data As HistogramGroup,
                         Optional size As Size = Nothing,
                         Optional margin As Size = Nothing,
                         Optional bg As String = "white",
                         Optional showGrid As Boolean = True) As Bitmap

        Return GraphicsPlots(
            size, margin, bg,
            Sub(g)
                Dim mapper As New Scaling(data)
                Dim n = data.Samples.Sum(Function(x) x.data.Length)
                Dim dx = (size.Width - 2 * margin.Width - 2 * margin.Width) / n
                Dim interval = 2 * margin.Width / n
                Dim left As Single = margin.Width
                Dim sy = mapper.YScaler(size, margin)
                Dim bottom = size.Height - margin.Height

                Call g.DrawAxis(size, margin, mapper, showGrid)

                For Each sample In data.Samples.SeqIterator
                    Dim x = left + interval

                    For Each val As SeqValue(Of Double) In sample.obj.data.SeqIterator
                        Dim right = x + dx
                        Dim top = sy(val.obj)
                        Dim rect As Rectangle = Rectangle(top, x, right, size.Height - margin.Height)

                        Call g.DrawRectangle(Pens.Black, rect)
                        Call g.FillRectangle(
                            New SolidBrush(data.Serials(val.i).x),
                            Rectangle(top + 1,
                                      x + 1,
                                      right - 1,
                                      size.Height - margin.Height - 1))
                        x += dx
                    Next

                    left = x
                Next
            End Sub)
    End Function

    Public Function Rectangle(top As Single, left As Single, right As Single, bottom As Single) As Rectangle
        Dim pt As New Point(left, top)
        Dim size As New Size(right - left, bottom - top)
        Return New Rectangle(pt, size)
    End Function

    <Extension>
    Public Function FromData(data As IEnumerable(Of Double)) As HistogramGroup
        Return New HistogramGroup With {
            .Serials = {
                New NamedValue(Of Color) With {
                    .Name = "",
                    .x = Color.Lime
                }
            },
            .Samples = LinqAPI.Exec(Of HistogramSample) <=
                From n
                In data.SeqIterator
                Select New HistogramSample With {
                    .data = {n.obj},
                    .Tag = n.i
                }
        }
    End Function

    Public Function FromODE(ParamArray odes As ODE()) As HistogramGroup
        Dim colors = Imaging.ChartColors.Shuffles
        Dim serials = LinqAPI.Exec(Of NamedValue(Of Color)) <=
 _
            From x As SeqValue(Of ODE)
            In odes.SeqIterator
            Select New NamedValue(Of Color) With {
                .Name = x.obj.df.ToString,
                .x = colors(x.i)
            }
        Dim samples = LinqAPI.Exec(Of HistogramSample) <=
 _
            From i As Integer
            In odes.First.y.Sequence
            Select New HistogramSample With {
                .Tag = i,
                .data = odes.ToArray(Function(x) x.y(i))
            }

        Return New HistogramGroup With {
            .Samples = samples,
            .Serials = serials
        }
    End Function
End Module

Public Class HistogramSample

    Public Property Tag As String
    Public Property data As Double()

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class

Public Class HistogramGroup

    ''' <summary>
    ''' 与<see cref="HistogramSample.data"/>里面的数据顺序是一致的
    ''' </summary>
    ''' <returns></returns>
    Public Property Serials As NamedValue(Of Color)()
    Public Property Samples As HistogramSample()

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class