Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports sys = System.Math

Namespace ComponentModel.DataStructures.BinaryTree

    Public Module Extensions

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Add(Of T)(tree As BinaryTree(Of NamedValue(Of T)), node As NamedValue(Of T)) As TreeNode(Of NamedValue(Of T))
            Return tree.insert(node.Name, node)
        End Function

        Public Function NameCompare(a$, b$) As Integer
            Dim null1 = String.IsNullOrEmpty(a)
            Dim null2 = String.IsNullOrEmpty(b)

            If null1 AndAlso null2 Then
                Return 0
            ElseIf null1 Then
                Return -1
            ElseIf null2 Then
                Return 1
            ElseIf String.Equals(a, b, StringComparison.Ordinal) Then
                Return 0
            Else

                Dim minl = sys.Min(a.Length, b.Length)
                Dim c1, c2 As Char

                For i As Integer = 0 To minl - 1
                    c1 = Char.ToLower(a.Chars(i))
                    c2 = Char.ToLower(b.Chars(i))

                    If c1 <> c2 Then
                        Return c1.CompareTo(c2)
                    End If
                Next

                If a.Length < b.Length Then
                    Return -1
                Else
                    Return 1
                End If
            End If
        End Function
    End Module
End Namespace