'Imports System.Text

'Namespace BasicR.Helpers

'    Public Class Variable

'        Friend Variables As Dictionary(Of String, MATRIX) = New Dictionary(Of String, MATRIX)

'        Public Const LEFT_OPERATOR_TOKENS As String = "+-*/\^("
'        Public Const RIGHT_OPERATOR_TOKENS As String = "+-*/\^)!"

'        Friend VariableList As List(Of String) = New List(Of String)

'        ''' <summary>
'        ''' Add a user constant to the dictionary.
'        ''' (向字典之中添加用户自定义常数)
'        ''' </summary>
'        ''' <param name="Name"></param>
'        ''' <param name="value"></param>
'        ''' <remarks>
'        ''' const [name] [value]
'        ''' </remarks>
'        Public Sub [Set](Name As String, value As String)
'            Dim var As String = Name.ToLower

'            If VariableList.IndexOf(var) > -1 Then
'                Variables.Remove(var)
'            Else
'                Call VariableList.Clear()
'                Call VariableList.AddRange(Variables.Keys)
'            End If

'            Variables(var) = Expression.Evaluate(value)
'        End Sub

'        ''' <summary>
'        ''' Add a user const from the input of user on the console.
'        ''' </summary>
'        ''' <param name="statement"></param>
'        ''' <remarks></remarks>
'        Public Sub [Set](statement As String)
'            Dim Name As String = statement.Split.First
'            Call [Set](Name, Mid(statement, Len(Name) + 2))
'        End Sub

'        ''' <summary>
'        ''' Assign the new value for a variable
'        ''' </summary>
'        ''' <param name="statement"></param>
'        ''' <remarks></remarks>
'        Public Sub AssignValue(statement As String)
'            'var <- new_value_expression
'            Dim Tokens As String() = Split(statement, "<-")
'            Call [Set](Tokens.First.Trim, Expression.Evaluate(Tokens.Last.Trim))
'        End Sub
'    End Class
'End Namespace
