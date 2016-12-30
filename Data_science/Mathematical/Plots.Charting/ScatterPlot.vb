Public Class ScatterPlot

    Public Sub SetVariables(vars$())
        Call FlowLayoutPanel1.Controls.Clear()

        For Each var As String In vars
            Call FlowLayoutPanel1 _
                .Controls _
                .Add(New CheckBox With {
                    .Text = var,
                    .Checked = True,
                    .Visible = True
            })
        Next

        Call Refresh()
    End Sub
End Class
