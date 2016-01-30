Imports System.Reflection

Public Class TaskHost

    Public Shared Function Shell(exe As String, args As String) As Integer
        Return -55
    End Function

    ''' <summary>
    ''' 本地服务器通过这个方法调用远程主机
    ''' </summary>
    ''' <param name="target"></param>
    ''' <param name="args"></param>
    ''' <returns></returns>
    Public Function Invoke(target As [Delegate], ParamArray args As Object()) As Object
        Dim params As Invoke = ComputingServices.Invoke.CreateObject(target, args)
        Return Invoke(params) ' 测试
    End Function

    ''' <summary>
    ''' 远程服务器上面通过这个方法执行函数调用
    ''' </summary>
    ''' <param name="params"></param>
    ''' <returns></returns>
    Public Function Invoke(params As Invoke) As Object
        Dim func As MethodInfo = params.GetMethod
        Dim paramsValue As Object() = ComputingServices.Invoke.GetParameters(func, params.Parameters)
        Dim value As Object = func.Invoke(Nothing, paramsValue)
        Return value
    End Function

    Public Function Invoke(Of T)(target As [Delegate], ParamArray args As Object()) As T
        Dim value As Object = Invoke(target, args)
        If value Is Nothing Then
            Return Nothing
        Else
            Return DirectCast(value, T)
        End If
    End Function

    Delegate Function __shell(exe As String, args As String) As Integer

    Shared Sub test()
        Dim host As New TaskHost
        Dim ss As __shell = AddressOf TaskHost.Shell
        Dim n = host.Invoke(Of Integer)(ss, {"1", "2"})
    End Sub
End Class

Public Class Invoke
    Public Property Assembly As String
    Public Property Type As String
    Public Property Name As String
    ''' <summary>
    ''' json value
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

    Public Shared Function CreateObject(func As [Delegate], args As Object()) As Invoke
        Dim type As Type = func.Method.DeclaringType
        Dim assm As Assembly = type.Assembly
        Dim name As String = func.Method.Name
        Dim callsType As Type() = func.Method.GetParameters.ToArray(Function(x) x.ParameterType)
        Dim params As String() = args.ToArray(Function(x, idx) Serialization.JsonContract.GetJson(x, callsType(idx)))
        Return New Invoke With {
            .Assembly = FileIO.FileSystem.GetFileInfo(assm.Location).Name,
            .Name = name,
            .Parameters = params,
            .Type = type.FullName
        }
    End Function
End Class