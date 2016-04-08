Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.UnixBash

Namespace CommandLine

    ''' <summary>
    ''' A command line object that parse from the user input commandline string.
    ''' (从用户所输入的命令行字符串之中解析出来的命令行对象，标准的命令行格式为：
    ''' <example>&lt;EXE> &lt;CLI_Name> ["Parameter" "Value"]</example>)
    ''' </summary>
    ''' <remarks></remarks>
    '''
    Public Class CommandLine : Implements Generic.ICollection(Of KeyValuePair(Of String, String))

        Friend __lstParameter As List(Of KeyValuePair(Of String, String)) = New List(Of KeyValuePair(Of String, String))
        ''' <summary>
        ''' 原始的命令行字符串
        ''' </summary>
        Friend _CLICommandArgvs As String

        Friend _name As String

        ''' <summary>
        ''' The command name that parse from the input command line.
        ''' (从输入的命令行中所解析出来的命令的名称)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Name As String
            Get
                Return _name
            End Get
        End Property

        ''' <summary>
        ''' The command tokens that were parsed from the input commandline.
        ''' (从所输入的命令行之中所解析出来的命令参数单元)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Tokens As String()

        ''' <summary>
        ''' The parameters in the commandline without the first token of the command name.
        ''' (将命令行解析为词元之后去掉命令的名称之后所剩下的所有的字符串列表)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <DumpNode> Public ReadOnly Property Parameters As String()
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
        ''' Get the original command line string.(获取所输入的命令行对象的原始的字符串)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property CLICommandArgvs As String
            Get
                Return _CLICommandArgvs
            End Get
        End Property

        ''' <summary>
        ''' 开关的名称是不区分大小写的
        ''' </summary>
        ''' <param name="paramName"></param>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Default Public ReadOnly Property Item(paramName As String) As String
            Get
                Dim LQuery As KeyValuePair(Of String, String) = (
                    From obj As KeyValuePair(Of String, String)
                    In Me.__lstParameter
                    Where String.Equals(obj.Key, paramName, StringComparison.OrdinalIgnoreCase)
                    Select obj).FirstOrDefault  ' 是值类型，不会出现空引用的情况，
                Return LQuery.Value
            End Get
        End Property

        Public Property SingleValue As String

        Public Function HavebFlag(name As String) As Boolean
            Return Array.IndexOf(Me.BoolFlags, name.ToLower) > -1
        End Function

        ''' <summary>
        ''' Returns the original cli command line argument string.(返回所传入的命令行的原始字符串)
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function ToString() As String
            Return CLICommandArgvs
        End Function

        ''' <summary>
        ''' Gets the brief summary information of current cli command line object.(获取当前的命令行对象的参数摘要信息)
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetCommandsOverview() As String
            Dim sBuilder As StringBuilder = New StringBuilder(vbCrLf, 1024)
            Call sBuilder.AppendLine($"Commandline arguments overviews{vbCrLf}Command Name  --  ""{Me.Name}""")
            Call sBuilder.AppendLine()
            Call sBuilder.AppendLine("---------------------------------------------------------")
            Call sBuilder.AppendLine()

            If __lstParameter.Count = 0 Then
                Call sBuilder.AppendLine("No parameter was define in this commandline.")
                Return sBuilder.ToString
            End If

            Dim MaxSwitchName As Integer = (From item As KeyValuePair(Of String, String)
                                            In __lstParameter
                                            Select Len(item.Key)).Max
            For Each sw As KeyValuePair(Of String, String) In __lstParameter
                Call sBuilder.AppendLine($"  {sw.Key}  {New String(" "c, MaxSwitchName - Len(sw.Key))}= ""{sw.Value}"";")
            Next

            Return sBuilder.ToString
        End Function

        ''' <summary>
        ''' Checking for the missing required parameter, this function will returns the missing parameter
        ''' in the current cli command line object using a specific parameter name list.
        ''' (检查<paramref name="list"></paramref>之中的所有参数是否存在，函数会返回不存在的参数名)
        ''' </summary>
        ''' <param name="list"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function CheckMissingRequiredParameters(list As Generic.IEnumerable(Of String)) As String()
            Dim LQuery = (From p As String In list Where String.IsNullOrEmpty(Me(p)) Select p).ToArray
            Return LQuery
        End Function

        Public Function CheckMissingRequiredParameters(ParamArray args As String()) As String()
            Return CheckMissingRequiredParameters(list:=args)
        End Function

        ''' <summary>
        ''' Does this cli command line object contains any parameter argument information.(查看本命令行参数对象之中是否存在有参数信息)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property IsNullOrEmpty As Boolean
            Get
                Return Tokens.IsNullOrEmpty OrElse (Tokens.Length = 1 AndAlso String.IsNullOrEmpty(Tokens.First))
            End Get
        End Property

        Public ReadOnly Property IsNothing As Boolean
            Get
                Return String.IsNullOrEmpty(Me.Name) AndAlso IsNullOrEmpty
            End Get
        End Property

        ''' <summary>
        ''' 大小写不敏感，
        ''' </summary>
        ''' <param name="parameterName"></param>
        ''' <returns></returns>
        Public Function ContainsParameter(parameterName As String, trim As Boolean) As Boolean
            Dim namer As String = If(trim, parameterName.TrimParamPrefix, parameterName)
            Dim LQuery = (From para As KeyValuePair(Of String, String)
                          In Me.__lstParameter  '  名称都是没有处理过的
                          Where String.Equals(namer, para.Key, StringComparison.OrdinalIgnoreCase)
                          Select 1).FirstOrDefault
            Return LQuery > 0
        End Function

        Public Shared Widening Operator CType(CommandLine As String) As CommandLine
            Return TryParse(CommandLine)
        End Operator

        Public Shared Widening Operator CType(CommandLine As System.Func(Of String)) As CommandLine
            Return Microsoft.VisualBasic.CommandLine.TryParse(CommandLine())
        End Operator

#Region "IDataRecord Methods"

        ''' <summary>
        ''' Gets the value Of the specified column As a Boolean.
        ''' (这个函数也同时包含有开关参数的，开关参数默认为逻辑值类型，当包含有开关参数的时候，其逻辑值为True，反之函数会检查参数列表，参数不存在则为空值字符串，则也为False)
        ''' </summary>
        ''' <param name="parameter">可以包含有开关参数</param>
        ''' <returns></returns>
        Public Function GetBoolean(parameter As String) As Boolean
            If Me.HavebFlag(parameter) Then
                Return True
            End If
            Return Me(parameter).getBoolean
        End Function

        ''' <summary>
        ''' Gets the 8-bit unsigned Integer value Of the specified column.
        ''' </summary>
        ''' <param name="parameter"></param>
        ''' <returns></returns>
        Public Function GetByte(parameter As String) As Byte
            Return CByte(Val(Me(parameter)))
        End Function

        ''' <summary>
        ''' Reads a stream Of bytes from the specified column offset into the buffer As an array, starting at the given buffer offset.
        ''' </summary>
        ''' <returns></returns>
        Public Function GetBytes(parameter As String) As Byte()
            Dim Tokens As String() = Me(parameter).Split(","c)
            Return (From s As String In Tokens Select CByte(Val(s))).ToArray
        End Function
        ''' <summary>
        ''' Gets the character value Of the specified column.
        ''' </summary>
        ''' <returns></returns>
        Public Function GetChar(parameter As String) As Char
            Dim s As String = Me(parameter)
            If String.IsNullOrEmpty(s) Then
                Return NIL
            Else
                Return s.First
            End If
        End Function

        ''' <summary>
        ''' Reads a stream Of characters from the specified column offset into the buffer As an array, starting at the given buffer offset.
        ''' </summary>
        ''' <returns></returns>
        Public Function GetChars(parameter As String) As Char()
            Return Me(parameter)
        End Function

        ''' <summary>
        ''' Gets the Date And time data value Of the specified field.
        ''' </summary>
        ''' <returns></returns>
        Public Function GetDateTime(parameter As String) As DateTime
            Return Me(parameter).ParseDateTime
        End Function

        ''' <summary>
        ''' Gets the fixed-position numeric value Of the specified field.
        ''' </summary>
        ''' <returns></returns>
        Public Function GetDecimal(parameter As String) As Decimal
            Return CDec(Val(Me(parameter)))
        End Function

        ''' <summary>
        ''' Gets the Double-precision floating point number Of the specified field.
        ''' </summary>
        ''' <returns></returns>
        Public Function GetDouble(parameter As String) As Double
            Return Val(Me(parameter))
        End Function

        ''' <summary>
        ''' Gets the Single-precision floating point number Of the specified field.
        ''' </summary>
        ''' <returns></returns>
        Public Function GetFloat(parameter As String) As Single
            Return CSng(Val(Me(parameter)))
        End Function

        ''' <summary>
        ''' Returns the GUID value Of the specified field.
        ''' </summary>
        ''' <returns></returns>
        Public Function GetGuid(parameter As String) As Guid
            Return Guid.Parse(Me(parameter))
        End Function
        ''' <summary>
        ''' Gets the 16-bit signed Integer value Of the specified field.
        ''' </summary>
        ''' <returns></returns>
        Public Function GetInt16(parameter As String) As Int16
            Return CType(Val(Me(parameter)), Int16)
        End Function

        ''' <summary>
        ''' Gets the 32-bit signed Integer value Of the specified field.
        ''' </summary>
        ''' <returns></returns>
        Public Function GetInt32(parameter As String) As Int32
            Return CInt(Val(Me(parameter)))
        End Function

        ''' <summary>
        ''' Gets the 64-bit signed Integer value Of the specified field.
        ''' </summary>
        ''' <returns></returns>
        Public Function GetInt64(parameter As String) As Int64
            Return CLng(Val(Me(parameter)))
        End Function

        ''' <summary>
        ''' Return the index Of the named field. If the name is not exists in the parameter list, then a -1 value will be return.
        ''' </summary>
        ''' <returns></returns>
        Public Function GetOrdinal(parameter As String) As Integer
            Dim i = (From entry As KeyValuePair(Of String, String) In Me.__lstParameter
                     Where String.Equals(parameter, entry.Key, StringComparison.OrdinalIgnoreCase)
                     Select New With {
                         .index = Me.__lstParameter.IndexOf(entry)
                         }).FirstOrDefault
            If i Is Nothing Then
                Return -1
            Else
                Return i.index
            End If
        End Function

        ''' <summary>
        ''' Gets the String value Of the specified field.
        ''' </summary>
        ''' <returns></returns>
        Public Function GetString(parameter As String) As String
            Return Me(parameter)
        End Function

        ''' <summary>
        ''' Return whether the specified field Is Set To null.
        ''' </summary>
        ''' <returns></returns>
        Public Function IsNull(parameter As String) As Boolean
            Return Not Me.ContainsParameter(parameter, False)
        End Function

        ''' <summary>
        '''
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="parameter">Command parameter name in the command line inputs.</param>
        ''' <param name="__getObject"></param>
        ''' <returns></returns>
        Public Function GetObject(Of T)(parameter As String, __getObject As Func(Of String, T)) As T
            If __getObject Is Nothing Then
                Return Nothing
            End If

            Dim value As String = Me(parameter)
            Dim obj As T = __getObject(arg:=value)
            Return obj
        End Function

        ''' <summary>
        ''' If the given parameter is not exists in the user input arguments, then a developer specific default value will be return.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="name"></param>
        ''' <param name="[default]">The default value for returns when the parameter is not exists in the user input.</param>
        ''' <returns></returns>
        Public Function GetValue(Of T)(name As String, [default] As T) As T
            If Not Me.ContainsParameter(name, False) Then
                If GetType(T).Equals(GetType(Boolean)) Then
                    If HavebFlag(name) Then
                        Return DirectCast(DirectCast(GetBoolean(name), Object), T)
                    End If
                End If

                Return [default]
            End If

            Dim str As String = Me(name)
            Dim value As Object = Scripting.InputHandler.CTypeDynamic(str, GetType(T))
            Return DirectCast(value, T)
        End Function

        Public Function OpenHandle(name As String, Optional [default] As String = "") As Int
            Dim file As String = Me(name)
            If String.IsNullOrEmpty(file) Then
                file = [default]
            End If
            Return New Int(FileHandles.OpenHandle(file))
        End Function
#End Region

#Region "Implements IReadOnlyCollection(Of KeyValuePair(Of String, String))"

        ''' <summary>
        ''' 这个枚举函数也会将开关给包含进来
        ''' </summary>
        ''' <returns></returns>
        Public Iterator Function GetEnumerator() As IEnumerator(Of KeyValuePair(Of String, String)) Implements IEnumerable(Of KeyValuePair(Of String, String)).GetEnumerator
            Dim List As New List(Of KeyValuePair(Of String, String))(Me.__lstParameter)

            If Not Me.BoolFlags.IsNullOrEmpty Then
                Call List.AddRange((From name As String
                                    In BoolFlags
                                    Select New KeyValuePair(Of String, String)(name, "true")).ToArray)
            End If

            For Each Item As KeyValuePair(Of String, String) In List
                Yield Item
            Next
        End Function

        Public Iterator Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function

        Public Sub Add(item As KeyValuePair(Of String, String)) Implements ICollection(Of KeyValuePair(Of String, String)).Add
            Call __lstParameter.Add(item)
        End Sub

        Public Sub Add(key As String, value As String)
            Call __lstParameter.Add(New KeyValuePair(Of String, String)(key.ToLower, value))
        End Sub

        Public Sub Clear() Implements ICollection(Of KeyValuePair(Of String, String)).Clear
            Call __lstParameter.Clear()
        End Sub

        Public Function Contains(item As KeyValuePair(Of String, String)) As Boolean Implements ICollection(Of KeyValuePair(Of String, String)).Contains
            Dim LQuery = (From obj As KeyValuePair(Of String, String) In Me.__lstParameter Where String.Equals(obj.Key, item.Key, StringComparison.OrdinalIgnoreCase) Select obj).ToArray
            Return LQuery.IsNullOrEmpty
        End Function

        Public Sub CopyTo(array() As KeyValuePair(Of String, String), arrayIndex As Integer) Implements ICollection(Of KeyValuePair(Of String, String)).CopyTo
            Call __lstParameter.ToArray.CopyTo(array, arrayIndex)
        End Sub

        ''' <summary>
        ''' Get the switch counts in this commandline object.(获取本命令行对象中的所定义的开关的数目)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Count As Integer Implements ICollection(Of KeyValuePair(Of String, String)).Count
            Get
                Return Me.__lstParameter.Count
            End Get
        End Property

        Public ReadOnly Property IsReadOnly As Boolean Implements ICollection(Of KeyValuePair(Of String, String)).IsReadOnly
            Get
                Return True
            End Get
        End Property

        Public Function Remove(paramName As String) As Boolean
            Dim LQuery = (From obj As KeyValuePair(Of String, String)
                              In Me.__lstParameter
                          Where String.Equals(obj.Key, paramName, StringComparison.OrdinalIgnoreCase)
                          Select obj).ToArray
            If LQuery.IsNullOrEmpty Then
                Return False
            Else
                Call __lstParameter.Remove(LQuery.First)
                Return True
            End If
        End Function

        Public Function Remove(item As KeyValuePair(Of String, String)) As Boolean Implements ICollection(Of KeyValuePair(Of String, String)).Remove
            Dim LQuery = (From obj As KeyValuePair(Of String, String)
                              In Me.__lstParameter
                          Where String.Equals(obj.Key, item.Key, StringComparison.OrdinalIgnoreCase)
                          Select obj).ToArray
            If LQuery.IsNullOrEmpty Then
                Return False
            Else
                Call __lstParameter.Remove(LQuery.First)
                Return True
            End If
        End Function
#End Region

        ''' <summary>
        ''' ToArray拓展好像是有BUG的，所以请使用这个函数来获取所有的参数信息，请注意，逻辑值开关的名称会被去掉前缀
        ''' </summary>
        ''' <returns></returns>
        Public Function GetValueArray() As KeyValuePair(Of String, String)()
            Dim List = New List(Of KeyValuePair(Of String, String))
            If Not Me.__lstParameter.IsNullOrEmpty Then
                Call List.AddRange((From obj As KeyValuePair(Of String, String)
                                    In __lstParameter
                                    Select New KeyValuePair(Of String, String)(obj.Key, obj.Value)).ToArray)
            End If

            If Not Me.BoolFlags.IsNullOrEmpty Then
                Call List.AddRange((From bs As String
                                    In Me.BoolFlags
                                    Select New KeyValuePair(Of String, String)(TrimParamPrefix(bs), True)).ToArray)
            End If

            Return List.ToArray
        End Function

        ''' <summary>
        ''' Open a handle for a file system object.
        ''' </summary>
        ''' <param name="args"></param>
        ''' <param name="fs"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator +(args As CommandLine, fs As String) As Integer
            Dim path As String = args(fs)
            Return Language.UnixBash.OpenHandle(path)
        End Operator

        ''' <summary>
        ''' Gets the CLI parameter value.
        ''' </summary>
        ''' <param name="args"></param>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator <=(args As CommandLine, name As String) As String
            If args Is Nothing Then
                Return Nothing
            Else
                Return args(name)
            End If
        End Operator

        Public Shared Operator <=(opt As String, args As CommandLine) As CommandLine
            Return TryParse(args(opt))
        End Operator

        Public Shared Operator ^(args As CommandLine, [default] As String) As String
            If args Is Nothing OrElse String.IsNullOrEmpty(args.CLICommandArgvs) Then
                Return [default]
            Else
                Return args.CLICommandArgvs
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

        Public Overloads Shared Operator -(args As CommandLine, name As String) As String
            Return args(name)
        End Operator

        ''' <summary>
        ''' Try get parameter value.
        ''' </summary>
        ''' <param name="args"></param>
        ''' <returns></returns>

        Public Overloads Shared Operator -(args As CommandLine, null As CommandLine) As CommandLine
            Return args
        End Operator

        Public Overloads Shared Operator -(args As CommandLine, name As IEnumerable(Of String)) As String
            Return args.GetValue(name.First, name.Last)
        End Operator

        Public Shared Operator -(args As CommandLine) As CommandLine
            Return args
        End Operator

        Public Shared Operator >(args As CommandLine, name As String) As String
            Return args(name)
        End Operator

        Public Shared Operator <(args As CommandLine, name As String) As String
            Return args(name)
        End Operator

        Public Shared Operator >=(args As CommandLine, name As String) As String
            Throw New NotSupportedException
        End Operator
    End Class
End Namespace