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

        ''' <summary>
        ''' 字符串名字的比较规则：
        ''' 
        ''' 假若字符串是空值或者空字符串，则该变量小
        ''' 假若字符串相等（忽略大小写），则变量值一样
        ''' 最后逐个字符进行比较，按照字母的charcode大小来比较，第一个charcode大的变量大
        ''' </summary>
        ''' <param name="a$"></param>
        ''' <param name="b$"></param>
        ''' <returns></returns>
        Public Function NameCompare(a$, b$) As Integer
            Dim null1 = String.IsNullOrEmpty(a)
            Dim null2 = String.IsNullOrEmpty(b)

            If null1 AndAlso null2 Then
                Return 0
            ElseIf null1 Then
                Return -1
            ElseIf null2 Then
                Return 1
            ElseIf String.Equals(a, b, StringComparison.OrdinalIgnoreCase) Then
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

        ''' <summary>
        ''' The term index search engine.
        ''' 
        ''' + If the string similarity less than threshold, then will returns negative value
        ''' + If the string similarity greater than threshold, then will returns positive value
        ''' + If the string text equals to other, then will reutrns ZERO
        ''' </summary>
        ''' <param name="a$"></param>
        ''' <param name="b$"></param>
        ''' <returns></returns>
        Public Function NameFuzzyMatch(a$, b$) As Integer
            Dim similarity = Text.Levenshtein.ComputeDistance(a, b)

            If a.TextEquals(b) Then
                Return 0
            ElseIf similarity Is Nothing Then
                Return -1
            ElseIf similarity.MatchSimilarity < 0.6 Then
                Return -1
            Else
                Return 1
            End If
        End Function
    End Module
End Namespace