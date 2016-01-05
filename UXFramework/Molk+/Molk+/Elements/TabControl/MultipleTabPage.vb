Namespace Visualise.Elements

    ''' <summary>
    ''' 请记住，使用<see cref="MultipleTabPage.SetupResource(ByRef Windows.Forms.Controls.TabControl.TabPage.MultipleTabpagePanel)"/>方法进行主题的渲染工作要在加载标签页之前进行
    ''' </summary>
    Public Class MultipleTabPage

        Public Property LabelHeight As Integer
        Public Property UIResource As ButtonResource
        Public Property Font As Font
        Public Property SeperatorBarResource As Image
        Public Property BorderColor As Drawing.Pen
        ''' <summary>
        ''' 标签页和分割线之间的距离
        ''' </summary>
        ''' <returns></returns>
        Public Property SeperatorBarSpacing As Integer = 5
        ''' <summary>
        ''' 标签页之间的间隔距离
        ''' </summary>
        ''' <returns></returns>
        Public Property TabSpacing As Integer

        Public Overridable Sub SetupResource(ByRef ctrl As MolkPlusTheme.Windows.Forms.Controls.TabControl.TabPage.MultipleTabpagePanel)
            ctrl.Font = Font
            ctrl.UIResource = UIResource
            ctrl.Separable.BackgroundImage = SeperatorBarResource
        End Sub

        Public Shared ReadOnly Property MolkPlusTheme As MultipleTabPage
            Get
                Dim Theme = New MultipleTabPage With {.LabelHeight = 40, .Font = New Font(YaHei, 9, FontStyle.Regular)}
                Theme.UIResource = New ButtonResource With {.Active = My.Resources.Active, .InSensitive = My.Resources.Inactive, .Normal = My.Resources.Inactive, .PreLight = My.Resources.InactivePrelight}
                Return Theme
            End Get
        End Property
    End Class
End Namespace