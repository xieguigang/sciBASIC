#Region "Microsoft.VisualBasic::85c2c17cc1d069e22803ee5d177402fa, Microsoft.VisualBasic.Core\src\CommandLine\CommandLine.vb"

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

'   Total Lines: 879
'    Code Lines: 457 (51.99%)
' Comment Lines: 321 (36.52%)
'    - Xml Docs: 89.72%
' 
'   Blank Lines: 101 (11.49%)
'     File Size: 37.33 KB


'     Class CommandLine
' 
'         Properties: BoolFlags, cli, Count, EnvironmentVariables, IsNothing
'                     IsNullOrEmpty, IsReadOnly, Keys, Name, ParameterList
'                     Parameters, SingleValue, Tokens
' 
'         Function: Assert, (+2 Overloads) BuildFromArguments, CheckMissingRequiredArguments, CheckMissingRequiredParameters, Contains
'                   ContainsParameter, GetDataReader, GetDictionary, GetEnumerator, GetEnumerator1
'                   GetFullDIRPath, GetFullFilePath, GetObject, GetOrdinal, GetSize
'                   (+2 Overloads) GetString, GetValue, hasKey, HavebFlag, IsTrue
'                   MoveNext, OpenHandle, OpenStreamInput, OpenStreamOutput, Parse
'                   ParseTokens, ReadInput, (+2 Overloads) Remove, ToArgumentVector, ToString
'                   TrimNamePrefix
' 
'         Sub: (+2 Overloads) Add, Clear, CopyTo
' 
'         Operators: (+4 Overloads) -, ^, +, <, (+2 Overloads) <=
'                    >, (+2 Overloads) >=
' 
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.CommandLine.Parsers
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting
Imports Microsoft.VisualBasic.Scripting.Expressions
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Parser
Imports StringReader = Microsoft.VisualBasic.ComponentModel.DataSourceModel.StringReader

Namespace CommandLine

    ' file path: C://path/to/file
    ' standard input: std_in://
    ' standard output: std_out://
    ' memory mapping file: memory://file/uri

    ''' <summary>
    ''' A command line object that parse from the user input commandline string.
    ''' (从用户所输入的命令行字符串之中解析出来的命令行对象，标准的命令行格式为：
    ''' <example>&lt;EXE> &lt;CLI_Name> ["Parameter" "Value"]</example>)
    ''' </summary>
    ''' <remarks></remarks>
    '''
    Public Class CommandLine
        Implements ICollection(Of NamedValue(Of String))
        Implements INamedValue
        Implements IStringGetter

        Friend arguments As New List(Of NamedValue(Of String))
        ''' <summary>
        ''' 原始的命令行字符串
        ''' </summary>
        Friend cliCommandArgvs As String

        Dim _name As String

        ''' <summary>
        ''' The command name that parse from the input command line.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>
        ''' (从输入的命令行中所解析出来的命令的名称)
        ''' </remarks>
        Public Property Name As String Implements INamedValue.Key
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return _name
            End Get
            Protected Friend Set(value As String)
                _name = value
            End Set
        End Property

        ''' <summary>
        ''' The command tokens that were parsed from the input commandline.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>
        ''' (从所输入的命令行之中所解析出来的命令参数单元)
        ''' </remarks>
        Public Property Tokens As String()

        ''' <summary>
        ''' Listing all of the parameter value collection that parsed from the commandline string.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ParameterList As NamedValue(Of String)()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return arguments.ToArray
            End Get
        End Property

        ''' <summary>
        ''' 得到当前的命令行对象之中的所有的参数的名称的列表
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Keys As String()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return arguments.Select(Function(v) v.Name).ToArray
            End Get
        End Property

        ''' <summary>
        ''' The parameters in the commandline without the first token of the command name.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>
        ''' (将命令行解析为词元之后去掉命令的名称之后所剩下的所有的字符串列表)
        ''' </remarks>
        Public ReadOnly Property Parameters As String()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Tokens.Skip(1).ToArray
            End Get
        End Property

        ''' <summary>
        ''' 对于参数而言，都是--或者-或者/或者\开头的，下一个单词为单引号或者非上面的字符开头的，例如/o &lt;path>
        ''' 对于开关而言，与参数相同的其实符号，但是后面不跟参数而是其他的开关，通常开关用来进行简要表述一个逻辑值
        ''' </summary>
        ''' <returns></returns>
        Public Property BoolFlags As String()

        ''' <summary>
        ''' 获取得到通过``/@set``参数所传入的环境变量(键值对之间使用分号分隔)
        ''' </summary>
        ''' <returns>
        ''' this readonly property ensure that the result dictionary is always not null, but may be empty.
        ''' </returns>
        Public ReadOnly Property EnvironmentVariables As Dictionary(Of String, String)
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                ' 在命令行之中没有任何其他的参数，但是存在一个/@set的话，这个标记会被当作为命令名称
                ' 则在读取环境变量的时候就会失败
                If Name.TextEquals("/@set") Then
                    Return DictionaryParser.TryParse(Parameters(Scan0))
                Else
                    Return If(GetDictionary("/@set"), New Dictionary(Of String, String))
                End If
            End Get
        End Property

        ''' <summary>
        ''' Get the original command line string.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>
        ''' (获取所输入的命令行对象的原始的字符串)
        ''' </remarks>
        Public ReadOnly Property cli As String
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return cliCommandArgvs
            End Get
        End Property

        ''' <summary>
        ''' The parameter name is not case sensitive.
        ''' </summary>
        ''' <param name="paramName">
        ''' The argument name in the commandline.
        ''' 
        ''' ##### 2018-09-10 
        ''' 为了兼容VB的!字典取值语法，这个属性是会自动处理开关参数的前缀的
        ''' 即会自动的将开关参数的/\--等前缀删除尝试进行取值
        ''' 这个自动转换不会应用于逻辑开关参数上面
        ''' </param>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>
        ''' (开关的名称是不区分大小写的，进行字符串插值脚本化处理的时候，是使用的<see cref="App.GetVariable"/>函数来获取环境变量值)
        ''' </remarks>
        Default Public ReadOnly Property Item(paramName As String) As DefaultString
            Get
                Dim LQuery As NamedValue(Of String) = arguments _
                    .Where(Function(a)
                               Return a.Name.TextEquals(paramName) OrElse
                                      a.Name.DoCall(AddressOf TrimNamePrefix) _
                                            .TextEquals(paramName)
                           End Function) _
                    .FirstOrDefault

                ' 是值类型，不会出现空引用的情况
                Dim value As String = LQuery.Value

                If value.StringEmpty Then
                    ' 2018-1-22
                    '
                    ' 如果是需要获取逻辑值的话，直接查找__arguments值列表是获取不到结果的
                    ' 在这里使用IsTrue来判断，如果开关存在则返回TRUE字符串
                    ' 否则返回空字符串表示不存在

                    If HavebFlag(paramName) Then
                        value = "TRUE"
                    Else
                        value = ""
                    End If
                Else
                    ' 尝试进行字符串插值，从而实现命令行部分脚本化

                    ' 2017-3-13
                    ' 对于Windows文件路径而言， 不推荐转义
                    ' 因为Windows的文件路径分隔符为\，很容易引起误解，例如C:\tsv会被误转义为C:<TAB>sv而导致错误
                    ' 所以在这里关闭escape参数选项
                    value = value.Interpolate(environment, escape:=False)
                End If

                Return New DefaultString(value)
            End Get
        End Property

        ReadOnly environment As Func(Of String, String) = AddressOf App.GetVariable

        Public Property SingleValue As String

        ''' <summary>
        ''' See if the target logical flag argument is exists in the commandline?
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns>(查看命令行之中是否存在某一个逻辑开关)</returns>
        Public Function HavebFlag(name As String) As Boolean
            If Me.BoolFlags.IsNullOrEmpty Then
                Return False
            Else
                ' boolflags 已经全部都被转换为小写形式了
                Return Array.IndexOf(BoolFlags, name.ToLower) > -1
            End If
        End Function

        ''' <summary>
        ''' Returns the original cli command line argument string.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' (返回所传入的命令行的原始字符串)
        ''' </remarks>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return cliCommandArgvs
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function TrimNamePrefix(argName As String) As String
            Return argName.Trim("\", "/", "-")
        End Function

        ''' <summary>
        ''' Get specific argument value as full directory path.
        ''' </summary>
        ''' <param name="name">parameter name</param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetFullDIRPath(name As String) As String
            Return FileIO.FileSystem.GetDirectoryInfo(Me(name)).FullName
        End Function

        ''' <summary>
        ''' Get specific argument value as full file path.
        ''' </summary>
        ''' <param name="name">parameter name</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' (这个函数还会同时修正file://协议的头部)
        ''' </remarks>
        Public Function GetFullFilePath(name As String) As String
            Dim path$ = Me(name)
            path = FixPath(path)
            Return FileIO.FileSystem.GetFileInfo(path).FullName
        End Function

        Public Function Required(name As String, error$) As String
            Dim val As String = Me(name)

            If String.IsNullOrEmpty(val) Then
                Throw New InvalidProgramException([error])
            Else
                Return val
            End If
        End Function

        ''' <summary>
        ''' Checking for the missing required parameter, this function will returns the missing parameter
        ''' in the current cli command line object using a specific parameter name list.
        ''' </summary>
        ''' <param name="list"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' (检查<paramref name="list"></paramref>之中的所有参数是否存在，函数会返回不存在的参数名)
        ''' </remarks>
        Public Function CheckMissingRequiredParameters(list As IEnumerable(Of String)) As String()
            Dim LQuery$() = LinqAPI.Exec(Of String) _
                                                    _
                () <= From p As String
                      In list
                      Where String.IsNullOrEmpty(Me(p))
                      Select p

            Return LQuery
        End Function

        ''' <summary>
        ''' Gets a list of missing required argument name.
        ''' </summary>
        ''' <param name="args"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function CheckMissingRequiredArguments(ParamArray args As String()) As String()
            Return CheckMissingRequiredParameters(list:=args)
        End Function

        ''' <summary>
        ''' Does this cli command line object contains any parameter argument information.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>
        ''' (查看本命令行参数对象之中是否存在有参数信息)
        ''' </remarks>
        Public ReadOnly Property IsNullOrEmpty As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Tokens.IsNullOrEmpty OrElse (Tokens.Length = 1 AndAlso String.IsNullOrEmpty(Tokens.First))
            End Get
        End Property

        ''' <summary>
        ''' <see cref="String.IsNullOrEmpty"/> of <see cref="Name"/> AndAlso <see cref="IsNullOrEmpty"/>
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsNothing As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return String.IsNullOrEmpty(Me.Name) AndAlso IsNullOrEmpty
            End Get
        End Property

        ''' <summary>
        ''' Does the specific argument exists in this commandline? argument name is not case sensitity.
        ''' </summary>
        ''' <param name="parameterName"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' (参数名称字符串大小写不敏感)
        ''' </remarks>
        Public Function ContainsParameter(parameterName As String, Optional trim As Boolean = False) As Boolean
            Dim namer As String = If(trim, parameterName.TrimParamPrefix, parameterName)
            Dim LQuery = LinqAPI.DefaultFirst(Of Integer) _
                                                          _
                () <= From para As NamedValue(Of String)
                      In Me.arguments  '  名称都是没有处理过的
                      Where String.Equals(namer, para.Name, StringComparison.OrdinalIgnoreCase)
                      Select 100

            Return LQuery > 50
        End Function

        Private Function hasKey(name As String) As Boolean Implements IStringGetter.HasKey
            Return ContainsParameter(name)
        End Function

        ''' <summary>
        ''' Parsing the commandline string as object model
        ''' </summary>
        ''' <param name="CommandLine"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(CommandLine As String) As CommandLine
            Return Parsers.TryParse(CommandLine)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(CLI As Func(Of String)) As CommandLine
            Return Parsers.TryParse(CLI())
        End Operator

        ''' <summary>
        ''' Determined that the specific Boolean flag is exists or not? 
        ''' if not then returns <paramref name="failure"/>, if exists such flag, then returns the <paramref name="name"/>.
        ''' </summary>
        ''' <param name="name">Boolean flag name</param>
        ''' <param name="failure"></param>
        ''' <returns></returns>
        Public Function Assert(name As String, Optional failure As String = "") As String
            If IsTrue(name) Then
                Return name
            Else
                Return failure
            End If
        End Function

        ''' <summary>
        ''' If the target parameter is not presents in the CLI, then this function will returns nothing.
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' (键值对之间使用分号分隔)
        ''' </remarks>
        Public Function GetDictionary(name$, Optional default$ = Nothing) As Dictionary(Of String, String)
            Dim s$ = Me(name$)

            If String.IsNullOrEmpty(s$) Then
                If [default].StringEmpty Then
                    Return Nothing
                Else
                    Return DictionaryParser.TryParse([default])
                End If
            Else
                Return DictionaryParser.TryParse(s$)
            End If
        End Function

        ''' <summary>
        ''' Gets the value Of the specified column As a Boolean.
        ''' </summary>
        ''' <param name="parameter">可以包含有开关参数</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' (这个函数也同时包含有开关参数的，开关参数默认为逻辑值类型，当包含有开关参数的时候，其逻辑值为True，反之函数会检查参数列表，参数不存在则为空值字符串，则也为False)
        ''' </remarks>
        Public Function IsTrue(parameter$) As Boolean
            If Me.HavebFlag(parameter) Then
                Return True
            End If
            Return Me(parameter).DefaultValue.ParseBoolean
        End Function

#Region "Pipeline"

        ''' <summary>About <paramref name="s"/>:
        ''' 
        ''' + If the file path is not a value path, then is the value is not null, the argument value will be returned from this parameter. 
        ''' + If the value is nothing, then this function will open the standard input as input.
        ''' + If the file path is valid as input file, then a local file system pointer will be returned.
        ''' 
        ''' [管道函数] 假若参数名存在并且所指向的文件也存在，则返回本地文件的文件指针，否则返回标准输入的指针
        ''' </summary>
        ''' <param name="param"></param>
        ''' <param name="s">
        ''' + If the file path is not a value path, then is the value is not null, the argument value will be returned from this parameter. 
        ''' + If the value is nothing, then this function will open the standard input as input.
        ''' + If the file path is valid as input file, then a local file system pointer will be returned.
        ''' </param>
        ''' <returns></returns>
        Public Function OpenStreamInput(param As String, Optional ByRef s As String = Nothing) As StreamReader
            Dim path As String = Me(param)
            Dim type As FileTypes = StreamExtensions.FileType(path)

            Select Case type
                Case FileTypes.MemoryFile, FileTypes.PipelineFile
                    Return New StreamReader(StreamExtensions.OpenForRead(path))
                Case Else
                    If path.FileExists Then
                        Return New StreamReader(New FileStream(path, FileMode.Open, access:=FileAccess.Read))
                    ElseIf Not String.IsNullOrEmpty(path) Then
                        s = path
                        Return Nothing
                    Else
                        Return New StreamReader(Console.OpenStandardInput)
                    End If
            End Select
        End Function

        ''' <summary>
        ''' If the <see cref="StreamWriter.BaseStream"/> is <see cref="FileStream"/>, then it means not a ``std_out`` pointer.
        ''' ([管道函数] 假若参数名存在，则返回本地文件的文件指针，否则返回标准输出的指针)
        ''' </summary>
        ''' <param name="param"></param>
        ''' <returns></returns>
        Public Function OpenStreamOutput(param$, Optional encoding As Encodings = Encodings.UTF8, Optional size& = 512 * 1024 * 1024) As StreamWriter
            Dim path$ = Me(param)
            Dim type As FileTypes = StreamExtensions.FileType(path)

            Select Case type
                Case FileTypes.MemoryFile, FileTypes.PipelineFile
                    Return New StreamWriter(StreamExtensions.OpenForWrite(path, size))
                Case Else
                    If path.StringEmpty Then
                        Return New StreamWriter(Console.OpenStandardOutput, encoding.CodePage)
                    Else
                        Return path.OpenWriter(encoding)
                    End If
            End Select
        End Function

        ''' <summary>
        ''' Read all of the text input from the file or ``std_in``
        ''' </summary>
        ''' <remarks>
        ''' 这个函数会首先尝试使用<see cref="OpenStreamInput"/>打开本地文件和标准输出流
        ''' 如果失败的话<see cref="OpenStreamInput"/>函数会返回空值，这个时候参数字符串将会直接被
        ''' 返回作为结果，如果打开成功的话，会将得到的输入流之中的所有字符串读出来返回
        ''' </remarks>
        ''' <param name="param"></param>
        ''' <returns></returns>
        Public Function ReadInput(param As String) As String
            Dim s As String = Nothing
            Dim read As StreamReader = OpenStreamInput(param, s)

            If read Is Nothing Then
                Return s
            Else
                Return read.ReadToEnd
            End If
        End Function
#End Region

#Region "IDataRecord Methods"

        ''' <summary>
        ''' Return the index Of the named field. If the name is not exists in the parameter list, then a -1 value will be return.
        ''' </summary>
        ''' <returns></returns>
        Public Function GetOrdinal(parameter As String) As Integer Implements IStringGetter.GetOrdinal
            Dim i% = LinqAPI.DefaultFirst(Of Integer)(-1) _
                                                          _
                <= From entry As NamedValue(Of String)
                   In Me.arguments
                   Where String.Equals(parameter, entry.Name, StringComparison.OrdinalIgnoreCase)
                   Select arguments.IndexOf(entry)

            Return i
        End Function

        ''' <summary>
        ''' Gets the String value Of the specified field.
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetString(parameter As String) As String Implements IStringGetter.GetString
            If IsTrue(parameter) Then
                Return "true"
            End If

            Return Me(parameter)
        End Function

        ''' <summary>
        '''
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="parameter">Command parameter name in the command line inputs.</param>
        ''' <param name="__getObject"></param>
        ''' <returns></returns>
        Public Function GetObject(Of T)(parameter$, Optional __getObject As Func(Of String, T) = Nothing) As T
            Dim value$ = Me(parameter)
            Dim obj = (__getObject Or StringParser(GetType(T)))(arg:=value)
            Dim x As T = DirectCast(obj, T)
            Return x
        End Function

        ''' <summary>
        ''' If the given parameter is not exists in the user input arguments, then a developer specific default value will be return.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="name">The optional argument parameter name</param>
        ''' <param name="[default]">The default value for returns when the parameter is not exists in the user input.</param>
        ''' <param name="cast">The custom string parser for the CLI argument value</param>
        ''' <returns></returns>
        Public Function GetValue(Of T)(name$, [default] As T, Optional cast As Func(Of String, T) = Nothing) As T
            If Not Me.ContainsParameter(name, False) Then
                If GetType(T).Equals(GetType(Boolean)) Then
                    If HavebFlag(name) Then
                        Return CType(CObj(True), T)
                    End If
                End If

                Return [default]
            End If

            Dim str As String = Me(name).DefaultValue

            If cast Is Nothing Then
                Dim value As Object = InputHandler.CTypeDynamic(str, GetType(T))
                Return DirectCast(value, T)
            Else
                Return cast(str)
            End If
        End Function

        ''' <summary>
        ''' Open a file handle by using the parameter value
        ''' </summary>
        ''' <param name="name">The parameter name, and its argument value should be a valid file path</param>
        ''' <param name="[default]">Default file path if the argument value is not exists</param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function OpenHandle(name$, Optional default$ = "", Optional encoding As Encodings = Encodings.UTF8) As i32
            Return My.File.OpenHandle(Me(name) Or [default].AsDefault, encoding)
        End Function
#End Region

#Region "Implements IReadOnlyCollection(Of KeyValuePair(Of String, String))"

        ''' <summary>
        ''' 这个枚举函数也会将开关给包含进来，与<see cref="ToArgumentVector"/>方法所不同的是，这个函数里面的逻辑值开关的名称没有被修饰剪裁
        ''' </summary>
        ''' <returns></returns>
        Public Iterator Function GetEnumerator() As IEnumerator(Of NamedValue(Of String)) Implements IEnumerable(Of NamedValue(Of String)).GetEnumerator
            Dim source As New List(Of NamedValue(Of String))(Me.arguments)

            If Not Me.BoolFlags.IsNullOrEmpty Then
                source += From name As String
                          In BoolFlags
                          Select New NamedValue(Of String)(name, "true")
            End If

            For Each x As NamedValue(Of String) In source
                Yield x
            Next
        End Function

        Public Iterator Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function

        ''' <summary>
        ''' Adds an item to the System.Collections.Generic.ICollection`1.
        ''' </summary>
        ''' <param name="item"></param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Add(item As NamedValue(Of String)) Implements ICollection(Of NamedValue(Of String)).Add
            Call arguments.Add(item)
        End Sub

        ''' <summary>
        ''' Add a parameter with name and its value.
        ''' </summary>
        ''' <param name="key"></param>
        ''' <param name="value"></param>
        Public Sub Add(key$, value$,
                       Optional allowDuplicated As Boolean = False,
                       <CallerMemberName> Optional stack$ = Nothing)

            Dim item As New NamedValue(Of String) With {
                .Name = key.ToLower,
                .Value = value,
                .Description = stack & "->" & NameOf(Add)
            }

            If Not allowDuplicated Then
                For i As Integer = 0 To arguments.Count - 1
                    With arguments(i)
                        If .Name.TextEquals(key) Then
                            arguments(i) = item
                            Return
                        End If
                    End With
                Next

                ' 没有查找到需要被替换掉的下标，则直接在下面的代码之中进行添加
            End If

            arguments += item
        End Sub

        ''' <summary>
        ''' Clear the inner list buffer
        ''' </summary>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Clear() Implements ICollection(Of NamedValue(Of String)).Clear
            Call arguments.Clear()
        End Sub

        ''' <summary>
        ''' 只是通过比较名称来判断是否存在，值没有进行比较
        ''' </summary>
        ''' <param name="item"></param>
        ''' <returns></returns>
        Public Function Contains(item As NamedValue(Of String)) As Boolean Implements ICollection(Of NamedValue(Of String)).Contains
            Dim LQuery% = LinqAPI.DefaultFirst(-1) _
                                                   _
                <= From obj As NamedValue(Of String)
                   In Me.arguments
                   Where String.Equals(obj.Name, item.Name, StringComparison.OrdinalIgnoreCase)
                   Select 100

            Return LQuery > 50
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub CopyTo(array() As NamedValue(Of String), arrayIndex As Integer) Implements ICollection(Of NamedValue(Of String)).CopyTo
            Call arguments.ToArray.CopyTo(array, arrayIndex)
        End Sub

        ''' <summary>
        ''' Get the switch counts in this commandline object.(获取本命令行对象中的所定义的开关的数目)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Count As Integer Implements ICollection(Of NamedValue(Of String)).Count
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Me.arguments.Count
            End Get
        End Property

        Private ReadOnly Property IsReadOnly As Boolean Implements ICollection(Of NamedValue(Of String)).IsReadOnly
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return True
            End Get
        End Property

        ''' <summary>
        ''' Removes a parameter by name
        ''' </summary>
        ''' <param name="paramName"></param>
        ''' <returns></returns>
        Public Function Remove(paramName As String) As Boolean
            Dim LQuery = LinqAPI.DefaultFirst(Of NamedValue(Of String)) _
                                                                        _
                () <= From obj As NamedValue(Of String)
                      In Me.arguments
                      Where String.Equals(obj.Name, paramName, StringComparison.OrdinalIgnoreCase)
                      Select obj

            If LQuery.IsEmpty Then
                Return False
            Else
                Call arguments.Remove(LQuery)
                Return True
            End If
        End Function

        ''' <summary>
        ''' Removes a parameter by <see cref="NamedValue(Of String).Name"/>
        ''' </summary>
        ''' <param name="item"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Remove(item As NamedValue(Of String)) As Boolean Implements ICollection(Of NamedValue(Of String)).Remove
            Return Remove(item.Name)
        End Function
#End Region

        ''' <summary>
        ''' 将当前的这个命令行对象之中的所有的参数值都合并到一个向量之中返回.
        ''' (``ToArray``拓展好像是有BUG的，所以请使用这个函数来获取所有的参数信息。
        ''' 请注意，逻辑值开关的名称会被去掉前缀)
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' the description value in the populated vector is indicates the argument value type.
        ''' </remarks>
        Public Function ToArgumentVector() As NamedValue(Of String)()
            Dim list As New List(Of NamedValue(Of String))

            list += From arg As NamedValue(Of String)
                    In arguments.SafeQuery
                    Select New NamedValue(Of String) With {
                        .Name = arg.Name,
                        .Value = arg.Value,
                        .Description = "string"
                    }
            list += From bs As String
                    In BoolFlags.SafeQuery
                    Select New NamedValue(Of String) With {
                        .Name = bs,
                        .Value = "True",
                        .Description = "boolean"
                    }

            Return list
        End Function

        ''' <summary>
        ''' Open a handle for a file system object.
        ''' </summary>
        ''' <param name="args"></param>
        ''' <param name="fs"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator +(args As CommandLine, fs$) As Integer
            Dim path As String = args(fs)
            Return My.File.OpenHandle(path)
        End Operator

        ''' <summary>
        ''' Gets the CLI parameter value.
        ''' </summary>
        ''' <param name="args"></param>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator <=(args As CommandLine, name$) As String
            If args Is Nothing Then
                Return Nothing
            Else
                Return args(name)
            End If
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator <=(opt As String, args As CommandLine) As CommandLine
            Return Parsers.TryParse(args(opt))
        End Operator

        Public Shared Operator ^(args As CommandLine, [default] As String) As String
            If args Is Nothing OrElse String.IsNullOrEmpty(args.cliCommandArgvs) Then
                Return [default]
            Else
                Return args.cliCommandArgvs
            End If
        End Operator

        Public Shared Operator >=(opt As String, args As CommandLine) As CommandLine
            Throw New NotSupportedException
        End Operator

        ''' <summary>
        ''' Try get parameter value.
        ''' </summary>
        ''' <param name="args"></param>
        ''' <param name="name"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator -(args As CommandLine, name As String) As String
            Return args(name)
        End Operator

        ''' <summary>
        ''' Try get parameter value.
        ''' </summary>
        ''' <param name="args"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator -(args As CommandLine, null As CommandLine) As CommandLine
            Return args
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator -(args As CommandLine, name As IEnumerable(Of String)) As String
            Return args.GetValue(name.First, name.Last)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator -(args As CommandLine) As CommandLine
            Return args
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator >(args As CommandLine, name As String) As String
            Return args(name)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator <(args As CommandLine, name As String) As String
            Return args(name)
        End Operator

        Public Shared Operator >=(args As CommandLine, name As String) As String
            Throw New NotSupportedException
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Parse(commandlineStr As String) As CommandLine
            Return Parsers.TryParse(commandlineStr.GetTokens, False, commandlineStr)
        End Function

        Public Shared Function ParseTokens(commandlineStr As String) As String()
            Return commandlineStr.GetTokens
        End Function

        Public Shared Function BuildFromArguments(args As String()) As CommandLine
            Return Parsers.TryParse(args, False, rawInput:=args.Select(Function(s) s.CLIToken).JoinBy(" "))
        End Function

        Public Shared Function BuildFromArguments(name As String, args As String()) As CommandLine
            Return Parsers.TryParse({name}.JoinIterates(args), False, name.CLIToken & " " & args.Select(Function(s) s.CLIToken).JoinBy(" "))
        End Function

        Public Function GetDataReader() As StringReader
            Return New StringReader(Me)
        End Function

        Private Function GetString(ordinal As Integer) As String Implements IStringGetter.GetString
            Return arguments(ordinal).Value
        End Function

        Private Function GetSize() As Integer Implements IStringGetter.GetSize
            Return Count
        End Function

        Private Function MoveNext() As Boolean Implements IStringGetter.MoveNext
            Return False
        End Function
    End Class
End Namespace
