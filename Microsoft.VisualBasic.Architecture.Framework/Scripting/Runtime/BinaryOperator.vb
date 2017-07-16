Imports System.Reflection

Namespace Scripting.Runtime

    ''' <summary>
    ''' Binary operator invoker
    ''' </summary>
    Public Class BinaryOperator

        Dim name$
        Dim methods As MethodInfo()

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="[overloads]">重名的运算符函数方法</param>
        Sub New([overloads] As MethodInfo())
            name = [overloads](Scan0).Name
            methods = [overloads]
        End Sub

        Public Overrides Function ToString() As String
            If methods.Length = 1 Then
                Return name
            Else
                Return $"{name} (+{methods.Length} Overloads)"
            End If
        End Function

        Public Shared Function CreateOperator(methods As MethodInfo()) As BinaryOperator
            If methods.IsNullOrEmpty Then
                Return Nothing
            Else
                Return New BinaryOperator([overloads]:=methods)
            End If
        End Function
    End Class
End Namespace