
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
