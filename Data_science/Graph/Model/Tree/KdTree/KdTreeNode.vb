#Region "Microsoft.VisualBasic::71259577c5cc09adcd8c9ee1e57207b5, Data_science\Graph\Model\Tree\KdTree\KdTreeNode.vb"

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

    '   Total Lines: 53
    '    Code Lines: 30
    ' Comment Lines: 13
    '   Blank Lines: 10
    '     File Size: 1.57 KB


    '     Class KdTreeNode
    ' 
    '         Properties: data, dimension, isLeaf, left, parent
    '                     right
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: distanceSquared, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace KdTree

    ''' <summary>
    ''' K-D Tree node class
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class KdTreeNode(Of T)

        ''' <summary>
        ''' payload
        ''' </summary>
        ''' <returns></returns>
        Public Property data As T
        Public Property left As KdTreeNode(Of T)
        Public Property right As KdTreeNode(Of T)
        Public Property parent As KdTreeNode(Of T)

        ''' <summary>
        ''' axis dimension
        ''' </summary>
        ''' <returns></returns>
        Public Property dimension As Integer

        Public ReadOnly Property isLeaf As Boolean
            Get
                Return Not left Is Nothing
            End Get
        End Property

        Sub New(obj As T, dimension%, parent As KdTreeNode(Of T))
            Me.data = obj
            Me.dimension = dimension
            Me.parent = parent
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function distanceSquared(point As T, access As KdNodeAccessor(Of T)) As Double
            ' sqrt Is expensive, we dont need the real value
            Return access.metric(data, point)
        End Function

        Public Overrides Function ToString() As String
            Return $"{data}, dim={dimension}"
        End Function

        Public Shared Narrowing Operator CType(node As KdTreeNode(Of T)) As T
            Return node.data
        End Operator

    End Class
End Namespace
