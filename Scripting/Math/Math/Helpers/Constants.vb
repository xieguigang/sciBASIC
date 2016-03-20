Imports System.Text.RegularExpressions
Imports System.Text

Namespace Helpers

    Public Class Constants : Inherits MemoryCollection(Of Double)

        Sub New()
            Call MyBase.New(Nothing)
            Call MyBase.Add(NameOf(Math.E), Math.E, False, True)
            Call MyBase.Add(NameOf(Math.PI), Math.PI, False, True)
            Call MyBase.Add(NameOf(Scan0), Scan0, False, True)
            Call __buildCache()
        End Sub

        ''' <summary>
        ''' 常量是区分大小写的
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Function [GET](x As String, ByRef success As Boolean) As Double
            If _objHash.ContainsKey(x) Then
                success = True
                Return _objHash(x)
            Else
                success = False
                Return -1
            End If
        End Function

        ''' <summary>
        ''' Add a user constant to the dictionary.
        ''' (向字典之中添加用户自定义常数)
        ''' </summary>
        ''' <param name="Name">常数名称是大小写敏感的，变量的大小写却不敏感</param>
        ''' <param name="value"></param>
        ''' <remarks>
        ''' const [name] [value]
        ''' </remarks>
        Public Overloads Sub Add(Name As String, value As Double)
            If _objHash.ContainsKey(Name) Then
                Dim msg As String = $"Constant not set as the const ""{Name}"" is already set in this engine."
                Throw New Exception(msg)
            Else
                Call MyBase.Add(Name, value, True, True)
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
