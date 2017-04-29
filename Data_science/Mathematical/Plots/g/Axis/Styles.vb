#Region "Microsoft.VisualBasic::d1fa87228b48d2c17017726539adf886, ..\sciBASIC#\Data_science\Mathematical\Plots\g\Axis\Styles.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region


Namespace Graphic.Axis

    Public Enum XAxisLayoutStyles
        ''' <summary>
        ''' X轴位于图表的顶端
        ''' </summary>
        Top
        ''' <summary>
        ''' x轴位于图表的中部
        ''' </summary>
        Centra
        ''' <summary>
        ''' x轴位于图表的底部
        ''' </summary>
        Bottom
        ZERO
        None
    End Enum

    Public Enum YAxisLayoutStyles
        ''' <summary>
        ''' y轴位于图表的左侧
        ''' </summary>
        Left
        ''' <summary>
        ''' y轴位于图表的中部
        ''' </summary>
        Centra
        ''' <summary>
        ''' y轴位于图表的右侧
        ''' </summary>
        Right
        ZERO
        None
    End Enum

    Public Enum YlabelPosition
        None
        InsidePlot
        LeftCenter
    End Enum
End Namespace
