Public Class MetroColorSchemes

#Region "Internal ReadOnly Fields"

    Protected _SelectedNormal As Color()
    Protected _SelectedHover As Color()
    Protected _SelectedPressed As Color()
    Protected _SelectedBorder As SolidBrush
    Protected _UnSelectedNormal As Color()
    Protected _UnSelectedHover As Color()
    Protected _UnSelectedPressed As Color()
    Protected _UnSelectedBorder As SolidBrush
    Protected _DisabledBorder As SolidBrush
    Protected _DisabledAllColor As Color()

#End Region

#Region "Public ReadOnly Property Interface"

    Public ReadOnly Property SelectedNormal As Color()
        Get
            Return _SelectedNormal
        End Get
    End Property
    Public ReadOnly Property SelectedHover As Color()
        Get
            Return _SelectedHover
        End Get
    End Property
    Public ReadOnly Property SelectedPressed As Color()
        Get
            Return _SelectedPressed
        End Get
    End Property
    Public ReadOnly Property SelectedBorder As SolidBrush
        Get
            Return _SelectedBorder
        End Get
    End Property
    Public ReadOnly Property UnSelectedNormal As Color()
        Get
            Return _UnSelectedNormal
        End Get
    End Property
    Public ReadOnly Property UnSelectedHover As Color()
        Get
            Return _UnSelectedHover
        End Get
    End Property
    Public ReadOnly Property UnSelectedPressed As Color()
        Get
            Return _UnSelectedPressed
        End Get
    End Property
    Public ReadOnly Property UnSelectedBorder As SolidBrush
        Get
            Return _UnSelectedBorder
        End Get
    End Property
    Public ReadOnly Property DisabledBorder As SolidBrush
        Get
            Return _DisabledBorder
        End Get
    End Property
    Public ReadOnly Property DisabledAllColor As Color()
        Get
            Return _DisabledAllColor
        End Get
    End Property
#End Region

    Sub New(UnSelectedNormal As Color, UnSelectedHover As Color, Selected As Color)
        _SelectedNormal = New Color() {Selected, Selected, Selected, Selected, Selected}
        _SelectedHover = New Color() {Selected, Selected, Selected, Selected, Selected}
        _SelectedPressed = New Color() {Selected, Selected, Selected, Selected, Selected}
        _SelectedBorder = New SolidBrush(Selected)
        _UnSelectedNormal = New Color() {UnSelectedNormal, UnSelectedNormal, UnSelectedNormal, UnSelectedNormal, UnSelectedNormal}
        _UnSelectedHover = New Color() {UnSelectedHover, UnSelectedHover, UnSelectedHover, UnSelectedHover, UnSelectedHover}
        _UnSelectedPressed = New Color() {UnSelectedHover, UnSelectedHover, UnSelectedHover, UnSelectedHover, UnSelectedHover}
        _UnSelectedBorder = New SolidBrush(UnSelectedHover)
        _DisabledBorder = New SolidBrush(UnSelectedNormal)
        _DisabledAllColor = New Color() {UnSelectedNormal, UnSelectedNormal, UnSelectedNormal, UnSelectedNormal, UnSelectedNormal}
    End Sub

    Public Shared Function WeChatMetroColor() As MetroColorSchemes
        Return New MetroColorSchemes(Color.FromArgb(67, 69, 72), Color.FromArgb(78, 80, 84), Color.FromArgb(96, 100, 105))
    End Function

End Class

Public Class QQFlatColorSchema : Inherits MetroColorSchemes

    Sub New()
        Call MyBase.New(Nothing, Nothing, Nothing)

        Dim Color As Color = System.Drawing.Color.FromArgb(253, 237, 174)

        _SelectedNormal = New Color() {Color, Color, Color, Color, Color}
        _SelectedHover = New Color() {Color, Color, Color, Color, Color}
        _SelectedPressed = New Color() {Color, Color, Color, Color, Color}
        _SelectedBorder = New SolidBrush(System.Drawing.Color.FromArgb(232, 215, 150))

        Color = Drawing.Color.FromArgb(252, 252, 252)

        _UnSelectedNormal = New Color() {Color, Color, Color, Color, Color}

        Color = Drawing.Color.FromArgb(252, 240, 196)

        _UnSelectedHover = New Color() {Color, Color, Color, Color, Color}
        _UnSelectedPressed = New Color() {Color, Color, Color, Color, Color}
        _UnSelectedBorder = New SolidBrush(Drawing.Color.FromArgb(232, 232, 232))
        _DisabledBorder = New SolidBrush(Drawing.Color.FromArgb(232, 232, 232))
        _DisabledAllColor = New Color() {Drawing.Color.White, Drawing.Color.White, Drawing.Color.White, Drawing.Color.White, Drawing.Color.White}
    End Sub

End Class
