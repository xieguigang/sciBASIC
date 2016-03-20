Public Module ScriptEngine

    Public ReadOnly Property Expression As Expression =
        Mathematical.Expression.DefaultEngine

    ''' <summary>
    ''' all of the commands are stored at here
    ''' </summary>
    ''' <remarks>
    ''' .quit for do nothing and end of this program.
    ''' </remarks>
    Public ReadOnly Property StatementEngine As Dictionary(Of String, System.Action(Of String)) =
        New Dictionary(Of String, System.Action(Of String)) From {
            {"const", AddressOf Expression.Constant.Add},
            {"function", AddressOf Expression.Functions.Add},
            {"func", AddressOf Expression.Functions.Add},
            {"var", AddressOf Expression.Variables.Set},
            {".quit", Sub(NULL As String) NULL = Nothing}}

    Public ReadOnly Property Scripts As New Hashtable

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
            Dim Result As Double = Expression.Evaluate(statement)
            Expression.Variables.Set("$", Result)  'You can treat the variable 'last' as a system variable for return the result of a multiple function script in the future of this feature will add to this module.

            Return Result
        End If
    End Function

    Public Sub SetVariable(Name As String, expr As String)
        Call Expression.Variables.Set(Name, expr)
    End Sub

    Public Sub AddConstant(Name As String, expr As String)
        Call Expression.Constant.Add(Name, expr)
    End Sub
End Module
