Namespace Visualise.Elements

    Public Class MultipleTabPage

        Public Property LabelHeight As Integer
        Public Property UIResource As ButtonResource
        Public Property Font As Font
        Public Property SeperatorBarResource As Image

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