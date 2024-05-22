Namespace MinimumSpanningTree

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

End Namespace