#Region "Microsoft.VisualBasic::c978f7d92b9a31d21d23af67f74f499d, gr\network-visualization\Visualizer\Styling\CSS\StyleCreator.vb"

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

    '   Total Lines: 46
    '    Code Lines: 22 (47.83%)
    ' Comment Lines: 18 (39.13%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (13.04%)
    '     File Size: 1.54 KB


    '     Structure StyleCreator
    ' 
    '         Function: CompileSelector
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Styling.FillBrushes
Imports Microsoft.VisualBasic.Data.visualize.Network.Styling.Numeric
Imports Microsoft.VisualBasic.Scripting.Expressions
Imports Microsoft.VisualBasic.Serialization

Namespace Styling.CSS

    Public Structure StyleCreator

        ''' <summary>
        ''' 类似于CSS选择器的字符串表达式
        ''' </summary>
        Dim selector$
        ''' <summary>
        ''' 进行对象绘制的时候的边框样式
        ''' </summary>
        Dim stroke As Pen
        ''' <summary>
        ''' 对象标签的字体
        ''' </summary>
        Dim font As Font
        ''' <summary>
        ''' 对对象进行填充的样式画笔
        ''' </summary>
        Dim fill As IGetBrush
        ''' <summary>
        ''' 主要是针对于节点对象的大小直径的获取函数
        ''' </summary>
        Dim size As IGetSize

        ''' <summary>
        ''' 从对象之中得到标签字符串的方法函数指针
        ''' </summary>
        Dim label As IStringBuilder

        Public Function CompileSelector() As Func(Of IEnumerable(Of Node), IEnumerable(Of Node))
            Dim expression$ = selector

            Return Function(nodes)
                       Return nodes.[Select](expression, AddressOf SelectNodeValue)
                   End Function
        End Function
    End Structure
End Namespace
