' Copyright (c) 2018 GPL3 Licensed
' DQN 火柴人跑步 Demo 入口。

Imports System
Imports System.Windows.Forms

Namespace DeepQNetwork.Demo

    Module Program

        <STAThread>
        Sub Main()
            Application.EnableVisualStyles()
            Application.SetCompatibleTextRenderingDefault(False)
            Application.Run(New MainForm())
        End Sub
    End Module
End Namespace
