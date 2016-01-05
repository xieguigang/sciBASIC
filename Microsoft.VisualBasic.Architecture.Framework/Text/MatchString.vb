Imports Score = System.Collections.Generic.KeyValuePair(Of String, Double)

Namespace Text

    ''' <summary>
    ''' 这个模块仅作用于英文文本之上
    ''' </summary>
    Public Module FuzzyMatchString

        ''' <summary>
        ''' 两个参数字符串是否模糊等价？
        ''' </summary>
        ''' <param name="string1"></param>
        ''' <param name="string2"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Equals(string1 As String, string2 As String) As Boolean
            Dim Score As Double

            string1 = string1.Trim.ToUpper
            string2 = string2.Trim.ToUpper

            If String.Equals(string1, string2, StringComparison.OrdinalIgnoreCase) Then
                Return True
            End If

            Return InternalMatch(string1, string2, Score) OrElse InternalMatch(string2, string1, Score)
        End Function

        Private Function InternalMatch(Query As String, Subject As String, ByRef Score As Double) As Boolean
            Dim Tokens_a = Query.Replace(vbTab, " ").Replace("-", " ").Split
            Dim Tokens_b = Subject.Replace(vbTab, " ").Replace("-", " ").Split
            Dim d = System.Math.Abs(Tokens_a.Length - Tokens_b.Length) + 2

            For i As Integer = 0 To Tokens_b.Length - 1
                Dim idx = Array.IndexOf(Tokens_a, Tokens_b(i))
                If idx > -1 AndAlso System.Math.Abs(idx - i) < d Then
                    Score += 1
                End If
            Next

            Dim Succeeded As Boolean = Score >= System.Math.Max(Tokens_a.Length, Tokens_b.Length) * 0.8
            If Succeeded = False Then
                Score = -1
            End If

            Return Succeeded
        End Function

        ''' <summary>
        ''' 从一个不直接相等的字符串列表之中模糊的查找出匹配度最高的目标字符串
        ''' </summary>
        ''' <param name="query"></param>
        ''' <param name="List"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function StringSelection(query As String, List As Generic.IEnumerable(Of String)) As String

            Dim DirectMatches As String() = (From str As String
                                         In List
                                             Where String.Equals(str, query, StringComparison.OrdinalIgnoreCase)
                                             Select str).ToArray

            If Not DirectMatches.IsNullOrEmpty Then
                Return DirectMatches.First
            Else
                query = query.ToUpper
            End If

            Dim ChunkBuffer As Score() = New Score(List.Count - 1) {}

            For i As Integer = 0 To ChunkBuffer.Length - 1

                Dim str As String = List(i).ToUpper
                Dim Score As Double = 0

                Call InternalMatch(query, str, Score)

                ChunkBuffer(i) = New Score(List(i), Score)
            Next

            Dim LQuery = (From ScoreValue As Score
                      In ChunkBuffer
                          Where ScoreValue.Value > -1
                          Select strData = ScoreValue.Key, scoreValue = ScoreValue.Value
                          Order By scoreValue Descending).ToArray

            If LQuery.IsNullOrEmpty Then
                Return ""
            Else
                Return LQuery.First.strData
            End If
        End Function
    End Module
End Namespace