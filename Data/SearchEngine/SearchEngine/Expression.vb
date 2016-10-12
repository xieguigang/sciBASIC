Imports Microsoft.VisualBasic.Linq

''' <summary>
''' The query expression
''' </summary>
Public Class Expression

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
        Return String.Join(" ", Tokens.ToArray(Function(x) $"<{x.Operator}>"))
    End Function

    Public Function Evaluate(def As IObject, obj As Object) As Boolean
        Return Expression.Evaluate(def, obj, Tokens)
    End Function

    ''' <summary>
    ''' 逻辑运算都是从左往右计算的
    ''' </summary>
    ''' <param name="def"></param>
    ''' <returns></returns>
    Public Shared Function Evaluate(def As IObject, obj As Object, tokens As IEnumerable(Of MetaExpression)) As Boolean
        Dim notPending As Boolean
        Dim exp As New List(Of MetaExpression)(tokens)
        Dim b As Boolean

        ' 1 OR 0 -> {1, OR} {0, undefine}
        ' NOT 0 OR 1 -> {undefine, NOT}, {0, OR}, {1, undefine}
        ' NOT 0 OR NOT 0 -> {undefine, NOT}, {0, OR}, {undefine, NOT}, {0, undefine}

        For Each i As SeqValue(Of MetaExpression) In exp.SeqIterator
            Dim m As MetaExpression = i.obj

            If m.Operator = SyntaxParser.Tokens.op_NOT Then
                notPending = True
                Continue For
            End If

            b = m.Expression(def, obj)

            If notPending Then
                b = Not b
                notPending = False
            End If

            If b = True Then ' 成立
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

        Return b
    End Function
End Class



