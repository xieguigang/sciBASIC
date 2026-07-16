' /********************************************************************************/
'
'     FluidSim3D - a 3D water simulator driven by the 3D SPH fluid engine.
' 
'     This is the custom application entry point (MyType = WindowsFormsWithCustomSubMain).
'
' /********************************************************************************/

Imports System.Windows.Forms

Namespace FluidSim3D

    ''' <summary>
    ''' custom application entry point for the WinForm 3D water simulator.
    ''' </summary>
    Public Module Program

        <System.STAThread()>
        Public Sub Main()
            Application.EnableVisualStyles()
            Application.SetCompatibleTextRenderingDefault(False)
            Application.Run(New MainForm())
        End Sub

    End Module

End Namespace
