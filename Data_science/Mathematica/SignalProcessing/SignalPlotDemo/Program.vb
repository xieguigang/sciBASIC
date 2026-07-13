Imports System.Windows.Forms
Imports Microsoft.VisualBasic.Imaging.Driver

Module Program

    Sub New()
        Call ImageDriver.Register()
    End Sub

    <STAThread>
    Sub Main()
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
        Application.Run(New MainForm())
    End Sub

End Module
