Imports System.Text

Namespace Helpers

    Public Class Variable : Inherits MemoryCollection(Of Double)

        ''' <summary>
        ''' Add a variable to the dictionary, if the variable is exists then will update its value.
        ''' (向字典之中添加一个变量，假若该变量存在，则更新他的值)
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <remarks>
        ''' const [name] [value]
        ''' </remarks>
        Default Public Property Variable(Name As String) As Double
            Get
                If _ObjHash.ContainsKey(Name.ToLower.ShadowCopy(Name)) Then
                    Return _ObjHash(Name)
                Else
                    Return 0
                End If
            End Get
            Set(value As Double)
                Call [Set](Name, value)
            End Set
        End Property

        Sub New()
            Call MyBase.Add("$", "0", True, False)
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
        ''' <param name="statement">var &lt;- new_value_expression</param>
        ''' <remarks></remarks>
        Public Function AssignValue(statement As String) As Double
            Dim Tokens As String() = Strings.Split(statement, "<-")
            Return AssignValue(Tokens(Scan0), Tokens(1))
        End Function

        Public Function AssignValue(var As String, statement As String) As Double

        End Function
    End Class
End Namespace
