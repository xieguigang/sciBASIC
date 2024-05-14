#Region "Microsoft.VisualBasic::e0ce23a30581c9c3b9043373de069721, Data_science\Visualization\Plots\g\Axis\Styles.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 55
    '    Code Lines: 21
    ' Comment Lines: 27
    '   Blank Lines: 7
    '     File Size: 1.25 KB


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
