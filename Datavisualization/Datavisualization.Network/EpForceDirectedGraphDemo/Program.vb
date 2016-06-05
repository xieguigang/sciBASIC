Imports System.Collections.Generic
Imports System.Linq
Imports System.Windows.Forms
Imports EpForceDirectedGraph.EpForceDirectedGraphDemo

Module Program

    ''' <summary>
    ''' The main entry point for the application.
    ''' </summary>
    <STAThread>
    Sub Main()
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
        Application.Run(New ForceDirectedGraphForm())
    End Sub
End Module
