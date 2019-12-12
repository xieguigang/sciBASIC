Imports Microsoft.VisualBasic.ComponentModel.Algorithm.BinaryTree
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.DynamicProgramming.Levenshtein
Imports Microsoft.VisualBasic.Language

Namespace Data.Trinity

    ''' <summary>
    ''' 使用模拟比较来建立所给定的字符串的快速模糊查找的索引对象模型
    ''' </summary>
    Public Class WordSimilarityIndex

        ''' <summary>
        ''' 二叉树的主键是字符串的ASCII编码缓存
        ''' </summary>
        ReadOnly bin As AVLTree(Of Integer(), String)

        Sub New(similarity As WordSimilarity)
            bin = New AVLTree(Of Integer(), String)(AddressOf similarity.CompareAsciiVector, AddressOf asciiToString)
        End Sub

        Private Shared Function asciiToString(ascii As Integer()) As String
            Return ascii.Select(AddressOf ChrW).CharString
        End Function

    End Class

    Public Class WordSimilarity : Implements IEqualityComparer(Of Integer())

        ReadOnly equalsThreshold, right As Double

        Sub New(Optional equals As Double = 0.85, Optional right As Double = 0.6)
            Me.equalsThreshold = equals
            Me.right = right
        End Sub

        Public Function CompareAsciiVector(x As Integer(), y As Integer()) As Integer
            With LevenshteinDistance.ComputeDistance(x, y, Function(a, b) a = b, Function(c) ChrW(c))
                If .IsNothing Then
                    Return -1
                ElseIf .MatchSimilarity >= equalsThreshold Then
                    Return 0
                Else
                    Return 1
                End If
            End With
        End Function

        Public Overloads Function GetHashCode(obj() As Integer) As Integer Implements IEqualityComparer(Of Integer()).GetHashCode
            Return obj.GetHashCode
        End Function

        Private Function IEqualityComparer_Equals(x() As Integer, y() As Integer) As Boolean Implements IEqualityComparer(Of Integer()).Equals
            With LevenshteinDistance.ComputeDistance(x, y, Function(a, b) a = b, Function(c) ChrW(c))
                Return?.MatchSimilarity >= Me.equalsThreshold
            End With
        End Function
    End Class
End Namespace