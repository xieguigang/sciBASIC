#Region "Microsoft.VisualBasic::4c84c763c709b5ea37b4a8eb60775e83, G:/GCModeller/src/runtime/sciBASIC#/gr/Microsoft.VisualBasic.Imaging//d3js/scale/MapperTypes.vb"

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

    '   Total Lines: 21
    '    Code Lines: 7
    ' Comment Lines: 12
    '   Blank Lines: 2
    '     File Size: 536 B


    '     Enum MapperTypes
    ' 
    '         Continuous, Discrete, Passthrough
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region


Namespace d3js.scale

    ''' <summary>
    ''' 从graphics的属性值到相应的图形属性(大小，颜色，字体，形状)的映射操作类型
    ''' </summary>
    Public Enum MapperTypes
        ''' <summary>
        ''' 连续的数值型的映射
        ''' </summary>
        Continuous
        ''' <summary>
        ''' 离散的分类映射
        ''' </summary>
        Discrete
        ''' <summary>
        ''' 直接映射
        ''' </summary>
        Passthrough
    End Enum
End Namespace
