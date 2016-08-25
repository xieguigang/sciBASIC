#Region "Microsoft.VisualBasic::2d804593f3aa023176bc2b10128f267b, ..\visualbasic_App\UXFramework\Molk+\Molk+\Forms.Controls\ListControl\EXListView\MyForms.vb"

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

'Imports System.Windows.Forms
'Imports System.Collections
'Imports EXControls
'Imports System.Drawing
'Imports System.Threading

'Class MyForm
'	Inherits Form

'	Private lstv As EXListView
'	Private btn As Button
'	Private btn2 As Button
'	Private statusstrip1 As StatusStrip
'	Private toolstripstatuslabel1 As ToolStripStatusLabel
'	Private Delegate Sub del_do_update(pb As ProgressBar)
'	Private Delegate Sub del_do_changetxt(l As LinkLabel, text As String)

'	Public Sub New()
'		statusstrip1 = New StatusStrip()
'		toolstripstatuslabel1 = New ToolStripStatusLabel()
'		btn = New Button()
'		btn2 = New Button()
'		InitializeComponent()
'	End Sub

'	Private Sub InitializeComponent()
'		'imglst_genre
'		Dim imglst_genre As New ImageList()
'		imglst_genre.ColorDepth = ColorDepth.Depth32Bit
'		imglst_genre.Images.Add(Image.FromFile("music.png"))
'		imglst_genre.Images.Add(Image.FromFile("love.png"))
'		imglst_genre.Images.Add(Image.FromFile("comedy.png"))
'		imglst_genre.Images.Add(Image.FromFile("drama.png"))
'		imglst_genre.Images.Add(Image.FromFile("horror.ico"))
'		imglst_genre.Images.Add(Image.FromFile("family.ico"))
'		'excmbx_genre
'		Dim excmbx_genre As New EXComboBox()
'		excmbx_genre.DropDownStyle = ComboBoxStyle.DropDownList
'		excmbx_genre.MyHighlightBrush = Brushes.Goldenrod
'		excmbx_genre.ItemHeight = 20
'		excmbx_genre.Items.Add(New EXComboBox.EXImageItem(imglst_genre.Images(0), "Music"))
'		excmbx_genre.Items.Add(New EXComboBox.EXImageItem(imglst_genre.Images(1), "Romantic"))
'		excmbx_genre.Items.Add(New EXComboBox.EXImageItem(imglst_genre.Images(2), "Comedy"))
'		excmbx_genre.Items.Add(New EXComboBox.EXImageItem(imglst_genre.Images(3), "Drama"))
'		excmbx_genre.Items.Add(New EXComboBox.EXImageItem(imglst_genre.Images(4), "Horror"))
'		excmbx_genre.Items.Add(New EXComboBox.EXImageItem(imglst_genre.Images(5), "Family"))
'		excmbx_genre.Items.Add(New EXComboBox.EXMultipleImagesItem(New ArrayList(New Object() {Image.FromFile("love.png"), Image.FromFile("comedy.png")}), "Romantic comedy"))
'		'excmbx_rate
'		Dim excmbx_rate As New EXComboBox()
'		excmbx_rate.MyHighlightBrush = Brushes.Goldenrod
'		excmbx_rate.DropDownStyle = ComboBoxStyle.DropDownList
'		Dim imglst_rate As New ImageList()
'		imglst_rate.ColorDepth = ColorDepth.Depth32Bit
'		imglst_rate.Images.Add(Image.FromFile("rate.png"))
'		For i As Integer = 1 To 5
'			Dim _arlst1 As New ArrayList()
'			For j As Integer = 0 To i - 1
'				_arlst1.Add(imglst_rate.Images(0))
'			Next
'			excmbx_rate.Items.Add(New EXComboBox.EXMultipleImagesItem("", _arlst1, i.ToString()))
'		Next
'		'lstv
'		lstv = New EXListView()
'		lstv.MySortBrush = SystemBrushes.ControlLight
'		lstv.MyHighlightBrush = Brushes.Goldenrod
'		lstv.GridLines = True
'		lstv.Location = New Point(10, 40)
'		lstv.Size = New Size(500, 400)
'		lstv.ControlPadding = 4
'		AddHandler lstv.MouseMove, New MouseEventHandler(AddressOf lstv_MouseMove)
'		lstv.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) Or System.Windows.Forms.AnchorStyles.Left) Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
'		'add SmallImageList to ListView - images will be shown in ColumnHeaders
'		Dim colimglst As New ImageList()
'		colimglst.Images.Add("down", Image.FromFile("down.png"))
'		colimglst.Images.Add("up", Image.FromFile("up.png"))
'		colimglst.ColorDepth = ColorDepth.Depth32Bit
'		colimglst.ImageSize = New Size(20, 20)
'		' this will affect the row height
'		lstv.SmallImageList = colimglst
'		'add columns and items
'		lstv.Columns.Add(New EXEditableColumnHeader("Movie", 20))
'		lstv.Columns.Add(New EXColumnHeader("Progress", 120))
'		lstv.Columns.Add(New EXEditableColumnHeader("Genre", excmbx_genre, 60))
'		lstv.Columns.Add(New EXEditableColumnHeader("Rate", excmbx_rate, 100))
'		lstv.Columns.Add(New EXColumnHeader("Status", 80))
'		lstv.Columns.Add(New EXColumnHeader("eeeee", 30))


'		Dim boolcol As New EXBoolColumnHeader("Conclusion", 80)
'		boolcol.Editable = True
'		boolcol.TrueImage = Image.FromFile("true.png")
'		boolcol.FalseImage = Image.FromFile("false.png")
'		lstv.Columns.Add(boolcol)
'		lstv.BeginUpdate()
'		For i As Integer = 0 To 99
'			'movie
'			Dim item As New EXListViewItem(i.ToString())
'			Dim cs As New EXControlListViewSubItem()
'			Dim b As New ProgressBar()
'			b.Tag = item
'			b.Minimum = 0
'			b.Maximum = 1000
'			b.[Step] = 1
'			item.SubItems.Add(cs)
'			lstv.AddControlToSubItem(b, cs)
'			'genre
'			item.SubItems.Add(New EXMultipleImagesListViewSubItem(New ArrayList(New Object() {imglst_genre.Images(1), imglst_genre.Images(2)}), "Romantic comedy"))
'			'rate
'			item.SubItems.Add(New EXMultipleImagesListViewSubItem(New ArrayList(New Object() {imglst_rate.Images(0)}), "1"))
'			'cancel and resume
'			Dim cs1 As New EXControlListViewSubItem()
'			Dim llbl As New LinkLabel()
'			llbl.Text = "Start"
'			llbl.Tag = cs
'			AddHandler llbl.LinkClicked, New LinkLabelLinkClickedEventHandler(AddressOf llbl_LinkClicked)
'			item.SubItems.Add(cs1)
'			lstv.AddControlToSubItem(llbl, cs1)

'			Dim cs12 As New EXControlListViewSubItem()
'			item.SubItems.Add(cs12)
'			lstv.AddControlToSubItem(New TextBox(), cs12)
'			'conclusion
'			item.SubItems.Add(New EXBoolListViewSubItem(True))
'			lstv.Items.Add(item)
'		Next
'		lstv.EndUpdate()
'		'statusstrip1
'		statusstrip1.Items.AddRange(New ToolStripItem() {toolstripstatuslabel1})
'		'btn
'		btn.Location = New Point(10, 450)
'		btn.Text = "Remove Control"
'		btn.AutoSize = True
'		AddHandler btn.Click, New EventHandler(AddressOf btn_Click)
'		'btn2
'		btn2.Location = New Point(btn.Right + 20, 450)
'		btn2.Text = "Remove Image"
'		btn2.AutoSize = True
'		AddHandler btn2.Click, New EventHandler(AddressOf btn2_Click)
'		'this
'		Me.ClientSize = New Size(520, 510)
'		Me.Controls.Add(statusstrip1)
'		Dim lbl As New Label()
'		lbl.Text = "Doubleclick on the subitems to edit..."
'		lbl.Bounds = New Rectangle(10, 10, 480, 20)
'		Me.Controls.Add(lbl)
'		Me.Controls.Add(lstv)
'		Me.Controls.Add(btn)
'		Me.Controls.Add(btn2)
'	End Sub

'	Private Sub llbl_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs)
'		Dim l As LinkLabel = DirectCast(sender, LinkLabel)
'		If l.Text = "Downloading" Then
'			Return
'		End If
'		Dim subitem As EXControlListViewSubItem = TryCast(l.Tag, EXControlListViewSubItem)
'		Dim p As ProgressBar = TryCast(subitem.MyControl, ProgressBar)
'		Dim th As New Thread(New ParameterizedThreadStart(AddressOf UpdateProgressBarMethod))
'		th.IsBackground = True
'		th.Start(p)
'		DirectCast(sender, LinkLabel).Text = "Downloading"
'	End Sub

'	Private Sub lstv_MouseMove(sender As Object, e As MouseEventArgs)
'		Dim lstvinfo As ListViewHitTestInfo = lstv.HitTest(e.X, e.Y)
'		Dim subitem As ListViewItem.ListViewSubItem = lstvinfo.SubItem
'		If subitem Is Nothing Then
'			Return
'		End If
'		If TypeOf subitem Is EXListViewSubItemAB Then
'			toolstripstatuslabel1.Text = DirectCast(subitem, EXListViewSubItemAB).MyValue
'		End If
'	End Sub

'	Private Sub ChangeTextMethod(l As LinkLabel, text As String)
'		l.Text = text
'	End Sub

'	Private Sub UpdateProgressBarMethod(pb As Object)
'		Dim pp As ProgressBar = DirectCast(pb, ProgressBar)
'		If pp.Value = pp.Maximum Then
'			pp.Value = 0
'		End If
'		Dim delupdate As New del_do_update(AddressOf do_update)
'		For i As Integer = pp.Value To pp.Maximum - 1
'			pp.BeginInvoke(delupdate, New Object() {pp})
'			Thread.Sleep(10)
'		Next
'		Dim item As ListViewItem = DirectCast(pp.Tag, ListViewItem)
'		Dim l As LinkLabel = DirectCast(DirectCast(item.SubItems(4), EXControlListViewSubItem).MyControl, LinkLabel)
'		Dim delchangetxt As New del_do_changetxt(AddressOf ChangeTextMethod)
'		l.BeginInvoke(delchangetxt, New Object() {l, "OK"})
'	End Sub

'	Private Sub do_update(p As ProgressBar)
'		p.PerformStep()
'	End Sub

'	Private Sub btn_Click(sender As Object, e As EventArgs)
'		lstv.RemoveControlFromSubItem(DirectCast(lstv.Items(1).SubItems(4), EXControlListViewSubItem))
'	End Sub

'	Private Sub btn2_Click(sender As Object, e As EventArgs)
'		DirectCast(lstv.Items(1).SubItems(2), EXMultipleImagesListViewSubItem).MyImages.Clear()
'		lstv.Invalidate(lstv.Items(1).SubItems(2).Bounds)
'	End Sub

'	Public Shared Sub Main()
'		Application.EnableVisualStyles()
'		Application.Run(New MyForm())
'	End Sub

'End Class
