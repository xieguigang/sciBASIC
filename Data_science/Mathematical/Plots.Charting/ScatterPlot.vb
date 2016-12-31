Imports System.Windows.Forms.DataVisualization.Charting
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Logging
Imports Microsoft.VisualBasic.Mathematical.Calculus

Public Class ScatterPlot

    Dim vars As New List(Of CheckBox)
    Dim points As ODEsOut

    Public Property Logger As Action(Of String, MSG_TYPES)

    Public Sub SetVariables(vars$())
        With Me
            Call FlowLayoutPanel1.Controls.Clear()
            Call .vars.Clear()

            For Each var As String In vars
                .vars += New CheckBox With {
                    .Text = var,
                    .Checked = True,
                    .Visible = True,
                    .AutoSize = True
                }

                Call FlowLayoutPanel1 _
                    .Controls _
                    .Add(.vars.Last)
            Next
        End With

        Call Refresh()
    End Sub

    Public Sub Plot()
        Call Chart1.Series.Clear()

        For Each v In vars
            If v.Checked Then
                Try
                    Dim y = points.y(v.Text)
                    Dim s = Chart1.Series.Add(y.Name)

                    s.ChartType = SeriesChartType.Line

                    For Each x As SeqValue(Of Double) In points.x.SeqIterator
                        Call s.Points.AddXY(x.value, y.Value(x))
                    Next
                Catch ex As Exception
                    If Not Logger Is Nothing Then
                        Call Logger()(ex.ToString, MSG_TYPES.ERR)
                    End If
                End Try
            End If
        Next
    End Sub

    Public Sub Plot(data As ODEsOut)
        points = data
        Call Plot()
    End Sub

    Private Sub c_CheckStateChanged(sender As Object, e As EventArgs)
        Call Plot()
    End Sub
End Class
