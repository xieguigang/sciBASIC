#Region "Microsoft.VisualBasic::6d96cd2f21ae792a271c09513e8f2bfa, Data_science\Mathematica\Math\Math.Statistics\ShapleyValue\PermutationLinkList.vb"

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

    '   Total Lines: 101
    '    Code Lines: 75 (74.26%)
    ' Comment Lines: 4 (3.96%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 22 (21.78%)
    '     File Size: 3.30 KB


    '     Class PermutationLinkList
    ' 
    '         Properties: LastNode, NextPermutation, PermutationRange
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: MakeNextPermutation, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ShapleyValue

    Public Class PermutationLinkList

        Private root As Node
        Private permutationSize As Long

        Public Overridable ReadOnly Property PermutationRange As Long

        Private ReadOnly Property LastNode As Node
            Get
                Dim tempNode = root

                While tempNode.NextNode IsNot Nothing
                    tempNode = tempNode.NextNode
                End While

                Return tempNode
            End Get
        End Property

        Public Sub New(size As Integer)
            PermutationRange = 0
            permutationSize = size

            Dim ints As New List(Of Integer)()
            For i As Integer = 1 To size
                Call ints.Add(i)
            Next
            Dim nodeValue As New NodeValue(ints)
            root = New Node(nodeValue, Nothing)
        End Sub

        Public Overridable ReadOnly Property NextPermutation As IList(Of Integer)
            Get
                If root Is Nothing Then
                    Return New List(Of Integer)()
                Else
                    Return MakeNextPermutation()
                End If
            End Get
        End Property

        Private Function MakeNextPermutation() As IList(Of Integer)
            Dim list As New List(Of Integer)()
            Dim ints As New List(Of Integer)()

            For i As Integer = 1 To permutationSize
                Call ints.Add(i)
            Next

            Dim node As Node = root

            While node IsNot Nothing
                list.Add(node.Value.Value)
                node = node.NextNode
            End While

            Dim currentNode = LastNode
            Dim previousNode = currentNode.PrevNode

            While previousNode IsNot Nothing AndAlso currentNode.Value.NextValues.Count = 0
                ' if(previousNode!=null) {
                ' 						//System.out.println("ffo");
                ' 						ints.remove(previousNode.getValue().getValue());
                ' 					}	
                currentNode = previousNode
                previousNode = currentNode.PrevNode
            End While

            If currentNode.Value.NextValues.Count > 0 Then
                currentNode.updateValue()
                ints.RemoveAt(currentNode.Value.Value)
                Dim tempNode = currentNode.PrevNode
                While tempNode IsNot Nothing
                    ints.RemoveAt(tempNode.Value.Value)
                    tempNode = tempNode.PrevNode
                End While

                Dim nodeValue As New NodeValue(ints)
                Dim newNode As New Node(nodeValue, currentNode)
                currentNode.NextNode = newNode
            Else
                root = Nothing
            End If

            _PermutationRange += 1

            If PermutationRange Mod 1000000 = 0 Then
                Call $"percentage calculation {100 * PermutationRange / FactorialUtil.factorial(permutationSize)}".debug
            End If

            Return list
        End Function

        Public Overrides Function ToString() As String
            Return "PermutationLinkList [root=" & root.ToString() & "]"
        End Function
    End Class

End Namespace
