Imports System.Reflection
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Debugging
Imports Microsoft.VisualBasic.Linq.Extensions

Namespace CommandLine.Reflection.EntryPoints

    ''' <summary>
    ''' The entry point data of the commands in the command line which was original loaded 
    ''' from the source meta data in the compiled target.
    ''' (命令行命令的执行入口点)
    ''' </summary>
    ''' <remarks></remarks>
    Public Class APIEntryPoint : Inherits APIDelegate

#Region "ReadOnly Properties"

        Public ReadOnly Property ParameterInfo As ParameterInfoCollection
        ''' <summary>
        ''' The reflection entry point in the assembly for the target method object.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property EntryPoint As MethodInfo

        ''' <summary>
        ''' If the target invoked <see cref="EntryPoint">method delegate</see> is a instance method, 
        ''' then this property value should be the target object instance which has the method delegate.
        ''' (假若目标方法不是共享的方法，则必须要使用本对象来进行Invoke的调用)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property InvokeOnObject As Object

        ''' <summary>
        ''' The shared method did not requires of the object instance.(这个方法是否为实例方法)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property IsInstanceMethod As Boolean
            Get
                Return Not Me.EntryPoint.IsStatic OrElse Not InvokeOnObject Is Nothing
            End Get
        End Property

        ''' <summary>
        ''' The full name path of the target invoked method delegate in the namespace library.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function EntryPointFullName(relativePath As Boolean) As String
            Dim path As String

            If relativePath Then
                path = ProgramPathSearchTool.RelativePath(EntryPoint.DeclaringType.Assembly.Location)
            Else
                path = EntryPoint.DeclaringType.Assembly.Location.ToFileURL
            End If

            Return $"{path}!{EntryPoint.DeclaringType.FullName}::{EntryPoint.ToString}"
        End Function
#End Region

#Region "Constructors"

        Public Sub New()
        End Sub

        Public Sub New(invokeOn As Object)
            Me.InvokeOnObject = invokeOn
        End Sub

        ''' <summary>
        ''' Instance method can be initialize from this constructor.
        ''' (假若目标方法为实例方法，请使用本方法进行初始化)
        ''' </summary>
        ''' <param name="attribute"></param>
        ''' <param name="Invoke"></param>
        ''' <remarks></remarks>
        Public Sub New(attribute As ExportAPIAttribute, [Invoke] As MethodInfo, Optional [Throw] As Boolean = True)
            _metaData = New Binding(Of ExportAPIAttribute, MethodInfo) With {
                .Bind = attribute,
                .Target = Invoke
            }
            __funcInvoker = Function(args As Object()) InvokeCLI(parameters:=args, target:=Nothing, [Throw]:=[Throw])
            _EntryPoint = Invoke
            _ParameterInfo = New ParameterInfoCollection(methodInfo:=Invoke)
            _NumberOfParameters = Invoke.GetParameters.Length
        End Sub
#End Region

#Region "Public Methods"

        ''' <summary>
        ''' Returns the help information details for this command line entry object.(获取本命令行执行入口点的详细帮助信息)
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function HelpInformation(Optional md As Boolean = False) As String
            Dim sBuilder As StringBuilder = New StringBuilder(MyBase.HelpInformation(md))

            If Not _ParameterInfo.IsNullOrEmpty Then
                Call sBuilder.AppendLine(vbCrLf & vbCrLf)
                Call sBuilder.AppendLine("  Parameters information:")
                If Not md Then
                    Call sBuilder.AppendLine(vbCrLf & "   ---------------------------------------")
                Else
                    Call sBuilder.AppendLine("```")
                End If
                Call sBuilder.AppendLine("    " & _ParameterInfo.ToString)
                If md Then
                    Call sBuilder.AppendLine("```")
                End If
            End If

            Return sBuilder.ToString
        End Function

        ''' <summary>
        ''' Invoke this command line and returns the function value.(函数会补齐可选参数)
        ''' </summary>
        ''' <param name="parameters">The function parameter for the target invoked method, the optional value will be filled 
        ''' using the paramter default value if you are not specific the optional paramter value is the element position of 
        ''' this paramter value.</param>
        ''' <param name="Throw">If throw then if the exception happened from delegate invocation then the program will throw an 
        ''' exception and terminated, if not then the program will save the exception information into a log file and then 
        ''' returns a failure status.</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Invoke(parameters As Object(), Optional [Throw] As Boolean = True) As Object
            Return Invoke(parameters, Me.InvokeOnObject, [Throw])
        End Function

        ''' <summary>
        ''' 不会自动调整补齐参数
        ''' </summary>
        ''' <param name="callParameters"></param>
        ''' <param name="[Throw]"></param>
        ''' <returns></returns>
        Public Function DirectInvoke(callParameters As Object(), Optional [Throw] As Boolean = True) As Object
            Return __directInvoke(callParameters, Me.InvokeOnObject, [Throw])
        End Function

        ''' <summary>
        ''' 记录错误信息的最上层的堆栈
        ''' </summary>
        ''' <param name="callParameters"></param>
        ''' <param name="target"></param>
        ''' <param name="[Throw]"></param>
        ''' <returns></returns>
        Private Function __directInvoke(callParameters As Object(), target As Object, [Throw] As Boolean) As Object
            Dim rtvl As Object

            Try
                rtvl = EntryPoint.Invoke(target, callParameters)
            Catch ex As Exception
                Dim args As String() = callParameters.ToArray(AddressOf Scripting.ToString)
                Dim paramTrace As String = String.Join(vbCrLf, args)
                Dim source As Exception = ex

                ex = New Exception(paramTrace, ex)
                ex = New VBDebugger.VisualBasicAppException(ex, EntryPoint.GetFullName(True))

                VBDebugger.Mute = False ' Enable output the exception details on the console.

                Call App.LogException(ex, MethodBase.GetCurrentMethod.GetFullName)
                Call DebuggerArgs.SaveErrorLog(App.BugsFormatter(ex))

                If [Throw] Then
                    Throw ex
                Else
                    Call ExceptionHandler.Print(source, EntryPoint)
                    rtvl = -100
                End If
            End Try

            Return rtvl
        End Function

        ''' <summary>
        ''' Invoke this command line and returns the function value.
        ''' (函数会补齐可选参数)
        ''' </summary>
        ''' <param name="parameters">The function parameter for the target invoked method, the optional value will be filled 
        ''' using the paramter default value if you are not specific the optional paramter value is the element position of 
        ''' this paramter value.</param>
        ''' <param name="target">Target entry pointer of this function method delegate.</param>
        ''' <param name="Throw">If throw then if the exception happened from delegate invocation then the program will throw an 
        ''' exception and terminated, if not then the program will save the exception information into a log file and then 
        ''' returns a failure status.</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Invoke(parameters As Object(), target As Object, Optional [Throw] As Boolean = True) As Object
            Dim callParameters() As Object

            If parameters.Length < _NumberOfParameters Then
                callParameters = New Object(_NumberOfParameters - 1) {}
                Call parameters.CopyTo(callParameters, 0)

            ElseIf parameters.Length > _NumberOfParameters Then
                callParameters = New Object(_NumberOfParameters - 1) {}
                Call Array.ConstrainedCopy(parameters, 0, callParameters, 0, _NumberOfParameters)
            Else
                callParameters = parameters
            End If

            Return __directInvoke(callParameters, target, [Throw])
        End Function

        ''' <summary>
        ''' Invoke this command line but returns the function execute success, Zero for success and -1 for failure.
        ''' (函数会补齐可选参数)
        ''' </summary>
        ''' <param name="parameters"></param>
        ''' <param name="target"></param>
        ''' <param name="Throw"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function InvokeCLI(parameters As Object(), target As Object, Optional [Throw] As Boolean = True) As Integer
            Dim rtvl As Object = Invoke(parameters, target, [Throw])
            Dim Type As Type = rtvl.GetType

            If Type = GetType(Integer) OrElse
                Type = GetType(Long) OrElse
                Type = GetType(Double) OrElse
                Type = GetType(Short) Then

                Return CType(rtvl, Integer)
            Else
                Dim value As Integer = If(rtvl Is Nothing, -1, 0)
                Return value
            End If
        End Function
#End Region
    End Class
End Namespace