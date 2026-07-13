Imports Microsoft.VisualBasic.Imaging.Driver

Namespace ModelViewer

    Module Program

        Sub New()
            Call ImageDriver.Register()
        End Sub

        <STAThread>
        Public Sub Main()
            Application.EnableVisualStyles()
            Application.SetCompatibleTextRenderingDefault(False)
            Application.Run(New MainForm())
        End Sub

    End Module

End Namespace
