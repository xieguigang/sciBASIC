#Region "Microsoft.VisualBasic::d0f2ef09b2c80afc50945da43f581005, ..\visualbasic_App\UXFramework\Molk+\Molk+\Forms.Controls\ListControl\EXListView\EXListView.vb"

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

Imports System.Windows.Forms
Imports System.Drawing
Imports System.Collections
Imports System.Runtime.InteropServices

Namespace Windows.Forms.Controls.ListControl

    Public Class EXListView
        Inherits ListView

        Private _clickedsubitem As ListViewItem.ListViewSubItem
        'clicked ListViewSubItem
        Private _clickeditem As ListViewItem
        'clicked ListViewItem
        Private _col As Integer
        'index of doubleclicked ListViewSubItem
        Private txtbx As TextBox
        'the default edit control
        Private _sortcol As Integer
        'index of clicked ColumnHeader
        Private _sortcolbrush As Brush
        'color of items in sorted column
        Private _highlightbrush As Brush
        'color of highlighted items
        Private _cpadding As Integer
        'padding of the embedded controls
        Private Const LVM_FIRST As UInt32 = &H1000
        Private Const LVM_SCROLL As UInt32 = (LVM_FIRST + 20)
        Private Const WM_HSCROLL As Integer = &H114
        Private Const WM_VSCROLL As Integer = &H115
        Private Const WM_MOUSEWHEEL As Integer = &H20A
        Private Const WM_PAINT As Integer = &HF

        Private Structure EmbeddedControl
            Public MyControl As Control
            Public MySubItem As EXControlListViewSubItem
        End Structure

        Private _controls As ArrayList

        <DllImport("user32.dll")>
        Private Shared Function SendMessage(hWnd As IntPtr, m As UInt32, wParam As Integer, lParam As Integer) As Boolean
        End Function

        Protected Overrides Sub WndProc(ByRef m As Message)
            If m.Msg = WM_PAINT Then
                For Each c As EmbeddedControl In _controls
                    Dim r As Rectangle = c.MySubItem.Bounds
                    If r.Y > 0 AndAlso r.Y < Me.ClientRectangle.Height Then
                        c.MyControl.Visible = True
                        c.MyControl.Bounds = New Rectangle(r.X + _cpadding, r.Y + _cpadding, r.Width - (2 * _cpadding), r.Height - (2 * _cpadding))
                    Else
                        c.MyControl.Visible = False
                    End If
                Next
            End If
            Select Case m.Msg
                Case WM_HSCROLL, WM_VSCROLL, WM_MOUSEWHEEL
                    Me.Focus()
                    Exit Select
            End Select
            MyBase.WndProc(m)
        End Sub

        Private Sub ScrollMe(x As Integer, y As Integer)
            SendMessage(CType(Me.Handle, IntPtr), LVM_SCROLL, x, y)
        End Sub

        Public Sub New()
            _cpadding = 4
            _controls = New ArrayList()
            _sortcol = -1
            _sortcolbrush = SystemBrushes.ControlLight
            _highlightbrush = SystemBrushes.Highlight
            Me.OwnerDraw = True
            Me.FullRowSelect = True
            Me.View = View.Details
            AddHandler Me.MouseDown, New MouseEventHandler(AddressOf this_MouseDown)
            AddHandler Me.MouseDoubleClick, New MouseEventHandler(AddressOf this_MouseDoubleClick)
            AddHandler Me.DrawColumnHeader, New DrawListViewColumnHeaderEventHandler(AddressOf this_DrawColumnHeader)
            AddHandler Me.DrawSubItem, New DrawListViewSubItemEventHandler(AddressOf this_DrawSubItem)
            AddHandler Me.MouseMove, New MouseEventHandler(AddressOf this_MouseMove)
            AddHandler Me.ColumnClick, New ColumnClickEventHandler(AddressOf this_ColumnClick)
            txtbx = New TextBox()
            txtbx.Visible = False
            Me.Controls.Add(txtbx)
            AddHandler txtbx.Leave, New EventHandler(AddressOf c_Leave)
            AddHandler txtbx.KeyPress, New KeyPressEventHandler(AddressOf txtbx_KeyPress)
        End Sub

        Public Sub AddControlToSubItem(control As Control, subitem As EXControlListViewSubItem)
            Me.Controls.Add(control)
            subitem.MyControl = control
            Dim ec As EmbeddedControl
            ec.MyControl = control
            ec.MySubItem = subitem
            Me._controls.Add(ec)
        End Sub

        Public Sub RemoveControlFromSubItem(subitem As EXControlListViewSubItem)
            Dim c As Control = subitem.MyControl
            For i As Integer = 0 To Me._controls.Count - 1
                If CType(Me._controls(i), EmbeddedControl).MySubItem Is subitem Then
                    Me._controls.RemoveAt(i)
                    subitem.MyControl = Nothing
                    Me.Controls.Remove(c)
                    c.Dispose()
                    Return
                End If
            Next
        End Sub

        Public Property ControlPadding() As Integer
            Get
                Return _cpadding
            End Get
            Set
                _cpadding = Value
            End Set
        End Property

        Public Property MySortBrush() As Brush
            Get
                Return _sortcolbrush
            End Get
            Set
                _sortcolbrush = Value
            End Set
        End Property

        Public Property MyHighlightBrush() As Brush
            Get
                Return _highlightbrush
            End Get
            Set
                _highlightbrush = Value
            End Set
        End Property

        Private Sub txtbx_KeyPress(sender As Object, e As KeyPressEventArgs)
            If Asc(e.KeyChar) = System.Windows.Forms.Keys.[Return] Then
                _clickedsubitem.Text = txtbx.Text
                txtbx.Visible = False
                _clickeditem.Tag = Nothing
            End If
        End Sub

        Private Sub c_Leave(sender As Object, e As EventArgs)
            Dim c As Control = DirectCast(sender, Control)
            _clickedsubitem.Text = c.Text
            c.Visible = False
            _clickeditem.Tag = Nothing
        End Sub

        Private Sub this_MouseDown(sender As Object, e As MouseEventArgs)
            Dim lstvinfo As ListViewHitTestInfo = Me.HitTest(e.X, e.Y)
            Dim subitem As ListViewItem.ListViewSubItem = lstvinfo.SubItem
            If subitem Is Nothing Then
                Return
            End If
            Dim subx As Integer = subitem.Bounds.Left
            If subx < 0 Then
                Me.ScrollMe(subx, 0)
            End If
        End Sub

        Private Sub this_MouseDoubleClick(sender As Object, e As MouseEventArgs)
            Dim lstvItem As EXListViewItem = TryCast(Me.GetItemAt(e.X, e.Y), EXListViewItem)
            If lstvItem Is Nothing Then
                Return
            End If
            _clickeditem = lstvItem
            Dim x As Integer = lstvItem.Bounds.Left
            Dim i As Integer
            For i = 0 To Me.Columns.Count - 1
                x = x + Me.Columns(i).Width
                If x > e.X Then
                    x = x - Me.Columns(i).Width
                    _clickedsubitem = lstvItem.SubItems(i)
                    _col = i
                    Exit For
                End If
            Next
            If Not (TypeOf Me.Columns(i) Is EXColumnHeader) Then
                Return
            End If
            Dim col As EXColumnHeader = DirectCast(Me.Columns(i), EXColumnHeader)
            If col.[GetType]() Is GetType(EXEditableColumnHeader) Then
                Dim editcol As EXEditableColumnHeader = DirectCast(col, EXEditableColumnHeader)
                If editcol.MyControl IsNot Nothing Then
                    Dim c As Control = editcol.MyControl
                    If c.Tag IsNot Nothing Then
                        Me.Controls.Add(c)
                        c.Tag = Nothing
                        If TypeOf c Is ComboBox Then
                            AddHandler DirectCast(c, ComboBox).SelectedValueChanged, New EventHandler(AddressOf cmbx_SelectedValueChanged)
                        End If
                        AddHandler c.Leave, New EventHandler(AddressOf c_Leave)
                    End If
                    c.Location = New Point(x, Me.GetItemRect(Me.Items.IndexOf(lstvItem)).Y)
                    c.Width = Me.Columns(i).Width
                    If c.Width > Me.Width Then
                        c.Width = Me.ClientRectangle.Width
                    End If
                    c.Text = _clickedsubitem.Text
                    c.Visible = True
                    c.BringToFront()
                    c.Focus()
                Else
                    txtbx.Location = New Point(x, Me.GetItemRect(Me.Items.IndexOf(lstvItem)).Y)
                    txtbx.Width = Me.Columns(i).Width
                    If txtbx.Width > Me.Width Then
                        txtbx.Width = Me.ClientRectangle.Width
                    End If
                    txtbx.Text = _clickedsubitem.Text
                    txtbx.Visible = True
                    txtbx.BringToFront()
                    txtbx.Focus()
                End If
            ElseIf col.[GetType]() Is GetType(EXBoolColumnHeader) Then
                Dim boolcol As EXBoolColumnHeader = DirectCast(col, EXBoolColumnHeader)
                If boolcol.Editable Then
                    Dim boolsubitem As EXBoolListViewSubItem = DirectCast(_clickedsubitem, EXBoolListViewSubItem)
                    If boolsubitem.BoolValue = True Then
                        boolsubitem.BoolValue = False
                    Else
                        boolsubitem.BoolValue = True
                    End If
                    Me.Invalidate(boolsubitem.Bounds)
                End If
            End If
        End Sub

        Private Sub cmbx_SelectedValueChanged(sender As Object, e As EventArgs)
            If DirectCast(sender, Control).Visible = False OrElse _clickedsubitem Is Nothing Then
                Return
            End If
            If sender.[GetType]() Is GetType(EXComboBox) Then
                Dim excmbx As EXComboBox = DirectCast(sender, EXComboBox)
                Dim item As Object = excmbx.SelectedItem
                'Is this an combobox item with one image?
                If item.[GetType]() Is GetType(EXComboBox.EXImageItem) Then
                    Dim imgitem As EXComboBox.EXImageItem = DirectCast(item, EXComboBox.EXImageItem)
                    'Is the first column clicked -- in that case it's a ListViewItem
                    If _col = 0 Then
                        If _clickeditem.[GetType]() Is GetType(EXImageListViewItem) Then
                            DirectCast(_clickeditem, EXImageListViewItem).MyImage = imgitem.MyImage
                        ElseIf _clickeditem.[GetType]() Is GetType(EXMultipleImagesListViewItem) Then
                            Dim imglstvitem As EXMultipleImagesListViewItem = DirectCast(_clickeditem, EXMultipleImagesListViewItem)
                            imglstvitem.MyImages.Clear()
                            imglstvitem.MyImages.AddRange(New Object() {imgitem.MyImage})
                            'another column than the first one is clicked, so we have a ListViewSubItem
                        End If
                    Else
                        If _clickedsubitem.[GetType]() Is GetType(EXImageListViewSubItem) Then
                            Dim imgsub As EXImageListViewSubItem = DirectCast(_clickedsubitem, EXImageListViewSubItem)
                            imgsub.MyImage = imgitem.MyImage
                        ElseIf _clickedsubitem.[GetType]() Is GetType(EXMultipleImagesListViewSubItem) Then
                            Dim imgsub As EXMultipleImagesListViewSubItem = DirectCast(_clickedsubitem, EXMultipleImagesListViewSubItem)
                            imgsub.MyImages.Clear()
                            imgsub.MyImages.Add(imgitem.MyImage)
                            imgsub.MyValue = imgitem.MyValue
                        End If
                        'or is this a combobox item with multiple images?
                    End If
                ElseIf item.[GetType]() Is GetType(EXComboBox.EXMultipleImagesItem) Then
                    Dim imgitem As EXComboBox.EXMultipleImagesItem = DirectCast(item, EXComboBox.EXMultipleImagesItem)
                    If _col = 0 Then
                        If _clickeditem.[GetType]() Is GetType(EXImageListViewItem) Then
                            DirectCast(_clickeditem, EXImageListViewItem).MyImage = DirectCast(imgitem.MyImages(0), System.Drawing.Image)
                        ElseIf _clickeditem.[GetType]() Is GetType(EXMultipleImagesListViewItem) Then
                            Dim imglstvitem As EXMultipleImagesListViewItem = DirectCast(_clickeditem, EXMultipleImagesListViewItem)
                            imglstvitem.MyImages.Clear()
                            imglstvitem.MyImages.AddRange(imgitem.MyImages)
                        End If
                    Else
                        If _clickedsubitem.[GetType]() Is GetType(EXImageListViewSubItem) Then
                            Dim imgsub As EXImageListViewSubItem = DirectCast(_clickedsubitem, EXImageListViewSubItem)
                            If imgitem.MyImages IsNot Nothing Then
                                imgsub.MyImage = DirectCast(imgitem.MyImages(0), System.Drawing.Image)
                            End If
                        ElseIf _clickedsubitem.[GetType]() Is GetType(EXMultipleImagesListViewSubItem) Then
                            Dim imgsub As EXMultipleImagesListViewSubItem = DirectCast(_clickedsubitem, EXMultipleImagesListViewSubItem)
                            imgsub.MyImages.Clear()
                            imgsub.MyImages.AddRange(imgitem.MyImages)
                            imgsub.MyValue = imgitem.MyValue
                        End If
                    End If
                End If
            End If
            Dim c As ComboBox = DirectCast(sender, ComboBox)
            _clickedsubitem.Text = c.Text
            c.Visible = False
            _clickeditem.Tag = Nothing
        End Sub

        Private Sub this_MouseMove(sender As Object, e As MouseEventArgs)
            Dim item As ListViewItem = Me.GetItemAt(e.X, e.Y)
            If item IsNot Nothing AndAlso item.Tag Is Nothing Then
                Me.Invalidate(item.Bounds)
                item.Tag = "t"
            End If
        End Sub

        Private Sub this_DrawColumnHeader(sender As Object, e As DrawListViewColumnHeaderEventArgs)
            e.DrawDefault = True
        End Sub

        Private Sub this_DrawSubItem(sender As Object, e As DrawListViewSubItemEventArgs)
            e.DrawBackground()
            If e.ColumnIndex = _sortcol Then
                e.Graphics.FillRectangle(_sortcolbrush, e.Bounds)
            End If
            If (e.ItemState And ListViewItemStates.Selected) <> 0 Then
                e.Graphics.FillRectangle(_highlightbrush, e.Bounds)
            End If
            Dim fonty As Integer = e.Bounds.Y + CInt(e.Bounds.Height \ 2) - CInt(e.SubItem.Font.Height \ 2)
            Dim x As Integer = e.Bounds.X + 2
            If e.ColumnIndex = 0 Then
                Dim item As EXListViewItem = DirectCast(e.Item, EXListViewItem)
                If item.[GetType]() Is GetType(EXImageListViewItem) Then
                    Dim imageitem As EXImageListViewItem = DirectCast(item, EXImageListViewItem)
                    If imageitem.MyImage IsNot Nothing Then
                        Dim img As System.Drawing.Image = imageitem.MyImage
                        Dim imgy As Integer = e.Bounds.Y + CInt(e.Bounds.Height \ 2) - CInt(img.Height \ 2)
                        e.Graphics.DrawImage(img, x, imgy, img.Width, img.Height)
                        x += img.Width + 2
                    End If
                End If
                e.Graphics.DrawString(e.SubItem.Text, e.SubItem.Font, New SolidBrush(e.SubItem.ForeColor), x, fonty)
                Return
            End If
            Dim subitem As EXListViewSubItemAB = TryCast(e.SubItem, EXListViewSubItemAB)
            If subitem Is Nothing Then
                e.DrawDefault = True
            Else
                x = subitem.DoDraw(e, x, TryCast(Me.Columns(e.ColumnIndex), EXColumnHeader))
                e.Graphics.DrawString(e.SubItem.Text, e.SubItem.Font, New SolidBrush(e.SubItem.ForeColor), x, fonty)
            End If
        End Sub

        Private Sub this_ColumnClick(sender As Object, e As ColumnClickEventArgs)
            If Me.Items.Count = 0 Then
                Return
            End If
            For i As Integer = 0 To Me.Columns.Count - 1
                Me.Columns(i).ImageKey = Nothing
            Next
            For i As Integer = 0 To Me.Items.Count - 1
                Me.Items(i).Tag = Nothing
            Next
            If e.Column <> _sortcol Then
                _sortcol = e.Column
                Me.Sorting = SortOrder.Ascending
                Me.Columns(e.Column).ImageKey = "up"
            Else
                If Me.Sorting = SortOrder.Ascending Then
                    Me.Sorting = SortOrder.Descending
                    Me.Columns(e.Column).ImageKey = "down"
                Else
                    Me.Sorting = SortOrder.Ascending
                    Me.Columns(e.Column).ImageKey = "up"
                End If
            End If
            If _sortcol = 0 Then
                'ListViewItem
                If Me.Items(0).[GetType]() Is GetType(EXListViewItem) Then
                    'sorting on text
                    Me.ListViewItemSorter = New ListViewItemComparerText(e.Column, Me.Sorting)
                Else
                    'sorting on value
                    Me.ListViewItemSorter = New ListViewItemComparerValue(e.Column, Me.Sorting)
                End If
            Else
                'ListViewSubItem
                If Me.Items(0).SubItems(_sortcol).[GetType]() Is GetType(EXListViewSubItemAB) Then
                    'sorting on text
                    Me.ListViewItemSorter = New ListViewSubItemComparerText(e.Column, Me.Sorting)
                Else
                    'sorting on value
                    Me.ListViewItemSorter = New ListViewSubItemComparerValue(e.Column, Me.Sorting)
                End If
            End If
        End Sub

        Private Class ListViewSubItemComparerText
            Implements System.Collections.IComparer

            Private _col As Integer
            Private _order As SortOrder

            Public Sub New()
                _col = 0
                _order = SortOrder.Ascending
            End Sub

            Public Sub New(col As Integer, order As SortOrder)
                _col = col
                _order = order
            End Sub

            Public Function Compare(x As Object, y As Object) As Integer Implements IComparer.Compare
                Dim returnVal As Integer = -1

                Dim xstr As String = DirectCast(x, ListViewItem).SubItems(_col).Text
                Dim ystr As String = DirectCast(y, ListViewItem).SubItems(_col).Text

                Dim dec_x As Decimal
                Dim dec_y As Decimal
                Dim dat_x As DateTime
                Dim dat_y As DateTime

                If [Decimal].TryParse(xstr, dec_x) AndAlso [Decimal].TryParse(ystr, dec_y) Then
                    returnVal = [Decimal].Compare(dec_x, dec_y)
                ElseIf DateTime.TryParse(xstr, dat_x) AndAlso DateTime.TryParse(ystr, dat_y) Then
                    returnVal = DateTime.Compare(dat_x, dat_y)
                Else
                    returnVal = [String].Compare(xstr, ystr)
                End If
                If _order = SortOrder.Descending Then
                    returnVal *= -1
                End If
                Return returnVal
            End Function

        End Class

        Private Class ListViewSubItemComparerValue
            Implements System.Collections.IComparer

            Private _col As Integer
            Private _order As SortOrder

            Public Sub New()
                _col = 0
                _order = SortOrder.Ascending
            End Sub

            Public Sub New(col As Integer, order As SortOrder)
                _col = col
                _order = order
            End Sub

            Public Function Compare(x As Object, y As Object) As Integer Implements IComparer.Compare
                Dim returnVal As Integer = -1

                Dim xstr As String = DirectCast(DirectCast(x, ListViewItem).SubItems(_col), EXListViewSubItemAB).MyValue
                Dim ystr As String = DirectCast(DirectCast(y, ListViewItem).SubItems(_col), EXListViewSubItemAB).MyValue

                Dim dec_x As Decimal
                Dim dec_y As Decimal
                Dim dat_x As DateTime
                Dim dat_y As DateTime

                If [Decimal].TryParse(xstr, dec_x) AndAlso [Decimal].TryParse(ystr, dec_y) Then
                    returnVal = [Decimal].Compare(dec_x, dec_y)
                ElseIf DateTime.TryParse(xstr, dat_x) AndAlso DateTime.TryParse(ystr, dat_y) Then
                    returnVal = DateTime.Compare(dat_x, dat_y)
                Else
                    returnVal = [String].Compare(xstr, ystr)
                End If
                If _order = SortOrder.Descending Then
                    returnVal *= -1
                End If
                Return returnVal
            End Function

        End Class

        Private Class ListViewItemComparerText
            Implements System.Collections.IComparer

            Private _col As Integer
            Private _order As SortOrder

            Public Sub New()
                _col = 0
                _order = SortOrder.Ascending
            End Sub

            Public Sub New(col As Integer, order As SortOrder)
                _col = col
                _order = order
            End Sub

            Public Function Compare(x As Object, y As Object) As Integer Implements IComparer.Compare
                Dim returnVal As Integer = -1

                Dim xstr As String = DirectCast(x, ListViewItem).Text
                Dim ystr As String = DirectCast(y, ListViewItem).Text

                Dim dec_x As Decimal
                Dim dec_y As Decimal
                Dim dat_x As DateTime
                Dim dat_y As DateTime

                If [Decimal].TryParse(xstr, dec_x) AndAlso [Decimal].TryParse(ystr, dec_y) Then
                    returnVal = [Decimal].Compare(dec_x, dec_y)
                ElseIf DateTime.TryParse(xstr, dat_x) AndAlso DateTime.TryParse(ystr, dat_y) Then
                    returnVal = DateTime.Compare(dat_x, dat_y)
                Else
                    returnVal = [String].Compare(xstr, ystr)
                End If
                If _order = SortOrder.Descending Then
                    returnVal *= -1
                End If
                Return returnVal
            End Function

        End Class

        Private Class ListViewItemComparerValue
            Implements System.Collections.IComparer

            Private _col As Integer
            Private _order As SortOrder

            Public Sub New()
                _col = 0
                _order = SortOrder.Ascending
            End Sub

            Public Sub New(col As Integer, order As SortOrder)
                _col = col
                _order = order
            End Sub

            Public Function Compare(x As Object, y As Object) As Integer Implements IComparer.Compare
                Dim returnVal As Integer = -1

                Dim xstr As String = DirectCast(x, EXListViewItem).MyValue
                Dim ystr As String = DirectCast(y, EXListViewItem).MyValue

                Dim dec_x As Decimal
                Dim dec_y As Decimal
                Dim dat_x As DateTime
                Dim dat_y As DateTime

                If [Decimal].TryParse(xstr, dec_x) AndAlso [Decimal].TryParse(ystr, dec_y) Then
                    returnVal = [Decimal].Compare(dec_x, dec_y)
                ElseIf DateTime.TryParse(xstr, dat_x) AndAlso DateTime.TryParse(ystr, dat_y) Then
                    returnVal = DateTime.Compare(dat_x, dat_y)
                Else
                    returnVal = [String].Compare(xstr, ystr)
                End If
                If _order = SortOrder.Descending Then
                    returnVal *= -1
                End If
                Return returnVal
            End Function

        End Class

    End Class

    Public Class EXColumnHeader
        Inherits ColumnHeader


        Public Sub New()
        End Sub

        Public Sub New(text As String)
            Me.Text = text
        End Sub

        Public Sub New(text As String, width As Integer)
            Me.Text = text
            Me.Width = width
        End Sub

    End Class

    Public Class EXEditableColumnHeader
        Inherits EXColumnHeader

        Private _control As Control


        Public Sub New()
        End Sub

        Public Sub New(text As String)
            Me.Text = text
        End Sub

        Public Sub New(text As String, width As Integer)
            Me.Text = text
            Me.Width = width
        End Sub

        Public Sub New(text As String, control As Control)
            Me.Text = text
            Me.MyControl = control
        End Sub

        Public Sub New(text As String, control As Control, width As Integer)
            Me.Text = text
            Me.MyControl = control
            Me.Width = width
        End Sub

        Public Property MyControl() As Control
            Get
                Return _control
            End Get
            Set
                _control = Value
                _control.Visible = False
                _control.Tag = "not_init"
            End Set
        End Property

    End Class

    Public Class EXBoolColumnHeader
        Inherits EXColumnHeader

        Private _trueimage As System.Drawing.Image
        Private _falseimage As System.Drawing.Image
        Private _editable As Boolean

        Public Sub New()
            init()
        End Sub

        Public Sub New(text As String)
            init()
            Me.Text = text
        End Sub

        Public Sub New(text As String, width As Integer)
            init()
            Me.Text = text
            Me.Width = width
        End Sub

        Public Sub New(text As String, trueimage As System.Drawing.Image, falseimage As System.Drawing.Image)
            init()
            Me.Text = text
            _trueimage = trueimage
            _falseimage = falseimage
        End Sub

        Public Sub New(text As String, trueimage As System.Drawing.Image, falseimage As System.Drawing.Image, width As Integer)
            init()
            Me.Text = text
            _trueimage = trueimage
            _falseimage = falseimage
            Me.Width = width
        End Sub

        Private Sub init()
            _editable = False
        End Sub

        Public Property TrueImage() As System.Drawing.Image
            Get
                Return _trueimage
            End Get
            Set
                _trueimage = Value
            End Set
        End Property

        Public Property FalseImage() As System.Drawing.Image
            Get
                Return _falseimage
            End Get
            Set
                _falseimage = Value
            End Set
        End Property

        Public Property Editable() As Boolean
            Get
                Return _editable
            End Get
            Set
                _editable = Value
            End Set
        End Property

    End Class

    Public MustInherit Class EXListViewSubItemAB
        Inherits ListViewItem.ListViewSubItem

        Private _value As String = ""


        Public Sub New()
        End Sub

        Public Sub New(text As String)
            Me.Text = text
        End Sub

        Public Property MyValue() As String
            Get
                Return _value
            End Get
            Set
                _value = Value
            End Set
        End Property

        'return the new x coordinate
        Public MustOverride Function DoDraw(e As DrawListViewSubItemEventArgs, x As Integer, ch As EXColumnHeader) As Integer

    End Class

    Public Class EXListViewSubItem
        Inherits EXListViewSubItemAB


        Public Sub New()
        End Sub

        Public Sub New(text As String)
            Me.Text = text
        End Sub

        Public Overrides Function DoDraw(e As DrawListViewSubItemEventArgs, x As Integer, ch As EXColumnHeader) As Integer
            Return x
        End Function

    End Class

    Public Class EXControlListViewSubItem
        Inherits EXListViewSubItemAB

        Private _control As Control


        Public Sub New()
        End Sub

        Public Property MyControl() As Control
            Get
                Return _control
            End Get
            Set
                _control = Value
            End Set
        End Property

        Public Overrides Function DoDraw(e As DrawListViewSubItemEventArgs, x As Integer, ch As EXColumnHeader) As Integer
            Return x
        End Function

    End Class

    Public Class EXImageListViewSubItem
        Inherits EXListViewSubItemAB

        Private _image As System.Drawing.Image


        Public Sub New()
        End Sub

        Public Sub New(text As String)
            Me.Text = text
        End Sub

        Public Sub New(image As System.Drawing.Image)
            _image = image
        End Sub

        Public Sub New(image As System.Drawing.Image, value As String)
            _image = image
            Me.MyValue = value
        End Sub

        Public Sub New(text As String, image As System.Drawing.Image, value As String)
            Me.Text = text
            _image = image
            Me.MyValue = value
        End Sub

        Public Property MyImage() As System.Drawing.Image
            Get
                Return _image
            End Get
            Set
                _image = Value
            End Set
        End Property

        Public Overrides Function DoDraw(e As DrawListViewSubItemEventArgs, x As Integer, ch As EXColumnHeader) As Integer
            If Me.MyImage IsNot Nothing Then
                Dim img As System.Drawing.Image = Me.MyImage
                Dim imgy As Integer = e.Bounds.Y + CInt(e.Bounds.Height \ 2) - CInt(img.Height \ 2)
                e.Graphics.DrawImage(img, x, imgy, img.Width, img.Height)
                x += img.Width + 2
            End If
            Return x
        End Function

    End Class

    Public Class EXMultipleImagesListViewSubItem
        Inherits EXListViewSubItemAB

        Private _images As ArrayList


        Public Sub New()
        End Sub

        Public Sub New(text As String)
            Me.Text = text
        End Sub

        Public Sub New(images As ArrayList)
            _images = images
        End Sub

        Public Sub New(images As ArrayList, value As String)
            _images = images
            Me.MyValue = value
        End Sub

        Public Sub New(text As String, images As ArrayList, value As String)
            Me.Text = text
            _images = images
            Me.MyValue = value
        End Sub

        Public Property MyImages() As ArrayList
            Get
                Return _images
            End Get
            Set
                _images = Value
            End Set
        End Property

        Public Overrides Function DoDraw(e As DrawListViewSubItemEventArgs, x As Integer, ch As EXColumnHeader) As Integer
            If Me.MyImages IsNot Nothing AndAlso Me.MyImages.Count > 0 Then
                For i As Integer = 0 To Me.MyImages.Count - 1
                    Dim img As System.Drawing.Image = DirectCast(Me.MyImages(i), System.Drawing.Image)
                    Dim imgy As Integer = e.Bounds.Y + CInt(e.Bounds.Height \ 2) - CInt(img.Height \ 2)
                    e.Graphics.DrawImage(img, x, imgy, img.Width, img.Height)
                    x += img.Width + 2
                Next
            End If
            Return x
        End Function

    End Class

    Public Class EXBoolListViewSubItem
        Inherits EXListViewSubItemAB

        Private _value As Boolean


        Public Sub New()
        End Sub

        Public Sub New(val As Boolean)
            _value = val
            Me.MyValue = val.ToString()
        End Sub

        Public Property BoolValue() As Boolean
            Get
                Return _value
            End Get
            Set
                _value = Value
                Me.MyValue = Value.ToString()
            End Set
        End Property

        Public Overrides Function DoDraw(e As DrawListViewSubItemEventArgs, x As Integer, ch As EXColumnHeader) As Integer
            Dim boolcol As EXBoolColumnHeader = DirectCast(ch, EXBoolColumnHeader)
            Dim boolimg As System.Drawing.Image
            If Me.BoolValue = True Then
                boolimg = boolcol.TrueImage
            Else
                boolimg = boolcol.FalseImage
            End If
            Dim imgy As Integer = e.Bounds.Y + CInt(e.Bounds.Height \ 2) - CInt(boolimg.Height \ 2)
            e.Graphics.DrawImage(boolimg, x, imgy, boolimg.Width, boolimg.Height)
            x += boolimg.Width + 2
            Return x
        End Function

    End Class

    Public Class EXListViewItem
        Inherits ListViewItem

        Private _value As String


        Public Sub New()
        End Sub

        Public Sub New(text As String)
            Me.Text = text
        End Sub

        Public Property MyValue() As String
            Get
                Return _value
            End Get
            Set
                _value = Value
            End Set
        End Property

    End Class

    Public Class EXImageListViewItem
        Inherits EXListViewItem

        Private _image As System.Drawing.Image


        Public Sub New()
        End Sub

        Public Sub New(text As String)
            Me.Text = text
        End Sub

        Public Sub New(image As System.Drawing.Image)
            _image = image
        End Sub

        Public Sub New(text As String, image As System.Drawing.Image)
            _image = image
            Me.Text = text
        End Sub

        Public Sub New(text As String, image As System.Drawing.Image, value As String)
            Me.Text = text
            _image = image
            Me.MyValue = value
        End Sub

        Public Property MyImage() As System.Drawing.Image
            Get
                Return _image
            End Get
            Set
                _image = Value
            End Set
        End Property

    End Class

    Public Class EXMultipleImagesListViewItem
        Inherits EXListViewItem

        Private _images As ArrayList


        Public Sub New()
        End Sub

        Public Sub New(text As String)
            Me.Text = text
        End Sub

        Public Sub New(images As ArrayList)
            _images = images
        End Sub

        Public Sub New(text As String, images As ArrayList)
            Me.Text = text
            _images = images
        End Sub

        Public Sub New(text As String, images As ArrayList, value As String)
            Me.Text = text
            _images = images
            Me.MyValue = value
        End Sub

        Public Property MyImages() As ArrayList
            Get
                Return _images
            End Get
            Set
                _images = Value
            End Set
        End Property

    End Class

End Namespace
