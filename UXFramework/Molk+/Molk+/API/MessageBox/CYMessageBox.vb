#Region "Microsoft.VisualBasic::41e1fd5c3911f731e70ecf49358f975a, ..\visualbasic_App\UXFramework\Molk+\Molk+\API\MessageBox\CYMessageBox.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Collections.Generic
Imports System.Text
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports System.Runtime.InteropServices

Friend Class CYMessageBox : Inherits Microsoft.VisualBasic.MolkPlusTheme.Windows.Forms.Form

    <DllImport("user32.dll", CharSet:=CharSet.Auto)>
    Private Shared Function MessageBeep(type As UInteger) As Boolean
    End Function

    <DllImport("Shell32.dll")>
    Public Shared Function ExtractIconEx(libName As String, iconIndex As Integer, largeIcon As IntPtr(), smallIcon As IntPtr(), nIcons As Integer) As Integer
    End Function

    Private Shared largeIcon As IntPtr()
    Private Shared smallIcon As IntPtr()

    Private Shared newMessageBox As CYMessageBox
    Private Shared frmTitle As Label
    Private Shared frmMessage As Label
    Private Shared pIcon As PictureBox
    Private Shared flpButtons As FlowLayoutPanel
    Private Shared frmIcon As Icon

    Private Shared btnOK As Button
    Private Shared btnAbort As Button
    Private Shared btnRetry As Button
    Private Shared btnIgnore As Button
    Private Shared btnCancel As Button
    Private Shared btnYes As Button
    Private Shared btnNo As Button
    Private Shared CYReturnButton As DialogResult

    Private Shared Sub BuildMessageBox(title As String)
        newMessageBox = New CYMessageBox()
        newMessageBox.Text = title
        newMessageBox.Size = New System.Drawing.Size(400, 200)
        newMessageBox.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        newMessageBox.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        AddHandler newMessageBox.Paint, New PaintEventHandler(AddressOf PaintNewMessageBox)
        newMessageBox.BackColor = System.Drawing.Color.White

        Dim tlp As New TableLayoutPanel()
        tlp.RowCount = 3
        tlp.ColumnCount = 0
        tlp.Dock = System.Windows.Forms.DockStyle.Fill
        tlp.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22))
        tlp.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0F))
        tlp.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50))
        tlp.BackColor = System.Drawing.Color.Transparent
        tlp.Padding = New Padding(2, 5, 2, 2)

        frmTitle = New Label()
        frmTitle.Dock = System.Windows.Forms.DockStyle.Fill
        frmTitle.BackColor = System.Drawing.Color.Transparent
        frmTitle.ForeColor = System.Drawing.Color.White
        frmTitle.Font = New Font("Tahoma", 9, FontStyle.Bold)

        frmMessage = New Label()
        frmMessage.Dock = System.Windows.Forms.DockStyle.Fill
        frmMessage.BackColor = System.Drawing.Color.White
        frmMessage.Font = New Font("Tahoma", 9, FontStyle.Regular)
        frmMessage.Text = "hiii"

        largeIcon = New IntPtr(249) {}
        smallIcon = New IntPtr(249) {}
        pIcon = New PictureBox()
        ExtractIconEx("shell32.dll", 0, largeIcon, smallIcon, 250)

        flpButtons = New FlowLayoutPanel()
        flpButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft
        flpButtons.Padding = New Padding(0, 5, 5, 0)
        flpButtons.Dock = System.Windows.Forms.DockStyle.Fill
        flpButtons.BackColor = System.Drawing.Color.FromArgb(240, 240, 240)

        Dim tlpMessagePanel As New TableLayoutPanel()
        tlpMessagePanel.BackColor = System.Drawing.Color.White
        tlpMessagePanel.Dock = System.Windows.Forms.DockStyle.Fill
        tlpMessagePanel.ColumnCount = 2
        tlpMessagePanel.RowCount = 0
        tlpMessagePanel.Padding = New Padding(4, 5, 4, 4)
        tlpMessagePanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50))
        tlpMessagePanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0F))
        tlpMessagePanel.Controls.Add(pIcon)
        tlpMessagePanel.Controls.Add(frmMessage)

        tlp.Controls.Add(frmTitle)
        tlp.Controls.Add(tlpMessagePanel)
        tlp.Controls.Add(flpButtons)
        newMessageBox.Controls.Add(tlp)
        tlp.SendToBack()
        'newMessageBox.Caption1.Dock = DockStyle.Top
        ' newMessageBox.Caption1.BringToFront()
    End Sub

    ''' <summary>
    ''' Message: Text to display in the message box.
    ''' </summary>
    Public Overloads Shared Function Show(Message As String) As DialogResult
        BuildMessageBox("")
        frmMessage.Text = Message
        ShowOKButton()
        newMessageBox.ShowDialog()
        Return CYReturnButton
    End Function

    ''' <summary>
    ''' Title: Text to display in the title bar of the messagebox.
    ''' </summary>
    Public Overloads Shared Function Show(Message As String, Title As String) As DialogResult
        BuildMessageBox(Title)
        frmTitle.Text = Title
        frmMessage.Text = Message
        ShowOKButton()
        newMessageBox.ShowDialog()
        Return CYReturnButton
    End Function

    ''' <summary>
    ''' MButtons: Display CYButtons on the message box.
    ''' </summary>
    Public Overloads Shared Function Show(Message As String, Title As String, MButtons As CYButtons) As DialogResult
        BuildMessageBox(Title)
        ' BuildMessageBox method, responsible for creating the MessageBox
        frmTitle.Text = Title
        ' Set the title of the MessageBox
        frmMessage.Text = Message
        'Set the text of the MessageBox
        ButtonStatements(MButtons)
        ' ButtonStatements method is responsible for showing the appropreiate buttons
        newMessageBox.ShowDialog()
        ' Show the MessageBox as a Dialog.
        Return CYReturnButton
        ' Return the button click as an Enumerator
    End Function

    ''' <summary>
    ''' MIcon: Display CYIcon on the message box.
    ''' </summary>
    Public Overloads Shared Function Show(Message As String, Title As String, MButtons As CYButtons, MIcon As CYIcon) As DialogResult
        BuildMessageBox(Title)
        frmTitle.Text = Title
        frmMessage.Text = Message
        ButtonStatements(MButtons)
        IconStatements(MIcon)
        Dim imageIcon As Image = New Bitmap(frmIcon.ToBitmap(), 38, 38)
        pIcon.Image = imageIcon
        newMessageBox.ShowDialog()
        Return CYReturnButton
    End Function

    Private Shared Sub btnOK_Click(sender As Object, e As EventArgs)
        CYReturnButton = DialogResult.OK
        newMessageBox.Dispose()
    End Sub

    Private Shared Sub btnAbort_Click(sender As Object, e As EventArgs)
        CYReturnButton = DialogResult.Abort
        newMessageBox.Dispose()
    End Sub

    Private Shared Sub btnRetry_Click(sender As Object, e As EventArgs)
        CYReturnButton = DialogResult.Retry
        newMessageBox.Dispose()
    End Sub

    Private Shared Sub btnIgnore_Click(sender As Object, e As EventArgs)
        CYReturnButton = DialogResult.Ignore
        newMessageBox.Dispose()
    End Sub

    Private Shared Sub btnCancel_Click(sender As Object, e As EventArgs)
        CYReturnButton = DialogResult.Cancel
        newMessageBox.Dispose()
    End Sub

    Private Shared Sub btnYes_Click(sender As Object, e As EventArgs)
        CYReturnButton = DialogResult.Yes
        newMessageBox.Dispose()
    End Sub

    Private Shared Sub btnNo_Click(sender As Object, e As EventArgs)
        CYReturnButton = DialogResult.No
        newMessageBox.Dispose()
    End Sub

    Private Shared Sub ShowOKButton()
        btnOK = New Button()
        btnOK.Text = "OK"
        btnOK.Size = New System.Drawing.Size(80, 25)
        btnOK.BackColor = System.Drawing.Color.FromArgb(255, 255, 255)
        btnOK.Font = New Font("Tahoma", 8, FontStyle.Regular)
        AddHandler btnOK.Click, New EventHandler(AddressOf btnOK_Click)
        flpButtons.Controls.Add(btnOK)
    End Sub

    Private Shared Sub ShowAbortButton()
        btnAbort = New Button()
        btnAbort.Text = "Abort"
        btnAbort.Size = New System.Drawing.Size(80, 25)
        btnAbort.BackColor = System.Drawing.Color.FromArgb(255, 255, 255)
        btnAbort.Font = New Font("Tahoma", 8, FontStyle.Regular)
        AddHandler btnAbort.Click, New EventHandler(AddressOf btnAbort_Click)
        flpButtons.Controls.Add(btnAbort)
    End Sub

    Private Shared Sub ShowRetryButton()
        btnRetry = New Button()
        btnRetry.Text = "Retry"
        btnRetry.Size = New System.Drawing.Size(80, 25)
        btnRetry.BackColor = System.Drawing.Color.FromArgb(255, 255, 255)
        btnRetry.Font = New Font("Tahoma", 8, FontStyle.Regular)
        AddHandler btnRetry.Click, New EventHandler(AddressOf btnRetry_Click)
        flpButtons.Controls.Add(btnRetry)
    End Sub

    Private Shared Sub ShowIgnoreButton()
        btnIgnore = New Button()
        btnIgnore.Text = "Ignore"
        btnIgnore.Size = New System.Drawing.Size(80, 25)
        btnIgnore.BackColor = System.Drawing.Color.FromArgb(255, 255, 255)
        btnIgnore.Font = New Font("Tahoma", 8, FontStyle.Regular)
        AddHandler btnIgnore.Click, New EventHandler(AddressOf btnIgnore_Click)
        flpButtons.Controls.Add(btnIgnore)
    End Sub

    Private Shared Sub ShowCancelButton()
        btnCancel = New Button()
        btnCancel.Text = "Cancel"
        btnCancel.Size = New System.Drawing.Size(80, 25)
        btnCancel.BackColor = System.Drawing.Color.FromArgb(255, 255, 255)
        btnCancel.Font = New Font("Tahoma", 8, FontStyle.Regular)
        AddHandler btnCancel.Click, New EventHandler(AddressOf btnCancel_Click)
        flpButtons.Controls.Add(btnCancel)
    End Sub

    Private Shared Sub ShowYesButton()
        btnYes = New Button()
        btnYes.Text = "Yes"
        btnYes.Size = New System.Drawing.Size(80, 25)
        btnYes.BackColor = System.Drawing.Color.FromArgb(255, 255, 255)
        btnYes.Font = New Font("Tahoma", 8, FontStyle.Regular)
        AddHandler btnYes.Click, New EventHandler(AddressOf btnYes_Click)
        flpButtons.Controls.Add(btnYes)
    End Sub

    Private Shared Sub ShowNoButton()
        btnNo = New Button()
        btnNo.Text = "No"
        btnNo.Size = New System.Drawing.Size(80, 25)
        btnNo.BackColor = System.Drawing.Color.FromArgb(255, 255, 255)
        btnNo.Font = New Font("Tahoma", 8, FontStyle.Regular)
        AddHandler btnNo.Click, New EventHandler(AddressOf btnNo_Click)
        flpButtons.Controls.Add(btnNo)
    End Sub

    Private Shared Sub ButtonStatements(MButtons As CYButtons)
        If MButtons = CYButtons.AbortRetryIgnore Then
            ShowIgnoreButton()
            ShowRetryButton()
            ShowAbortButton()
        End If

        If MButtons = CYButtons.OK Then
            ShowOKButton()
        End If

        If MButtons = CYButtons.OKCancel Then
            ShowCancelButton()
            ShowOKButton()
        End If

        If MButtons = CYButtons.RetryCancel Then
            ShowCancelButton()
            ShowRetryButton()
        End If

        If MButtons = CYButtons.YesNo Then
            ShowNoButton()
            ShowYesButton()
        End If

        If MButtons = CYButtons.YesNoCancel Then
            ShowCancelButton()
            ShowNoButton()
            ShowYesButton()
        End If
    End Sub

    Private Shared Sub IconStatements(MIcon As CYIcon)
        If MIcon = CYIcon.[Error] Then
            MessageBeep(30)
            frmIcon = Icon.FromHandle(largeIcon(109))
        End If

        If MIcon = CYIcon.Explorer Then
            MessageBeep(0)
            frmIcon = Icon.FromHandle(largeIcon(220))
        End If

        If MIcon = CYIcon.Find Then
            MessageBeep(0)
            frmIcon = Icon.FromHandle(largeIcon(22))
        End If

        If MIcon = CYIcon.Information Then
            MessageBeep(0)
            frmIcon = Icon.FromHandle(largeIcon(221))
        End If

        If MIcon = CYIcon.Mail Then
            MessageBeep(0)
            frmIcon = Icon.FromHandle(largeIcon(156))
        End If

        If MIcon = CYIcon.Media Then
            MessageBeep(0)
            frmIcon = Icon.FromHandle(largeIcon(116))
        End If

        If MIcon = CYIcon.Print Then
            MessageBeep(0)
            frmIcon = Icon.FromHandle(largeIcon(136))
        End If

        If MIcon = CYIcon.Question Then
            MessageBeep(0)
            frmIcon = Icon.FromHandle(largeIcon(23))
        End If

        If MIcon = CYIcon.RecycleBinEmpty Then
            MessageBeep(0)
            frmIcon = Icon.FromHandle(largeIcon(31))
        End If

        If MIcon = CYIcon.RecycleBinFull Then
            MessageBeep(0)
            frmIcon = Icon.FromHandle(largeIcon(32))
        End If

        If MIcon = CYIcon.[Stop] Then
            MessageBeep(0)
            frmIcon = Icon.FromHandle(largeIcon(27))
        End If

        If MIcon = CYIcon.User Then
            MessageBeep(0)
            frmIcon = Icon.FromHandle(largeIcon(170))
        End If

        If MIcon = CYIcon.Warning Then
            MessageBeep(30)
            frmIcon = Icon.FromHandle(largeIcon(217))
        End If
    End Sub

    Private Shared Sub PaintNewMessageBox(sender As Object, e As PaintEventArgs)
        Dim g As Graphics = e.Graphics
        Dim frmTitleL As New Rectangle(0, 0, (newMessageBox.Width \ 2), 22)
        Dim frmTitleR As New Rectangle((newMessageBox.Width \ 2), 0, (newMessageBox.Width \ 2), 22)
        Dim frmMessageBox As New Rectangle(0, 0, (newMessageBox.Width - 1), (newMessageBox.Height - 1))
        Dim frmLGBL As New LinearGradientBrush(frmTitleL, Color.FromArgb(87, 148, 160), Color.FromArgb(209, 230, 243), LinearGradientMode.Horizontal)
        Dim frmLGBR As New LinearGradientBrush(frmTitleR, Color.FromArgb(209, 230, 243), Color.FromArgb(87, 148, 160), LinearGradientMode.Horizontal)
        Dim frmPen As New Pen(Color.FromArgb(63, 119, 143), 1)
        g.FillRectangle(frmLGBL, frmTitleL)
        g.FillRectangle(frmLGBR, frmTitleR)
        g.DrawRectangle(frmPen, frmMessageBox)
    End Sub

    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(CYMessageBox))
        Me.SuspendLayout()
        '
        'CYMessageBox
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.ClientSize = New System.Drawing.Size(612, 384)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "CYMessageBox"
        Me.ResumeLayout(False)

    End Sub

    Private Sub CYMessageBox_Load(sender As Object, e As EventArgs) Handles Me.Load

    End Sub
End Class
