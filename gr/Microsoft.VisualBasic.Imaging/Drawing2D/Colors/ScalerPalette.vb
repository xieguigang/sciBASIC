Imports System.ComponentModel

Namespace Drawing2D.Colors

    ''' <summary>
    ''' color set for visualize linear scale data color mapping
    ''' </summary>
    Public Enum ScalerPalette

        ''' <summary>
        ''' <see cref="ColorMap.PatternJet"/>
        ''' </summary>
        Jet = 0

        ''' <summary>
        ''' <see cref="ColorMap.PatternAutumn"/>
        ''' </summary>
        Autumn
        ''' <summary>
        ''' <see cref="ColorMap.PatternCool"/>
        ''' </summary>
        Cool
        ''' <summary>
        ''' <see cref="ColorMap.PatternGray"/>
        ''' </summary>
        Gray
        ''' <summary>
        ''' <see cref="ColorMap.PatternHot"/>
        ''' </summary>
        Hot
        ''' <summary>
        ''' <see cref="ColorMap.PatternSpring"/>
        ''' </summary>
        Spring
        ''' <summary>
        ''' <see cref="ColorMap.PatternSummer"/>
        ''' </summary>
        Summer
        ''' <summary>
        ''' <see cref="ColorMap.PatternWinter"/>
        ''' </summary>
        Winter

        <Description("viridis")> viridis
        <Description("viridis:magma")> magma
        <Description("viridis:inferno")> inferno
        <Description("viridis:plasma")> plasma
        <Description("viridis:cividis")> cividis
        <Description("viridis:mako")> mako
        <Description("viridis:rocket")> rocket
        <Description("viridis:turbo")> turbo

    End Enum

End Namespace