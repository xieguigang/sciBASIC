Imports System.Windows.Forms

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class UserControl
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose( disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.wbCtlRender = New System.Windows.Forms.WebBrowser()
        Me.SuspendLayout()
        '
        'wbCtlRender
        '
        Me.wbCtlRender.AllowWebBrowserDrop = False
        Me.wbCtlRender.Dock = System.Windows.Forms.DockStyle.Fill
        Me.wbCtlRender.IsWebBrowserContextMenuEnabled = False
        Me.wbCtlRender.Location = New System.Drawing.Point(0, 0)
        Me.wbCtlRender.MinimumSize = New System.Drawing.Size(20, 20)
        Me.wbCtlRender.Name = "wbCtlRender"
        Me.wbCtlRender.ScrollBarsEnabled = False
        Me.wbCtlRender.Size = New System.Drawing.Size(593, 349)
        Me.wbCtlRender.TabIndex = 0
        Me.wbCtlRender.WebBrowserShortcutsEnabled = False
        '
        'HtmlUserControl
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.wbCtlRender)
        Me.Name = "HtmlUserControl"
        Me.Size = New System.Drawing.Size(593, 349)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents wbCtlRender As WebBrowser
End Class