Namespace ComponentModel.Algorithm.base

    Public Class Permutation(Of tT As IComparable)

        Private dataField As tT()
        Private K As Integer = -1
        Private L As Integer = -1
        Private first As Boolean = True

        Public ReadOnly Property CanPermute As Boolean
            Get
                Return first OrElse K >= 0
            End Get
        End Property

        Public ReadOnly Property Data As tT()
            Get
                Return dataField
            End Get
        End Property

        Public Sub New(data As tT())
            dataField = data
            Array.Sort(dataField)
        End Sub

        Private Sub FindIndices()
            Dim k = 0

            Me.K = -1

            While k + 1 < dataField.Length
                If dataField(k).CompareTo(dataField(k + 1)) < 0 Then
                    Me.K = k
                End If

                k += 1
            End While

            If Me.K >= 0 Then
                For L = Me.K + 1 To dataField.Length - 1

                    If dataField(Me.K).CompareTo(dataField(L)) < 0 Then
                        Me.L = L
                    End If
                Next
            End If
        End Sub

        Private Sub Reverse(a As Integer, b As Integer)
            While a < b
                Swap(a, b)
                a += 1
                b -= 1
            End While
        End Sub

        Private Sub Swap(a As Integer, b As Integer)
            Dim t = dataField(a)
            dataField(a) = dataField(b)
            dataField(b) = t
        End Sub

        Public Sub Permutate()
            If first Then
                first = False
                FindIndices()
                Return
            End If

            If Not CanPermute Then
                Return
            End If

            Swap(K, L)
            Reverse(K + 1, dataField.Length - 1)
            FindIndices()
        End Sub
    End Class
End Namespace
