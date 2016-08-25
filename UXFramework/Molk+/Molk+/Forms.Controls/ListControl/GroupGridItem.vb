#Region "Microsoft.VisualBasic::8f590f135b7ec40e62613cd01179e273, ..\visualbasic_App\UXFramework\Molk+\Molk+\Forms.Controls\ListControl\GroupGridItem.vb"

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

Public Class GroupGridItem : Inherits ListControlItem
    Friend WithEvents Panel1 As System.Windows.Forms.Panel
    Friend WithEvents Label1 As System.Windows.Forms.Label

    Public Property GroupTag As String
        Get
            Return Label1.Text
        End Get
        Set(value As String)
            Label1.Text = value
        End Set
    End Property

    Dim List As List(Of Control) = New List(Of Control)

    Public Sub AddControl(Control As Control)
        Call Panel1.Controls.Add(Control)
        Call List.Add(Control)
        Call LayoutControls()
    End Sub

    Public Sub Remove(Control As Control)
        Call List.Remove(Control)
        Call Panel1.Controls.Remove(Control)
        Call LayoutControls()
    End Sub

    Public Sub LayoutControls()
        Dim X, Y As Integer
        Dim MaxBottom As Integer

        For Each ctrl In List
            If ctrl.Width + X > Panel1.Width Then
                Y = MaxBottom
                X = 0
            Else
                If MaxBottom < Y + ctrl.Height Then
                    MaxBottom = Y + ctrl.Height
                End If
            End If
            ctrl.Location = New Point(X, Y)
            X += ctrl.Width
        Next
    End Sub

    Sub New()
        Call InitializeComponent()
    End Sub

    Private Sub InitializeComponent()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Dock = System.Windows.Forms.DockStyle.Top
        Me.Label1.Location = New System.Drawing.Point(0, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(42, 17)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Label1"
        '
        'Panel1
        '
        Me.Panel1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Panel1.Location = New System.Drawing.Point(0, 17)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(444, 132)
        Me.Panel1.TabIndex = 1
        '
        'GroupGridItem
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 17.0!)
        Me.Controls.Add(Me.Panel1)
        Me.Controls.Add(Me.Label1)
        Me.Name = "GroupGridItem"
        Me.Size = New System.Drawing.Size(444, 149)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Private Sub GroupGridItem_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        Call LayoutControls()
    End Sub
End Class
