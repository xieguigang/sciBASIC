Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting

Namespace CommandLine.InteropService.SharedORM

    ''' <summary>
    ''' CommandLine source code mapper API
    ''' </summary>
    Public Module API

        ' Usage:= /command1 /param1 <value1, means a string value?> /param2 <value2> [/boolean1 /boolean2 /opt <blabla, default=value233>]

        ''' <summary>
        ''' 从<see cref="ExportAPIAttribute.Usage"/>之中解析出命令行的模型
        ''' </summary>
        ''' <param name="usage$">
        ''' + 由于习惯于在命令行的usage说明之中使用``&lt;>``括号对来包裹参数值，所以usage字符串数据还不能直接使用普通的命令行函数进行解析
        ''' + ``[]``方括号所包裹的参数对象都是可选参数
        ''' + 对于可选参数的默认值，默认值的解析形式为``default=...``，如果没有这个表达式，则默认为Nothing空值为默认值
        ''' </param>
        ''' <returns></returns>
        <Extension> Public Function CommandLineModel(usage$) As CommandLine
            Dim name$ = Nothing
            Dim arguments$() = Nothing
            Dim optionals$() = Nothing

            usage = usage.Trim
            usage.Tokenize(name, arguments, optionals)

            ' 逻辑符号只存在于optional之中
            Dim booleans$() = Nothing
            Dim params As NamedValue(Of String)() = arguments.BuildArguments(optionals, booleans)

            Dim model As New CommandLine With {
                .Name = name,
                .__listArguments = params.AsList,
                .BoolFlags = booleans,
                ._CLICommandArgvs = usage
            }

            Return model
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="args$">因为逻辑变量只存在于可选参数之中，所以在这里必须参数直接split2即可</param>
        ''' <param name="optionals$"></param>
        ''' <param name="booleans$"></param>
        ''' <returns></returns>
        <Extension>
        Public Function BuildArguments(args$(), optionals$(), ByRef booleans$()) As NamedValue(Of String)()
            Dim out As List(Of NamedValue(Of String)) = args _
                .Split(2) _
                .Select(Function(a)
                            Return New NamedValue(Of String) With {
                                .Name = a(0),
                                .Value = a(1)
                            }
                        End Function).AsList

            booleans = GetLogicalArguments(optionals, Nothing)
            out += optionals _
                .CreateParameterValues(False, note:=NameOf(optionals)) _
                .As(Of IEnumerable(Of NamedValue(Of String)))

            Return out
        End Function

        <Extension>
        Public Sub Tokenize(usage$, ByRef name$, ByRef arguments$(), ByRef optionals$())
            Dim opts$ = Regex.Match(usage, "\[.+\]").Value

            If opts.Length > 0 Then
                usage = usage.Replace(opts, "")
                opts = opts.GetStackValue("[", "]")
            End If
            name = usage.Split.First ' 命令的名称肯定是没有空格的，所以在里直接split取第一个元素
            usage = Mid(usage, name.Length + 1).Trim

            ' 在下面将arguments和optionals参数进行分词就行了
            ' 因为在usage之中并不会使用双引号来分割value值，而是使用尖括号
            ' 所以在这里需要额外注意下

            arguments = usage.Tokenize
            optionals = opts.Tokenize
        End Sub

        ''' <summary>
        ''' 使用空格分隔，但是需要对value额外注意
        ''' </summary>
        ''' <param name="s$"></param>
        ''' <returns></returns>
        <Extension> Public Function Tokenize(s$) As String()
            Dim t As New Pointer(Of Char)(s)
            Dim c As Char
            Dim valueEscape As Boolean = False
            Dim tmp As New List(Of Char)
            Dim out As New List(Of String)

            Do While Not t.EndRead
                c = +t

                If c = " "c Then
                    If Not valueEscape Then
                        out += New String(tmp)
                        tmp *= 0
                    Else
                        ' 检查上一个字符是不是value的结束符号: >
                        If tmp.Last = ">"c Then
                            ' 则这个空格表示value的结束
                            out += New String(tmp)
                            valueEscape = False
                            tmp *= 0
                        Else
                            ' 这个空格是value值之中的一部分，则添加到临时列表
                            tmp += c
                        End If
                    End If
                Else
                    ' 检查是否是value的开始符号: <
                    If c = "<"c AndAlso tmp.Count = 0 Then
                        valueEscape = True
                    End If

                    tmp += c
                End If
            Loop

            If tmp.Count > 0 Then
                Return out + New String(tmp)
            Else
                Return out
            End If
        End Function
    End Module
End Namespace