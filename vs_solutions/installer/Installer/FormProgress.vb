Imports System.Text
Imports System.Threading
Imports Microsoft.VisualBasic.ApplicationServices.SoftwareToolkits

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