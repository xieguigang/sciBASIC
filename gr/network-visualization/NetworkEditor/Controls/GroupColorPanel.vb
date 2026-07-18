#Region "Microsoft.VisualBasic::917de53366d09591c50b9508d61011dc, gr\network-visualization\NetworkEditor\Controls\GroupColorPanel.vb"

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

    '   Total Lines: 155
    '    Code Lines: 132 (85.16%)
    ' Comment Lines: 3 (1.94%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 20 (12.90%)
    '     File Size: 6.19 KB


    '     Class GroupColorPanel
    ' 
    '         Properties: State, TargetCanvas
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: btnAdd_Click, btnApply_Click, btnColor_Click, btnDel_Click, lstGroups_DrawItem
    '              RaiseChanged, RefreshGroups
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports NetworkEditor.Models

Namespace NetworkEditor.Controls

    ''' <summary>
    ''' 分组与颜色映射面板：管理分组、取色、批量将分组写入选中节点
    ''' </summary>
    Public Class GroupColorPanel : Inherits UserControl

        Private _state As EditorState = Nothing
        Private _targetCanvas As NetworkEditorCanvas = Nothing

        Private lstGroups As New ListBox() With {.Dock = DockStyle.Fill, .DrawMode = DrawMode.OwnerDrawFixed, .ItemHeight = 18, .IntegralHeight = False}
        Private txtNew As New TextBox() With {.Dock = DockStyle.Fill, .Text = "新分组名"}
        Private btnAdd As New Button() With {.Dock = DockStyle.Fill, .Text = "新增分组"}
        Private btnDel As New Button() With {.Dock = DockStyle.Fill, .Text = "删除分组"}
        Private btnColor As New Button() With {.Dock = DockStyle.Fill, .Text = "编辑颜色"}
        Private btnApply As New Button() With {.Dock = DockStyle.Fill, .Text = "应用到选中节点"}
        Private colorDlg As New ColorDialog()

        Public Event Changed As EventHandler

        Public Property State As EditorState
            Get
                Return _state
            End Get
            Set(value As EditorState)
                _state = value
                RefreshGroups()
            End Set
        End Property

        Public Property TargetCanvas As NetworkEditorCanvas
            Get
                Return _targetCanvas
            End Get
            Set(value As NetworkEditorCanvas)
                _targetCanvas = value
            End Set
        End Property

        Public Sub New()
            Me.BackColor = Color.FromArgb(&HFF262D3A)
            Me.ForeColor = Color.FromArgb(&HFFE6EAF0)

            Dim tbl As New TableLayoutPanel() With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 1,
                .RowCount = 6,
                .BackColor = Me.BackColor
            }
            tbl.RowStyles.Add(New RowStyle(SizeType.Absolute, 24))
            tbl.RowStyles.Add(New RowStyle(SizeType.Absolute, 28))
            tbl.RowStyles.Add(New RowStyle(SizeType.Absolute, 28))
            tbl.RowStyles.Add(New RowStyle(SizeType.Absolute, 28))
            tbl.RowStyles.Add(New RowStyle(SizeType.Absolute, 28))
            tbl.RowStyles.Add(New RowStyle(SizeType.Percent, 100))

            tbl.Controls.Add(txtNew, 0, 0)
            tbl.Controls.Add(btnAdd, 0, 1)
            tbl.Controls.Add(btnDel, 0, 2)
            tbl.Controls.Add(btnColor, 0, 3)
            tbl.Controls.Add(btnApply, 0, 4)
            tbl.Controls.Add(lstGroups, 0, 5)

            Me.Controls.Add(tbl)

            AddHandler lstGroups.DrawItem, AddressOf lstGroups_DrawItem
            AddHandler btnAdd.Click, AddressOf btnAdd_Click
            AddHandler btnDel.Click, AddressOf btnDel_Click
            AddHandler btnColor.Click, AddressOf btnColor_Click
            AddHandler btnApply.Click, AddressOf btnApply_Click
        End Sub

        Public Sub RefreshGroups()
            lstGroups.Items.Clear()
            If _state IsNot Nothing Then
                For Each g In _state.GroupColors.Groups
                    lstGroups.Items.Add(g)
                Next
            End If
        End Sub

        Private Sub lstGroups_DrawItem(sender As Object, e As DrawItemEventArgs)
            If e.Index < 0 Then
                Return
            End If
            Dim g = e.Graphics
            g.FillRectangle(New SolidBrush(e.BackColor), e.Bounds)
            Dim name = CStr(lstGroups.Items(e.Index))
            Dim col = If(_state IsNot Nothing, _state.GroupColors(name), Color.Gray)
            g.FillRectangle(New SolidBrush(col), e.Bounds.X + 2, e.Bounds.Y + 2, 14, 14)
            g.DrawRectangle(Pens.Black, e.Bounds.X + 2, e.Bounds.Y + 2, 14, 14)
            Using fnt = New Font("Segoe UI", 9)
                g.DrawString(name, fnt, New SolidBrush(e.ForeColor), e.Bounds.X + 20, e.Bounds.Y + 2)
            End Using
        End Sub

        Private Sub btnAdd_Click(sender As Object, e As EventArgs)
            Dim name = txtNew.Text.Trim()
            If name = "" OrElse name = "新分组名" Then
                name = "group" & (_state.GroupColors.Groups.Length + 1)
            End If
            If Not _state.GroupColors.Contains(name) Then
                _state.GroupColors.Add(name)
                RefreshGroups()
                RaiseChanged()
            End If
        End Sub

        Private Sub btnDel_Click(sender As Object, e As EventArgs)
            If lstGroups.SelectedItem Is Nothing Then
                Return
            End If
            _state.GroupColors.Remove(CStr(lstGroups.SelectedItem))
            RefreshGroups()
            RaiseChanged()
        End Sub

        Private Sub btnColor_Click(sender As Object, e As EventArgs)
            If lstGroups.SelectedItem Is Nothing Then
                Return
            End If
            Dim name = CStr(lstGroups.SelectedItem)
            colorDlg.Color = _state.GroupColors(name)
            If colorDlg.ShowDialog() = DialogResult.OK Then
                _state.GroupColors(name) = colorDlg.Color
                RefreshGroups()
                RaiseChanged()
            End If
        End Sub

        Private Sub btnApply_Click(sender As Object, e As EventArgs)
            If lstGroups.SelectedItem Is Nothing Then
                Return
            End If
            If _targetCanvas Is Nothing OrElse _targetCanvas.State Is Nothing Then
                Return
            End If
            Dim name = CStr(lstGroups.SelectedItem)
            For Each n As Node In _targetCanvas.State.SelectedNodes
                _targetCanvas.State.SetNodeGroup(n, name)
            Next
            RaiseChanged()
        End Sub

        Private Sub RaiseChanged()
            RaiseEvent Changed(Me, EventArgs.Empty)
        End Sub
    End Class

End Namespace

