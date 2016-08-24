#Region "Microsoft.VisualBasic::5d3644627b785e2901acdfd98fca7c82, ..\visualbasic_App\UXFramework\Molk+\Molk+\Forms.Controls\TabControls\Tabpage\MultipleTabpagePanel.vb"

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

Namespace Windows.Forms.Controls.TabControl.TabSwitch

    ''' <summary>
    ''' 多标签页
    ''' </summary>
    ''' <remarks></remarks>
    Public Class MultipleTabpagePanel : Inherits Windows.Forms.Controls.TabControl.ITabControl(Of Windows.Forms.Controls.TabControl.TabSwitch.TabPage)

        Public Event RemoveTab(Name As String)
        Public Event RemoveAllTabsButOne(Tabpage As Windows.Forms.Controls.TabControl.TabSwitch.TabPage)
        Public Event RemoveAllTabs()

        Dim InternalUIResource As Visualise.Elements.ButtonResource

        Public Property UIResource As Visualise.Elements.ButtonResource
            Get
                Return InternalUIResource
            End Get
            Set(value As Visualise.Elements.ButtonResource)
                InternalUIResource = value
                For Each TabPage As TabPage In Me._InternalTabList.Values
                    TabPage.UI = value
                Next

                If Not value Is Nothing Then
                    Me.Separable.Location = New Point(0, If(value.Normal Is Nothing, 20, value.Normal.Height))
                    Me.Separable.Size = New Size(Width, Separable.Height)
                End If
            End Set
        End Property

        Public Enum TabpageSizeModes
            UIResourceAutoSize
            EvenSize
            CaptionTextLengthAutoSize
        End Enum

        Public Property SizeMode As TabpageSizeModes
            Get
                Return _SizeMode
            End Get
            Set(value As TabpageSizeModes)
                _SizeMode = value
                Call InternalAutoSizeTabpages()
            End Set
        End Property

        Private Sub InternalAutoSizeTabpages()
            If SizeMode = TabpageSizeModes.UIResourceAutoSize Then
                For Each TAB As TabPage In Me._InternalTabList.Values
                    TAB.AutoSize = True
                Next
            End If

            If SizeMode = TabpageSizeModes.EvenSize Then
                Call InternalEvenSizeAndFilled()
            End If
        End Sub

        Private Sub InternalEvenSizeAndFilled()

            If Me._InternalTabList.Count = 0 Then
                Return
            End If

            Dim wd As Integer = Width / Me._InternalTabList.Count
            Dim i As Integer = 0

            For Each TAB As TabPage In Me._InternalTabList.Values
                TAB.AutoSize = False
                TAB.Size = New Size(wd, TAB.Height)
                TAB.Location = New Point(i * wd, 0)

                i += 1
            Next
        End Sub

        Dim _SizeMode As TabpageSizeModes
        Dim _DisabledCloseControl As Boolean = False

        Public Property DisabledCloseControl As Boolean
            Get
                Return _DisabledCloseControl
            End Get
            Set(value As Boolean)
                _DisabledCloseControl = value

                '标签页将不会被关闭
                For Each ctrl In Me._InternalTabList.Values
                    Call InternalSetClosedProperty(ctrl)
                Next
            End Set
        End Property

        Dim _EnableMenu As Boolean = True

        Public Property EnableMenu As Boolean
            Get
                Return _EnableMenu
            End Get
            Set(value As Boolean)
                _EnableMenu = value

                For Each TAB As TabControl.TabSwitch.TabPage In Me._InternalTabList.Values
                    Call TAB.SetEnableMenu(value)
                Next
            End Set
        End Property

        Private Sub InternalSetClosedProperty(ctrl As TabPage)
            If DisabledCloseControl Then
                ctrl.CanbeClosed = False
                ctrl.EnableCloseButton = False
            Else
                ctrl.CanbeClosed = True
                ctrl.EnableCloseButton = True
            End If
        End Sub

        Public ReadOnly Property PanelPageSize As Size
            Get
                If Not UIResource Is Nothing OrElse UIResource.Normal Is Nothing Then
                    Return New Size With {.Width = Width - 4, .Height = Height - UIResource.Normal.Height - 20}
                Else
                    Return New Size With {.Width = Width, .Height = Height - 40}
                End If
            End Get
        End Property

        Public ReadOnly Property TabPanelControlLocation As Point
            Get
                Return New Point(0, UIResource.Normal.Height + Separable.Height)
            End Get
        End Property

        Public Overrides Function ToString() As String
            If Me._InternalTabList.IsNullOrEmpty Then
                Return MyBase.ToString
            Else
                Return "last_added_tabpage=  " & Me.LastAddedTag.ToString
            End If
        End Function

        Public Overrides Sub AddTabPage(Name As String, Control As Control, Optional TabCloseEventHandle As Action = Nothing)
            Dim NewTabPage As MolkPlusTheme.Windows.Forms.Controls.TabControl.TabSwitch.TabPage = MyBase.InternalAddTabPage(Name, Control, TabCloseEventHandle)
            Dim NewPanel = NewTabPage._InternalTabPageControlItem

            NewTabPage.BackColor = Color.FromArgb(255, 240, 208)
            NewTabPage.UI = UIResource
            Separable.Visible = True

            NewPanel.Size = New Size With {.Width = Width, .Height = Height - NewTabPage.Height - 3}
            NewPanel.Location = New Point With {.X = 0, .Y = NewTabPage.Height + NewTabPage.Location.Y + 3}
            NewPanel.BringToFront()

            Call Me.Controls.Add(NewPanel)
            Call NewTabPage.SetEnableMenu(EnableMenu)

            'Call (Sub() NewTabPage.ActiveTabPage()).BeginInvoke(Nothing, Nothing)

            If SizeMode = TabpageSizeModes.EvenSize Then
                Call InternalEvenSizeAndFilled()
            Else
                If MyBase.PagesCount > 0 Then
                    NewTabPage.Location = New Point With {.Y = 0, .X = (MyBase._Tabs - 1) * NewTabPage.Width + 1}
                Else
                    NewTabPage.Location = New Point With {.Y = 0, .X = 1}
                End If
            End If

            Call InternalSetClosedProperty(NewTabPage)

            AddHandler NewTabPage.TabPageActive, Sub() Call ActiveTabPage(Name)
            AddHandler NewTabPage.CloseTabPage, Sub() Call RemoveTabPage(Name, TabCloseEventHandle)
            AddHandler NewTabPage.CloseAllTabs, Sub() Call RemoveAllTabPagesEx()
            AddHandler NewTabPage.CloseAllTabsButThis, Sub() Call RemoveAllTabPagesButOneEx(NewTabPage)

            Call ActiveTabPage(Name)
        End Sub

        Private Sub InternalLoadHandle_Initialization()

            On Error Resume Next

            Separable.Location = New Point With {.X = 0, .Y = 20}

            MyBase.InternalEventRemoveTab = Sub() Call Me.EventRemovesTab()
            MyBase.EventRemoveTab = Sub(sName As String) RaiseEvent RemoveTab(sName)
            MyBase.InternalEventRemoveAllTabsButOne = AddressOf Me.RemovesAllButLeftOne
            MyBase.EventRemoveAllTabs = Sub() RaiseEvent RemoveAllTabs()
        End Sub

        Private Sub RemovesAllButLeftOne(Tabpage As Windows.Forms.Controls.TabControl.TabSwitch.TabPage)
            Tabpage.Location = New Point With {.X = 1, .Y = 0}
            Me.Separable.Visible = True

            RaiseEvent RemoveAllTabsButOne(Tabpage)
        End Sub

        Sub New()

            ' 此调用是设计器所必需的。
            InitializeComponent()

            ' 在 InitializeComponent() 调用之后添加任何初始化。
            Call InternalLoadHandle_Initialization()
        End Sub

        Private Sub EventRemovesTab()
            If MyBase.PagesCount < 0 Then
                Me.Separable.Visible = False
            End If
        End Sub

        Public Sub MultipleTabpagePanelResizeEventHandler() Handles Me.Resize
            Dim TabPages = MyBase._InternalTabList.Values.ToArray

            Separable.Size = New Size With {.Width = Width, .Height = 2}

            For i As Integer = 0 To TabPages.Length - 1

                Dim TabPage = TabPages(i)

                TabPage._InternalTabPageControlItem.Location = New Point(0, TabPage.Height + Separable.Height)
                TabPage._InternalTabPageControlItem.Size = New Size With {.Width = Width, .Height = Height - TabPage.Height - Separable.Height}
            Next
        End Sub
    End Class
End Namespace
