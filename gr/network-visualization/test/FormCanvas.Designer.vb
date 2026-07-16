#Region "Microsoft.VisualBasic::488d0207aaded3ae2b7a3bfea383ed27, gr\network-visualization\test\FormCanvas.Designer.vb"

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

    '   Total Lines: 128
    '    Code Lines: 90 (70.31%)
    ' Comment Lines: 32 (25.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 6 (4.69%)
    '     File Size: 6.18 KB


    ' Class FormCanvas
    ' 
    '     Sub: Dispose, InitializeComponent
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Windows.Forms

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FormCanvas
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(disposing As Boolean)
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
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        MenuStrip1 = New MenuStrip()
        FileToolStripMenuItem = New ToolStripMenuItem()
        SaveAsSVGToolStripMenuItem = New ToolStripMenuItem()
        RefreshParametersToolStripMenuItem = New ToolStripMenuItem()
        DToolStripMenuItem = New ToolStripMenuItem()
        AutoRotateToolStripMenuItem = New ToolStripMenuItem()
        ShowLabelsToolStripMenuItem = New ToolStripMenuItem()
        TrackBar1 = New TrackBar()
        MenuStrip1.SuspendLayout()
        CType(TrackBar1, System.ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' MenuStrip1
        ' 
        MenuStrip1.Items.AddRange(New ToolStripItem() {FileToolStripMenuItem})
        MenuStrip1.Location = New System.Drawing.Point(0, 0)
        MenuStrip1.Name = "MenuStrip1"
        MenuStrip1.Padding = New Padding(7, 2, 0, 2)
        MenuStrip1.Size = New System.Drawing.Size(803, 24)
        MenuStrip1.TabIndex = 0
        MenuStrip1.Text = "MenuStrip1"
        ' 
        ' FileToolStripMenuItem
        ' 
        FileToolStripMenuItem.DropDownItems.AddRange(New ToolStripItem() {SaveAsSVGToolStripMenuItem, RefreshParametersToolStripMenuItem, DToolStripMenuItem, AutoRotateToolStripMenuItem, ShowLabelsToolStripMenuItem})
        FileToolStripMenuItem.Name = "FileToolStripMenuItem"
        FileToolStripMenuItem.Size = New System.Drawing.Size(37, 20)
        FileToolStripMenuItem.Text = "File"
        ' 
        ' SaveAsSVGToolStripMenuItem
        ' 
        SaveAsSVGToolStripMenuItem.Name = "SaveAsSVGToolStripMenuItem"
        SaveAsSVGToolStripMenuItem.Size = New System.Drawing.Size(175, 22)
        SaveAsSVGToolStripMenuItem.Text = "Save As SVG"
        ' 
        ' RefreshParametersToolStripMenuItem
        ' 
        RefreshParametersToolStripMenuItem.Name = "RefreshParametersToolStripMenuItem"
        RefreshParametersToolStripMenuItem.Size = New System.Drawing.Size(175, 22)
        RefreshParametersToolStripMenuItem.Text = "Refresh Parameters"
        ' 
        ' DToolStripMenuItem
        ' 
        DToolStripMenuItem.CheckOnClick = True
        DToolStripMenuItem.Name = "DToolStripMenuItem"
        DToolStripMenuItem.Size = New System.Drawing.Size(175, 22)
        DToolStripMenuItem.Text = "3D"
        ' 
        ' AutoRotateToolStripMenuItem
        ' 
        AutoRotateToolStripMenuItem.Checked = True
        AutoRotateToolStripMenuItem.CheckOnClick = True
        AutoRotateToolStripMenuItem.CheckState = CheckState.Checked
        AutoRotateToolStripMenuItem.Name = "AutoRotateToolStripMenuItem"
        AutoRotateToolStripMenuItem.Size = New System.Drawing.Size(175, 22)
        AutoRotateToolStripMenuItem.Text = "Auto Rotate"
        ' 
        ' ShowLabelsToolStripMenuItem
        ' 
        ShowLabelsToolStripMenuItem.Checked = True
        ShowLabelsToolStripMenuItem.CheckOnClick = True
        ShowLabelsToolStripMenuItem.CheckState = CheckState.Checked
        ShowLabelsToolStripMenuItem.Name = "ShowLabelsToolStripMenuItem"
        ShowLabelsToolStripMenuItem.Size = New System.Drawing.Size(175, 22)
        ShowLabelsToolStripMenuItem.Text = "Show Labels"
        ' 
        ' TrackBar1
        ' 
        TrackBar1.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        TrackBar1.Location = New System.Drawing.Point(626, 487)
        TrackBar1.Margin = New Padding(4, 3, 4, 3)
        TrackBar1.Maximum = 0
        TrackBar1.Minimum = -60
        TrackBar1.Name = "TrackBar1"
        TrackBar1.Size = New System.Drawing.Size(162, 45)
        TrackBar1.TabIndex = 1
        ' 
        ' FormCanvas
        ' 
        AutoScaleDimensions = New System.Drawing.SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New System.Drawing.Size(803, 553)
        Controls.Add(TrackBar1)
        Controls.Add(MenuStrip1)
        MainMenuStrip = MenuStrip1
        Margin = New Padding(4, 3, 4, 3)
        Name = "FormCanvas"
        Text = "Network Canvas"
        MenuStrip1.ResumeLayout(False)
        MenuStrip1.PerformLayout()
        CType(TrackBar1, System.ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()

    End Sub

    Friend WithEvents MenuStrip1 As MenuStrip
    Friend WithEvents FileToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents SaveAsSVGToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents RefreshParametersToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents DToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents TrackBar1 As TrackBar
    Friend WithEvents AutoRotateToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ShowLabelsToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
End Class

