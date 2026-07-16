' Copyright (c) 2018 GPL3 Licensed
' DQN 火柴人强化学习 Demo 入口。

Imports System.Threading
Imports System.Windows.Forms

''' <summary>Application entry point for the DQN Stickman Runner demo.</summary>
Module Program

    <STAThread()>
    Sub Main()
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
        Application.Run(New MainForm())
    End Sub

End Module
