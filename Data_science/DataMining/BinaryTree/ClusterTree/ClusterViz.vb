#Region "Microsoft.VisualBasic::cf573c109d51df442ee5bcfacb95d813, Data_science\DataMining\BinaryTree\ClusterTree\ClusterViz.vb"

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

    '   Total Lines: 61
    '    Code Lines: 41 (67.21%)
    ' Comment Lines: 11 (18.03%)
    '    - Xml Docs: 81.82%
    ' 
    '   Blank Lines: 9 (14.75%)
    '     File Size: 2.37 KB


    ' Module ClusterViz
    ' 
    '     Function: EmptyMetadata, MakeTreeGraph
    ' 
    '     Sub: PullTreeGraph
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph

Public Module ClusterViz

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="tree"></param>
    ''' <param name="metadata">
    ''' there are some special metadata key:
    ''' 
    ''' 1. text - for node data label
    ''' 2. value - for node data mass weight
    ''' </param>
    ''' <returns></returns>
    <Extension>
    Public Function MakeTreeGraph(tree As BTreeCluster, Optional metadata As Func(Of String, Dictionary(Of String, String)) = Nothing) As NetworkGraph
        Dim g As New NetworkGraph
        Call tree.PullTreeGraph(g, If(metadata, EmptyMetadata()))
        Return g
    End Function

    Private Function EmptyMetadata() As Func(Of String, Dictionary(Of String, String))
        Return Function(any) New Dictionary(Of String, String)
    End Function

    <Extension>
    Private Sub PullTreeGraph(tree As BTreeCluster, g As NetworkGraph, metadata As Func(Of String, Dictionary(Of String, String)))
        Dim root As Node = g.CreateNode(tree.uuid)

        root.data.Add(metadata(root.label))
        root.data.Add(NamesOf.REFLECTION_ID_MAPPING_NODETYPE, root.label)
        root.data.label = root.data.Properties.Popout("text")
        root.data.mass = Val(root.data.Properties.Popout("value"))

        For Each id As String In tree.members
            If id <> root.label Then
                Dim v As Node = g.CreateNode(id)

                v.data.Add(metadata(id))
                v.data.Add(NamesOf.REFLECTION_ID_MAPPING_NODETYPE, root.label)
                v.data.label = v.data.Properties.Popout("text")
                v.data.mass = Val(v.data.Properties.Popout("value"))

                g.CreateEdge(root, v)
            End If
        Next

        If tree.left IsNot Nothing Then
            Call tree.left.PullTreeGraph(g, metadata)
            Call g.CreateEdge(root, g.GetElementByID(tree.left.uuid))
        End If
        If tree.right IsNot Nothing Then
            Call tree.right.PullTreeGraph(g, metadata)
            Call g.CreateEdge(root, g.GetElementByID(tree.right.uuid))
        End If
    End Sub
End Module

