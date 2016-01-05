Public Class FormMainTest

    Dim PlugInManager As PlugIn.PlugInManager

    Private Sub TestLoad(sender As Object, e As EventArgs) Handles MyBase.Load
        PlugInManager = PlugIn.PlugInManager.LoadPlugins(Me.MenuStrip1, PluginDir:="./plugins", ProfileXml:="./PlugInsManager.xml")

        'Call PlugIn.PlugInEntry.LoadPlugIn(Me.MenuStrip1, "./plugins/TestPlugIn.dll")
        'Call PlugIn.PlugInEntry.LoadPlugIn(Me.MenuStrip1, "./plugins/TestPlugIn2.dll")
    End Sub

    Private Sub PlugInManagerToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles PlugInManagerToolStripMenuItem.Click
        Call PlugInManager.ShowDialog()
    End Sub
End Class
