Namespace MinimumSpanningTree

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