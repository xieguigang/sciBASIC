#Region "Microsoft.VisualBasic::a25347672374ea1825af8b654ae45f90, Microsoft.VisualBasic.Core\src\CommandLine\Reflection\EntryPoints\APIEntryPoint.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 308
    '    Code Lines: 164 (53.25%)
    ' Comment Lines: 101 (32.79%)
    '    - Xml Docs: 94.06%
    ' 
    '   Blank Lines: 43 (13.96%)
    '     File Size: 13.09 KB


    '     Class APIEntryPoint
    ' 
    '         Properties: Arguments, EntryPoint, IsInstanceMethod, target
    ' 
    '         Constructor: (+3 Overloads) Sub New
    ' 
    '         Function: DirectInvoke, EntryPointFullName, handleUnexpectedErrorCalls, HelpInformation, (+2 Overloads) Invoke
    '                   InvokeCLI, logError, tryInvoke
    ' 
    '         Sub: argumentNote
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.CommandLine.ManView
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Linq.Extensions
Imports any = Microsoft.VisualBasic.Scripting

Namespace CommandLine.Reflection.EntryPoints

    ''' <summary>
    ''' The entry point data of the commands in the command line which was original loaded 
    ''' from the source meta data in the compiled target.
    ''' (命令行命令的执行入口点)
    ''' </summary>
    ''' <remarks></remarks>
    Public Class APIEntryPoint : Inherits APIDelegate

#Region "ReadOnly Properties"

        ''' <summary>
        ''' 当前的这个命令对象的参数帮助信息列表
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Arguments As ArgumentCollection
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
        Public Property target As Object

        ''' <summary>
        ''' The shared method did not requires of the object instance.(这个方法是否为实例方法)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property IsInstanceMethod As Boolean
            Get
                Return Not Me.EntryPoint.IsStatic OrElse Not target Is Nothing
            End Get
        End Property

        ''' <summary>
        ''' The full name path of the target invoked method delegate in the namespace library.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function EntryPointFullName(relativePath As Boolean) As String
            Dim path$ = EntryPoint.DeclaringType.Assembly.Location

            If relativePath Then
                path = PathExtensions.RelativePath(path)
            Else
                path = path.ToFileURL
            End If

            Return $"{path}!{EntryPoint.DeclaringType.FullName}::{EntryPoint.ToString}"
        End Function
#End Region

#Region "Constructors"

        Public Sub New()
        End Sub

        ''' <summary>
        ''' 这个构造函数只设置目标实例对象，其他的数据从属性进行设置
        ''' </summary>
        ''' <param name="invokeOn"></param>
        Public Sub New(invokeOn As Object)
            Me.target = invokeOn
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
            _Arguments = New ArgumentCollection(methodInfo:=Invoke)
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
            Dim sb As New StringBuilder(MyBase.HelpInformation(md))

            If Not Arguments.IsNullOrEmpty Then
                Call argumentNote(sb, md)
            End If

            Dim note As NoteAttribute = EntryPoint.GetCustomAttribute(Of NoteAttribute)

            If Not note Is Nothing Then
                Call sb.AppendLine()
                Call sb.AppendLine("Author Comment About This Command:")
                Call sb.AppendLine(note.noteText)
            End If

            Return sb.ToString
        End Function

        Private Sub argumentNote(sb As StringBuilder, md As Boolean)
            Call sb.AppendLine(vbCrLf)
            Call sb.AppendLine("  #### Arguments")

            If Not md Then
                Call sb.AppendLine("  ---------------------------------------")
                Call sb.AppendLine()
                Call sb.AppendLine("    " & Arguments.ToString)
            Else
                For Each param In Arguments
                    Call sb.AppendLine("##### " & If(param.Value.Optional, $"[{param.Name}]", param.Name))
                    Call sb.AppendLine(param.Value.Description)
                    Call sb.AppendLine("###### Example")
                    Call sb.AppendLine("```bash")

                    If param.Value.TokenType = CLITypes.Boolean Then
                        Call sb.AppendLine(param.Name)
                        Call sb.AppendLine("#" & ManualBuilder.boolFlag)
                    Else
                        Call sb.AppendLine(param.Name & " " & param.Value.ExampleValue)
                        If param.Value.Pipeline <> PipelineTypes.undefined Then
                            Call sb.AppendLine("# " & param.Value.Pipeline.Description)
                        End If
                    End If

                    Call sb.AppendLine("```")
                Next
            End If
        End Sub

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
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Invoke(parameters As Object(), Optional [Throw] As Boolean = True) As Object
            Return Invoke(parameters, Me.target, [Throw])
        End Function

        ''' <summary>
        ''' 不会自动调整补齐参数
        ''' </summary>
        ''' <param name="callParameters"></param>
        ''' <param name="[Throw]"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function DirectInvoke(callParameters As Object(), Optional [Throw] As Boolean = True) As Object
            Return tryInvoke(callParameters, Me.target, [Throw])
        End Function

        ''' <summary>
        ''' 记录错误信息的最上层的堆栈
        ''' </summary>
        ''' <param name="callParameters"></param>
        ''' <param name="target"></param>
        ''' <param name="[throw]"></param>
        ''' <returns></returns>
        Private Function tryInvoke(callParameters As Object(), target As Object, [throw] As Boolean) As Object
#If DEBUG Then
            Return EntryPoint.Invoke(target, callParameters)
#Else
            Return handleUnexpectedErrorCalls(callParameters, target, [throw])
#End If
        End Function

        Private Function handleUnexpectedErrorCalls(callParameters As Object(), target As Object, [throw] As Boolean) As Object
            Dim rtvl As Object

            Try
                rtvl = EntryPoint.Invoke(target, callParameters)
            Catch ex As Exception
                rtvl = logError(ex, callParameters, [throw])
            End Try

            Return rtvl
        End Function

        Private Function logError(ex As Exception, callParameters As Object(), [throw] As Boolean) As Integer
            Dim args$() = callParameters _
                .Select(AddressOf any.ToString) _
                .ToArray
            Dim paramTrace As String = String.Join(vbCrLf, args)
            Dim source As Exception = ex
            Dim trace$ = MethodBase.GetCurrentMethod.GetFullName

            Call "".EchoLine
            Call ManView.ExceptionHandler.Print(source, EntryPoint)
            Call "".EchoLine
            Call VBDebugger.WaitOutput()

            ex = New Exception(paramTrace, ex)
            ex = New VisualBasicAppException(ex, EntryPoint.GetFullName(True))

            ' Enable output the exception details on the console.
            VBDebugger.Mute = False

            Call App.LogException(ex, trace)
            Call DebuggerArgs.SaveErrorLog(ErrorLog.BugsFormatter(ex))
            Call VBDebugger.WaitOutput()

            Return 500
        End Function

        ''' <summary>
        ''' Invoke this command line and returns the function value.
        ''' (函数会补齐可选参数)
        ''' </summary>
        ''' <param name="parameters">The function parameter for the target invoked method, the optional value will be filled 
        ''' using the parameter default value if you are not specific the optional parameter value is the element position of 
        ''' this parameter value.</param>
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

            Return tryInvoke(callParameters, target, [Throw])
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

            If EntryPoint.ReturnType Is GetType(Void) Then
                ' is a sub
                ' always return zero
                Return 0
            ElseIf rtvl Is Nothing Then
                ' function return value is nothing
                Return -1
            Else
                Dim type As Type = rtvl.GetType

                If type = GetType(Integer) OrElse
                   type = GetType(Long) OrElse
                   type = GetType(Double) OrElse
                   type = GetType(Short) Then

                    Return CType(rtvl, Integer)
                Else
                    Return 0
                End If
            End If
        End Function
#End Region
    End Class
End Namespace
