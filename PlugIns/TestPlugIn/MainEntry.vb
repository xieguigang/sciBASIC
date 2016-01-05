Imports PlugIn

<PlugInEntry(name:="PlugIn Test Command", Icon:="FireFox", description:="This is a example description text for this test plugin.")>
Module MainEntry

    <PlugIn.PlugInCommand(name:="Test Command1", path:="\Folder1\A")> Public Function Command1(Form As System.Windows.Forms.Form) As String
        MsgBox("Test Command 1 " & vbCrLf & String.Format("Target form title is ""{0}""", Form.Text))
        Return 1
    End Function

    <PlugIn.PlugInCommand(name:="Open Terminal", path:="\Item2")> Public Function TestCommand2() As Integer
        Process.Start("cmd")
        Return 1
    End Function

    <PlugIn.PlugInCommand(name:="Open File", path:="\Folder1\", icon:="FireFox")> Public Function TestCommand3() As Integer
        Process.Start(My.Application.Info.DirectoryPath & "./test2.vbs")
        Return 1
    End Function

    <EntryFlag(entrytype:=EntryFlag.EntryTypes.IconLoader)> Public Function Icon(Name As String) As System.Drawing.Image
        Dim Objedc = My.Resources.ResourceManager.GetObject(Name)
        Return DirectCast(Objedc, System.Drawing.Image)
    End Function
End Module
