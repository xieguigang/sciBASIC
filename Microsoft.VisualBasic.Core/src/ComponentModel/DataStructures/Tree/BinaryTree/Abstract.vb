#Region "Microsoft.VisualBasic::cf604f709a0ada068726574cd3cccdda, Microsoft.VisualBasic.Core\src\ComponentModel\DataStructures\Tree\BinaryTree\Abstract.vb"

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

    '   Total Lines: 17
    '    Code Lines: 12 (70.59%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 5 (29.41%)
    '     File Size: 620 B


    '     Class TreeMap
    ' 
    '         Properties: Key, value
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Language

Namespace ComponentModel.DataStructures.BinaryTree

    Public MustInherit Class TreeMap(Of K, V)
        Implements IKeyedEntity(Of K)
        Implements Value(Of V).IValueOf
        Implements IComparable(Of K)

        Public Property Key As K Implements IKeyedEntity(Of K).Key
        Public Property value As V Implements Value(Of V).IValueOf.Value

        Public MustOverride Function CompareTo(other As K) As Integer Implements IComparable(Of K).CompareTo

    End Class
End Namespace
