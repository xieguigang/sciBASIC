Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Score = System.Collections.Generic.KeyValuePair(Of String, Double)

Namespace Text.Similarity

    <PackageNamespace("String.FuzzyMatch")>
    Public Module FuzzyMatchString

        ''' <summary>
        ''' 忽略大小写
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        Private Function __fuzzyCharEquals(a As Char, b As Char) As Boolean
            Return Char.ToUpper(a) = Char.ToUpper(b)
        End Function

        Public Function __exactlyCharEquals(a As Char, b As Char) As Boolean
            Return a = b
        End Function

        ''' <summary>
        ''' 两个参数字符串是否模糊等价？
        ''' </summary>
        ''' <param name="string1"></param>
        ''' <param name="string2"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("Equals")>
        Public Function Equals(string1 As String, string2 As String, Optional exactly As Boolean = False, Optional cut As Double = 0.8) As Boolean
            If exactly Then
                If String.Equals(string1.Trim.ShadowCopy(string1),  ' 由于是fuzzy，所以即使是exactly，也会将空格去除
                                 string2.Trim.ShadowCopy(string2),
                                 StringComparison.OrdinalIgnoreCase) Then
                    Return True
                End If
            Else
                If String.Equals(string1.Trim.ToUpper.ShadowCopy(string1),
                                 string2.Trim.ToUpper.ShadowCopy(string2),
                                 StringComparison.Ordinal) Then
                    Return True
                End If
            End If

            Dim levEquals As Equals(Of Char) = GetCharEquals(exactly)
            Return __matched(string1, string2, cut, levEquals) OrElse
                __matched(string2, string1, cut, levEquals)
        End Function

        Private Function __matched(query As String, subject As String, ByRef cut As Double, charEqual As Equals(Of Char)) As Boolean
            Dim edits = LevenshteinDistance.ComputeDistance(query.ToArray, subject.ToArray, charEqual, Function(x) x)
            Dim matched As Boolean = edits.MatchSimilarity >= cut
            cut = edits.MatchSimilarity
            Return matched
        End Function

        <ExportAPI("CharEquals")>
        Public Function GetCharEquals(exactly As Boolean) As Equals(Of Char)
            Dim levEquals As Equals(Of Char)

            If exactly Then
                levEquals = AddressOf __exactlyCharEquals
            Else
                levEquals = AddressOf __fuzzyCharEquals
            End If

            Return levEquals
        End Function

        ''' <summary>
        ''' 从一个不直接相等的字符串列表之中模糊的查找出匹配度最高的目标字符串
        ''' </summary>
        ''' <param name="query"></param>
        ''' <param name="List"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("Select")>
        Public Function StringSelection(query As String, List As IEnumerable(Of String), Optional exactly As Boolean = False, Optional cut As Double = 0.8) As String
            Dim directMatches As String = (From str As String
                                           In List
                                           Where String.Equals(str, query, StringComparison.OrdinalIgnoreCase)
                                           Select str).FirstOrDefault

            If Not String.IsNullOrEmpty(directMatches) Then
                Return directMatches
            Else
                query = query.ToUpper
            End If

            Dim result As Score() = New Score(List.Count - 1) {}
            Dim levEquals As Equals(Of Char) = GetCharEquals(exactly)

            For i As Integer = 0 To result.Length - 1
                Dim str As String = List(i).ToUpper
                Dim Score As Double = 0

                Call __matched(query, str, Score, levEquals)

                result(i) = New Score(List(i), Score)
            Next

            Dim LQuery = (From ScoreValue As Score
                          In result
                          Where ScoreValue.Value > -1 AndAlso
                              ScoreValue.Value >= cut
                          Select strData = ScoreValue.Key,
                              scoreValue = ScoreValue.Value
                          Order By scoreValue Descending).ToArray

            If LQuery.IsNullOrEmpty Then
                Return ""
            Else
                Return LQuery.First.strData
            End If
        End Function
    End Module
End Namespace