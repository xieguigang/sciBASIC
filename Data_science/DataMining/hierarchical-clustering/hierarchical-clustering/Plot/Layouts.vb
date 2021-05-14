#Region "Microsoft.VisualBasic::939f8204158f16cacbc8ec4d825c0eeb, Data_science\DataMining\hierarchical-clustering\hierarchical-clustering\Plot\Layouts.vb"

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

    ' Enum Layouts
    ' 
    '     Circular, Horizon
    ' 
    '  
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

''' <summary>
''' 层次聚类树的绘制布局枚举
''' </summary>
Public Enum Layouts As Byte
    ''' <summary>
    ''' 默认的竖直的布局
    ''' </summary>
    Vertical = 0
    ''' <summary>
    ''' 水平布局样式
    ''' </summary>
    Horizon
    Circular
End Enum
