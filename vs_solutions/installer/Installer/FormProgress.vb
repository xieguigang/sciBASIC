#Region "Microsoft.VisualBasic::cabffe0d5bdc2db0dc87f2abdfa93c20, sciBASIC#\vs_solutions\installer\Installer\FormProgress.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 45
    '    Code Lines: 34
    ' Comment Lines: 0
    '   Blank Lines: 11
    '     File Size: 1.69 KB


    ' Class FormProgress
    ' 
    '     Sub: ButtonNext_Click, FormWelcome_Load, InstallerScript
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports System.Threading
Imports Microsoft.VisualBasic.ApplicationServices

Public Class FormProgress

    Protected Overrides Sub ButtonNext_Click(sender As Object, e As EventArgs)
        Call Microsoft.VisualBasic.Parallel.RunTask(AddressOf New FormFinish() With {.Location = Location}.ShowDialog)
        Me.Close()
    End Sub

    Private Sub FormWelcome_Load(sender As Object, e As EventArgs) Handles Me.Load
        ButtonNext.Text = "Next"
        ButtonNext.Enabled = False
        ButtonNext.BackColor = Color.FromArgb(204, 204, 204)
        ProgressBar1.Focus()
        LabelTitle.Text = "Installing..."

        Highlight(Label2)

        Call New Thread(AddressOf InstallerScript).Start()
    End Sub

    Sub InstallerScript()
        Dim zip$ = App.GetAppSysTempFile(".zip", App.PID)
        Dim logs$

        Call TextBox1.Text.MkDIR
        Call My.Resources.installer.FlushStream(zip)
        Call GZip.ImprovedExtractToDirectory(zip, TextBox1.Text, Overwrite.Always)

        logs = NgenInstaller.Install(PATH:=TextBox1.Text).JoinBy(vbCrLf & vbCrLf)
        logs &= vbCrLf & vbCrLf & "Registering Extensions of the .NET Framework" & vbCrLf & vbCrLf & RegistryUtils.RegisterExtensions(TextBox1.Text, "github/xieguigang")

        Call logs.SaveTo(App.Desktop & "/sciBASIC-install.log", Encoding.UTF8)

        ButtonNext.BackColor = FormTemplate.TemplateColor
        ButtonNext.Enabled = True

        LabelTitle.Text = "Done!"
        ProgressBar1.MarqueeAnimationSpeed = 1000000
        ProgressBar1.Style = ProgressBarStyle.Continuous
        ProgressBar1.Value = ProgressBar1.Maximum
    End Sub
End Class
