Imports System.Windows.Forms

Namespace ModelViewer

    Module Program

        <STAThread>
        Public Sub Main()
            Application.EnableVisualStyles()
            Application.SetCompatibleTextRenderingDefault(False)
            Application.Run(New MainForm())
        End Sub

    End Module

End Namespace
