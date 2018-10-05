#Region "Microsoft.VisualBasic::2c2fe4cbad6a9c10ae837fd102038bbb, Microsoft.VisualBasic.Core\Scripting\TextGrepScriptEngine.vb"

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

    '     Delegate Function
    ' 
    ' 
    '     Delegate Function
    ' 
    ' 
    '     Class TextGrepScriptEngine
    ' 
    '         Properties: MethodPointers, PipelinePointer
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Compile, EnsureNotEmpty, Grep, Match, MidString
    '                   NoOperation, Replace, Reverse, Tokens, ToString
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language
Imports Token = System.Collections.Generic.KeyValuePair(Of String(), Microsoft.VisualBasic.Scripting.TextGrepMethodToken)

Namespace Scripting

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="source">文本源</param>
    ''' <param name="args">脚本命令的参数</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Delegate Function TextGrepMethodToken(source$, args$()) As String
    Public Delegate Function TextGrepMethod(source As String) As String

    ''' <summary>
    ''' A script object for grep the gene id in the blast output query and subject title.
    ''' (用于解析基因名称的脚本类，这个对象是在项目的初始阶段，为了方便命令行操作而设置的)
    ''' </summary>
    ''' <remarks></remarks>
    Public NotInheritable Class TextGrepScriptEngine

        Public Shared ReadOnly Property MethodPointers As New SortedDictionary(Of String, TextGrepMethodToken) From {
 _
            {"tokens", AddressOf TextGrepScriptEngine.Tokens},
            {"match", AddressOf TextGrepScriptEngine.Match},
            {"-", AddressOf TextGrepScriptEngine.NoOperation},
            {"replace", AddressOf TextGrepScriptEngine.Replace},
            {"mid", AddressOf TextGrepScriptEngine.MidString},
            {"reverse", AddressOf TextGrepScriptEngine.Reverse}
        }

        Public Shared Function EnsureNotEmpty(ptr As TextGrepMethod) As TextGrepMethod
            Return Function(str) As String
                       Dim gs$ = ptr(str)

                       If gs.StringEmpty Then
                           Return str
                       Else
                           Return gs
                       End If
                   End Function
        End Function

        ''' <summary>
        ''' Source,Script,ReturnValue
        ''' </summary>
        ''' <remarks></remarks>
        Dim _operations As Token()
        Dim _script$

        ''' <summary>
        ''' 对用户所输入的脚本进行编译，对于内部的空格，请使用单引号``'``进行分割
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <ExportAPI("compile", Info:="", Usage:="script_tokens1;script_tokens2;....", Example:="")>
        Public Shared Function Compile(scriptText As String) As TextGrepScriptEngine
            Dim script$() = TryParse(scriptText, TokenDelimited:=";", InnerDelimited:="'"c)
            Dim builder = LinqAPI.Exec(Of Token) <=
 _
                From sToken As String
                In script
                Let tokens As String() = TryParse(sToken, TokenDelimited:=" ", InnerDelimited:="'"c)
                Let entryPoint As String = sToken.Split.First.ToLower
                Where MethodPointers.ContainsKey(entryPoint)
                Select New Token(tokens, _MethodPointers(entryPoint))

            If script.Length > builder.Length Then
                ' 有非法的命令短语，则为了保护数据的一致性，这个
                ' 含有错误的语法的脚本是不能够用于操作的， 
                ' 则函数返回空指针
                Return Nothing
            Else
                Return New TextGrepScriptEngine With {
                    ._script = scriptText,
                    ._operations = builder
                }
            End If
        End Function

        ''' <summary>
        ''' <see cref="Grep"/>
        ''' 字符串剪裁操作的函数指针
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property PipelinePointer As TextGrepMethod
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return AddressOf Me.Grep
            End Get
        End Property

        ''' <summary>
        ''' 修整目标字符串，按照脚本之中的方法取出所需要的字符串信息
        ''' </summary>
        ''' <param name="source"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Grep(source As String) As String
            Dim __parser As Func(Of String, Token, Integer) =
                Function(sourceText As String, method As Token) As Integer
                    source = method.Value()(sourceText, method.Key) '迭代解析
                    Return Len(source)
                End Function

            ' 这里是迭代计算，所以请不要使用并行拓展
            For Each operation As Token In _operations
                Call __parser(source, operation)
            Next

            Return source
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return _script
        End Function

        Protected Friend Sub New()
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <ExportAPI("-", Info:="DO_NOTHING")>
        Private Shared Function NoOperation(source As String, script As String()) As String
            Return source
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <ExportAPI("Reverse")>
        Private Shared Function Reverse(source As String, Script As String()) As String
            Return source.Reverse.ToArray
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Source"></param>
        ''' <param name="Script"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <ExportAPI("Tokens", Info:="", Usage:="tokens p_str pointer", Example:="")>
        <Argument("pointer", False, Description:="pointer must be a zero base integer number which is smaller than 
            the tokens array's length; pointer can also be assign of a specific string ""last"" to get the last 
            element and ""first"" to get the first element in the tokens array.")>
        Private Shared Function Tokens(source As String, script As String()) As String
            Dim Delimiter As String = script(1)
            Dim Tstr As String() = Strings.Split(source, Delimiter)

            If String.Equals(script(2), "last", StringComparison.OrdinalIgnoreCase) Then
                Return If(Tstr.IsNullOrEmpty, "", Tstr.Last)
            Else
                ' first指示符被计算为0，所以在这里不需要为first进行额外的处理
                Dim p As Integer = CInt(Val(script(2)))

                If Tstr.Length - 1 < p OrElse p < 0 Then
                    Return ""
                Else
                    Return If(Tstr.IsNullOrEmpty, "", Tstr(p))
                End If
            End If
        End Function

        <ExportAPI("match", Info:="", Usage:="match pattern", Example:="")>
        Private Shared Function Match(source As String, script As String()) As String
            Dim pattern As String = script.Last
            Return Regex.Match(source, pattern).Value
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="source"></param>
        ''' <param name="ScriptTokens">
        ''' 向量之中的第一个元素为命令的名字，第二个元素为Mid函数的Start参数，第三个元素为Mid函数的Length参数，可以被忽略掉
        ''' </param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <ExportAPI("mid", Info:="Substring a token from the input text source.")>
        Private Shared Function MidString(Source As String, ScriptTokens As String()) As String
            Dim Start As Integer = CInt(Val(ScriptTokens(1)))

            If ScriptTokens.Length > 2 Then
                Dim Length As Integer = CInt(Val(ScriptTokens(2)))
                Return Mid(Source, Start, Length)
            Else
                Return Mid(Source, Start)
            End If
        End Function

        <ExportAPI("replace", Usage:="replace <regx_text> <replace_value>")>
        Private Shared Function Replace(source As String, script As String()) As String
            Dim Regx As New Regex(script(1))
            Dim Matchs = Regx.Matches(source)
            Dim sBuilder As New StringBuilder(source)
            Dim NewValue = script(2)

            For Each m As Match In Matchs
                Call sBuilder.Replace(m.Value, NewValue)
            Next

            Return sBuilder.ToString
        End Function
    End Class
End Namespace
