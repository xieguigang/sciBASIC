
Namespace CCL

    Friend Class Label

        Public Property Name As Integer
        Public Property Root As Label
        Public Property Rank As Integer

        Public Sub New(Name As Integer)
            Me.Name = Name
            Root = Me
            Rank = 0
        End Sub

        Public Function GetRoot() As Label
            If Root IsNot Me Then
                Root = Root.GetRoot() ' 路径压缩
            End If
            Return Root
        End Function

        Public Sub Join(other As Label)
            Dim root1 = Me.GetRoot()
            Dim root2 = other.GetRoot()

            If root1.Name = root2.Name Then
                Return ' 已在同一集合
            End If

            ' 按秩合并
            If root1.Rank < root2.Rank Then
                root1.Root = root2
            ElseIf root1.Rank > root2.Rank Then
                root2.Root = root1
            Else
                root2.Root = root1
                root1.Rank += 1
            End If
        End Sub

        Public Overrides Function Equals(obj As Object) As Boolean
            Dim other = TryCast(obj, Label)

            If other Is Nothing Then
                Return False
            End If

            Return other.Name = Name
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return Name
        End Function

        Public Overrides Function ToString() As String
            Return Name.ToString()
        End Function
    End Class
End Namespace
