Imports System
Imports System.Collections.Generic

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

    Friend Class DJSet
        Private rootField As Integer
        Private [set] As HashSet(Of Integer)

        Public Sub New(root As Integer)
            [set] = New HashSet(Of Integer)()
            rootField = root
        End Sub

        Public Sub add(i As Integer)
            [set].Add(i)
        End Sub


        Public Property Root As Integer
            Get
                Return rootField
            End Get
            Set(value As Integer)
                rootField = value
            End Set
        End Property

        Public Sub print()
            Dim firstTime = True

            Console.Write("{")
            For Each i In [set]
                If firstTime Then
                    firstTime = False
                    Console.Write(i)
                Else
                    Console.Write(",{0}", i)
                End If
            Next
            Console.Write("}")
        End Sub
    End Class

    Friend Class DJSets
        Private djSets As List(Of DJSet)

        Public Sub New()
            djSets = New List(Of DJSet)()
        End Sub

        Public Sub printSets()
            Dim firstTime = True
            For Each djSet In djSets
                If firstTime Then
                    firstTime = False
                    djSet.print()
                Else
                    Console.Write(",  ")
                    djSet.print()
                End If
            Next
            Console.WriteLine()
        End Sub

        Public Sub addSet(root As Integer)
            djSets.Add(New DJSet(root))
        End Sub

        Public Function inDJSets(root As Integer) As Boolean
            For Each djSet In djSets
                If djSet.Root = root Then
                    Return True
                End If
            Next
            Return False
        End Function

        Public Sub insertIntoSet(root As Integer, i As Integer)
            For Each djSet In djSets
                If djSet.Root = root Then
                    djSet.add(i)
                End If
            Next
        End Sub
    End Class
End Namespace
