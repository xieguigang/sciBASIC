Public Module ScriptEngine

    ''' <summary>
    ''' all of the commands are stored at here
    ''' </summary>
    ''' <remarks>
    ''' .quit for do nothing and end of this program.
    ''' </remarks>
    Friend ReadOnly StatementEngine As Dictionary(Of String, System.Action(Of String)) =
        New Dictionary(Of String, System.Action(Of String)) From {
            {"const", AddressOf Expression.Constant.Add},
            {"function", AddressOf Expression.Functions.Add},
            {"var", AddressOf Expression.Variables.Set},
            {".quit", Sub(NULL As String) NULL = Nothing}}

    Friend ReadOnly Scripts As New Hashtable

    Public Function Shell(statement As String) As String
        Dim Token As String = statement.Split.First.ToLower

        If InStr(statement, "<-") Then  'This is a value assignment statement
            Call Expression.Variables.AssignValue(statement)
            Return String.Empty
        End If

        If StatementEngine.ContainsKey(Token) Then
            Call StatementEngine(Token)(Mid(statement, Len(Token) + 1).Trim)
            Return String.Empty
        Else  'if the statement input from the user is not appears in the engine dictionary, then maybe is a mathematics expression. 
            Dim Result As Double = Expression.Evaluate(expression:=statement)
            Expression.Variables.Set("$", Result)  'You can treat the variable 'last' as a system variable for return the result of a multiple function script in the future of this feature will add to this module.

            Return Result
        End If
    End Function
End Module
