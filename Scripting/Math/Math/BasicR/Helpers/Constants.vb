'Imports System.Text.RegularExpressions
'Imports System.Text

'Namespace BasicR.Helpers

'    Public Class Constants

'        ''' <summary>
'        ''' The constant PI and E is the system reserved constant
'        ''' </summary>
'        ''' <remarks></remarks>
'        Friend Constants As Dictionary(Of String, BasicR.MATRIX) = New Dictionary(Of String, BasicR.MATRIX) From {
'            {"e", Math.E},
'            {"pi", Math.PI}}

'        Public Const LEFT_OPERATOR_TOKENS As String = "+-*/\^(,"
'        Public Const RIGHT_OPERATOR_TOKENS As String = "+-*/\^)!,"

'        Friend ConstantList As List(Of String) = New List(Of String) From {"pi", "e"}

'        ''' <summary>
'        ''' Add a user constant to the dictionary.
'        ''' (向字典之中添加用户自定义常数)
'        ''' </summary>
'        ''' <param name="Name"></param>
'        ''' <param name="value"></param>
'        ''' <remarks>
'        ''' const [name] [value]
'        ''' </remarks>
'        Public Sub Add(Name As String, value As String)
'            Call Constants.Add(Name.ToLower, Expression.Evaluate(value))
'            Call ConstantList.Clear()
'            Call ConstantList.AddRange(Constants.Keys)
'        End Sub

'        ''' <summary>
'        ''' Add a user const from the input of user on the console.
'        ''' </summary>
'        ''' <param name="statement"></param>
'        ''' <remarks></remarks>
'        Public Sub Add(statement As String)
'            Dim Name As String = statement.Split.First
'            Call Add(Name, Mid(statement, Len(Name) + 2))
'        End Sub
'    End Class
'End Namespace
