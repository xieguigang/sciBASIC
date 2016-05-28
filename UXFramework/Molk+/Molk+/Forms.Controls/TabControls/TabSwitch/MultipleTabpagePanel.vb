Imports Microsoft.VisualBasic.Imaging

Namespace Windows.Forms.Controls.TabControl.TabPage

    ''' <summary>
    ''' 多标签页
    ''' </summary>
    ''' <remarks></remarks>
    Public Class MultipleTabpagePanel : Inherits Windows.Forms.Controls.TabControl.ITabControl(Of Windows.Forms.Controls.TabControl.TabPage.TabPage)

        Public Event RemoveTab(Name As String)
        Public Event RemoveAllTabsButOne(Tabpage As Windows.Forms.Controls.TabControl.TabPage.TabPage)
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

        Dim _Renderer As Microsoft.VisualBasic.MolkPlusTheme.Visualise.Elements.MultipleTabPage

        Public Property Renderer As Microsoft.VisualBasic.MolkPlusTheme.Visualise.Elements.MultipleTabPage
            Get
                Return _Renderer
            End Get
            Set(value As Microsoft.VisualBasic.MolkPlusTheme.Visualise.Elements.MultipleTabPage)
                _Renderer = value
                If Not value Is Nothing Then
                    UIResource = Renderer.UIResource
                End If
            End Set
        End Property

        Public Enum TabpageSizeModes
            ''' <summary>
            ''' 标签页的大小为资源图片的大小
            ''' </summary>
            UIResourceAutoSize
            ''' <summary>
            ''' 宽度根据标签页的数量进行平均分配
            ''' </summary>
            EvenSize
            ''' <summary>
            ''' 根据文本标题的文字长度自动调整
            ''' </summary>
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
            Else
                If SizeMode = TabpageSizeModes.EvenSize Then
                    Call InternalEvenSizeAndFilled()
                Else
                    Call InternalTextLengthAutoSize()
                End If
            End If


        End Sub

        Private Sub InternalTextLengthAutoSize()
            If Me._InternalTabList.Count = 0 Then
                Return
            End If

            Dim Gr = New Size(10, 10).CreateGDIDevice
            Dim i As Integer = 0
            Dim x As Integer = 0

            For Each TAB As TabPage In Me._InternalTabList.Values
                Dim wd As Integer = Gr.Graphics.MeasureString(TAB.Text, TAB.Font).Width + Renderer.UIResource.TextoffSets.X * 2

                TAB.AutoSize = False
                TAB.Size = New Size(wd, Me.Renderer.LabelHeight)
                TAB.Location = New Point(x, 0)
                Call TAB.UpdateUI()

                x += wd + Renderer.TabSpacing
                i += 1
            Next
        End Sub

        ''' <summary>
        ''' 宽度根据标签页的数量进行平均分配
        ''' </summary>
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

                For Each TAB As TabControl.TabPage.TabPage In Me._InternalTabList.Values
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

        Public Property PageInterval As Integer = 0

        Public Overrides Sub AddTabPage(Name As String, Control As Control, Optional TabCloseEventHandle As Action = Nothing)
            Dim NewTabPage As MolkPlusTheme.Windows.Forms.Controls.TabControl.TabPage.TabPage =
                MyBase.InternalAddTabPage(Name, Control, TabCloseEventHandle)

            NewTabPage.AutoSize = Me.SizeMode = TabpageSizeModes.UIResourceAutoSize
            NewTabPage.BackColor = Color.FromArgb(255, 240, 208)
            NewTabPage.Font = Me.Renderer.Font
            NewTabPage.UI = UIResource
            Separable.Visible = True

            If Not Me.SizeMode = TabpageSizeModes.UIResourceAutoSize Then

                '设置高度
                Call Debug.WriteLine(NewTabPage.Size.ToString)
                NewTabPage.Size = New Size(NewTabPage.Width, Me.Renderer.LabelHeight)
                Call Debug.WriteLine(NewTabPage.Size.ToString)
            End If

            If Not NewTabPage._InternalTabPageControlItem Is Nothing Then
                Dim NewPanel = NewTabPage._InternalTabPageControlItem
                NewPanel.Size = New Size With {.Width = Width, .Height = Height - NewTabPage.Height - 3}
                NewPanel.Location = New Point With {.X = 0, .Y = NewTabPage.Height + NewTabPage.Location.Y + 3}
                NewPanel.BringToFront()

                Call Me.Controls.Add(NewPanel)
            End If

            Call NewTabPage.SetEnableMenu(EnableMenu)

            'Call (Sub() NewTabPage.ActiveTabPage()).BeginInvoke(Nothing, Nothing)

            If SizeMode = TabpageSizeModes.EvenSize Then
                Call InternalEvenSizeAndFilled()
            Else
                If MyBase.PagesCount > 0 Then  '放置标签页的位置
                    NewTabPage.Location = New Point With {.Y = 0, .X = (MyBase._Tabs - 1) * NewTabPage.Width + 1 + PageInterval}
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

        Private Sub RemovesAllButLeftOne(Tabpage As Windows.Forms.Controls.TabControl.TabPage.TabPage)
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

#Region "UI layouts and UI drawings"

        Public Sub MultipleTabpagePanelResizeEventHandler() Handles Me.Resize
            Dim TabPages = MyBase._InternalTabList.Values.ToArray

            Separable.Size = New Size With {.Width = Width, .Height = 2}

            For i As Integer = 0 To TabPages.Length - 1

                Dim TabPage = TabPages(i)

                TabPage._InternalTabPageControlItem.Location = New Point(0, TabPage.Height + Separable.Height + Renderer.SeperatorBarSpacing)
                TabPage._InternalTabPageControlItem.Size = New Size With {.Width = Width, .Height = Height - TabPage.Height - Separable.Height - Renderer.SeperatorBarSpacing}
            Next
        End Sub

        ''' <summary>
        ''' 在赋值了新的主题属性之后使用这个方法强制刷新整个控件的外观样式
        ''' </summary>
        Public Sub UpdatesUILayout()
            Call Me.InternalAutoSizeTabpages()
            Me.Separable.BackgroundImage = Renderer.SeperatorBarResource
            Me.Separable.Location = New Point(0, Me._InternalTabList.First.Value.Height + Renderer.SeperatorBarSpacing)

            Call Me.MultipleTabpagePanelResizeEventHandler()

        End Sub

#End Region
    End Class
End Namespace