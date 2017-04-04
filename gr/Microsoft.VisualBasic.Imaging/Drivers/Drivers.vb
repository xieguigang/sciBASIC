Imports Microsoft.VisualBasic.Imaging.SVG

Namespace Driver

    ''' <summary>
    ''' The imaging graphics engine list for <see cref="IGraphics"/>
    ''' </summary>
    Public Enum Drivers As Byte

        ''' <summary>
        ''' 与具体上下文相关的。当用户从命令行参数之中设置了环境变量之后，这个Default的含义与用户所设置的驱动程序类型一致，但是会被程序开发人员所设置的类型值所覆盖
        ''' </summary>
        [Default]
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