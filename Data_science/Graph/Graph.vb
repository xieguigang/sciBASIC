#Region "Microsoft.VisualBasic::77c0a0dc940244d91e3fc03d4f0d5d5a, ..\sciBASIC#\gr\Datavisualization.Network\Graph\Graph.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2016 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository

''' <summary>
''' A graph ``G = (V, E)`` consists of a set V of vertices and a set E edges, that is, unordered
''' pairs Of vertices. Unless explicitly stated otherwise, we assume that the graph Is simple,
''' that Is, it has no multiple edges And no self-loops.
''' </summary>
Public Class Graph

#Region "Let G=(V, E) be a simple graph"
    Dim edges As Dictionary(Of Edge)
    Dim vertices As Dictionary(Of Vertex)
    Dim buffer As HandledList(Of Vertex)
#End Region

    Public Function AddEdge(u As Vertex, v As Vertex) As Graph
        edges += New Edge With {
            .U = u,
            .V = v
        }
        If Not vertices.ContainsKey(u.Label) Then
            vertices += u
            buffer.Add(u)
        End If
        If Not vertices.ContainsKey(v.Label) Then
            vertices += v
            buffer.Add(v)
        End If

        Return Me
    End Function

    Public Function AddEdge(i%, j%) As Graph
        edges += New Edge With {
            .U = buffer(i),
            .V = buffer(j)
        }

        Return Me
    End Function

    Public Function AddEdge(u$, v$) As Graph
        edges += New Edge With {
            .U = vertices(u),
            .V = vertices(v)
        }

        Return Me
    End Function

    ''' <summary>
    ''' 只会删除边，并不会删除节点<paramref name="U"/>和<paramref name="V"/>
    ''' </summary>
    ''' <param name="U"></param>
    ''' <param name="V"></param>
    ''' <returns></returns>
    Public Function Delete(U As Vertex, V As Vertex) As Graph
        Return Delete(U.ID, V.ID)
    End Function

    Public Function Delete(u$, v$) As Graph
        Return Delete(vertices(u).ID, vertices(v).ID)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Delete(u%, v%) As Graph
        Dim key$ = $"{u}-{v}"

        If edges.ContainsKey(key) Then
            Call edges.Remove(key)
        End If

        Return Me
    End Function
End Class

''' <summary>
''' 图之中的节点
''' </summary>
Public Class Vertex : Implements INamedValue
    Implements IAddressOf

    <XmlAttribute> Public Property Label As String Implements IKeyedEntity(Of String).Key
    <XmlAttribute> Public Property ID As Integer Implements IAddress(Of Integer).Address

    Public Overrides Function ToString() As String
        Return $"({ID}) {Label}"
    End Function
End Class

''' <summary>
''' 节点之间的边
''' </summary>
Public Class Edge : Implements INamedValue

    Public Property U As Vertex
    Public Property V As Vertex

    ''' <summary>
    ''' ReadOnly unique-ID
    ''' </summary>
    ''' <returns></returns>
    ''' 
    Private Property Key As String Implements IKeyedEntity(Of String).Key
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return $"{U.ID}-{V.ID}"
        End Get
        Set(value As String)
            ' DO Nothing
        End Set
    End Property

    Public Overrides Function GetHashCode() As Integer
        Return Key.GetHashCode
    End Function

    Public Overrides Function ToString() As String
        Return $"({GetHashCode()}) {U} => {V}"
    End Function
End Class
