Imports System
Imports System.Windows.Forms

    ''' <summary>
    ''' 应用程序入口。禁用应用框架（UseApplicationFramework=false），以 Sub Main 显式启动窗体。
    ''' </summary>
    Module Program

        <STAThread>
        Sub Main()
            Application.EnableVisualStyles()
            Application.SetCompatibleTextRenderingDefault(False)
            Application.Run(New MainForm())
        End Sub

    End Module
