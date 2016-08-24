#Region "Microsoft.VisualBasic::2721734a856bf7c560565c7cdda04b40, ..\visualbasic_App\UXFramework\Molk+\Molk+\Forms.Controls\ListControl\DropDownList\UserList\UserPicker.vb"

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
Imports System.ComponentModel
Imports System.Drawing
Imports System.Data
Imports System.Linq
Imports System.Text
Imports System.Windows.Forms
Imports Microsoft.VisualBasic.MolkPlusTheme.Unity3.Controls
Imports Microsoft.VisualBasic.Scripting

Partial Public Class UserPicker
    Inherits DropDownControl

    Public Sub New()
        InitializeComponent()
        InitializeDropDown(listView1)

        listView1.Width = 182
        Me.Text = ""
    End Sub

    Friend WithEvents listView1 As ListControl
    Public Event ItemClick(obj As UserCard)

    Public ReadOnly Property Items As UserCard()
        Get
            Return Me.listView1.Items
        End Get
    End Property

    Dim _SelectedIndex As Integer

    Public Property SelectedIndex As Integer
        Get
            Return _SelectedIndex
        End Get
        Set(value As Integer)
            _SelectedIndex = value

            If _SelectedIndex = -1 OrElse listView1.Items.IsNullOrEmpty Then
                SelectedItem = Nothing
            Else
                SelectedItem = listView1.Items(SelectedIndex).As(Of UserCard)
            End If
        End Set
    End Property

    Public Property SelectedItem As UserCard

    Private Sub listView1_ItemClick(sender As Object, Index As Integer) Handles listView1.ItemClick
        If Me.DropState = DropStates.Dropping OrElse Me.DropState = DropStates.Closing Then
            Return
        End If

        Dim obj As UserCard = Me.listView1.Items(Index).As(Of UserCard)

        Call Me.CloseDropDown()
        RaiseEvent ItemClick(obj)

        Me.Text = obj.Title
    End Sub

    Public Sub Add(Title As String, Summary As String, Icon As Image)
        Dim Card As New UserCard With {.Title = Title, .Summary = Summary, .IconHead = Icon}
        Card.Size = New Size(192, 46)
        Call Me.listView1.Add(Icon, Card)
    End Sub

    Private Sub InitializeComponent()
        Me.listView1 = New Microsoft.VisualBasic.MolkPlusTheme.ListControl()
        Me.SuspendLayout()
        '
        'listView1
        '
        Me.listView1.BackColor = System.Drawing.SystemColors.Window
        Me.listView1.ColorSchema = Nothing
        Me.listView1.Location = New System.Drawing.Point(82, 42)
        Me.listView1.Margin = New System.Windows.Forms.Padding(0)
        Me.listView1.Name = "listView1"
        Me.listView1.Size = New System.Drawing.Size(150, 150)
        Me.listView1.TabIndex = 2
        '
        'UserPicker
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.listView1)
        Me.Name = "UserPicker"
        Me.Size = New System.Drawing.Size(305, 41)
        Me.Controls.SetChildIndex(Me.listView1, 0)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Private Sub UserPicker_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub
End Class
