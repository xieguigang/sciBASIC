#Region "Microsoft.VisualBasic::22e2a331ac17ac6e9b883c0a5ba6c8fc, ..\visualbasic_App\UXFramework\Molk+\Molk+\Forms.Controls\ListControl\ListControl.vb"

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

''' <summary>
''' Copyright © Faraz's Creationz 2012
''' </summary>
''' <remarks></remarks>
Public Class ListControl

    Public Event ItemClick(sender As Object, Index As Integer)

    Default Public ReadOnly Property Item(Index As Integer) As ListControlItem
        Get
            Return DirectCast(flpListBox.Controls.Item(Index), ListControlItem)
        End Get
    End Property

    Public ReadOnly Property Items As ListControlItem()
        Get
            Return (From entryItem As Object
                    In flpListBox.Controls
                    Where TypeOf entryItem Is ListControlItem
                    Select DirectCast(entryItem, ListControlItem)).ToArray
        End Get
    End Property

    Dim _ColorSchema As MetroColorSchemes

    Public Property ColorSchema As MetroColorSchemes
        Get
            Return _ColorSchema
        End Get
        Set(value As MetroColorSchemes)
            _ColorSchema = value
            If Not value Is Nothing Then
                BackColor = ColorSchema.UnSelectedNormal(0)
            End If
        End Set
    End Property

    Protected Overridable Function GetTooltip(EntryItem As ListControlItem) As String
        Return EntryItem.Text
    End Function

    'Public Sub Add(Song As String, Artist As String, Album As String, Duration As String, SongImage As Image, Rating As Integer)
    '    Dim c As New ListControlItem With {.MetroColorSchema = ColorSchema}
    '    With c
    '        ' Assign an auto generated name
    '        .Name = "item" & flpListBox.Controls.Count + 1
    '        .Margin = New Padding(0)
    '        ' set properties
    '        .Song = Song
    '        .Artist = Artist
    '        .Album = Album
    '        .Duration = Duration
    '        .Image = SongImage
    '        .Rating = Rating
    '    End With
    '    ' To check when the selection is changed
    '    AddHandler c.SelectionChanged, AddressOf SelectionChanged
    '    AddHandler c.Click, AddressOf ItemClicked
    '    '
    '    flpListBox.Controls.Add(c)
    '    SetupAnchors()
    'End Sub

    Public Sub Add(Image As Image, Item As ListControlItem)
        Item.MetroColorSchema = ColorSchema
        With Item
            ' Assign an auto generated name
            .Name = "item" & flpListBox.Controls.Count + 1
            .Margin = New Padding(0)
            .Image = Image
            ' set properties
        End With

        ' To check when the selection is changed
        AddHandler Item.SelectionChanged, AddressOf SelectionChanged
        AddHandler Item.Click, AddressOf ItemClicked
        '
        Call flpListBox.Controls.Add(Item)
        Call SetupAnchors()
    End Sub

    Public Sub Remove(Index As Integer)
        Dim c As ListControlItem = flpListBox.Controls(Index)
        Remove(c.Name)  ' call the below sub
    End Sub

    Public Sub Remove(name As String)
        ' grab which control is being removed
        Dim c As ListControlItem = flpListBox.Controls(name)
        flpListBox.Controls.Remove(c)
        ' remove the event hook
        RemoveHandler c.SelectionChanged, AddressOf SelectionChanged
        RemoveHandler c.Click, AddressOf ItemClicked
        ' now dispose off properly
        c.Dispose()
        SetupAnchors()
    End Sub

    ''' <summary>
    ''' 清理掉本列表控件之中的所有入口点对象
    ''' </summary>
    Public Sub Clear()

        For Each ctrl In Items

            If Not TypeOf ctrl Is ListControlItem Then
                Continue For
            End If

            Dim Control = DirectCast(ctrl, ListControlItem)

            Call flpListBox.Controls.Remove(Control)
            ' remove the event hook
            RemoveHandler Control.SelectionChanged, AddressOf SelectionChanged
            RemoveHandler Control.Click, AddressOf ItemClicked
            ' now dispose off properly
            Call Control.Dispose()
        Next

        _LastSelectedEntry = Nothing
    End Sub

    ''' <summary>
    ''' 返回列表之中的Entry的数目
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Count As Integer
        Get
            Return flpListBox.Controls.Count
        End Get
    End Property

    Private Sub SetupAnchors()
        If flpListBox.Controls.Count = 0 Then Return

        For i As Integer = 0 To flpListBox.Controls.Count - 1
            Dim c As Control = flpListBox.Controls(i)
            Call SetupAnchor(i, c)
        Next
    End Sub

    Private Sub SetupAnchor(idx As Integer, InternalControl As Control)
        If idx = 0 Then

            ' Its the first control, all subsequent controls follow 
            ' the anchor behavior of this control.
            InternalControl.Anchor = AnchorStyles.Left + AnchorStyles.Top
            InternalControl.Width = flpListBox.Width - SystemInformation.VerticalScrollBarWidth

        Else

            ' It is not the first control. Set its anchor to
            ' copy the width of the first control in the list.
            InternalControl.Anchor = AnchorStyles.Left + AnchorStyles.Right

        End If
    End Sub

    Private Sub flpListBox_Resize(sender As Object, e As System.EventArgs) Handles flpListBox.Resize
        If flpListBox.Controls.Count Then
            flpListBox.Controls(0).Width = flpListBox.Width - SystemInformation.VerticalScrollBarWidth
        End If
    End Sub

    Dim _LastSelectedEntry As ListControlItem = Nothing
    Private Sub SelectionChanged(sender As Object)
        If _LastSelectedEntry IsNot Nothing Then
            _LastSelectedEntry.Selected = False
        End If
        _LastSelectedEntry = sender
    End Sub

    Public ReadOnly Property CurrentSelectedEntry As ListControlItem
        Get
            Return _LastSelectedEntry
        End Get
    End Property

    Private Sub ItemClicked(sender As Object, e As System.EventArgs)
        RaiseEvent ItemClick(Me, flpListBox.Controls.IndexOfKey(sender.name))
    End Sub

End Class
