#Region "Microsoft.VisualBasic::79a4285e7236b8627f55679ae444eacc, gr\network-visualization\network_layout\Orthogonal\Blocks.vb"

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

    '   Total Lines: 176
    '    Code Lines: 140 (79.55%)
    ' Comment Lines: 22 (12.50%)
    '    - Xml Docs: 13.64%
    ' 
    '   Blank Lines: 14 (7.95%)
    '     File Size: 7.62 KB


    '     Class Blocks
    ' 
    '         Function: blocks, blocksInternal, single_node
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.Orthogonal.util
Imports Microsoft.VisualBasic.ListExtensions
Imports std = System.Math
' 
'  To change this license header, choose License Headers in Project Properties.
'  To change this template file, choose Tools | Templates
'  and open the template in the editor.
' 

Namespace Orthogonal

    ''' 
    ''' <summary>
    ''' @author santi
    ''' </summary>
    Public Class Blocks

        Private Shared Function single_node() As Pair(Of Dictionary(Of Integer, IList(Of Integer)), Dictionary(Of Integer, IList(Of Integer)))
            ' special case:
            Dim blocks As New Dictionary(Of Integer, IList(Of Integer))()
            Dim cutNodes As New Dictionary(Of Integer, IList(Of Integer))()
            Dim l As New List(Of Integer)() From {1}
            VBDebugger.EchoLine("Blocks: special case of a graph with a single node.")
            blocks(0) = l
            Return New Pair(Of Dictionary(Of Integer, IList(Of Integer)), Dictionary(Of Integer, IList(Of Integer)))(blocks, cutNodes)
        End Function

        Public Shared Function blocks(graph As Integer()()) As Pair(Of Dictionary(Of Integer, IList(Of Integer)), Dictionary(Of Integer, IList(Of Integer)))
            If graph.Length = 1 Then
                Return single_node()
            Else
                Return blocksInternal(graph)
            End If
        End Function

        Private Shared Function blocksInternal(graph As Integer()()) As Pair(Of Dictionary(Of Integer, IList(Of Integer)), Dictionary(Of Integer, IList(Of Integer)))
            ' the following code assumes that nodes are numbered 1,2,... (i.e. they start at 1 not at 0!)
            ' this is because the algorithms uses node numebers and their negative to determine
            ' certain properties of the graph, and 0 and its negative are the same, so it wouldn't work
            Dim n = graph.Length
            Dim b = New Integer(n + 1 - 1) {} ' one label per edge in the spanning tree
            Dim p = New Integer(n + 1 - 1) {} ' this will store the spanning tree
            Dim d = New Integer(n + 1 - 1) {} ' the distance of each node to the root of the tree
            Dim A = New Boolean(n + 1 - 1) {} ' one per node in the graph
            Dim X As ISet(Of Integer) = New HashSet(Of Integer)()
            Dim R As ISet(Of Integer) = New HashSet(Of Integer)()
            Dim U As List(Of Integer) = New List(Of Integer)()
            Dim treeroot = 1

            For i = 1 To n
                b(i) = 0
                A(i) = False
                If i <> treeroot Then
                    R.Add(i)
                End If
                p(i) = -1
            Next
            '        T.add(treeroot);
            U.Add(treeroot)
            d(treeroot) = 0

            While U.Count > 0
                '            for(int i = 1;i<=n;i++) A[i] = false;

                Dim toAddInU As New List(Of Integer)()
                Dim v = U.PopAt(U.Count - 1) ' get the last element of U
                Dim lL = 0
                For z = 1 To n
                    If graph(v - 1)(z - 1) = 1 Then ' we have to subtract 1, since in 'graph' nodes start from 0
                        If R.Contains(z) Then
                            ' add (v,z) to T???
                            R.Remove(z)
                            toAddInU.Add(z)
                            p(z) = v
                            d(z) = d(v) + 1
                            b(z) = -z
                        End If
                    End If
                Next
                For z = 1 To n
                    If graph(v - 1)(z - 1) = 1 Then ' we have to subtract 1, since in 'graph' nodes start from 0
                        If U.Contains(z) Then
                            ' fundamental cycle detected:
                            Dim q = b(z)
                            b(z) = v

                            If q > 0 Then
                                A(q) = True
                            End If
                            lL = std.Max(lL, d(v) - d(p(z)))
                        End If
                    End If
                Next
                If lL > 0 Then
                    VBDebugger.EchoLine("  Consolidating...")

                    Dim k = v
                    Dim l = 0
                    While k <> treeroot AndAlso l < lL
                        If b(k) > 0 Then
                            A(b(k)) = True
                        End If
                        b(k) = v
                        k = p(k)
                        l += 1
                    End While
                    For t = 1 To n
                        If b(t) > 0 AndAlso A(b(t)) Then
                            b(t) = v
                        End If
                    Next
                End If
                CType(U, List(Of Integer)).AddRange(toAddInU)
                X.Add(v)
            End While

            ' Rename the blocks:
            Dim blockRenaming As Dictionary(Of Integer, Integer) = New Dictionary(Of Integer, Integer)()
            Dim trivialBlocks As IList(Of Integer) = New List(Of Integer)()
            Dim nextBlock = 1
            For i = 1 To n
                ' ignore
                If b(i) = 0 Then
                ElseIf b(i) = -i Then
                    trivialBlocks.Add(i)
                Else
                    If blockRenaming.ContainsKey(b(i)) Then
                        b(i) = blockRenaming(b(i))
                    Else
                        blockRenaming(b(i)) = nextBlock
                        b(i) = nextBlock
                        nextBlock += 1
                    End If
                End If
            Next
            For Each i In trivialBlocks
                b(i) = std.Min(Threading.Interlocked.Increment(nextBlock), nextBlock - 1)
            Next

            ' translate the blocks and cutedges:
            Dim lBlocks As Dictionary(Of Integer, IList(Of Integer)) = New Dictionary(Of Integer, IList(Of Integer))()
            Dim cutNodes As Dictionary(Of Integer, IList(Of Integer)) = New Dictionary(Of Integer, IList(Of Integer))()
            For i = 1 To n
                Dim nodeBlocks As IList(Of Integer) = New List(Of Integer)()
                'nodeBlocks.add(b[i]);
                For j = 1 To n
                    If p(j) = i Then
                        If Not nodeBlocks.Contains(b(j)) Then
                            nodeBlocks.Add(b(j))
                        End If
                    End If
                Next
                If p(i) > 0 Then
                    If Not nodeBlocks.Contains(b(i)) Then
                        nodeBlocks.Add(b(i))
                    End If
                End If

                For Each blockID In nodeBlocks
                    Dim block = lBlocks.TryGetValue(blockID)
                    If block Is Nothing Then
                        block = New List(Of Integer)()
                        lBlocks(blockID) = block
                    End If
                    block.Add(i - 1)
                Next
                If nodeBlocks.Count > 1 Then
                    cutNodes(i - 1) = nodeBlocks
                End If
            Next

            Return New Pair(Of Dictionary(Of Integer, IList(Of Integer)), Dictionary(Of Integer, IList(Of Integer)))(lBlocks, cutNodes)
        End Function
    End Class
End Namespace
