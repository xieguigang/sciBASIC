#Region "Microsoft.VisualBasic::08f65f66b73f6eea55b07e2b3bc34b9c, ..\visualbasic_App\UXFramework\Molk+\Molk+\Forms.Controls\TabControls\Tabpage\Tabpage.vb"

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
    ''' Tabpage in the multiple tabpage controls collection.(标签页控件)
    ''' </summary>
    ''' <remarks></remarks>
    Public Class TabPage : Inherits Windows.Forms.Controls.TabControl.ITabPage
        Implements ITabPage.ITabPageEx

        Dim _InternalLabelButtonUIResource As MolkPlusTheme.Visualise.Elements.ButtonResource
        Dim Close As Rectangle = New Rectangle

        ''' <summary>
        ''' 当前的标签页失活
        ''' </summary>
        ''' <remarks></remarks>
        Public Event TabPageInactive() Implements ITabPage.ITabPageEx.TabPageInactive
        ''' <summary>
        ''' 当前的标签页被激活
        ''' </summary>
        ''' <remarks></remarks>
        Public Event TabPageActive() Implements ITabPage.ITabPageEx.TabPageActive
        ''' <summary>
        ''' 关闭所有的标签页（用户点击了右键菜单上面的关闭所有标签页的按钮）
        ''' </summary>
        ''' <remarks></remarks>
        Public Event CloseAllTabs() Implements ITabPage.ITabPageEx.CloseAllTabs
        ''' <summary>
        ''' 关闭所有的标签页但是当前标签页除外（用户点击了右键菜单上面的关闭所有标签页但是当前的标签页除外的按钮）
        ''' </summary>
        ''' <remarks></remarks>
        Public Event CloseAllTabsButThis() Implements ITabPage.ITabPageEx.CloseAllTabsButThis
        ''' <summary>
        ''' 关闭当前标签页（用户点击了右键菜单上面的关闭当前标签页的按钮）
        ''' </summary>
        ''' <remarks></remarks>
        Public Event CloseTabPage() Implements ITabPage.ITabPageEx.CloseTabPage

        Public Overrides Property Enabled As Boolean
            Get
                Return MyBase.Enabled
            End Get
            Set(value As Boolean)
                If Not UI Is Nothing Then BackgroundImage = IIf(value = True, UI.Normal, UI.InSensitive)
                MyBase.Enabled = value
            End Set
        End Property

        ''' <summary>
        ''' 是否允许关闭按钮起作用
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property EnableCloseButton As Boolean

        ''' <summary>
        ''' 标签页是否可以被关闭，默认可以被关闭
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property CanbeClosed As Boolean = True

        Public Overrides Property Text As String
            Get
                Return MyBase.Text
            End Get
            Set(value As String)
                MyBase.Text = value
                Call InternalUpdateUI()
            End Set
        End Property

        Friend Sub SetEnableMenu(stateValue As Boolean)
            If stateValue = True Then
                Me.ContextMenuStrip = ContextMenuStrip1
            Else
                Me.ContextMenuStrip = Nothing
            End If
        End Sub

        Dim OriginalUIResource As MolkPlusTheme.Visualise.Elements.ButtonResource

        Public Property UI As MolkPlusTheme.Visualise.Elements.ButtonResource
            Get
                Return _InternalLabelButtonUIResource
            End Get
            Set(value As MolkPlusTheme.Visualise.Elements.ButtonResource)
                OriginalUIResource = value
                Call InternalUpdateUI()
            End Set
        End Property

        Private Sub InternalUpdateUI()
            If Not OriginalUIResource Is Nothing Then _InternalLabelButtonUIResource = OriginalUIResource.Clone

            If UI Is Nothing Then UI = MolkPlusTheme.Visualise.Elements.MultipleTabPage.MolkPlusTheme.UIResource

            If Me.AutoSize Then Size = _InternalLabelButtonUIResource.Normal.Size
            BackgroundImage = IIf(_Active, _InternalLabelButtonUIResource.Active, _InternalLabelButtonUIResource.Normal)

            UI.Active = InternalDrawingTabpage(_InternalLabelButtonUIResource.Active, Text, Brushes.Black)
            UI.PreLight = InternalDrawingTabpage(_InternalLabelButtonUIResource.PreLight, Text, Brushes.White)
            UI.Normal = InternalDrawingTabpage(_InternalLabelButtonUIResource.Normal, Text, Brushes.White)
            UI.InSensitive = InternalDrawingTabpage(_InternalLabelButtonUIResource.InSensitive, Text, Brushes.Gray)

            Call ToolTip1.SetToolTip(Me, Text)
            Call SetActive(MyBase._Active)
        End Sub

        Private Function InternalDrawingTabpage(UI As Drawing.Image, s As String, Color As Brush) As Drawing.Image
            Dim Image As Drawing.Image = UI.Clone

            Using Gr As Graphics = Graphics.FromImage(Image)
                Dim pt = New Point With {.X = 3, .Y = 2}
                Call Gr.DrawString(s, Me.Font, Color, pt)
                Return Image
            End Using
        End Function

        Private Sub TabPage_Click(sender As Object, e As EventArgs) Handles Me.Click
            If Close.Contains(PointToClient(MousePosition)) Then RaiseEvent TabPageActive()
        End Sub

        Private Sub TabPage_DoubleClick(sender As Object, e As EventArgs) Handles Me.DoubleClick
            If CanbeClosed Then
                RaiseEvent CloseTabPage()
            End If
        End Sub

        Protected Friend Sub Load()
            Me.ContextMenuStrip = Me.ContextMenuStrip1
            Me.Close.Size = New Size With {.Width = 12, .Height = 12}
            Me.Close.Location = New Point With {.X = Width - Close.Width - 3, .Y = 3}
            Me.Size = New Size With {.Width = 140, .Height = 20}

            Me._Active = False
            'Me.AutoSize = True

            MyBase.EventCloseAllTabs = Sub() RaiseEvent CloseAllTabs()
            MyBase.EventCloseAllTabsButThis = Sub() RaiseEvent CloseAllTabsButThis()
            MyBase.EventCloseTabPage = Sub() RaiseEvent CloseTabPage()
            MyBase.EventTabPageActive = Sub() RaiseEvent TabPageActive()
            MyBase.EventTabPageInactive = Sub() RaiseEvent TabPageInactive()
        End Sub

        Sub New()

            ' 此调用是设计器所必需的。
            InitializeComponent()

            ' 在 InitializeComponent() 调用之后添加任何初始化。
            Call Me.Load()
        End Sub

        Private Sub TabPage_MouseDown(sender As Object, e As MouseEventArgs) Handles Me.MouseDown
            BackgroundImage = UI.Active
        End Sub

        Private Sub TabPage_MouseEnter(sender As Object, e As EventArgs) Handles Me.MouseEnter
            If Not _Active Then BackgroundImage = UI.PreLight
        End Sub

        Private Sub TabPage_MouseLeave(sender As Object, e As EventArgs) Handles Me.MouseLeave
            If Not _Active Then BackgroundImage = UI.Normal
        End Sub

        Private Sub TabPage_MouseUp(sender As Object, e As MouseEventArgs) Handles Me.MouseUp
            If Not _Active AndAlso Not Parent Is Nothing Then
                Dim Query As Generic.IEnumerable(Of TabPage) = From ctl As Control In Parent.Controls
                                                               Where TypeOf ctl Is TabPage AndAlso Not Me.Equals(ctl)
                                                               Select DirectCast(ctl, TabPage)
                For Each TAB As TabPage In Query.ToArray
                    Call TAB.SetActive(False)
                Next
                Call SetActive(True)

                RaiseEvent TabPageActive()
            End If
        End Sub

        Private Sub InternalActiveTab()

            If Not _Active AndAlso Parent.Controls.Count > 0 Then
            Else
                Return
            End If

            Dim LQuery As Generic.IEnumerable(Of TabPage) = From ctl As Control In Parent.Controls
                                                            Where TypeOf ctl Is TabPage
                                                            Select DirectCast(ctl, TabPage)
            For Each TAB As TabPage In LQuery.ToArray
                Call TAB.SetActive(False)
            Next
        End Sub

        Public Overrides Sub ActiveTabPage()
            Call Me.InternalActiveTab()
            Call SetActive(True)
        End Sub

        Private Sub TabPage_Resize(sender As Object, e As EventArgs) Handles Me.Resize
            Close.Location = New Point With {.X = Width - Close.Width - 2, .Y = 2}
        End Sub

        Private Sub CloseAllTabpage_Click(sender As Object, e As EventArgs) Handles CloseAllTabpage.Click
            RaiseEvent CloseAllTabs()
        End Sub

        Private Sub CloseAllButThisTabpage_Click(sender As Object, e As EventArgs) Handles CloseAllButThisTabpage.Click
            RaiseEvent CloseAllTabsButThis()
        End Sub

        Public Overrides Function ToString() As String
            Return String.Format("{0};  location={1},  size={2},  control= [{3}, location={4}, size={5}]",
                                 Text, Location.ToString, Size.ToString,
                                 Me._InternalTabPageControlItem.ToString,
                                 _InternalTabPageControlItem.Location.ToString,
                                 _InternalTabPageControlItem.Size.ToString)
        End Function

        Public Overrides Sub SetActive(activeState As Boolean)
            Call MyBase.SetActive(activeState)

            If Not _InternalLabelButtonUIResource Is Nothing Then '更改UI样式
                BackgroundImage = IIf(_Active, _InternalLabelButtonUIResource.Active, _InternalLabelButtonUIResource.Normal)
            End If

            If _Active Then
                ' _Active = False
                Call Me.InternalActiveTab()
            End If
        End Sub

        Public Shared Narrowing Operator CType(obj As TabPage) As String
            Return obj.Text
        End Operator
    End Class
End Namespace
