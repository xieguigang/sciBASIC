#Region "Microsoft.VisualBasic::7869cfbdb108e160e37726b4bb3e8e35, G:/GCModeller/src/runtime/sciBASIC#/Microsoft.VisualBasic.Core/src//CommandLine/Interpreters/Interpreter.vb"

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

    '   Total Lines: 724
    '    Code Lines: 416
    ' Comment Lines: 215
    '   Blank Lines: 93
    '     File Size: 31.68 KB


    '     Class Interpreter
    ' 
    '         Properties: APIList, APINameList, Count, ExecuteEmptyCli, ExecuteFile
    '                     ExecuteNotFound, ExecuteQuery, Info, IsReadOnly, ListCommandInfo
    '                     Stack, Type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: __getsAllCommands, apiInvoke, apiInvokeEtc, Contains, CreateEmptyCLIObject
    '                   (+3 Overloads) CreateInstance, doExecuteNonCLIInput, doLoadApiInternal, exec_shell_script_internal, (+3 Overloads) Execute
    '                   ExistsCommand, GetAllCommands, getAPI, GetEnumerator, GetEnumerator1
    '                   GetPossibleCommand, Help, invokeSpecial, ListingRelated, (+2 Overloads) Remove
    '                   runShellScriptFile, SDKdocs, ToDictionary, ToString, TryGetValue
    ' 
    '         Sub: (+2 Overloads) Add, AddCommand, Clear, CopyTo, (+2 Overloads) Dispose
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Threading
Imports Microsoft.VisualBasic.CommandLine.ManView
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.CommandLine.Reflection.EntryPoints
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Serialization.JSON

#If DEBUG Then
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
#End If

#Const NET_45 = 0

Namespace CommandLine

    ''' <summary>
    ''' Command line interpreter for your **CLI** program.
    ''' (命令行解释器，请注意，在调试模式之下，命令行解释器会在运行完命令之后暂停，而Release模式之下则不会。
    ''' 假若在调试模式之下发现程序有很长一段时间处于cpu占用为零的静止状态，则很有可能已经运行完命令并且等待
    ''' 回车退出)
    ''' </summary>
    ''' <remarks></remarks>
    '''
    <[Namespace]("Interpreter")> Public Class Interpreter

        Implements IDisposable
        Implements IDictionary(Of String, APIEntryPoint)

        ''' <summary>
        ''' 在添加之前请确保键名是小写的字符串
        ''' </summary>
        Protected Friend apiTable As New Dictionary(Of String, APIEntryPoint)
        Protected rootNamespace$

#Region "Optional delegates"

        ''' <summary>
        ''' Public Delegate Function __ExecuteFile(path As String, args As String()) As Integer,
        ''' (<seealso cref="VisualBasic.CommandLine.ExecuteFile"/>: 假若所传入的命令行的name是文件路径，解释器就会执行这个函数指针)
        ''' 这个函数指针一般是用作于执行脚本程序的
        ''' </summary>
        ''' <returns></returns>
        Public Property ExecuteFile As ExecuteFile
        ''' <summary>
        ''' Public Delegate Function __ExecuteEmptyCli() As Integer,
        ''' (<seealso cref="VisualBasic.CommandLine.ExecuteEmptyCLI"/>: 假若所传入的命令行是空的，就会执行这个函数指针)
        ''' </summary>
        ''' <returns></returns>
        Public Property ExecuteEmptyCli As ExecuteEmptyCLI
        Public Property ExecuteNotFound As ExecuteNotFound
        Public Property ExecuteQuery As ExecuteQuery
#End Region

        ''' <summary>
        ''' Gets the dictionary data which contains all of the available command information in this assembly module.
        ''' (获取从本模块之中获取得到的所有的命令行信息)
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ToDictionary() As Dictionary(Of String, APIEntryPoint)
            Return apiTable
        End Function

        Public Overrides Function ToString() As String
            Return "CLI://" & rootNamespace
        End Function

        ''' <summary>
        ''' Execute the specific command line using this interpreter.
        ''' </summary>
        ''' <param name="args">The user input command line string.</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Function Execute(args As CommandLine) As Integer
            If Not args.IsNullOrEmpty Then
                Dim i As Integer = apiInvoke(args, args.Parameters)
#If DEBUG Then
                If Stack.TextEquals("Main") Then
                    If DebuggerArgs.AutoPaused Then
                        Call Pause()
                    End If
                End If
#End If
                Return i
            Else
                ' 命令行是空的
                Return doExecuteNonCLIInput()
            End If
        End Function

        ''' <summary>
        ''' 命令行是空的话，假若<see cref="ExecuteEmptyCli"/>不是空值的话，会优先执行<see cref="ExecuteEmptyCli"/>函数指针
        ''' 否则打印出所有的命令名称信息
        ''' </summary>
        ''' <returns></returns>
        Private Function doExecuteNonCLIInput() As Integer
            If Not ExecuteEmptyCli Is Nothing Then
#If DEBUG Then
                Return _ExecuteEmptyCli()
#Else
                Try
                    Return _ExecuteEmptyCli()
                Catch ex As Exception
                    Call App.LogException(ex)
                    Call ex.PrintException
                    Call VBDebugger.WaitOutput()
                End Try
#End If
                Return -100
            Else
                ' 当用户什么也不输入的时候，打印出所有的命令名称帮助信息
                Return Help("")
            End If
        End Function

        ''' <summary>
        ''' The interpreter runs all of the command from here.(所有的命令行都从这里开始执行)
        ''' </summary>
        ''' <param name="args">就只有一个命令行对象</param>
        ''' <param name="help_argvs"></param>
        ''' <returns></returns>
        Private Function apiInvoke(args As CommandLine, help_argvs$()) As Integer
            Dim command As String = args.Name.ToLower

            If apiTable.ContainsKey(command) Then
                Return apiTable(command).Execute(args)
            Else
                Return invokeSpecial(command, args, help_argvs)
            End If
        End Function

        Private Function invokeSpecial(command As String, args As CommandLine, help_argvs As String()) As Integer
            Select Case command
                Case "??vars"
                    Call ExecuteImpl.PrintVariables()
                Case "??history"
                    Call ExecuteImpl.HandleShellHistory(args)
                Case "?", "??", "--help"
                    If help_argvs.IsNullOrEmpty Then
                        Return Help("")
                    ElseIf (Not HasCommandName(help_argvs.First)) AndAlso Not ExecuteQuery Is Nothing Then
                        Return ExecuteQuery(args)
                    Else
                        Return Help(help_argvs.First)
                    End If
                Case "~"  ' 打印出应用程序的位置，linux里面的HOME
                    Call Console.WriteLine(App.ExecutablePath)
                Case "man"
                    Call ExecuteImpl.HandleProgramManual(Me, args)
                Case "/linux-bash"
                    Call My.UNIX.BashShell()
                Case "/cli.dev"
                    Call Me.CreateCLIPipelineFile(args)
                Case Else
                    If InStr(args.Name, "??") = 1 Then
                        ' 支持类似于R语言里面的 ??帮助命令
                        ' 去除前面的两个??问号，得到查询的term
                        Return Mid(args.Name, 3).DoCall(AddressOf Help)
                    Else
                        Return apiInvokeEtc(args.Name, args)
                    End If
            End Select

            Return 0
        End Function

        Private Function runShellScriptFile(filename As String, argv As CommandLine) As Integer
            App.InputFile = filename

            If argv.IsTrue("--debug") Then
                Return exec_shell_script_internal(filename, argv, debug:=True)
            End If

            Try
                Return exec_shell_script_internal(filename, argv, debug:=False)
            Catch ex As Exception
                ex = New Exception("Execute file failure!", ex)
                ex = New Exception(argv.ToString, ex)

                Call App.LogException(ex)
                Call ex.PrintException(enableRedirect:=False)
                Call VBDebugger.WaitOutput()

                Return 500
            End Try
        End Function

        ''' <summary>
        ''' Some magic tweaks will be made in this function call
        ''' </summary>
        ''' <param name="filename"></param>
        ''' <param name="argv"></param>
        ''' <returns></returns>
        Private Function exec_shell_script_internal(filename As String, argv As CommandLine, debug As Boolean) As Integer
            Dim exit_code As Integer = 0
            Dim mutex As New ManualResetEvent(initialState:=False)
            Dim exec As ThreadStart =
                Sub()
                    exit_code = ExecuteFile()(path:=filename, args:=argv)
                    mutex.Set()
                End Sub
            ' /STACK:64MB
            Dim max_stack_size_configuration As String = App.GetVariable("max_stack_size")

            If debug Then
                exit_code = ExecuteFile()(path:=filename, args:=argv)
            Else
                Call mutex.Reset()

                If max_stack_size_configuration.StringEmpty Then
                    Call New Thread(exec) With {.Name = MethodBase.GetCurrentMethod.Name}.Start()
                Else
                    ' run program with max stack size configuration from the
                    ' framework environment variable
                    Call New Thread(
                        start:=exec,
                        maxStackSize:=Unit.ParseByteSize(max_stack_size_configuration)
                    ) With {
                        .Name = MethodBase.GetCurrentMethod.Name
                    }.Start()
                End If

                Call mutex.WaitOne()
            End If

            Return exit_code
        End Function

        Private Function apiInvokeEtc(commandName$, cli As CommandLine) As Integer
            ' 命令行的名称和上面的都不符合，但是可以在文件系统之中找得到一个相应的文件，则执行文件句柄
            If (commandName.FileExists OrElse commandName.DirectoryExists) AndAlso Not Me.ExecuteFile Is Nothing Then
                Return runShellScriptFile(commandName, cli)
            ElseIf Not ExecuteNotFound Is Nothing Then
                Try
                    Return ExecuteNotFound()(cli)
                Catch ex As Exception
                    ex = New Exception("Execute not found failure!", ex)
                    ex = New Exception(cli.ToString, ex)

                    Call App.LogException(ex)
                    Call ex.PrintException

                    Return ex.InnerException.HResult
                End Try
            Else
                Dim list$() = Me.ListingRelated(commandName)

                If list.IsNullOrEmpty Then

                    Call Console.WriteLine(BAD_COMMAND_NAME, commandName)
                    Call Console.WriteLine()
                    Call Console.WriteLine(PS1.Fedora12.ToString & " " & cli.ToString)

                Else
                    Call listingCommands(list, commandName)
                End If
            End If

            Return 0
        End Function

        Const BAD_COMMAND_NAME$ = "Bad command, no such a command named ""{0}"", ? for command list or ""man"" for all of the commandline detail informations."

        ''' <summary>
        ''' Generate the sdk document for the target program assembly.(生成目标应用程序的命令行帮助文档，markdown格式的)
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function SDKdocs() As String
            Return Me.MarkdownDoc
        End Function

        ''' <summary>
        ''' Process the command option arguments of the main function:
        ''' <code>Public Function Main(argvs As String()) As Integer
        ''' </code>
        ''' </summary>
        ''' <param name="CommandLineArgs">The cli command line parameter string value collection.</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Execute(CommandLineArgs As String()) As Integer
            Dim CommandName As String = CommandLineArgs.First
            Dim argvs As String() = CommandLineArgs.Skip(1).ToArray
            Dim i As Integer = apiInvoke(CommandLine.BuildFromArguments(CommandLineArgs), help_argvs:=argvs)

            Return i
        End Function

        Public Function Execute(CommandName As String, args As String()) As Integer
            Return apiInvoke(CommandLine.BuildFromArguments(CommandName, args), help_argvs:=args)
        End Function

        ''' <summary>
        ''' Add a command in current cli interpreter.(x向当前的这个CLI命令行解释器之中添加一个命令)
        ''' </summary>
        ''' <param name="Command"></param>
        ''' <remarks></remarks>
        Public Sub AddCommand(Command As APIEntryPoint)
            Dim key$ = Command.Name.ToLower

            If Not apiTable.ContainsKey(key) Then
                Call apiTable.Add(key, Command)
            End If
        End Sub

        ''' <summary>
        ''' Gets the help information of a specific command using its name property value.(获取某一个命令的帮助信息)
        ''' </summary>
        ''' <param name="CommandName">If the paramteer command name value is a empty string then this function
        ''' will list all of the commands' help information.(假若本参数为空则函数会列出所有的命令的帮助信息)</param>
        ''' <returns>Error code, ZERO for no error</returns>
        ''' <remarks></remarks>
        <ExportAPI("?")>
        <Usage("? [CommandName]")>
        <Description("Show Application help")>
        <Example("? example_commandName")>
        Public Function Help(CommandName As String) As Integer
            If String.IsNullOrEmpty(CommandName) Then
                If Not Me.APIList.IsNullOrEmpty Then
                    ' List all commands when command name is empty.
                    Call Console.WriteLine(HelpSummary(False))
                End If
            Else
                ' listing the help for specific command name
                Call PrintCommandHelp(CommandName)
            End If

            Return 0
        End Function

        ''' <summary>
        ''' Returns the command entry info list array.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ListCommandInfo As EntryPoints.APIEntryPoint()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return apiTable.Values.ToArray
            End Get
        End Property

        Public ReadOnly Property Stack As String
        Public ReadOnly Property Info As [Namespace]

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="type">A module or a class which contains some shared method for the command entry.
        ''' (包含有若干使用<see cref="Reflection.ExportAPIAttribute"></see>进行标记的命令行执行入口点的Module或者Class对象类型，
        ''' 可以使用 Object.GetType/GetType 关键词操作来获取所需要的类型信息)</param>
        ''' <remarks></remarks>
        Sub New(type As Type, <CallerMemberName> Optional caller As String = Nothing)
            For Each cmd As APIEntryPoint In __getsAllCommands(type, False)
                Dim name As String = cmd.Name.ToLower

                If apiTable.ContainsKey(name) Then
                    Throw New Exception($"program's commandline argument {cmd.Name} is duplicated!")
                Else
                    Call apiTable.Add(name, cmd)
                End If
            Next

            Me.rootNamespace = type.Namespace
            Me._Stack = caller
            Me._Type = type
            Me._Info = type.NamespaceEntry(True)
        End Sub

        ''' <summary>
        ''' The CLI API container Module/Class type information.(申明这个解释器的命令行API容器类型)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Type As Type

        ''' <summary>
        ''' 导出所有符合条件的静态方法
        ''' </summary>
        ''' <param name="Type"></param>
        ''' <param name="[Throw]"></param>
        ''' <returns></returns>
        Protected Overridable Function __getsAllCommands(Type As Type, Optional [Throw] As Boolean = True) As List(Of EntryPoints.APIEntryPoint)
            Return GetAllCommands(Type, [Throw])
        End Function

        ''' <summary>
        ''' 导出所有符合条件的静态方法，请注意，在这里已经将外部的属性标记和所属的函数的入口点进行连接了
        ''' </summary>
        ''' <param name="type"></param>
        ''' <param name="[Throw]"></param>
        ''' <returns></returns>
        Public Shared Function GetAllCommands(type As Type, Optional [Throw] As Boolean = True) As List(Of EntryPoints.APIEntryPoint)
            If type Is Nothing Then
                Return New List(Of APIEntryPoint)
            End If

            Dim methods As MethodInfo() = type.GetMethods(BindingFlags.Public Or BindingFlags.Static)
            Dim commandAttribute As Type = GetType(ExportAPIAttribute)
            Dim commandsInfo = LinqAPI.MakeList(Of APIEntryPoint) <=
                                                                    _
                From methodInfo As MethodInfo
                In methods
                Let commandInfo As APIEntryPoint =
                    getAPI(methodInfo, commandAttribute, [Throw])
                Where Not commandInfo Is Nothing
                Select commandInfo
                Order By commandInfo.Name Ascending

            Return commandsInfo
        End Function

        ''' <summary>
        ''' 从方法定义<see cref="MethodInfo"/>之中解析出命令行的定义
        ''' </summary>
        ''' <param name="methodInfo"></param>
        ''' <param name="commandAttribute"></param>
        ''' <param name="[throw]"></param>
        ''' <returns></returns>
        Private Shared Function getAPI(methodInfo As MethodInfo, commandAttribute As Type, [throw] As Boolean) As APIEntryPoint
            Dim cmdAttr As ExportAPIAttribute = Nothing

            Try
                Dim attrs As Object() = methodInfo.GetCustomAttributes(commandAttribute, False)

                If attrs.IsNullOrEmpty Then
                    Return Nothing
                Else
                    cmdAttr = DirectCast(attrs(0), ExportAPIAttribute)
                End If

                Return doLoadApiInternal(cmdAttr, methodInfo, [throw])
            Catch ex As Exception
                If Not cmdAttr Is Nothing Then
                    ex = New Exception("This command API can not be imports: " & cmdAttr.GetJson, ex)
                    ex = New Exception(CheckNotice, ex)
                End If

                Call App.LogException(New Exception(methodInfo.FullName(True), ex))
                Call ex.PrintException

                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' 在这里将外部的属性标记和所属的函数的入口点进行连接
        ''' </summary>
        ''' <returns></returns>
        Private Shared Function doLoadApiInternal(cmdAttr As ExportAPIAttribute, methodInfo As MethodInfo, [throw] As Boolean) As APIEntryPoint
            Dim commandInfo As New APIEntryPoint(cmdAttr, methodInfo, [throw])

#Disable Warning
            If cmdAttr.Info.StringEmpty Then
                ' 帮助信息的获取兼容系统的Description方法
                cmdAttr.Info = methodInfo.Description([default]:="")
            End If
            If cmdAttr.Usage.StringEmpty Then
                ' 20240417
                '
                ' trim multiple line of the commandline usage text
                ' into one line. this is convient for copy to terminal 
                ' and modify value to use.
                cmdAttr.Usage = methodInfo.Usage _
                    .TrimNewLine _
                    .StringReplace("\s{2,}", " ")
            End If
            If cmdAttr.Example.StringEmpty Then
                cmdAttr.Example = methodInfo.ExampleInfo
            End If
#Enable Warning

            Return commandInfo
        End Function

        Const CheckNotice As String = "Please checks for the export api definition on your CLI interface function."

        ''' <summary>
        ''' Create an empty cli command line interpreter object which contains no commands entry.
        ''' (创建一个没有包含有任何命令入口点的空的CLI命令行解释器)
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function CreateEmptyCLIObject() As Interpreter
            Return New Interpreter(GetType(Interpreter))
        End Function

        ''' <summary>
        ''' Create a new interpreter instance from a specific type information.
        ''' (从目标类型之中构造出一个命令行解释器)
        ''' </summary>
        ''' <param name="Type"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <ExportAPI("CreateObject")>
        Public Shared Function CreateInstance(type As Type) As Interpreter
            Return New Interpreter(type)
        End Function

        ''' <summary>
        ''' Create a new interpreter instance using the specific type information.
        ''' (使用所制定的目标类型信息构造出一个CLI命令行解释器)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function CreateInstance(Of T As Class)() As Interpreter
            Return New Interpreter(type:=GetType(Type))
        End Function

#If NET_40 = 0 Then

        ''' <summary>
        ''' Create a new interpreter instance from a specific dll/exe path, this program assembly file should be a standard .NET assembly.
        ''' (从一个标准的.NET程序文件之中构建出一个命令行解释器)
        ''' </summary>
        ''' <param name="assmPath">DLL/EXE file path.(标准的.NET程序集文件的文件路径)</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        '''
        <ExportAPI("rundll")>
        Public Shared Function CreateInstance(assmPath As String) As Interpreter
            Dim assembly As Assembly = Assembly.LoadFrom(assmPath)
            Dim dllMain As Type = GetType(RunDllEntryPoint)
            Dim main As Type = LinqAPI.DefaultFirst(Of Type) _
                                                             _
                () <= From [mod] As Type
                      In assembly.DefinedTypes
                      Let attributes As Object() = [mod].GetCustomAttributes(dllMain, inherit:=False)
                      Where Not attributes Is Nothing AndAlso
                          attributes.Length = 1
                      Select [mod]

            If main Is Nothing Then
                Return Nothing  ' 没有找到执行入口点
            Else
                Return New Interpreter(main)
            End If
        End Function
#End If

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(      disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(      disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region

#Region "Implements System.Collections.Generic.IReadOnlyDictionary(Of String, CommandInfo)"

        Public Iterator Function GetEnumerator() As IEnumerator(Of KeyValuePair(Of String, EntryPoints.APIEntryPoint)) Implements IEnumerable(Of KeyValuePair(Of String, EntryPoints.APIEntryPoint)).GetEnumerator
            For Each key As String In Me.apiTable.Keys
                Yield New KeyValuePair(Of String, EntryPoints.APIEntryPoint)(key, Me.apiTable(key))
            Next
        End Function

        Public Iterator Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield Me.GetEnumerator
        End Function

        Public Sub Add(item As KeyValuePair(Of String, EntryPoints.APIEntryPoint)) Implements ICollection(Of KeyValuePair(Of String, EntryPoints.APIEntryPoint)).Add
            Call apiTable.Add(item.Key, item.Value)
        End Sub

        ''' <summary>
        ''' Clear the hash table of the cli command line interpreter command entry points.(清除本CLI解释器之中的所有的命令行执行入口点的哈希数据信息)
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub Clear() Implements ICollection(Of KeyValuePair(Of String, EntryPoints.APIEntryPoint)).Clear
            Call apiTable.Clear()
        End Sub

        Public Function Contains(item As KeyValuePair(Of String, EntryPoints.APIEntryPoint)) As Boolean Implements ICollection(Of KeyValuePair(Of String, EntryPoints.APIEntryPoint)).Contains
            Return apiTable.Contains(item)
        End Function

        Public Sub CopyTo(array() As KeyValuePair(Of String, EntryPoints.APIEntryPoint), arrayIndex As Integer) Implements ICollection(Of KeyValuePair(Of String, EntryPoints.APIEntryPoint)).CopyTo
            Call apiTable.ToArray.CopyTo(array, arrayIndex)
        End Sub

        ''' <summary>
        ''' Gets the command counts in current cli interpreter.(返回本CLI命令行解释器之中所包含有的命令的数目)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Count As Integer Implements ICollection(Of KeyValuePair(Of String, EntryPoints.APIEntryPoint)).Count
            Get
                Return Me.apiTable.Count
            End Get
        End Property

        Public ReadOnly Property IsReadOnly As Boolean Implements ICollection(Of KeyValuePair(Of String, EntryPoints.APIEntryPoint)).IsReadOnly
            Get
                Return False
            End Get
        End Property

        Public Function Remove(item As KeyValuePair(Of String, EntryPoints.APIEntryPoint)) As Boolean Implements ICollection(Of KeyValuePair(Of String, EntryPoints.APIEntryPoint)).Remove
            Return apiTable.Remove(item.Key)
        End Function

        Public Sub Add(key As String, value As EntryPoints.APIEntryPoint) Implements IDictionary(Of String, EntryPoints.APIEntryPoint).Add
            Call apiTable.Add(key, value)
        End Sub

        ''' <summary>
        ''' The target command line command is exists in this cli interpreter using it name property?(判断目标命令行命令是否存在于本CLI命令行解释器之中)
        ''' </summary>
        ''' <param name="CommandName">The command name value is not case sensitive.(命令的名称对大小写不敏感的)</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ExistsCommand(CommandName As String) As Boolean Implements IDictionary(Of String, EntryPoints.APIEntryPoint).ContainsKey
            Return Me.apiTable.ContainsKey(CommandName.ToLower)
        End Function

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="key">调用前需要转换为小写字母的形式</param>
        ''' <returns></returns>
        Default Public Overloads Property Item(key As String) As EntryPoints.APIEntryPoint Implements IDictionary(Of String, EntryPoints.APIEntryPoint).Item
            Get
                Return Me.apiTable(key)
            End Get
            Set(value As EntryPoints.APIEntryPoint)
                'DO NOTHING
            End Set
        End Property

        Public Function GetPossibleCommand(name As Value(Of String)) As APIEntryPoint
            Dim commands = ListingRelated(name)

            If commands.Length = 0 Then
                Return Nothing
            Else
                Return apiTable(commands.First.ToLower)
            End If
        End Function

        ''' <summary>
        ''' 列举出所有可能的命令
        ''' </summary>
        ''' <param name="query">模糊匹配</param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ListingRelated(query$) As String()
            Return Me.ListingRelatedCommands(query)
        End Function

        ''' <summary>
        ''' List all of the command line entry point name which were contains in this cli interpreter.
        ''' (列举出本CLI命令行解释器之中的所有的命令行执行入口点的名称)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property APINameList As ICollection(Of String) Implements IDictionary(Of String, EntryPoints.APIEntryPoint).Keys
            Get
                Return Me.apiTable.Keys
            End Get
        End Property

        Public Function Remove(CommandName As String) As Boolean Implements IDictionary(Of String, EntryPoints.APIEntryPoint).Remove
            Return apiTable.Remove(CommandName)
        End Function

        Public Function TryGetValue(key As String, ByRef value As EntryPoints.APIEntryPoint) As Boolean Implements IDictionary(Of String, EntryPoints.APIEntryPoint).TryGetValue
            Return Me.apiTable.TryGetValue(key, value)
        End Function

        ''' <summary>
        ''' 当前的解释器内所容纳的所有的CLI API列表
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property APIList As ICollection(Of EntryPoints.APIEntryPoint) Implements IDictionary(Of String, EntryPoints.APIEntryPoint).Values
            Get
                Return Me.apiTable.Values
            End Get
        End Property
#End Region
    End Class
End Namespace
