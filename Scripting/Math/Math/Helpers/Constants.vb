Imports System.Text.RegularExpressions
Imports System.Text

Namespace Helpers

    Public Class Constants : Inherits MemoryCollection(Of String)

        Public Const LEFT_OPERATOR_TOKENS As String = "+-*/\^(,"
        Public Const RIGHT_OPERATOR_TOKENS As String = "+-*/\^)!,"

        Sub New()
            Call _InnerObjectDictionary.Add("e", Math.E)
            Call _InnerObjectDictionary.Add("pi", Math.PI)
        End Sub

        Default Public ReadOnly Property Constant(Name As String) As String
            Get
                If _InnerObjectDictionary.ContainsKey(Name) Then
                    Return _InnerObjectDictionary(Name)
                Else
                    Return "0"
                End If
            End Get
        End Property

        Public Function [GET](x As String) As Double
            Return Val(Me(x.ToLower))
        End Function

        ''' <summary>
        ''' Add a user constant to the dictionary.
        ''' (向字典之中添加用户自定义常数)
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <param name="value"></param>
        ''' <remarks>
        ''' const [name] [value]
        ''' </remarks>
        Public Overloads Sub Add(Name As String, value As String)
            If _InnerObjectDictionary.ContainsKey(Name.ToLower) Then
                Console.WriteLine("Constant not set as the const ""{0}"" is already set in this engine.", Name)
            Else
                Call _InnerObjectDictionary.Add(Name.ToLower, Expression.Evaluate(value))
            End If
        End Sub

        ''' <summary>
        ''' Add a user const from the input of user on the console.
        ''' </summary>
        ''' <param name="statement"></param>
        ''' <remarks></remarks>
        Public Overloads Sub Add(statement As String)
            Dim Name As String = statement.Split.First
            Call Add(Name, Mid(statement, Len(Name) + 2))
        End Sub
    End Class
End Namespace
