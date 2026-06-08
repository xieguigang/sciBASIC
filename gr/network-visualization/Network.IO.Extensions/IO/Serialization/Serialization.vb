#Region "Microsoft.VisualBasic::df78ceb56cd85c629caa857ddb6bbe12, gr\network-visualization\Network.IO.Extensions\IO\Serialization\Serialization.vb"

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

    '   Total Lines: 64
    '    Code Lines: 53 (82.81%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 11 (17.19%)
    '     File Size: 2.09 KB


    ' Module Serialization
    ' 
    '     Function: (+2 Overloads) Load
    ' 
    '     Sub: (+2 Overloads) Dump
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Imaging

Public Module Serialization

    <Extension>
    Public Sub Dump(g As NetworkGraph, s As Stream)
        Using file As New BinaryDataWriter(s) With {.ByteOrder = ByteOrder.BigEndian}
            Call g.Dump(file)
        End Using
    End Sub

    <Extension>
    Public Sub Dump(g As NetworkGraph, file As BinaryDataWriter)
        Call file.Write(g.vertex.Count)
        Call file.Write(g.graphEdges.Count)
        Call file.Write(If(g.id, ""))
        Call file.Write(If(g.name, ""))

        For Each v As Graph.Node In g.vertex
            Call v.SaveOneNode(file)
        Next
        For Each edge As Graph.Edge In g.graphEdges
            Call edge.SaveOneEdge(file)
        Next
    End Sub

    Public Function Load(s As Stream) As NetworkGraph
        Using file As New BinaryDataReader(s) With {.ByteOrder = ByteOrder.BigEndian}
            Return Load(file)
        End Using
    End Function

    Public Function Load(file As BinaryDataReader) As NetworkGraph
        Dim g As New NetworkGraph
        Dim nv As Integer = file.ReadInt32
        Dim ne As Integer = file.ReadInt32

        g.id = file.ReadString
        g.name = file.ReadString

        For Each v As Graph.Node In NodeFile.ReadNode(file, nv)
            Call g.AddNode(v, assignId:=False)
        Next
        For Each e As NetworkEdge In EdgeFile.ReadNetwork(file, ne)
            Dim u = g.GetElementByID(e.fromNode)
            Dim v = g.GetElementByID(e.toNode)

            Call g.CreateEdge(u, v, e.value, New EdgeData With {
                .length = Val(e!length),
                .label = e!label,
                .style = New Pen(e!color.ToColor, Val(e!width)),
                .Properties = e.Properties
            })
        Next

        Return g
    End Function

End Module

