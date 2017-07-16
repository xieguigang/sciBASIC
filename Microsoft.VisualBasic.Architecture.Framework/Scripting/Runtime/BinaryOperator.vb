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

        Public Function InvokeSelfLeft(self As Object, obj As Object, ByRef result As Object) As Boolean
            Return __invokeInternal(self, obj, 0, result)
        End Function

        Private Function __invokeInternal(self As Object, obj As Object, pos%, ByRef result As Object) As Boolean
            Dim type As Type = obj.GetType
            Dim depthMin% = Integer.MaxValue
            Dim target As MethodInfo = Nothing

            For Each m As MethodInfo In methods
                ' 参数在右边，即第二个参数
                Dim parm As ParameterInfo = m.GetParameters(pos) ' base type
                Dim depth%

                If type.IsInheritsFrom(parm.ParameterType,, depth) Then
                    If depth < depthMin Then
                        depthMin = depth
                        target = m
                    End If
                End If
            Next

            If Not target Is Nothing Then
                If pos = 0 Then
                    result = target.Invoke(Nothing, {self, obj})
                Else
                    result = target.Invoke(Nothing, {obj, self})
                End If
            Else
                Return False
            End If

            Return True
        End Function

        Public Function InvokeSelfRight(obj As Object, self As Object, ByRef result As Object) As Boolean
            Return __invokeInternal(self, obj, 1, result)
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