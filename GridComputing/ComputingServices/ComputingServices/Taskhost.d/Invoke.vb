Imports System.Reflection

Namespace TaskHost

    ''' <summary>
    ''' 分布式计算框架之中的远程调用的参数信息
    ''' </summary>
    Public Class InvokeInfo

        ''' <summary>
        ''' 模块文件
        ''' </summary>
        ''' <returns></returns>
        Public Property Assembly As String
        ''' <summary>
        ''' 源
        ''' </summary>
        ''' <returns></returns>
        Public Property Type As String
        ''' <summary>
        ''' 函数名
        ''' </summary>
        ''' <returns></returns>
        Public Property Name As String
        ''' <summary>
        ''' json value.(函数参数)
        ''' </summary>
        ''' <returns></returns>
        Public Property Parameters As String()

        Public Function LoadAssembly() As Assembly
            Dim path As String = App.HOME & "/" & Assembly
            Dim assm As Assembly = System.Reflection.Assembly.LoadFile(path)
            Return assm
        End Function

        Public Overloads Function [GetType]() As Type
            Dim assm As Assembly = LoadAssembly()
            Dim type As Type = assm.GetType(Me.Type)
            Return type
        End Function

        Public Function GetMethod() As MethodInfo
            Dim type As Type = [GetType]()
            Dim func As MethodInfo = type.GetMethod(Name, BindingFlags.Public Or BindingFlags.Static)
            Return func
        End Function

        Public Overrides Function ToString() As String
            Return $"{Assembly}!{Type}::{Name}"
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="method"></param>
        ''' <param name="args">json</param>
        ''' <returns></returns>
        Public Shared Function GetParameters(method As MethodInfo, args As String()) As Object()
            Dim params As Type() = method.GetParameters.ToArray(Function(x) x.ParameterType)
            Dim values As Object() = args.ToArray(Function(x, idx) Serialization.LoadObject(x, params(idx)))
            Return values
        End Function

        Public Shared Function CreateObject(func As [Delegate], args As Object()) As InvokeInfo
            Dim type As Type = func.Method.DeclaringType
            Dim assm As Assembly = type.Assembly
            Dim name As String = func.Method.Name
            Dim callsType As Type() = func.Method.GetParameters.ToArray(Function(x) x.ParameterType)
            Dim params As String() = args.ToArray(Function(x, idx) Serialization.JsonContract.GetJson(x, callsType(idx)))
            Return New InvokeInfo With {
                .Assembly = FileIO.FileSystem.GetFileInfo(assm.Location).Name,
                .Name = name,
                .Parameters = params,
                .Type = type.FullName
            }
        End Function
    End Class
End Namespace