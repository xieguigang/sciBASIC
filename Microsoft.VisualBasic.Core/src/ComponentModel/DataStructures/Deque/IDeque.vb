#Region "Microsoft.VisualBasic::130b099b72134ae9e6244e9048907fc1, Microsoft.VisualBasic.Core\src\ComponentModel\DataStructures\Deque\IDeque.vb"

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

    '   Total Lines: 13
    '    Code Lines: 11
    ' Comment Lines: 0
    '   Blank Lines: 2
    '     File Size: 367 B


    '     Interface IDeque
    ' 
    '         Properties: First, Last
    ' 
    '         Function: RemoveHead, RemoveTail, Reverse
    ' 
    '         Sub: AddHead
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ComponentModel.Collection.Deque

    Public Interface IDeque(Of T)
        Inherits IList(Of T)

        ReadOnly Property First As T
        ReadOnly Property Last As T
        Sub AddHead(item As T)
        Function RemoveHead() As T
        Function RemoveTail() As T
        Function Reverse() As IDeque(Of T)
    End Interface
End Namespace
