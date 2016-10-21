Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Data.IO.SearchEngine
Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' The query expression
''' </summary>
Public Class Expression
    Implements IEnumerable(Of MetaExpression)

    Public Property Tokens As MetaExpression()

    ReadOnly __textType As New IObject(GetType(Text))

    ''' <summary>
    ''' Does this expression model in the target input <paramref name="text"/> have a match?
    ''' </summary>
    ''' <param name="text">The text data that using for data search.</param>
    ''' <returns></returns>
    Public Function Match(text As String) As Boolean
        Return Evaluate(
            __textType, New Text With {
                .Text = text
            })
    End Function

    Public Overrides Function ToString() As String
        Return Debug()
    End Function

    Public Function Evaluate(def As IObject, obj As Object) As Match
        Dim m As Match = Nothing
        Expression.Evaluate(def, obj, Tokens, m)
        Return m
    End Function

    ''' <summary>
    ''' 逻辑运算都是从左往右计算的
    ''' </summary>
    ''' <param name="def"></param>
    ''' <returns></returns>
    Public Shared Function Evaluate(def As IObject, obj As Object, tokens As IEnumerable(Of MetaExpression), ByRef match As Match) As Boolean
        Dim notPending As Boolean
        Dim exp As New List(Of MetaExpression)(tokens)
        Dim b As Boolean
        Dim result As Match
        Dim fields As New List(Of NamedValue(Of String))
        Dim score As Double

        ' 1 OR 0 -> {1, OR} {0, undefine}
        ' NOT 0 OR 1 -> {undefine, NOT}, {0, OR}, {1, undefine}
        ' NOT 0 OR NOT 0 -> {undefine, NOT}, {0, OR}, {undefine, NOT}, {0, undefine}

        For Each i As SeqValue(Of MetaExpression) In exp.SeqIterator
            Dim m As MetaExpression = i.obj

            If m.Operator = SyntaxParser.Tokens.op_NOT Then
                notPending = True
                Continue For
            End If

            result = m.Expression(def, obj)
            b = result.Success

            If notPending Then
                b = Not b
                notPending = False
            End If

            If b = True Then ' 成立
                score += result.score
                fields += result.Field

                If m.Operator = SyntaxParser.Tokens.op_OR Then  ' 短路，这里已经成立了则不必再计算下去了
                    Exit For
                ElseIf m.Operator = SyntaxParser.Tokens.op_AND Then
                    Continue For
                ElseIf i.i = exp.Count - 1 Then
                    Exit For
                Else
                    Throw New SyntaxErrorException
                End If
            Else
                If m.Operator = SyntaxParser.Tokens.op_OR Then
                    Continue For
                ElseIf m.Operator = SyntaxParser.Tokens.op_AND Then
                    Exit For
                ElseIf i.i = exp.Count - 1 Then
                    Exit For
                Else
                    Throw New SyntaxErrorException
                End If
            End If
        Next

        If b = True Then  ' 当b是False的时候，说明匹配失败了，则要将match置为空值，否则最终的结果任然是True
            match = New Match With {
                .score = score,
                .x = obj,
                .Field = New NamedValue(Of String) With {
                    .Name = tokens.Debug,
                    .x = fields _
                    .ToDictionary(Function(x) x.Name,
                                  Function(x) x.x).GetJson
                }
            }
        Else
            match = Nothing
        End If

        Return b
    End Function

    Public Iterator Function GetEnumerator() As IEnumerator(Of MetaExpression) Implements IEnumerable(Of MetaExpression).GetEnumerator
        For Each x In Tokens
            Yield x
        Next
    End Function

    Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Yield GetEnumerator()
    End Function
End Class



