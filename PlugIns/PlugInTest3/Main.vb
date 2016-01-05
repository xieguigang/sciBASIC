<PlugIn.PlugInEntry(name:="Test Plugin 3 for textbox context menu",
    description:="Here is a another plugin demo, this plugin using initialize command to dynamic add a menu in the context menu strip.", showonmenu:=False)>
Public Class Class1

    <PlugIn.EntryFlag(entrytype:=PlugIn.EntryFlag.EntryTypes.Initialize)> Public Shared Function Initialize(Target As Windows.Forms.Form) As Integer
        Dim txtBox = (From ctl As Windows.Forms.Control In Target.Controls Where TypeOf ctl Is Windows.Forms.RichTextBox Select DirectCast(ctl, Windows.Forms.RichTextBox)).First '
        Dim Menu = txtBox.ContextMenuStrip
        Dim NewMenuItem As Windows.Forms.ToolStripItem = New Windows.Forms.ToolStripMenuItem With {.Text = "Copy Selected And Save  (This item was dynamic add from the test plugin3)"}
        Call Menu.Items.Add(NewMenuItem)

        AddHandler NewMenuItem.Click, Sub()
                                          Dim s = txtBox.SelectedText
                                          Call My.Computer.Clipboard.SetText(s)
                                          MsgBox("Selected text in the target textbox was copy to the system clipboard!" & vbCrLf & "The selected text is:  """ & s & """", MsgBoxStyle.Information)
                                      End Sub
        Return 0
    End Function
End Class
