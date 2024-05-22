#Region "Microsoft.VisualBasic::99d4860696406b9714b25f7b08166abb, gr\network-visualization\Visualizer\Styling\Expression\Numeric\PassthroughNumber.vb"

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

    '   Total Lines: 49
    '    Code Lines: 22 (44.90%)
    ' Comment Lines: 18 (36.73%)
    '    - Xml Docs: 83.33%
    ' 
    '   Blank Lines: 9 (18.37%)
    '     File Size: 1.62 KB


    '     Interface IGetSize
    ' 
    '         Function: GetSize
    ' 
    '     Class PassthroughNumber
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetSize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph

Namespace Styling.Numeric

    Public Interface IGetSize

        ''' <summary>
        ''' 这个函数描述了这样的一个过程：
        ''' 
        ''' 对一个节点集合进行成员的枚举，然后将每一个成员映射为一个大小数值，并返回这些映射集合
        ''' </summary>
        ''' <param name="nodes"></param>
        ''' <returns></returns>
        Function GetSize(nodes As IEnumerable(Of Node)) As IEnumerable(Of Map(Of Node, Single))

    End Interface

    ''' <summary>
    ''' 从节点的给定属性之中得到对应的节点大小值
    ''' </summary>
    Public Class PassthroughNumber : Implements IGetSize

        ''' <summary>
        ''' 单词
        ''' </summary>
        ReadOnly expression As String

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="expression">这个表达式应该是属性名称</param>
        Sub New(expression As String)
            Me.expression = expression
        End Sub

        Public Iterator Function GetSize(nodes As IEnumerable(Of Node)) As IEnumerable(Of Map(Of Node, Single)) Implements IGetSize.GetSize
            ' 单词
            Dim selector = expression.SelectNodeValue

            For Each n As Node In nodes
                Yield New Map(Of Node, Single) With {
                    .Key = n,
                    .Maps = Val(selector(n))
                }
            Next
        End Function
    End Class
End Namespace
