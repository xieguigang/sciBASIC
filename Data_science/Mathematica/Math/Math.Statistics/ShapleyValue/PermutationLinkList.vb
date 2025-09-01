Namespace ShapleyValue

    Public Class PermutationLinkList

        Public Overrides Function ToString() As String
            Dim res As String = "PermutationLinkList [root=" & root.ToString() & "]"
            Return res

        End Function

        Private root As Node
        Private permutationRangeField As Long
        Private permutationSize As Long

        Private ReadOnly Property LastNode As Node
            Get
                Dim tempNode = root
                While tempNode.NextNode IsNot Nothing
                    'System.out.println("tempNode "+tempNode);
                    tempNode = tempNode.NextNode
                End While
                Return tempNode
            End Get
        End Property

        Public Sub New(size As Integer)
            permutationRangeField = 0
            permutationSize = size

            Dim ints As IList(Of Integer) = New List(Of Integer)()
            For i = 1 To size
                ints.Add(i)
            Next
            Dim nodeValue As NodeValue = New NodeValue(ints)
            'System.out.println(nodeValue);
            root = New Node(nodeValue, Nothing)
        End Sub

        Public Overridable ReadOnly Property NextPermutation As IList(Of Integer)
            Get
                If root Is Nothing Then
                    Return New List(Of Integer)()
                End If

                Dim list As IList(Of Integer) = New List(Of Integer)()

                Dim ints As IList(Of Integer) = New List(Of Integer)()
                For i As Integer = 1 To permutationSize
                    ints.Add(i)
                Next

                Dim node = root
                While node IsNot Nothing
                    'System.out.println(node.getValue().getValue());
                    list.Add(node.Value.Value)
                    node = node.NextNode
                End While






                Dim currentNode = LastNode
                'System.out.println("lastNode ="+currentNode);
                Dim previousNode = currentNode.PrevNode


                While previousNode IsNot Nothing AndAlso currentNode.Value.NextValues.Count = 0
                    ' if(previousNode!=null) {
                    ' 						//System.out.println("ffo");
                    ' 						ints.remove(previousNode.getValue().getValue());
                    ' 					}	
                    currentNode = previousNode
                    previousNode = currentNode.PrevNode
                End While
                'System.out.println("currentNode ="+currentNode);



                If currentNode.Value.NextValues.Count > 0 Then
                    currentNode.updateValue()
                    ints.RemoveAt(currentNode.Value.Value)
                    Dim tempNode = currentNode.PrevNode
                    While tempNode IsNot Nothing
                        ints.RemoveAt(tempNode.Value.Value)
                        tempNode = tempNode.PrevNode
                    End While
                    'System.out.println("ints"+ints);

                    Dim nodeValue As NodeValue = New NodeValue(ints)
                    Dim newNode As Node = New Node(nodeValue, currentNode)
                    currentNode.NextNode = newNode
                Else
                    root = Nothing
                End If



                permutationRangeField += 1

                If permutationRangeField Mod 1000000 = 0 Then
                    'System.out.println(permutationRange);
                    ' logger.debug("percentage calculation {}", 100 * permutationRange / FactorialUtil.factorial(permutationSize));
                End If
                Return list
            End Get
        End Property

        Public Overridable ReadOnly Property PermutationRange As Long
            Get
                Return permutationRangeField
            End Get
        End Property


    End Class

End Namespace
