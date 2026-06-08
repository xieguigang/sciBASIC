#Region "Microsoft.VisualBasic::37fe3731cbaeb510abdef53908c1a831, Data_science\Graph\Network\Bipartite\BipartiteEdge.vb"

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

    '   Total Lines: 69
    '    Code Lines: 44 (63.77%)
    ' Comment Lines: 15 (21.74%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 10 (14.49%)
    '     File Size: 2.15 KB


    ' Class BipartiteEdge
    ' 
    '     Properties: Capacity, Flow, fromVertex, toVertex
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: getOtherEndNode, residualCapacityTo, ToString
    ' 
    '     Sub: increaseFlowTo
    ' 
    ' /********************************************************************************/

#End Region


Imports Microsoft.VisualBasic.Data.GraphTheory.Network

Friend Class BipartiteEdge : Implements IndexEdge

    ''' <summary>
    ''' an edge is composed of 2 vertices
    ''' </summary>
    Public Property fromVertex As Integer Implements IndexEdge.U
    Public Property toVertex As Integer Implements IndexEdge.V
    ''' <summary>
    ''' edges also have a capacity &amp; a flow
    ''' </summary>
    Friend m_capacity As Integer
    Friend m_flow As Integer

    Public Sub New(fromVertex As Integer, toVertex As Integer, Optional capacity As Integer = 1)
        Me.fromVertex = fromVertex
        Me.toVertex = toVertex
        Me.m_capacity = capacity
    End Sub

    ''' <summary>
    ''' Given an end-node, Returns the other end-node (completes the edge)
    ''' </summary>
    ''' <param name="vertex"></param>
    ''' <returns></returns>
    Public Overridable Function getOtherEndNode(vertex As Integer) As Integer
        If vertex = fromVertex Then
            Return toVertex
        End If
        Return fromVertex
    End Function

    Public Overridable ReadOnly Property Capacity As Integer
        Get
            Return m_capacity
        End Get
    End Property

    Public Overridable ReadOnly Property Flow As Integer
        Get
            Return m_flow
        End Get
    End Property

    Public Overridable Function residualCapacityTo(vertex As Integer) As Integer
        If vertex = fromVertex Then
            Return m_flow
        End If
        Return m_capacity - m_flow
    End Function

    Public Overridable Sub increaseFlowTo(vertex As Integer, changeInFlow As Integer)
        If vertex = fromVertex Then
            m_flow = m_flow - changeInFlow
        Else
            m_flow = m_flow + changeInFlow
        End If
    End Sub

    ''' <summary>
    ''' Prints edge using Array indexes, not human readable ID's like "S" or "T"
    ''' </summary>
    ''' <returns></returns>
    Public Overrides Function ToString() As String
        Return "(" & fromVertex.ToString() & " --> " & toVertex.ToString() & ")"
    End Function
End Class

