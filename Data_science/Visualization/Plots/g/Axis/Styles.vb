#Region "Microsoft.VisualBasic::e0ce23a30581c9c3b9043373de069721, Data_science\Visualization\Plots\g\Axis\Styles.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Enum XAxisLayoutStyles
    ' 
    '         Centra, None, Top, ZERO
    ' 
    '  
    ' 
    ' 
    ' 
    '     Enum YAxisLayoutStyles
    ' 
    '         Centra, None, Right, ZERO
    ' 
    '  
    ' 
    ' 
    ' 
    '     Enum YlabelPosition
    ' 
    '         InsidePlot, LeftCenter, None
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Graphic.Axis

    Public Enum XAxisLayoutStyles As Byte

        ''' <summary>
        ''' (默认样式) x轴位于图表的底部
        ''' </summary>
        Bottom = 0

        ''' <summary>
        ''' X轴位于图表的顶端
        ''' </summary>
        Top
        ''' <summary>
        ''' x轴位于图表的中部
        ''' </summary>
        Centra
        ''' <summary>
        ''' X轴位于Y轴纵坐标值为零的位置
        ''' </summary>
        ZERO
        None
    End Enum

    Public Enum YAxisLayoutStyles As Byte

        ''' <summary>
        ''' (默认样式) y轴位于图表的左侧
        ''' </summary>
        Left = 0
        ''' <summary>
        ''' y轴位于图表的中部
        ''' </summary>
        Centra
        ''' <summary>
        ''' y轴位于图表的右侧
        ''' </summary>
        Right
        ''' <summary>
        ''' y轴位于X轴横坐标值等于零的位置
        ''' </summary>
        ZERO
        ''' <summary>
        ''' 不进行Y轴的绘制
        ''' </summary>
        None

    End Enum

    Public Enum YlabelPosition
        None
        InsidePlot
        LeftCenter
    End Enum
End Namespace
