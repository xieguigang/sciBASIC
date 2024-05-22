#Region "Microsoft.VisualBasic::3671c550d58132fc3b4056767b3e8568, Microsoft.VisualBasic.Core\src\ComponentModel\DataStructures\CapacityQueue.vb"

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

    '   Total Lines: 58
    '    Code Lines: 39 (67.24%)
    ' Comment Lines: 5 (8.62%)
    '    - Xml Docs: 80.00%
    ' 
    '   Blank Lines: 14 (24.14%)
    '     File Size: 1.70 KB


    '     Class CapacityQueue
    ' 
    '         Properties: Capacity
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Enqueue, GetEnumerator, IEnumerable_GetEnumerator, ToString
    ' 
    '         Sub: Clear
    ' 
    '         Operators: +
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ComponentModel.Collection

    Public Class CapacityQueue(Of T)
        Implements IEnumerable(Of T)

        ReadOnly queue As Queue(Of T)

        Public ReadOnly Property Capacity As Integer

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="capacity">The initial number of elements that the System.Collections.Generic.Queue`1 can
        ''' contain.</param>
        Sub New(capacity As Integer)
            queue = New Queue(Of T)(capacity)
        End Sub

        Public Function Enqueue(x As T) As T
            Dim o As T

            Call queue.Enqueue(x)

            If queue.Count = Capacity - 1 Then
                o = queue.Dequeue()
            Else
                o = queue.Peek
            End If

            Return o
        End Function

        Public Sub Clear()
            Call queue.Clear()
        End Sub

        Public Overrides Function ToString() As String
            Return GetType(T()).GetObjectJson(Me.ToArray)
        End Function

        Public Overloads Shared Operator +(q As CapacityQueue(Of T), x As T) As CapacityQueue(Of T)
            Call q.Enqueue(x)
            Return q
        End Operator

        Public Iterator Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            For Each x As T In queue
                Yield x
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace
