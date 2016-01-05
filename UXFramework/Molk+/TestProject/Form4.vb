Public Class Form4
    Private Sub Form4_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Call Me.ProcessingBar1.StartRollAnimation()
        '  Me.ProcessingBar1.PercentageValue = 60


        Call New Microsoft.VisualBasic.MolkPlusTheme.Visualise.Elements.MarginBlue().SetupResource(Me.MultipleTabpagePanel1)
        Call Me.MultipleTabpagePanel1.AddTabPage("查找好友", New Panel With {.BackColor = Color.Aqua})
        Call Me.MultipleTabpagePanel1.AddTabPage("好友请求列表", New Panel With {.BackColor = Color.DarkGoldenrod})
        Call Me.MultipleTabpagePanel1.UpdatesUILayout()
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Me.ProcessingBar1.PercentageValue += 1
    End Sub
End Class