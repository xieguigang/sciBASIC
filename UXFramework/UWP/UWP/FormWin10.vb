Imports System.Reflection

Public Class FormWin10 : Inherits Microsoft.VisualBasic.MolkPlusTheme.Windows.Forms.Form

    Public Overrides Property BorderColor As Color
        Get
            Return MyBase.BorderColor
        End Get
        Set(value As Color)
            MyBase.BorderColor = value
            _activeBorder = value
        End Set
    End Property

    Dim _activeBorder As Color
    Dim _lostFocusBorder As Color = Color.FromArgb(170, 170, 170)
    Dim _navigationStack As NavigationEvent

    Public Overloads ReadOnly Property Menu As MolkPlusTheme.ListControl
        Get
            Return UserAvatarMenu1.Menu.ListControl1
        End Get
    End Property

    Private Sub FormWin10_Activated(sender As Object, e As EventArgs) Handles Me.Activated
        MyBase.BorderColor = _activeBorder
    End Sub

    Private Sub FormWin10_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.DrawBorderFrame = True
        Me.BorderColor = Color.FromArgb(24, 131, 215)
        Me.Back.UI = New MolkPlusTheme.Visualise.Elements.ButtonResource With {
            .Active = My.Resources.BackPress,
            .InSensitive = My.Resources.BackInActive,
            .Normal = My.Resources.BackNormal,
            .PreLight = My.Resources.BackPrelight
        }
        Me.Caption1.BackColor = Color.White

        Dim res = New MolkPlusTheme.Visualise.Elements.CaptionResources With {
            .Close = New MolkPlusTheme.Visualise.Elements.ButtonResource With {
                .Active = My.Resources.ClosePress,
                .InSensitive = My.Resources.CloseNormal,
                .Normal = My.Resources.CloseNormal,
                .PreLight = My.Resources.ClosePreLight},
            .Maximum = New MolkPlusTheme.Visualise.Elements.ButtonResource With {
                .PreLight = My.Resources.MaxPreLight,
                .Active = My.Resources.MaxPress,
                .InSensitive = My.Resources.MaxNormal,
                .Normal = My.Resources.MaxNormal},
            .Minimize = New MolkPlusTheme.Visualise.Elements.ButtonResource With {
                .Normal = My.Resources.MinNormal,
                .Active = My.Resources.MinPress,
                .InSensitive = My.Resources.MinNormal,
                .PreLight = My.Resources.MinPreLight},
            .ControlboxButtonSize = New Drawing.Size(45, 29),
            .HeightLimits = 50,
            .BackgroundImage = New MolkPlusTheme.Visualise.Elements.ImageResource(Color.White),
            .ControlBox = New MolkPlusTheme.Visualise.Elements.ImageResource(color:=Color.White),
            .InformationArea = New MolkPlusTheme.Visualise.Elements.ImageResource(color:=Color.White),
            .BorderPen = MyBase._BorderPen
        }

        Me.Caption1.UIThemes = res
        Menu.ColorSchema = MolkPlusTheme.MetroColorSchemes.WeChatMetroColor
        _navigationStack = New NavigationEvent(Me)
        Me.Caption1.Update()

#If DEBUG Then
        UserAvatarMenu1.Text = "amethyst"
        UserAvatarMenu1.Avatar = My.Resources.Asuka
        UserAvatarMenu1.EMail = "amethyst.asuka@gcmodeller.org"

        Call Menu.Add(Nothing, New MolkPlusTheme.ListControlItem With {.Text = "ffff"})
#End If
    End Sub

#If DEBUG Then
    <Navigation> Public Event TestEvent1()

    Private Sub FormWin10_TestEvent1() Handles Me.TestEvent1
        MsgBox(Now.ToString)
    End Sub

    Private Sub UserAvatarMenu1_DoubleClick(sender As Object, e As EventArgs) Handles UserAvatarMenu1.DoubleClick
        RaiseEvent TestEvent1()
    End Sub
#End If

    Private Sub FormWin10_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        Call Me.UserAvatarMenu1.RePositionMenu()
    End Sub
End Class
