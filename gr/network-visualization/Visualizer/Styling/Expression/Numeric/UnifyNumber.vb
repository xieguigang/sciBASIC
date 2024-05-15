#Region "Microsoft.VisualBasic::e982ef20ba994cefcbc8f85ef733a242, gr\network-visualization\Visualizer\Styling\Expression\Numeric\UnifyNumber.vb"

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

    '   Total Lines: 26
    '    Code Lines: 18
    ' Comment Lines: 3
    '   Blank Lines: 5
    '     File Size: 774 B


    '     Class UnifyNumber
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

    ''' <summary>
    ''' 所有的节点都统一大小
    ''' </summary>
    Public Class UnifyNumber : Implements IGetSize

        ReadOnly r!

        Sub New(expression As String)
            Me.r! = Val(expression)
        End Sub

        Public Iterator Function GetSize(nodes As IEnumerable(Of Node)) As IEnumerable(Of Map(Of Node, Single)) Implements IGetSize.GetSize
            For Each n As Node In nodes
                Yield New Map(Of Node, Single) With {
                    .Key = n,
                    .Maps = r
                }
            Next
        End Function
    End Class
End Namespace
