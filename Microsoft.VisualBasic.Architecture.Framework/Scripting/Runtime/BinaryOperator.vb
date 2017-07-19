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

        ''' <summary>
        ''' 参数在左边
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        Public Function MatchLeft(type As Type) As MethodInfo
            Return Match(type, 0)
        End Function

        ''' <summary>
        ''' 参数在右边
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        Public Function MatchRight(type As Type) As MethodInfo
            Return Match(type, 1)
        End Function

        Public Function Match(type As Type, pos%) As MethodInfo
            Dim depthMin% = Integer.MaxValue
            Dim target As MethodInfo = Nothing

            For Each m As MethodInfo In methods
                ' 参数在右边，即第二个参数
                Dim parm As ParameterInfo = m.GetParameters(pos) ' base type
                Dim depth%

                If type.IsInheritsFrom(parm.ParameterType, False, depth) Then
                    If depth < depthMin Then
                        depthMin = depth
                        target = m
                    End If
                End If
            Next

            If target Is Nothing Then
                If Runtime.Numerics.IndexOf(Type.GetTypeCode(type)) > -1 Then
                    For Each m As MethodInfo In methods
                        Dim parm As ParameterInfo = m.GetParameters(pos) ' base type
                        If Runtime.Numerics.IndexOf(Type.GetTypeCode(parm.ParameterType)) > -1 Then
                            Return m
                        End If
                    Next
                End If
            End If

            Return target
        End Function

        ''' <summary>
        ''' 参数在右边
        ''' </summary>
        ''' <param name="self"></param>
        ''' <param name="obj"></param>
        ''' <param name="result"></param>
        ''' <returns></returns>
        Public Function InvokeSelfLeft(self As Object, obj As Object, ByRef result As Object) As Boolean
            Return __invokeInternal(self, obj, 0, result)
        End Function

        Private Function __invokeInternal(self As Object, obj As Object, pos%, ByRef result As Object) As Boolean
            Dim type As Type = obj.GetType
            Dim target As MethodInfo = Match(type, If(pos = 1, 0, 1))

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

        ''' <summary>
        ''' 参数在左边
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <param name="self"></param>
        ''' <param name="result"></param>
        ''' <returns></returns>
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