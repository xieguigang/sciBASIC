Imports System.Reflection
Imports Microsoft.VisualBasic.Serialization

Namespace TaskHost

    ''' <summary>
    ''' 由于是远程调用，所以运行的环境可能会很不一样，所以在设计程序的时候请尽量避免或者不要使用模块变量，以免出现难以调查的BUG
    ''' </summary>
    Public Class TaskHost

        ''' <summary>
        ''' 相当于Sub，调用远程的命令行程序，只会返回0或者错误代码
        ''' </summary>
        ''' <param name="exe"></param>
        ''' <param name="args"></param>
        ''' <returns></returns>
        Public Function Shell(exe As String, args As String) As Integer

        End Function

        ''' <summary>
        ''' 本地服务器通过这个方法调用远程主机
        ''' </summary>
        ''' <param name="target"></param>
        ''' <param name="args"></param>
        ''' <returns></returns>
        Public Function Invoke(target As [Delegate], ParamArray args As Object()) As Object
            Dim params As InvokeInfo = InvokeInfo.CreateObject(target, args)
            Dim rtvl As Returns = Invoke(params) ' 测试
            Dim value As Object = rtvl.GetValue(target)
            Return value
        End Function

        ''' <summary>
        ''' 远程服务器上面通过这个方法执行函数调用
        ''' </summary>
        ''' <param name="params"></param>
        ''' <returns></returns>
        Public Function Invoke(params As InvokeInfo) As Returns
            Dim func As MethodInfo = params.GetMethod
            Dim paramsValue As Object() = InvokeInfo.GetParameters(func, params.Parameters)
            Dim rtvl As Returns
            Try
                Dim value As Object = func.Invoke(Nothing, paramsValue)
                rtvl = New Returns(value, func.ReturnType)
            Catch ex As Exception
                ex = New Exception(params.GetJson, ex)
                rtvl = New Returns(ex)
            End Try

            Return rtvl
        End Function

        Public Function Invoke(Of T)(target As [Delegate], ParamArray args As Object()) As T
            Dim value As Object = Invoke(target, args)
            If value Is Nothing Then
                Return Nothing
            Else
                Return DirectCast(value, T)
            End If
        End Function
    End Class
End Namespace