
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
            Dim thisObj = Me
            Dim root = Me.Root

            While thisObj IsNot root
                thisObj = root
                root = root.Root
            End While

            Me.Root = root
            Return Me.Root
        End Function

        Public Sub Join(root2 As Label)
            If root2.Rank < Rank Then 'is the rank of Root2 less than that of Root1 ?
                root2.Root = Me 'yes! then Root1 is the parent of Root2 (since it has the higher rank)
                'rank of Root2 is greater than or equal to that of Root1
            Else
                Root = root2 'make Root2 the parent
                If Rank = root2.Rank Then 'both ranks are equal ?
                    root2.Rank += 1 'increment Root2, we need to reach a single root for the whole tree
                End If
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
