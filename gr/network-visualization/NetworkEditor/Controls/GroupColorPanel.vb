Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports NetworkEditor.Models

Namespace NetworkEditor.Controls

    ''' <summary>
    ''' 分组与颜色映射面板：管理分组、取色、批量将分组写入选中节点
    ''' </summary>
    Public Class GroupColorPanel : Inherits UserControl

        Private state As EditorState = Nothing
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
                Return state
            End Get
            Set(value As EditorState)
                state = value
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
            If state IsNot Nothing Then
                For Each g In state.GroupColors.Groups
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
            Dim col = If(state IsNot Nothing, state.GroupColors(name), Color.Gray)
            g.FillRectangle(New SolidBrush(col), e.Bounds.X + 2, e.Bounds.Y + 2, 14, 14)
            g.DrawRectangle(Pens.Black, e.Bounds.X + 2, e.Bounds.Y + 2, 14, 14)
            Using fnt = New Font("Segoe UI", 9)
                g.DrawString(name, fnt, New SolidBrush(e.ForeColor), e.Bounds.X + 20, e.Bounds.Y + 2)
            End Using
        End Sub

        Private Sub btnAdd_Click(sender As Object, e As EventArgs)
            Dim name = txtNew.Text.Trim()
            If name = "" OrElse name = "新分组名" Then
                name = "group" & (state.GroupColors.Groups.Length + 1)
            End If
            If Not state.GroupColors.Contains(name) Then
                state.GroupColors.Add(name)
                RefreshGroups()
                RaiseChanged()
            End If
        End Sub

        Private Sub btnDel_Click(sender As Object, e As EventArgs)
            If lstGroups.SelectedItem Is Nothing Then
                Return
            End If
            state.GroupColors.Remove(CStr(lstGroups.SelectedItem))
            RefreshGroups()
            RaiseChanged()
        End Sub

        Private Sub btnColor_Click(sender As Object, e As EventArgs)
            If lstGroups.SelectedItem Is Nothing Then
                Return
            End If
            Dim name = CStr(lstGroups.SelectedItem)
            colorDlg.Color = state.GroupColors(name)
            If colorDlg.ShowDialog() = DialogResult.OK Then
                state.GroupColors(name) = colorDlg.Color
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
