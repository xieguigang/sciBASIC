Imports System.IO
Imports System.CodeDom.Compiler

<PlugIn.PlugInEntry(name:="TextBox PlugIn Test Command",
    description:="This is another test plugin for this program, you can use this plugin to do some thing on  the text box, and maybe i will makes this plugin support script to modify the winform in the future.",
     icon:="word")>
Public Class MainEntry

    Shared cpara As CompilerParameters

    <PlugIn.PlugInCommand(name:="Fill TextBox")> Public Shared Function Fill(Target As System.Windows.Forms.Form) As Integer
        Dim TxBx = (From ctl As Windows.Forms.Control In Target.Controls Where TypeOf ctl Is Windows.Forms.RichTextBox Select DirectCast(ctl, Windows.Forms.RichTextBox)).First '
        TxBx.AppendText("XMLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLL" & vbCrLf)
        Return 0
    End Function

    <PlugIn.PlugInCommand(name:="Get VBScript Value")> Public Shared Function GetValue(Target As System.Windows.Forms.Form) As Integer
        Const MODULE_DEFINE As String = "Module MainEntry" & vbCrLf & "{0}" & vbCrLf & "End Module"

        Dim VBScript As String = String.Format(MODULE_DEFINE, FileIO.FileSystem.ReadAllText("./test.vbs"))
        Dim cdp = CodeDomProvider.CreateProvider("VisualBasic")
        Dim res = cdp.CompileAssemblyFromSource(cpara, VBScript)
        Dim asm = res.CompiledAssembly
        Dim newclass = asm.GetType("MainEntry")

        Dim ret = PlugIn.PlugInEntry.Invoke({Target.Text}, newclass.GetMethod("test"))

        Dim TxBx = (From ctl As Windows.Forms.Control In Target.Controls Where TypeOf ctl Is Windows.Forms.RichTextBox Select DirectCast(ctl, Windows.Forms.RichTextBox)).First '

        TxBx.AppendText("Target script file return value is " & ret.ToString & vbCrLf)

        Return 0
    End Function

    <PlugIn.EntryFlag(entrytype:=PlugIn.EntryFlag.EntryTypes.Initialize)> Public Shared Function ExampleInitialzieMethod() As Integer
        MsgBox("This is a example initialize method for this plugin, write some intialzie code at here!")

        'example plugin initialize statements
        cpara = New CompilerParameters
        cpara.CompilerOptions = "/optimize"
        cpara.GenerateInMemory = True
        cpara.IncludeDebugInformation = False

        cpara.ReferencedAssemblies.Add("mscorlib.dll")
        cpara.ReferencedAssemblies.Add("system.dll")

        Return 0
    End Function
End Class