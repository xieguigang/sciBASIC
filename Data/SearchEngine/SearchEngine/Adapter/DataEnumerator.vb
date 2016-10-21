Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Data.csv.DocumentStream
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text

Public Module DataEnumerator

    <Extension>
    Public Iterator Function [Where](df As DataFrame, query$) As IEnumerable(Of Dictionary(Of String, String))
        Dim exp As Expression = query.Build
        Dim def As New IObject(df.SchemaOridinal.Keys)

        For Each row In df.EnumerateData
            If exp.Evaluate(def, row) Then
                Yield row
            End If
        Next
    End Function

    ''' <summary>
    ''' 直接取出前n个
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="query$"></param>
    ''' <param name="n%"></param>
    ''' <returns></returns>
    <Extension>
    Public Iterator Function Limit(Of T)(source As IEnumerable(Of T), query$, n%,
                                         Optional anyDefault As Tokens = Tokens.op_OR,
                                         Optional allowInStr As Boolean = True,
                                         Optional caseSensitive As Boolean = False) As IEnumerable(Of T)
        Dim exp As Expression = query.Build(anyDefault, allowInStr, caseSensitive)
        Dim type As Type = GetType(T)
        Dim def As New IObject(type)

        For Each x As T In source
            If exp.Evaluate(def, x) Then
                Yield x

                If n > 0 Then
                    n -= 1
                Else
                    Exit For
                End If
            End If
        Next
    End Function

    ''' <summary>
    ''' 排序之后取得分最高的前n个
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="query$"></param>
    ''' <param name="n%"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Top(Of T)(source As IEnumerable(Of T), query$, n%,
                              Optional anyDefault As Tokens = Tokens.op_OR,
                              Optional allowInStr As Boolean = True,
                              Optional caseSensitive As Boolean = False) As IEnumerable(Of T)
        Dim exp As Expression = query.Build(anyDefault, allowInStr, caseSensitive)
        Dim type As Type = GetType(T)
        Dim def As New IObject(type)
        Dim LQuery = LinqAPI.Exec(Of Match) <=
 _
            From x As T
            In source.AsParallel
            Let result As Match = exp.Evaluate(def, x)
            Where result.Success
            Select result
            Order By result.score Descending

        Return LQuery _
            .Take(n) _
            .Select(Function(x) DirectCast(x.x, T))
    End Function

    Const TOPregexp = "TOP\s+\d+\s*$"
    Const LIMITregexp = "LIMIT\s+\d+\s*$"

    ''' <summary>
    ''' 这个函数可以接受``LIMIT``和``TOP``参数
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <returns></returns>
    Public Function Execute(Of T)(source As IEnumerable(Of T), query$,
                                  Optional anyDefault As Tokens = Tokens.op_OR,
                                  Optional allowInStr As Boolean = True,
                                  Optional caseSensitive As Boolean = False) As IEnumerable(Of T)
        Dim m As New Value(Of System.Text.RegularExpressions.Match)

        If (m = Regex.Match(query, TOPregexp, RegexOptions.Multiline)).Success Then

            Dim n% = CInt((+m).Value.Trim(" "c, ASCII.TAB).Split.Last)
            query = Mid(query, 1, query.Length - (+m).Value.Length)
            Return Top(source, query$, n%, anyDefault, allowInStr, caseSensitive)

        ElseIf (m = Regex.Match(query, LIMITregexp, RegexOptions.Multiline)).Success Then

            Dim n% = CInt((+m).Value.Trim(" "c, ASCII.TAB).Split.Last)
            query = Mid(query, 1, query.Length - (+m).Value.Length)
            Return Limit(source, query$, n%, anyDefault, allowInStr, caseSensitive)

        Else
            Dim exp As Expression = query.Build(anyDefault, allowInStr, caseSensitive)
            Dim type As Type = GetType(T)
            Dim def As New IObject(type)

            Return From x As T
                   In source
                   Where exp.Evaluate(def, x).Success
                   Select x
        End If
    End Function
End Module
