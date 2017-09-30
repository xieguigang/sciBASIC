#Region "Microsoft.VisualBasic::238eccb892af3f4ace656cfc34d160df, ..\sciBASIC#\Data_science\DataMining\hierarchical-clustering\hierarchical-clustering\HierarchyBuilder\DistanceMap.vb"

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

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language

Namespace Hierarchy

    ''' <summary>
    ''' Container for linkages
    ''' with the minimal methods needed in the package
    ''' Created by Alexandre Masselot on 7/18/14.
    ''' </summary>
    Public Class DistanceMap

        Dim linkTable As New Dictionary(Of String, HierarchyLink)
        Dim data As New PriorityQueue(Of HierarchyLink)

        ''' <summary>
        ''' Peak into the minimum distance
        ''' @return
        ''' </summary>
        Public ReadOnly Property MinimalDistance() As Double
            Get
                Dim peek As HierarchyLink = data.Peek()

                If peek IsNot Nothing Then
                    Return peek.Tree.LinkageDistance
                Else
                    Return Nothing
                End If
            End Get
        End Property

        Sub New()
        End Sub

        Sub New(links As IEnumerable(Of HierarchyTreeNode))
            For Each x In links
                Dim link As New HierarchyLink(x)

                If Not linkTable.ContainsKey(link.HashKey) Then
                    Call linkTable.Add(link.HashKey, link)
                End If
            Next

            data = New PriorityQueue(Of HierarchyLink)(linkTable.Values)
        End Sub

        Public Function ToList() As IList(Of HierarchyTreeNode)
            Dim l As IList(Of HierarchyTreeNode) = New List(Of HierarchyTreeNode)
            For Each clusterPair As HierarchyLink In data
                l.Add(clusterPair.Tree)
            Next
            Return l
        End Function

        Public Function FindByCodePair(c1 As Cluster, c2 As Cluster) As HierarchyTreeNode
            Dim inCode As String = hashCodePair(c1, c2)
            Return linkTable(inCode).Tree
        End Function

        Public Function RemoveFirst() As HierarchyTreeNode
            Dim poll As HierarchyLink = data.Dequeue

            Do While poll IsNot Nothing AndAlso poll.removed
                poll = data.Dequeue
            Loop

            If poll Is Nothing Then
                Return Nothing
            Else
                With poll.Tree
                    Call linkTable.Remove(poll.HashKey)
                    Return .ref
                End With
            End If
        End Function

        Public Function Remove(link As HierarchyTreeNode) As Boolean
            Dim ___remove As HierarchyLink = linkTable.RemoveAndGet(hashCodePair(link))
            If ___remove Is Nothing Then
                Return False
            End If
            ___remove.removed = True
            data.Remove(___remove)
            Return True
        End Function

        Public Sub Sort()
            Call data.Sort()
        End Sub

        Public Function Add(link As HierarchyTreeNode, Optional direct As Boolean = False) As Boolean
            Dim hlink As New HierarchyLink(link)

            If linkTable.ContainsKey(hlink.HashKey) Then
                Dim existingItem As HierarchyLink = linkTable(hlink.HashKey)
#If DEBUG Then
                Call Console _
                    .Error _
                    .WriteLine("hashCode = " & existingItem.HashKey & " adding redundant link:" & link.ToString & " (exist:" & existingItem.ToString & ")")
#End If
                Return False
            Else
                Call linkTable.Add(hlink.HashKey, hlink)

                If Not direct Then
                    Call data.Enqueue(hlink)
                Else
                    Call data.Add(hlink)
                End If

                Return True
            End If
        End Function

        Public Overloads Function ToString() As String
            Return $"Have {linkTable.Count} linkage with minimal distance {MinimalDistance}"
        End Function
    End Class
End Namespace
