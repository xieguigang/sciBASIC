#Region "Microsoft.VisualBasic::255bab6de515c3d59cf5a885f9d7f838, gr\network-visualization\NetworkEditor\MainForm.vb"

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

    '   Total Lines: 223
    '    Code Lines: 183 (82.06%)
    ' Comment Lines: 8 (3.59%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 32 (14.35%)
    '     File Size: 10.09 KB


    '     Class MainForm
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: UniqueNodeLabel
    ' 
    '         Sub: BindPropertyGrid, btnAddNode_Click, btnDelEdge_Click, btnNew_Click, btnRun_Click
    '              BuildUI, OnGraphChanged, OnSelectionChanged, WireEvents
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Generic
Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports NetworkEditor.Adapters
Imports NetworkEditor.Controls
Imports NetworkEditor.Layout
Imports NetworkEditor.Models


Namespace NetworkEditor

    Public Class MainForm : Inherits Form

        Private state As New EditorState
        Private canvas As NetworkEditorCanvas
        Private minimap As MinimapControl
        Private groupPanel As GroupColorPanel
        Private pgObject As PropertyGrid
        Private pgLayout As PropertyGrid
        Private toolStrip As ToolStrip
        Private statusLabel As ToolStripStatusLabel
        Private runners As List(Of ILayoutRunner)
        Private currentRunner As ILayoutRunner
        Private btnLink As ToolStripButton

        Public Sub New()
            Me.Text = "Network Editor"
            Me.Size = New Size(1100, 720)
            Me.BackColor = Color.FromArgb(&HFF1E2430)
            Me.ForeColor = Color.FromArgb(&HFFE6EAF0)
            Call BuildUI()
            Call WireEvents()
        End Sub

        Private Sub BuildUI()
            ' 画布
            canvas = New NetworkEditorCanvas() With {.Dock = DockStyle.Fill, .State = state}

            ' 工具栏
            toolStrip = New ToolStrip() With {
                .Dock = DockStyle.Top,
                .GripStyle = ToolStripGripStyle.Hidden,
                .BackColor = Color.FromArgb(&HFF262D3A),
                .ForeColor = Color.FromArgb(&HFFE6EAF0)
            }

            Dim btnNew = New ToolStripButton("新建")
            Dim btnAdd = New ToolStripButton("添加节点")
            btnLink = New ToolStripButton("连线模式") With {.CheckOnClick = True}
            Dim btnDelEdge = New ToolStripButton("删除选中边")
            Dim btnFit = New ToolStripButton("适配视图")
            Dim btnReset = New ToolStripButton("重置视图")

            Dim dropLayout = New ToolStripDropDownButton("布局")
            runners = New List(Of ILayoutRunner) From {
                New ForceDirectedRunner(),
                New SpringForceRunner(),
                New CircularRunner(),
                New RadialRunner()
            }
            For Each r In runners
                Dim r2 = r
                Dim mi = New ToolStripMenuItem(r2.Name)
                AddHandler mi.Click, Sub(s, e)
                                         currentRunner = r2
                                         pgLayout.SelectedObject = r2.GetParameters()
                                     End Sub
                dropLayout.DropDownItems.Add(mi)
            Next
            Dim btnRun = New ToolStripButton("运行布局")

            toolStrip.Items.AddRange(New ToolStripItem() {btnNew, btnAdd, btnLink, btnDelEdge, dropLayout, btnRun, btnFit, btnReset})

            ' 左侧分组面板
            groupPanel = New GroupColorPanel() With {.Dock = DockStyle.Fill, .State = state, .TargetCanvas = canvas}
            Dim leftPanel = New Panel() With {.Dock = DockStyle.Left, .Width = 230, .BackColor = Color.FromArgb(&HFF262D3A)}
            leftPanel.Controls.Add(groupPanel)

            ' 右侧属性 + 布局参数 + 小地图
            Dim rightPanel = New Panel() With {.Dock = DockStyle.Right, .Width = 300, .BackColor = Color.FromArgb(&HFF262D3A)}
            Dim tbl = New TableLayoutPanel() With {.Dock = DockStyle.Fill, .ColumnCount = 1, .RowCount = 3}
            tbl.RowStyles.Add(New RowStyle(SizeType.Percent, 50))
            tbl.RowStyles.Add(New RowStyle(SizeType.Percent, 30))
            tbl.RowStyles.Add(New RowStyle(SizeType.Percent, 20))

            pgObject = New PropertyGrid() With {.Dock = DockStyle.Fill, .HelpVisible = False, .ToolbarVisible = False}
            pgLayout = New PropertyGrid() With {.Dock = DockStyle.Fill, .HelpVisible = False, .ToolbarVisible = False}
            minimap = New MinimapControl() With {.Dock = DockStyle.Fill, .Canvas = canvas}

            Dim lblObj = New Label() With {.Text = "属性", .Dock = DockStyle.Top, .Height = 18, .BackColor = Color.FromArgb(&HFF2F3848), .ForeColor = Color.FromArgb(&HFFE6EAF0)}
            Dim lblLay = New Label() With {.Text = "布局参数", .Dock = DockStyle.Top, .Height = 18, .BackColor = Color.FromArgb(&HFF2F3848), .ForeColor = Color.FromArgb(&HFFE6EAF0)}
            Dim lblMap = New Label() With {.Text = "导航地图", .Dock = DockStyle.Top, .Height = 18, .BackColor = Color.FromArgb(&HFF2F3848), .ForeColor = Color.FromArgb(&HFFE6EAF0)}

            Dim p0 = New Panel() With {.Dock = DockStyle.Fill}
            p0.Controls.Add(pgObject)
            p0.Controls.Add(lblObj)

            Dim p1 = New Panel() With {.Dock = DockStyle.Fill}
            p1.Controls.Add(pgLayout)
            p1.Controls.Add(lblLay)

            Dim p2 = New Panel() With {.Dock = DockStyle.Fill}
            p2.Controls.Add(minimap)
            p2.Controls.Add(lblMap)

            tbl.Controls.Add(p0, 0, 0)
            tbl.Controls.Add(p1, 0, 1)
            tbl.Controls.Add(p2, 0, 2)
            rightPanel.Controls.Add(tbl)

            ' 状态栏
            Dim status = New StatusStrip() With {.Dock = DockStyle.Bottom, .BackColor = Color.FromArgb(&HFF262D3A)}
            statusLabel = New ToolStripStatusLabel("就绪") With {.ForeColor = Color.FromArgb(&HFF9AA4B2)}
            status.Items.Add(statusLabel)

            ' 组装
            Me.Controls.Add(canvas)
            Me.Controls.Add(leftPanel)
            Me.Controls.Add(rightPanel)
            Me.Controls.Add(toolStrip)
            Me.Controls.Add(status)

            ' 工具栏事件
            AddHandler btnNew.Click, AddressOf btnNew_Click
            AddHandler btnAdd.Click, AddressOf btnAddNode_Click
            AddHandler btnLink.CheckedChanged, Sub(s, e) canvas.LinkMode = btnLink.Checked
            AddHandler btnDelEdge.Click, AddressOf btnDelEdge_Click
            AddHandler btnFit.Click, Sub(s, e) canvas.FitView()
            AddHandler btnReset.Click, Sub(s, e) canvas.ResetView()
            AddHandler btnRun.Click, AddressOf btnRun_Click

            ' 默认布局
            currentRunner = runners(0)
            pgLayout.SelectedObject = currentRunner.GetParameters()
        End Sub

        Private Sub WireEvents()
            AddHandler state.SelectionChanged, AddressOf OnSelectionChanged
            AddHandler state.GraphChanged, AddressOf OnGraphChanged
            AddHandler canvas.ViewChanged, Sub(s, e) statusLabel.Text = "缩放 " & canvas.ViewScale.ToString("0.00")
            AddHandler pgObject.PropertyValueChanged, Sub(s, e)
                                                          canvas.Invalidate()
                                                          groupPanel.RefreshGroups()
                                                      End Sub
            AddHandler groupPanel.Changed, Sub(s, e)
                                               canvas.Invalidate()
                                               BindPropertyGrid()
                                           End Sub
            Call OnSelectionChanged(Nothing, EventArgs.Empty)
        End Sub

        Private Sub OnSelectionChanged(sender As Object, e As EventArgs)
            BindPropertyGrid()
        End Sub

        Private Sub OnGraphChanged(sender As Object, e As EventArgs)
            canvas.Invalidate()
        End Sub

        Private Sub BindPropertyGrid()
            If state.SelectedEdge IsNot Nothing Then
                pgObject.SelectedObject = New EdgePropertyAdapter(state, state.SelectedEdge)
            ElseIf state.SelectedNodes.Count >= 1 Then
                pgObject.SelectedObject = New NodePropertyAdapter(state, state.SelectedNodes)
            Else
                pgObject.SelectedObject = Nothing
            End If
        End Sub

        Private Sub btnNew_Click(sender As Object, e As EventArgs)
            state.Graph = New NetworkGraph()
            state.ClearSelection()
            canvas.ResetView()
            canvas.Invalidate()
            statusLabel.Text = "已新建空图"
        End Sub

        Private Sub btnAddNode_Click(sender As Object, e As EventArgs)
            Dim center = canvas.GetGraphPointAtClientCenter()
            Dim label = UniqueNodeLabel()
            Dim n = state.Graph.CreateNode(label)
            n.data.initialPostion = New FDGVector2(center.X, center.Y)
            state.SelectNode(n)
            state.RaiseGraphChanged()
            canvas.Invalidate()
            statusLabel.Text = "已添加节点 " & label
        End Sub

        Private Function UniqueNodeLabel() As String
            Dim i = 1
            Do While state.Graph.GetElementByID("Node" & i) IsNot Nothing
                i += 1
            Loop
            Return "Node" & i
        End Function

        Private Sub btnDelEdge_Click(sender As Object, e As EventArgs)
            If state.SelectedEdge IsNot Nothing Then
                state.Graph.RemoveEdge(state.SelectedEdge)
                state.SelectEdge(Nothing)
                state.RaiseGraphChanged()
                canvas.Invalidate()
                statusLabel.Text = "已删除选中边"
            End If
        End Sub

        Private Sub btnRun_Click(sender As Object, e As EventArgs)
            If currentRunner Is Nothing Then
                Return
            End If
            statusLabel.Text = "布局中..."
            Application.DoEvents()
            Dim prog = Sub(msg As String) statusLabel.Text = msg
            currentRunner.Apply(state.Graph, currentRunner.GetParameters(), prog)
            state.RaiseGraphChanged()
            canvas.FitView()
            statusLabel.Text = "布局完成：" & currentRunner.Name
        End Sub
    End Class

End Namespace

