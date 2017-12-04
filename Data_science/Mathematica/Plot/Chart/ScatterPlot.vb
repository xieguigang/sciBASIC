#Region "Microsoft.VisualBasic::eb1c0b59785ffcb37e029c0f3cac7d56, ..\sciBASIC#\Data_science\Mathematica\Plot\Chart\ScatterPlot.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Windows.Forms.DataVisualization.Charting
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Calculus

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
