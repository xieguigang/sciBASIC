#Region "Microsoft.VisualBasic::fe46f8f9ad6f56636e10ca462a48243b, Microsoft.VisualBasic.Core\src\ComponentModel\System.Collections.Generic\PriorityQueue\PriorityQueueCreator.vb"

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

    '   Total Lines: 19
    '    Code Lines: 15 (78.95%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 4 (21.05%)
    '     File Size: 600 B


    '     Module PriorityQueueCreator
    ' 
    '         Function: CreateEmptyPriorityQueue
    ' 
    '         Sub: AddAll
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace ComponentModel.Collection

    Public Module PriorityQueueCreator

        Public Function CreateEmptyPriorityQueue(Of T As IComparable(Of T))() As PriorityQueue(Of T)
            Return New PriorityQueue(Of T)(Function(a, b) a.CompareTo(b) < 0)
        End Function

        <Extension>
        Public Sub AddAll(Of T)([set] As HashSet(Of T), range As IEnumerable(Of T))
            For Each item As T In range
                Call [set].Add(item)
            Next
        End Sub
    End Module
End Namespace
