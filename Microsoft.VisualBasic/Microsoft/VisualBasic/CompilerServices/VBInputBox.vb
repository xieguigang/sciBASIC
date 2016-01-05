Imports System
Imports System.ComponentModel
Imports System.Drawing
Imports System.Globalization
Imports System.Windows.Forms

Namespace Microsoft.VisualBasic.CompilerServices
    Friend NotInheritable Class VBInputBox
        Inherits Form
        ' Methods
        Friend Sub New()
            Me.Output = ""
            Me.InitializeComponent
        End Sub

        Friend Sub New(Prompt As String, Title As String, DefaultResponse As String, XPos As Integer, YPos As Integer)
            Me.Output = ""
            Me.InitializeComponent()
            Me.InitializeInputBox(Prompt, Title, DefaultResponse, XPos, YPos)
        End Sub

        Protected Overrides Sub Dispose(disposing As Boolean)
            If (disposing AndAlso (Not Me.components Is Nothing)) Then
                Me.components.Dispose()
            End If
            MyBase.Dispose(disposing)
        End Sub

        Private Sub InitializeComponent()
            Me.OKButton = New Button
            Me.MyCancelButton = New Button
            Me.TextBox = New TextBox
            Me.Label = New Label
            MyBase.SuspendLayout()
            Dim manager1 As New ComponentResourceManager(GetType(VBInputBox))
            manager1.ApplyResources(Me.OKButton, "OKButton", CultureInfo.CurrentUICulture)
            Me.OKButton.Name = "OKButton"
            Me.MyCancelButton.DialogResult = DialogResult.Cancel
            manager1.ApplyResources(Me.MyCancelButton, "MyCancelButton", CultureInfo.CurrentUICulture)
            Me.MyCancelButton.Name = "MyCancelButton"
            manager1.ApplyResources(Me.TextBox, "TextBox", CultureInfo.CurrentUICulture)
            Me.TextBox.Name = "TextBox"
            manager1.ApplyResources(Me.Label, "Label", CultureInfo.CurrentUICulture)
            Me.Label.Name = "Label"
            MyBase.AcceptButton = Me.OKButton
            manager1.ApplyResources(Me, "$this", CultureInfo.CurrentUICulture)
            MyBase.CancelButton = Me.MyCancelButton
            MyBase.Controls.Add(Me.TextBox)
            MyBase.Controls.Add(Me.Label)
            MyBase.Controls.Add(Me.OKButton)
            MyBase.Controls.Add(Me.MyCancelButton)
            MyBase.FormBorderStyle = FormBorderStyle.FixedDialog
            MyBase.MaximizeBox = False
            MyBase.MinimizeBox = False
            MyBase.Name = "VBInputBox"
            MyBase.ResumeLayout(False)
            MyBase.PerformLayout()
        End Sub

        Private Sub InitializeInputBox(Prompt As String, Title As String, DefaultResponse As String, XPos As Integer, YPos As Integer)
            Me.Text = Title
            Me.Label.Text = Prompt
            Me.TextBox.Text = DefaultResponse
            AddHandler Me.OKButton.Click, New EventHandler(AddressOf Me.OKButton_Click)
            AddHandler Me.MyCancelButton.Click, New EventHandler(AddressOf Me.MyCancelButton_Click)
            Dim graphics1 As Graphics = Me.Label.CreateGraphics
            Dim ef As SizeF = graphics1.MeasureString(Prompt, Me.Label.Font, Me.Label.Width)
            graphics1.Dispose()

            If (ef.Height > Me.Label.Height) Then
                Dim num As Integer = (CInt(Math.Round(CDbl(ef.Height))) - Me.Label.Height)
                Dim label As Label = Me.Label
                label.Height = (label.Height + num)
                Dim textBox As TextBox = Me.TextBox
                textBox.Top = (textBox.Top + num)
                MyBase.Height = (MyBase.Height + num)
            End If
            If ((XPos = -1) AndAlso (YPos = -1)) Then
                MyBase.StartPosition = FormStartPosition.CenterScreen
            Else
                If (XPos = -1) Then
                    XPos = 600
                End If
                If (YPos = -1) Then
                    YPos = 350
                End If
                MyBase.StartPosition = FormStartPosition.Manual
                MyBase.DesktopLocation = New Point(XPos, YPos)
            End If
        End Sub

        Private Sub MyCancelButton_Click(sender As Object, e As EventArgs)
            MyBase.Close()
        End Sub

        Private Sub OKButton_Click(sender As Object, e As EventArgs)
            Me.Output = Me.TextBox.Text
            MyBase.Close()
        End Sub


        ' Fields
        Private components As Container
        Private Label As Label
        Private MyCancelButton As Button
        Private OKButton As Button
        Public Output As String
        Private TextBox As TextBox
    End Class
End Namespace

