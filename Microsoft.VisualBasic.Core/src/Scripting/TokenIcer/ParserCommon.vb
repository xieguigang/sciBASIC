#Region "Microsoft.VisualBasic::0c1951e4214c058cf64c4091e94a8136, Microsoft.VisualBasic.Core\src\Scripting\TokenIcer\ParserCommon.vb"

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

    '     Module ParserCommon
    ' 
    '         Function: [As], [CType], [TryCast], GetCodeComment, GetTokens
    '                   (+2 Overloads) StartEscaping, TokenParser
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text.Parser

Namespace Scripting.TokenIcer

    ''' <summary>
    ''' Generally expression parser codes
    ''' </summary>
    Public Module ParserCommon

        ''' <summary>
        ''' 当前的token对象之中是否是转义的起始，即当前的token之中的最后一个符号是否是转移符<paramref name="escape"/>?
        ''' </summary>
        ''' <param name="buffer"></param>
        ''' <param name="escape"></param>
        ''' <returns></returns>
        <Extension>
        Public Function StartEscaping(buffer As List(Of Char), Optional escape As Char = "\"c) As Boolean
            If buffer.IsNullOrEmpty Then
                Return False
            Else
                Return buffer.Last = escape
            End If
        End Function

        ''' <summary>
        ''' 当前的token对象之中是否是转义的起始，即当前的token之中的最后一个符号是否是转移符<paramref name="escape"/>?
        ''' </summary>
        ''' <param name="buffer"></param>
        ''' <param name="escape"></param>
        ''' <returns></returns>
        <Extension>
        Public Function StartEscaping(buffer As CharBuffer, Optional escape As Char = "\"c) As Boolean
            If buffer.Size = 0 Then
                Return False
            Else
                Return buffer.Last = escape
            End If
        End Function

        ''' <summary>
        ''' 假若返回来的是空字符串，则说明不是注释行
        ''' </summary>
        ''' <param name="line"></param>
        ''' <param name="prefix">The prefix of the code comment character/string</param>
        ''' <returns></returns>
        <Extension>
        Public Function GetCodeComment(line$, ParamArray prefix$()) As String
            Dim code As String = Trim(line)

            For Each s As String In prefix
                If InStr(code, s, CompareMethod.Text) = 1 Then
                    Return Mid(code, s.Length)
                End If
            Next

            Return Nothing
        End Function

        <Extension>
        Public Function GetTokens(Of Tokens As IComparable)(parser As TokenParser(Of Tokens), expr As String) As Token(Of Tokens)()
            Dim lstToken As New List(Of Token(Of Tokens))
            Dim tmp As New Value(Of Token(Of Tokens))

            parser.InputString = expr
            Do While Not (tmp = parser.GetToken) Is Nothing
                Call lstToken.Add(+tmp)
            Loop

            Return lstToken.ToArray
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <typeparam name="Tokens"></typeparam>
        ''' <param name="parser"></param>
        ''' <param name="expr">表达式字符串</param>
        ''' <param name="stackT"></param>
        ''' <returns></returns>
        <Extension>
        Public Function TokenParser(Of Tokens As IComparable)(parser As TokenParser(Of Tokens),
                                               expr$,
                                               stackT As StackTokens(Of Tokens)) As Func(Of Tokens)

            Dim lstToken As Token(Of Tokens)() = parser.GetTokens(expr)
            Dim whiteSpace As Tokens = stackT.WhiteSpace
            Dim source As Token(Of Tokens)() = LinqAPI.Exec(Of Token(Of Tokens)) <=
 _
                From x As Token(Of Tokens)
                In lstToken
                Where Not stackT.Equals(x.name, whiteSpace)
                Select x

            Dim func As Func(Of Tokens) =
                StackParser.Parsing(Of Tokens)(source, stackT)
            Return func
        End Function

        ''' <summary>
        ''' Dynamics casting the token value expression as target type object.
        ''' </summary>
        ''' <typeparam name="Tokens"></typeparam>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="x"></param>
        ''' <returns></returns>
        <Extension> Public Function [As](Of Tokens As IComparable, T)(x As Token(Of Tokens)) As T
            Dim obj As T = InputHandler.CTypeDynamic(Of T)(x.Value)
            Return obj
        End Function

        ''' <summary>
        ''' Dynamics casting the token value expression as target type object.
        ''' </summary>
        ''' <typeparam name="Tokens"></typeparam>
        ''' <param name="x"></param>
        ''' <param name="type"></param>
        ''' <returns></returns>
        <Extension> Public Function [CType](Of Tokens As IComparable)(x As Token(Of Tokens), type As Type) As Object
            Dim obj As Object = InputHandler.CTypeDynamic(x.Value, type)
            Return obj
        End Function

        ''' <summary>
        ''' Try cast the token value to a .NET object based on the token type name.
        ''' </summary>
        ''' <typeparam name="Tokens"></typeparam>
        ''' <param name="x"></param>
        ''' <returns></returns>
        <Extension> Public Function [TryCast](Of Tokens As IComparable)(x As Token(Of Tokens)) As Object
            Dim typeName As String = Scripting.ToString(x.name)
            Dim type As New Value(Of Type)

            If type = Scripting.GetType(typeName, False) Is Nothing Then
                Return x.Value
            Else
                Return CTypeDynamic(x.Value, +type)
            End If
        End Function
    End Module
End Namespace
