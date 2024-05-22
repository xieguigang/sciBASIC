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
