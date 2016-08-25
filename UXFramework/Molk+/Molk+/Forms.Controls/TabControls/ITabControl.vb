#Region "Microsoft.VisualBasic::05588b431d0f119cd1a544434a322eff, ..\visualbasic_App\UXFramework\Molk+\Molk+\Forms.Controls\TabControls\ITabControl.vb"

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

Namespace Windows.Forms.Controls.TabControl

    ''' <summary>
    ''' 多标签页的基本框架
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <remarks></remarks>
    Public Class ITabControl(Of T As MolkPlusTheme.Windows.Forms.Controls.TabControl.ITabPage) : Inherits System.Windows.Forms.UserControl

        Protected Friend _InternalTabList As Dictionary(Of String, T) = New Dictionary(Of String, T)
        Protected Friend _CurrentTab As T
        Protected Friend _Tabs As Integer, TabIds As Integer = Integer.MinValue

        Protected Friend EventRemoveTab As System.Action(Of String) = Sub(Name As String) Call Console.WriteLine("Tabpage {0} was closed, and you should implement this event delegate in your delivered type!", Name)
        Protected Friend EventRemoveAllTabs As System.Action = Sub() Call Console.WriteLine("Please implements this delegate event in the delivered type!")
        Protected InternalEventRemoveAllTabsButOne As System.Action(Of T) = Sub(TabPage As T) Call Console.WriteLine("Tabpage {0}", TabPage.Text)
        Protected InternalEventRemoveTab As System.Action = Sub() Call Console.WriteLine("Please implements this delegate event in the delivered type!")

        Public ReadOnly Property LastAddedTag As T
            Get
                If _InternalTabList.IsNullOrEmpty Then
                    Return Nothing
                Else
                    Return _InternalTabList.Last.Value
                End If
            End Get
        End Property

        ''' <summary>
        ''' 返回容器内的标签数目
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property PagesCount As Integer
            Get
                Return _Tabs - 1
            End Get
        End Property

        Default Public ReadOnly Property TabPage(Name As String) As Control
            Get
                Return _InternalTabList(Name)._InternalTabPageControlItem
            End Get
        End Property

        Public ReadOnly Property CurrentTab As Control
            Get
                Return Me._CurrentTab._InternalTabPageControlItem
            End Get
        End Property

        Public Overridable Sub AddTabPage(Name As String, Control As Control, Optional TabCloseEventHandle As System.Action = Nothing)
            Call Me.InternalAddTabPage(Name, Control, TabCloseEventHandle)
        End Sub

        ''' <summary>
        ''' 添加新标签页的逻辑，在本过程之中并不包含有UI布局等细节的控制
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <param name="TabCloseEventHandle"></param>
        ''' <remarks></remarks>
        Protected Function InternalAddTabPage(Name As String, Control As Control, TabCloseEventHandle As System.Action) As T
            Dim NewTabPage As T = Activator.CreateInstance(Of T)()

            TabIds += 1 : _Tabs += 1

            NewTabPage.Text = Name
            NewTabPage._Id = TabIds
            NewTabPage._InternalTabPageControlItem = Control
            NewTabPage._isFirstTabPage = _Tabs = 1

            Call Me.Controls.Add(NewTabPage)
            'Call Me.Controls.Add(NewPanel)  '添加新的标签页控件
            Call Me._InternalTabList.Add(Name, NewTabPage)

            Return NewTabPage
        End Function

        Public Sub ActiveTabPage(Name As String)
            Me._CurrentTab = _InternalTabList(Name)
            Call Me._CurrentTab.ActiveTabPage()
            If Not Me._CurrentTab._InternalTabPageControlItem Is Nothing Then
                Call Me._CurrentTab._InternalTabPageControlItem.BringToFront()
            End If
        End Sub

        Public Function ContainsTabPage(Name As String) As Boolean
            Return _InternalTabList.ContainsKey(Name)
        End Function

        Protected Friend Function InternalRemoveTabPage(TabPage As T) As Integer
            Call Me.Controls.Remove(TabPage._InternalTabPageControlItem)
            Call Me.Controls.Remove(TabPage)
            Call _InternalTabList.Remove(TabPage.Text)

            _Tabs -= 1

            Call InternalEventRemoveTab()

            Return 0
        End Function

        ''' <summary>
        ''' 用于处理用户鼠标点击事件的过程
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <param name="TabCloseEventHandler"></param>
        ''' <remarks></remarks>
        Public Sub RemoveTabPage(Name As String, TabCloseEventHandler As System.Action)
            Dim Tabpage = Me._InternalTabList(Name)
            For Each Tab As T In (From _Tab As T In _InternalTabList.Values Where _Tab._Id > Tabpage._Id Select _Tab).ToArray
                Tab.Location = New Point With {.X = Tab.Location.X - Tab.Width, .Y = Tab.Location.Y}
            Next

            Call InternalRemoveTabPage(Tabpage)

            If _Tabs > 0 Then
                Dim FirstTab As T = _InternalTabList.Values.First

                FirstTab._isFirstTabPage = True
                Call ActiveTabPage(FirstTab.Text)
            End If

            If Not TabCloseEventHandler Is Nothing Then Call TabCloseEventHandler()
            Call EventRemoveTab(Tabpage.Text)
        End Sub

        Public Sub RemoveTabs(TabPageCollection As T())
            Dim LQuery = From Tab In TabPageCollection Select InternalRemoveTabPage(Tab) '

            Me._Tabs -= (LQuery.ToArray.Length - 1)
        End Sub

        Public Overridable Sub RemoveAllTabPagesEx()
            Call RemoveTabs(_InternalTabList.Values.ToArray)
            _Tabs = 0

            Call InternalEventRemoveTab()
            Call EventRemoveAllTabs()
        End Sub

        Public Sub RemoveAllTabPagesButOneEx(NewTabPage As T)
            Dim LQuery = From TabPage As T In _InternalTabList.Values Where TabPage._Id <> NewTabPage._Id Select TabPage '
            Call RemoveTabs(LQuery.ToArray)
            NewTabPage._isFirstTabPage = True
            _Tabs = 1

            Call InternalEventRemoveTab()
            Call InternalEventRemoveAllTabsButOne(NewTabPage)
        End Sub

        Public Sub New()
            CheckForIllegalCrossThreadCalls = False
        End Sub
    End Class

    ''' <summary>
    ''' 标签页的基本框架
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ITabPage : Inherits System.Windows.Forms.Control

        ' ''' <summary>
        ' ''' Only contains one control in this panel control.
        ' ''' </summary>
        ' ''' <remarks></remarks>
        'Public Class Panel : Inherits System.Windows.Forms.Panel

        '    Sub Add(Control As Control)
        '        Controls.Add(Control)
        '        Controls(0).Size = Me.Size
        '    End Sub

        '    Sub New()
        '        BackColor = Color.White
        '        AutoScroll = True
        '    End Sub

        '    Private Sub Panel_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        '        If Controls.Count > 0 Then Controls(0).Size = Me.Size
        '    End Sub

        '    Public Overloads Sub Dispose()
        '        If Controls.Count > 0 Then Call Controls(0).Dispose()
        '    End Sub
        'End Class

        ''' <summary>
        ''' 用于标识当前的标签页对象的位置的属性值
        ''' </summary>
        ''' <remarks></remarks>
        Friend _Id As Integer
        Friend _InternalTabPageControlItem As Control
        Friend _isFirstTabPage As Boolean
        Friend _Active As Boolean

        Public ReadOnly Property SourceControl As Control
            Get
                Return _InternalTabPageControlItem
            End Get
        End Property

        Public Interface ITabPageEx
            ''' <summary>
            ''' 当前的标签页失活
            ''' </summary>
            ''' <remarks></remarks>
            Event TabPageInactive()
            ''' <summary>
            ''' 当前的标签页被激活
            ''' </summary>
            ''' <remarks></remarks>
            Event TabPageActive()
            ''' <summary>
            ''' 关闭所有的标签页（用户点击了右键菜单上面的关闭所有标签页的按钮）
            ''' </summary>
            ''' <remarks></remarks>
            Event CloseAllTabs()
            ''' <summary>
            ''' 关闭所有的标签页但是当前标签页除外（用户点击了右键菜单上面的关闭所有标签页但是当前的标签页除外的按钮）
            ''' </summary>
            ''' <remarks></remarks>
            Event CloseAllTabsButThis()
            ''' <summary>
            ''' 关闭当前标签页（用户点击了右键菜单上面的关闭当前标签页的按钮）
            ''' </summary>
            ''' <remarks></remarks>
            Event CloseTabPage()
        End Interface

        ''' <summary>
        ''' 当前的标签页失活
        ''' </summary>
        ''' <remarks></remarks>
        Protected Friend EventTabPageInactive As System.Action = Sub() Call Console.WriteLine("Please implements this delegate event in the delivered type!")
        ''' <summary>
        ''' 当前的标签页被激活
        ''' </summary>
        ''' <remarks></remarks>
        Protected Friend EventTabPageActive As System.Action = Sub() Call Console.WriteLine("Please implements this delegate event in the delivered type!")
        ''' <summary>
        ''' 关闭所有的标签页（用户点击了右键菜单上面的关闭所有标签页的按钮）
        ''' </summary>
        ''' <remarks></remarks>
        Protected Friend EventCloseAllTabs As System.Action = Sub() Call Console.WriteLine("Please implements this delegate event in the delivered type!")
        ''' <summary>
        ''' 关闭所有的标签页但是当前标签页除外（用户点击了右键菜单上面的关闭所有标签页但是当前的标签页除外的按钮）
        ''' </summary>
        ''' <remarks></remarks>
        Protected Friend EventCloseAllTabsButThis As System.Action = Sub() Call Console.WriteLine("Please implements this delegate event in the delivered type!")
        ''' <summary>
        ''' 关闭当前标签页（用户点击了右键菜单上面的关闭当前标签页的按钮）
        ''' </summary>
        ''' <remarks></remarks>
        Protected Friend EventCloseTabPage As System.Action = Sub() Call Console.WriteLine("Please implements this delegate event in the delivered type!")

        ''' <summary>
        ''' 本方法激活当前的标签页同时取消其他的标签页的激活状态，假若要仅仅更改当前标签页的激活状态，请使用Active属性
        ''' </summary>
        ''' <remarks></remarks>
        Public Overridable Sub ActiveTabPage()
            Throw New NotImplementedException
        End Sub

        ''' <summary>
        ''' 获取一个值来指示当前的标签页是否处于激活的状态
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Active As Boolean
            Get
                Return Me._Active
            End Get
        End Property

        Public Overridable Shadows Property Enabled As Boolean
            Get
                Return MyBase.Enabled
            End Get
            Set(value As Boolean)
                MyBase.Enabled = value
            End Set
        End Property

        Public Overridable Sub SetActive(active As Boolean)
            Me._Active = active
        End Sub
    End Class
End Namespace
