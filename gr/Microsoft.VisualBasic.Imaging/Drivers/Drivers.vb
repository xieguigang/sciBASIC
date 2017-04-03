Imports Microsoft.VisualBasic.Imaging.SVG

Namespace Driver

    ''' <summary>
    ''' The imaging graphics engine list for <see cref="IGraphics"/>
    ''' </summary>
    Public Enum Drivers
        ''' <summary>
        ''' <see cref="Graphics2D"/>
        ''' </summary>
        GDI
        ''' <summary>
        ''' <see cref="GraphicsSVG"/>
        ''' </summary>
        SVG
    End Enum
End Namespace