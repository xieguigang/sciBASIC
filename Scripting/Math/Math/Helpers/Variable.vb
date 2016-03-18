Imports System.Text

Namespace Helpers

    Public Class Variable : Inherits MemoryCollection(Of String)

        Public Const LEFT_OPERATOR_TOKENS As String = "+-*/\^("
        Public Const RIGHT_OPERATOR_TOKENS As String = "+-*/\^)!"

        ''' <summary>
        ''' Add a variable to the dictionary, if the variable is exists then will update its value.
        ''' (向字典之中添加一个变量，假若该变量存在，则更新他的值)
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <remarks>
        ''' const [name] [value]
        ''' </remarks>
        Default Public Property Variable(Name As String) As String
            Get
                Name = Name.ToLower

                If _ObjHash.ContainsKey(Name) Then
                    Return Variables(Name)
                Else
                    Return 0
                End If
            End Get
            Set(value As String)
                Call [Set](Name, value)
            End Set
        End Property

        Sub New()
            Call _ObjHash.Add("$", "0")
        End Sub

        ''' <summary>
        ''' Add a variable to the dictionary, if the variable is exists then will update its value.
        ''' (向字典之中添加一个变量，假若该变量存在，则更新他的值)
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <remarks>
        ''' const [name] [value]
        ''' </remarks>
        Public Sub [Set](Name As String, value As String)
            Call Add(Name, value:=Expression.Evaluate(value), cache:=True)
        End Sub

        ''' <summary>
        ''' Add a user const from the input of user on the console.
        ''' </summary>
        ''' <param name="statement"></param>
        ''' <remarks></remarks>
        Public Sub [Set](statement As String)
            Dim Name As String = statement.Split.First
            Call [Set](Name, Mid(statement, Len(Name) + 2))
        End Sub

        ''' <summary>
        ''' Assign the new value for a variable
        ''' </summary>
        ''' <param name="statement"></param>
        ''' <remarks></remarks>
        Public Sub AssignValue(statement As String)
            'var <- new_value_expression
            Dim Tokens As String() = Strings.Split(statement, "<-")
            Call [Set](Tokens.First.Trim, Expression.Evaluate(Tokens.Last.Trim))
        End Sub

        Public Function AssignValue(var As String, statement As String) As Object
            Dim value = Expression.Evaluate(statement)
            Call [Set](var, value)
            Return value
        End Function
    End Class
End Namespace
