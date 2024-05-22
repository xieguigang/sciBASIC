#Region "Microsoft.VisualBasic::346ee46165f60c3a7332c6088fc5057a, gr\network-visualization\network_layout\Cola\Layout\Link.vb"

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

    '   Total Lines: 30
    '    Code Lines: 11 (36.67%)
    ' Comment Lines: 14 (46.67%)
    '    - Xml Docs: 92.86%
    ' 
    '   Blank Lines: 5 (16.67%)
    '     File Size: 978 B


    '     Class Link
    ' 
    '         Properties: length, source, target, weight
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Cola

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="NodeRefType">可以是节点对象类型的实例或者节点数组的下标数字值</typeparam>
    Public Class Link(Of NodeRefType)

        Public Property source As NodeRefType
        Public Property target As NodeRefType

        ''' <summary>
        ''' ideal length the layout should try to achieve for this link 
        ''' </summary>
        ''' <returns></returns>
        Public Property length As Double

        ''' <summary>
        ''' how hard we should try to satisfy this link's ideal length
        ''' must be in the range: ``0 &lt; weight &lt;= 1``
        ''' if unspecified 1 is the default
        ''' </summary>
        ''' <returns></returns>
        Public Property weight As Double

        Public Overrides Function ToString() As String
            Return $"[{source}, {target}]"
        End Function
    End Class
End Namespace
