Imports System.Runtime.CompilerServices

Namespace ComponentModel.Algorithm.BinaryTree

    Public Module Extensions

        <Extension>
        Public Function Find(Of K, V)(tree As BinaryTree(Of K, V), key As K, compares As Comparison(Of K)) As BinaryTree(Of K, V)
            Do While Not tree Is Nothing
                Select Case compares(key, tree.Key)
                    Case < 0 : tree = tree.Left
                    Case > 0 : tree = tree.Right
                    Case = 0
                        Return tree
                End Select
            Loop

            Return Nothing
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function HasKey(Of K, V)(tree As BinaryTree(Of K, V), key As K, compares As Comparison(Of K)) As Boolean
            Return Not tree.Find(key, compares) Is Nothing
        End Function

        <Extension>
        Public Iterator Function TakeRange(Of K, V)(tree As BinaryTree(Of K, V), min As K, max As K, compares As Comparison(Of K)) As IEnumerable(Of Map(Of K, V))
            Do While Not tree Is Nothing
                Dim compare = (
                    min:=compares(min, tree.Key),
                    max:=compares(max, tree.Key)
                )

                If compare.min < 0 Then
                    tree = tree.Left
                ElseIf compare.min <= 0 AndAlso compare.max >= 0 Then
                    Yield New Map(Of K, V)(tree.Key, tree.Value)

                    For Each map In tree.Left.TakeRange(min, max, compares)
                        Yield map
                    Next
                    For Each map In tree.Right.TakeRange(min, max, compares)
                        Yield map
                    Next

                ElseIf compare.min > 0 OrElse compare.max > 0 Then
                    tree = tree.Right
                End If
            Loop
        End Function

        <Extension>
        Public Function Min(Of K, V)(tree As BinaryTree(Of K, V)) As BinaryTree(Of K, V)
            Do While Not tree Is Nothing
                If tree.Left Is Nothing Then
                    Return tree
                Else
                    tree = tree.Left
                End If
            Loop

            Return Nothing
        End Function

        <Extension>
        Public Function Max(Of K, V)(tree As BinaryTree(Of K, V)) As BinaryTree(Of K, V)
            Do While Not tree Is Nothing
                If tree.Right Is Nothing Then
                    Return tree
                Else
                    tree = tree.Right
                End If
            Loop

            Return Nothing
        End Function

        <Extension>
        Public Function MinKey(Of K, V)(tree As BinaryTree(Of K, V)) As K
            Dim min = tree.Min
            Return If(min Is Nothing, Nothing, min.Key)
        End Function

        <Extension>
        Public Function MaxKey(Of K, V)(tree As BinaryTree(Of K, V)) As K
            Dim max = tree.Max
            Return If(max Is Nothing, Nothing, max.Key)
        End Function
    End Module
End Namespace