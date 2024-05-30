#Region "Microsoft.VisualBasic::cd018528b0b89cca303857b16ff9ac2d, Data_science\Graph\MST\DisjointSet.vb"

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

    '   Total Lines: 76
    '    Code Lines: 63 (82.89%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 13 (17.11%)
    '     File Size: 2.36 KB


    '     Class DisjointSet
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: findset
    ' 
    '         Sub: makeset, mergetrees, printArray, printSets, union
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace MinimumSpanningTree

    Friend Class DisjointSet

        Private parents As Integer()
        Private rank As Integer()

        Public Sub New(Optional max_heap As Integer = 4096)
            parents = New Integer(max_heap - 1) {}
            rank = New Integer(max_heap - 1) {}
        End Sub

        Public Sub makeset([set] As Integer)
            parents([set]) = [set]
            rank([set]) = 0
        End Sub

        Public Function findset(node As Integer) As Integer
            Dim root = node
            While root <> parents(root)
                root = parents(root)
            End While
            Dim j = parents(node) ' j == parent of 'node'
            While j <> root
                parents(node) = root
                node = j
                j = parents(node)
            End While
            Return root
        End Function

        Public Sub union(i As Integer, j As Integer)
            mergetrees(findset(i), findset(j))
        End Sub

        Private Sub mergetrees(firstTree As Integer, secondTree As Integer)
            If rank(firstTree) < rank(secondTree) Then
                parents(firstTree) = secondTree
            ElseIf rank(firstTree) > rank(secondTree) Then
                parents(secondTree) = firstTree
            Else
                parents(firstTree) = secondTree
                rank(secondTree) += 1
            End If
        End Sub

        Public Sub printSets()
            Dim djSets As DJSets = New DJSets()

            Dim i = 0

            While i < parents.Length
                If parents(i) <> 0 Then
                    Dim root = findset(i)
                    If Not djSets.inDJSets(root) Then
                        djSets.addSet(root)
                        djSets.insertIntoSet(root, i)
                    Else
                        djSets.insertIntoSet(root, i)
                    End If
                End If

                Threading.Interlocked.Increment(i)
            End While
            djSets.printSets()
        End Sub

        Public Sub printArray()
            For i = 0 To parents.Length - 1
                Console.Write("{0} ", parents(i))
            Next
            Console.Write(Microsoft.VisualBasic.Constants.vbLf)
        End Sub
    End Class

End Namespace
