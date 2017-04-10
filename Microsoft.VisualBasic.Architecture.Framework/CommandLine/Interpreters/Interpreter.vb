#Region "Microsoft.VisualBasic::38322294e7a51ff6e0e5061b89a1cc36, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\CommandLine\Interpreters\Interpreter.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2016 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.CommandLine.Reflection.EntryPoints
Imports Microsoft.VisualBasic.ComponentModel.Settings
Imports Microsoft.VisualBasic.Debugging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Levenshtein

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
    <[Namespace]("Interpreter")>
    Public Class Interpreter

        Implements IDisposable
        Implements IDictionary(Of String, APIEntryPoint)

        ''' <summary>
        ''' 在添加之前请确保键名是小写的字符串
        ''' </summary>
        Protected __API_InfoHash As New Dictionary(Of String, APIEntryPoint)
        Protected _nsRoot As String

#Region "Optional delegates"

        ''' <summary>
        ''' Public Delegate Function __ExecuteFile(path As String, args As String()) As Integer,
        ''' (<seealso cref="__executefile"/>: 假若所传入的命令行的name是文件路径，解释器就会执行这个函数指针)
        ''' 这个函数指针一般是用作于执行脚本程序的
        ''' </summary>
        ''' <returns></returns>
        Public Property ExecuteFile As __ExecuteFile
        ''' <summary>
        ''' Public Delegate Function __ExecuteEmptyCli() As Integer,
        ''' (<seealso cref="__ExecuteEmptyCLI"/>: 假若所传入的命令行是空的，就会执行这个函数指针)
        ''' </summary>
        ''' <returns></returns>
        Public Property ExecuteEmptyCli As __ExecuteEmptyCLI
        Public Property ExecuteNotFound As __ExecuteNotFound
#End Region

        ''' <summary>
        ''' Gets the dictionary data which contains all of the available command information in this assembly module.
        ''' (获取从本模块之中获取得到的所有的命令行信息)
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ToDictionary() As Dictionary(Of String, APIEntryPoint)
            Return __API_InfoHash
        End Function

        Public Overrides Function ToString() As String
            Return "Cli://" & _nsRoot
        End Function

        ''' <summary>
        ''' Execute the specific command line using this interpreter.
        ''' </summary>
        ''' <param name="args">The user input command line string.</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Function Execute(args As CommandLine) As Integer
            If Not args.IsNullOrEmpty Then
                Dim i As Integer = __methodInvoke(args.Name.ToLower, {args}, args.Parameters)
#If DEBUG Then

#Else
                If Stack.TextEquals("Main") Then
                    If AutoPaused Then
                        Call Pause()
                    End If
                End If
#End If
                Return i
            Else
                Return __executeEmpty() ' 命令行是空的
            End If
        End Function

        ''' <summary>
        ''' 命令行是空的
        ''' </summary>
        ''' <returns></returns>
        Private Function __executeEmpty() As Integer
            If Not ExecuteEmptyCli Is Nothing Then
                Try
                    Return _ExecuteEmptyCli()
                Catch ex As Exception
                    Call App.LogException(ex)
                    Call ex.PrintException
                End Try

                Return -100
            Else
                Return -1
            End If
        End Function

        ''' <summary>
        ''' The interpreter runs all of the command from here.(所有的命令行都从这里开始执行)
        ''' </summary>
        ''' <param name="commandName"></param>
        ''' <param name="argvs">就只有一个命令行对象</param>
        ''' <param name="help_argvs"></param>
        ''' <returns></returns>
        Private Function __methodInvoke(commandName As String, argvs As Object(), help_argvs As String()) As Integer

            If __API_InfoHash.ContainsKey(commandName) Then _
                Return __API_InfoHash(commandName).Execute(argvs)

            If "??vars".TextEquals(commandName) Then
                Dim vars = App.GetAppVariables

                Call Console.WriteLine()
                Call Console.WriteLine(PS1.Fedora12.ToString)
                Call Console.WriteLine()
                Call Console.WriteLine($"Print environment variables for {GetType(App).FullName}:")
                Call Console.WriteLine(ConfigEngine.Prints(vars))

                Return 0

            ElseIf String.Equals(commandName, "?") OrElse commandName = "??" OrElse commandName.TextEquals("--help") Then
                If help_argvs.IsNullOrEmpty Then
                    Return Help("")
                Else
                    Return Help(help_argvs.First)
                End If

            ElseIf InStr(commandName, "??") = 1 Then  ' 支持类似于R语言里面的 ??帮助命令
                commandName = Mid(commandName, 3)
                Return Help(commandName)

            ElseIf String.Equals(commandName, "~") Then  ' 打印出应用程序的位置，linux里面的HOME
                Call Console.WriteLine(App.ExecutablePath)
                Return 0

            ElseIf String.Equals(commandName, "man") Then  ' 默认是分段打印帮助信息，假若加上了  --print参数的话，则才会一次性的打印所有的信息出来
                Dim CLI As CommandLine = DirectCast(argvs(Scan0), CommandLine)
                Dim doc As String = SDKdocs()

                If Not CLI.GetBoolean("--file") Then
                    If CLI.GetBoolean("--print") Then
                        Call Console.WriteLine(doc)
                    Else
                        Call SDKManual.LaunchManual(CLI:=Me)
                    End If
                Else
                    ' 只会写文件而不会在终端打开帮助窗口
                End If

                Return doc.SaveTo(DocPath, Encoding.UTF8).CLICode

            ElseIf String.Equals(commandName, "/linux-bash", StringComparison.OrdinalIgnoreCase) Then
                Return BashShell()

            Else
                If (commandName.FileExists OrElse commandName.DirectoryExists) AndAlso Not Me.ExecuteFile Is Nothing Then  '命令行的名称和上面的都不符合，但是可以在文件系统之中找得到一个相应的文件，则执行文件句柄
                    Try
                        Return ExecuteFile()(path:=commandName, args:=DirectCast(argvs(Scan0), CommandLine))
                    Catch ex As Exception
                        ex = New Exception("Execute file failure!", ex)
                        ex = New Exception(argvs(Scan0).ToString, ex)
                        Call App.LogException(ex)
                        Call ex.PrintException
                    End Try

                    Return -120
                ElseIf Not ExecuteNotFound Is Nothing Then
                    Try
                        Return ExecuteNotFound()(DirectCast(argvs(Scan0), CommandLine))
                    Catch ex As Exception
                        ex = New Exception("Execute not found failure!", ex)
                        ex = New Exception(argvs(Scan0).ToString, ex)
                        Call App.LogException(ex)
                        Call ex.PrintException
                    End Try

                    Return -1000
                Else
                    Dim lst As String() = Me.ListPossible(commandName)

                    If lst.IsNullOrEmpty Then

                        Call Console.WriteLine(BAD_COMMAND_NAME, commandName)
                        Call Console.WriteLine()
                        Call Console.WriteLine(PS1.Fedora12.ToString & " " & DirectCast(argvs(Scan0), CommandLine).ToString)

                    Else
                        Call Console.WriteLine(BAD_COMMAND_MAN, commandName)

                        For Each name As String In lst
                            Call Console.WriteLine("    " & name)
                        Next
                    End If
                End If
            End If

            Return -1
        End Function

        Const BAD_COMMAND_MAN As String = "Bad command, no such a command named ""{0}"", but you probably want to use commands:"
        Const BAD_COMMAND_NAME As String = "Bad command, no such a command named ""{0}"", ? for command list or ""man"" for all of the commandline detail informations."

        ''' <summary>
        ''' Generate the sdk document for the target program assembly.(生成目标应用程序的命令行帮助文档，markdown格式的)
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
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
            Dim i As Integer = __methodInvoke(CommandName, argvs, help_argvs:=argvs)
#If DEBUG Then
            Call Pause()
#Else
            If Stack.TextEquals("Main") Then
                If AutoPaused Then
                    Call Pause()
                End If
            End If
#End If
            Return i
        End Function

        Public Function Execute(CommandName As String, args As String()) As Integer
            Return __methodInvoke(CommandName.ToLower, args, help_argvs:=args)
        End Function

        ''' <summary>
        ''' Add a command in current cli interpreter.(x向当前的这个CLI命令行解释器之中添加一个命令)
        ''' </summary>
        ''' <param name="Command"></param>
        ''' <remarks></remarks>
        Public Sub AddCommand(Command As APIEntryPoint)
            Dim NameId As String = Command.Name.ToLower

            If Not __API_InfoHash.ContainsKey(NameId) Then
                Call __API_InfoHash.Add(NameId, Command)
            End If
        End Sub

        ''' <summary>
        ''' Gets the help information of a specific command using its name property value.(获取某一个命令的帮助信息)
        ''' </summary>
        ''' <param name="CommandName">If the paramteer command name value is a empty string then this function
        ''' will list all of the commands' help information.(假若本参数为空则函数会列出所有的命令的帮助信息)</param>
        ''' <returns>Error code, ZERO for no error</returns>
        ''' <remarks></remarks>
        <ExportAPI("?", Usage:="? [CommandName]", Info:="Show Application help", Example:="? example_commandName")>
        Public Function Help(CommandName As String) As Integer
            If String.IsNullOrEmpty(CommandName) Then     ' List all commands when command name is empty.
                Call Console.WriteLine(HelpSummary(False))
            Else ' listing the help for specific command name
                Dim name As New Value(Of String)

                If __API_InfoHash.ContainsKey(name = CommandName.ToLower) Then
                    Call __API_InfoHash(name).PrintHelp
                Else
                    Dim lst As String() = Me.ListPossible(CommandName)

                    If lst.IsNullOrEmpty Then
                        Call Console.WriteLine($"Bad command, no such a command named ""{CommandName}"", ? for command list.")
                        Call Console.WriteLine()
                        Call Console.WriteLine(PS1.Fedora12.ToString & " ?" & CommandName)
                    Else
                        Call Console.WriteLine(BAD_COMMAND_MAN, CommandName)

                        For Each cName As String In lst
                            Call Console.WriteLine("    " & cName)
                        Next
                    End If

                    Return -2
                End If
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
            Get
                Return __API_InfoHash.Values.ToArray
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
            For Each cInfo As APIEntryPoint In __getsAllCommands(type, False)
                If __API_InfoHash.ContainsKey(cInfo.Name.ToLower) Then
                    Throw New Exception(cInfo.Name & " is duplicated with other command!")
                Else
                    Call __API_InfoHash.Add(cInfo.Name.ToLower, cInfo)
                End If
            Next

            Me._nsRoot = type.Namespace
            Me._Stack = caller
            Me._Type = type
            Me._Info = type.NamespaceEntry
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
        ''' <param name="Type"></param>
        ''' <param name="[Throw]"></param>
        ''' <returns></returns>
        Public Shared Function GetAllCommands(Type As Type, Optional [Throw] As Boolean = True) As List(Of EntryPoints.APIEntryPoint)
            If Type Is Nothing Then
                Return New List(Of EntryPoints.APIEntryPoint)
            End If

            Dim Methods As MethodInfo() = Type.GetMethods(BindingFlags.Public Or BindingFlags.Static)
            Dim commandAttribute As Type = GetType(ExportAPIAttribute)

            Dim commandsInfo As List(Of APIEntryPoint) =
                LinqAPI.MakeList(Of APIEntryPoint) <=
 _
                From methodInfo As MethodInfo
                In Methods
                Let commandInfo As APIEntryPoint =
                    __getsAPI(methodInfo, commandAttribute, [Throw])
                Where Not commandInfo Is Nothing
                Select commandInfo
                Order By commandInfo.Name Ascending

            Return commandsInfo
        End Function

        Private Shared Function __getsAPI(methodInfo As MethodInfo, commandAttribute As System.Type, [throw] As Boolean) As EntryPoints.APIEntryPoint
            Dim cmdAttr As ExportAPIAttribute = Nothing
            Dim commandInfo As APIEntryPoint

            Try
                Dim attrs As Object() = methodInfo.GetCustomAttributes(commandAttribute, False)
                If attrs.IsNullOrEmpty Then Return Nothing

                cmdAttr = DirectCast(attrs(0), ExportAPIAttribute)
                commandInfo = New APIEntryPoint(cmdAttr, methodInfo, [throw]) '在这里将外部的属性标记和所属的函数的入口点进行连接
                Return commandInfo
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

        Const CheckNotice As String = "Please checks for the export api definition on your CLI interface function."

        ''' <summary>
        ''' Create an empty cli command line interpreter object which contains no commands entry.
        ''' (创建一个没有包含有任何命令入口点的空的CLI命令行解释器)
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
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
        '''
        <ExportAPI("CreateObject")>
        Public Shared Function CreateInstance(Type As System.Type) As Interpreter
            Return New Interpreter(Type)
        End Function

        ''' <summary>
        ''' Create a new interpreter instance using the specific type information.
        ''' (使用所制定的目标类型信息构造出一个CLI命令行解释器)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <returns></returns>
        ''' <remarks></remarks>
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
            Dim main As Type =
                LinqAPI.DefaultFirst(Of Type) <= From [mod] As Type
                                                 In assembly.DefinedTypes
                                                 Let attributes As Object() =
                                                     [mod].GetCustomAttributes(dllMain, inherit:=False)
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
            For Each key As String In Me.__API_InfoHash.Keys
                Yield New KeyValuePair(Of String, EntryPoints.APIEntryPoint)(key, Me.__API_InfoHash(key))
            Next
        End Function

        Public Iterator Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield Me.GetEnumerator
        End Function

        Public Sub Add(item As KeyValuePair(Of String, EntryPoints.APIEntryPoint)) Implements ICollection(Of KeyValuePair(Of String, EntryPoints.APIEntryPoint)).Add
            Call __API_InfoHash.Add(item.Key, item.Value)
        End Sub

        ''' <summary>
        ''' Clear the hash table of the cli command line interpreter command entry points.(清除本CLI解释器之中的所有的命令行执行入口点的哈希数据信息)
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub Clear() Implements ICollection(Of KeyValuePair(Of String, EntryPoints.APIEntryPoint)).Clear
            Call __API_InfoHash.Clear()
        End Sub

        Public Function Contains(item As KeyValuePair(Of String, EntryPoints.APIEntryPoint)) As Boolean Implements ICollection(Of KeyValuePair(Of String, EntryPoints.APIEntryPoint)).Contains
            Return __API_InfoHash.Contains(item)
        End Function

        Public Sub CopyTo(array() As KeyValuePair(Of String, EntryPoints.APIEntryPoint), arrayIndex As Integer) Implements ICollection(Of KeyValuePair(Of String, EntryPoints.APIEntryPoint)).CopyTo
            Call __API_InfoHash.ToArray.CopyTo(array, arrayIndex)
        End Sub

        ''' <summary>
        ''' Gets the command counts in current cli interpreter.(返回本CLI命令行解释器之中所包含有的命令的数目)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Count As Integer Implements ICollection(Of KeyValuePair(Of String, EntryPoints.APIEntryPoint)).Count
            Get
                Return Me.__API_InfoHash.Count
            End Get
        End Property

        Public ReadOnly Property IsReadOnly As Boolean Implements ICollection(Of KeyValuePair(Of String, EntryPoints.APIEntryPoint)).IsReadOnly
            Get
                Return False
            End Get
        End Property

        Public Function Remove(item As KeyValuePair(Of String, EntryPoints.APIEntryPoint)) As Boolean Implements ICollection(Of KeyValuePair(Of String, EntryPoints.APIEntryPoint)).Remove
            Return __API_InfoHash.Remove(item.Key)
        End Function

        Public Sub Add(key As String, value As EntryPoints.APIEntryPoint) Implements IDictionary(Of String, EntryPoints.APIEntryPoint).Add
            Call __API_InfoHash.Add(key, value)
        End Sub

        ''' <summary>
        ''' The target command line command is exists in this cli interpreter using it name property?(判断目标命令行命令是否存在于本CLI命令行解释器之中)
        ''' </summary>
        ''' <param name="CommandName">The command name value is not case sensitive.(命令的名称对大小写不敏感的)</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ExistsCommand(CommandName As String) As Boolean Implements IDictionary(Of String, EntryPoints.APIEntryPoint).ContainsKey
            Return Me.__API_InfoHash.ContainsKey(CommandName.ToLower)
        End Function

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="key">调用前需要转换为小写字母的形式</param>
        ''' <returns></returns>
        Default Public Overloads Property Item(key As String) As EntryPoints.APIEntryPoint Implements IDictionary(Of String, EntryPoints.APIEntryPoint).Item
            Get
                Return Me.__API_InfoHash(key)
            End Get
            Set(value As EntryPoints.APIEntryPoint)
                'DO NOTHING
            End Set
        End Property

        Public Function GetPossibleCommand(name As Value(Of String)) As EntryPoints.APIEntryPoint
            If Me.__API_InfoHash.ContainsKey(name = (+name).ToLower) Then
                Return __API_InfoHash(+name)
            Else
                Dim LQuery = (From x As KeyValuePair(Of String, APIEntryPoint)
                              In __API_InfoHash
                              Let similarity = LevenshteinDistance.ComputeDistance(x.Key, name)
                              Where Not similarity Is Nothing
                              Select similarity.Score,
                                  x.Value
                              Order By Score Descending).ToArray

                If LQuery.IsNullOrEmpty Then
                    Return Nothing
                Else
                    Return LQuery.First.Value
                End If
            End If
        End Function

        ''' <summary>
        ''' 列举出所有可能的命令
        ''' </summary>
        ''' <param name="Name">模糊匹配</param>
        ''' <returns></returns>
        Public Function ListPossible(Name As String) As String()
            Dim key As String = Name.ToLower
            Dim LQuery = From x As String
                         In __API_InfoHash.Keys.AsParallel
                         Let lev = LevenshteinDistance.ComputeDistance(x, key)
                         Where Not lev Is Nothing AndAlso
                              lev.Score > 0.3
                         Select lev.Score,
                             x
                         Order By Score Descending

            Return LQuery.ToArray(Function(x) x.x)
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
                Return Me.__API_InfoHash.Keys
            End Get
        End Property

        Public Function Remove(CommandName As String) As Boolean Implements IDictionary(Of String, EntryPoints.APIEntryPoint).Remove
            Return __API_InfoHash.Remove(CommandName)
        End Function

        Public Function TryGetValue(key As String, ByRef value As EntryPoints.APIEntryPoint) As Boolean Implements IDictionary(Of String, EntryPoints.APIEntryPoint).TryGetValue
            Return Me.__API_InfoHash.TryGetValue(key, value)
        End Function

        ''' <summary>
        ''' 当前的解释器内所容纳的所有的CLI API列表
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property APIList As ICollection(Of EntryPoints.APIEntryPoint) Implements IDictionary(Of String, EntryPoints.APIEntryPoint).Values
            Get
                Return Me.__API_InfoHash.Values
            End Get
        End Property
#End Region
    End Class
End Namespace
