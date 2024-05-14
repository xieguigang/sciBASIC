#Region "Microsoft.VisualBasic::fe5c5f094686e1d923ee2249b6412599, Microsoft.VisualBasic.Core\src\Scripting\TokenIcer\ParserCommon.vb"

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

    '   Total Lines: 104
    '    Code Lines: 50
    ' Comment Lines: 43
    '   Blank Lines: 11
    '     File Size: 4.09 KB


    '     Module ParserCommon
    ' 
    '         Function: [As], [CType], [TryCast], GetCodeComment, (+2 Overloads) StartEscaping
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
        ''' 当前的token对象之中是否是转义的起始，即当前的token之中的最后一个符号是否是转义符<paramref name="escape"/>?
        ''' </summary>
        ''' <param name="buffer"></param>
        ''' <param name="escape"></param>
        ''' <returns>
        ''' this function deal with the empty collection: returns false if the given buffer is empty
        ''' </returns>
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

        ''' <summary>
        ''' Dynamics casting the token value expression as target type object.
        ''' </summary>
        ''' <typeparam name="Tokens"></typeparam>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="x"></param>
        ''' <returns></returns>
        <Extension> Public Function [As](Of Tokens As IComparable, T)(x As CodeToken(Of Tokens)) As T
            Dim obj As T = InputHandler.CTypeDynamic(Of T)(x.text)
            Return obj
        End Function

        ''' <summary>
        ''' Dynamics casting the token value expression as target type object.
        ''' </summary>
        ''' <typeparam name="Tokens"></typeparam>
        ''' <param name="x"></param>
        ''' <param name="type"></param>
        ''' <returns></returns>
        <Extension> Public Function [CType](Of Tokens As IComparable)(x As CodeToken(Of Tokens), type As Type) As Object
            Dim obj As Object = InputHandler.CTypeDynamic(x.text, type)
            Return obj
        End Function

        ''' <summary>
        ''' Try cast the token value to a .NET object based on the token type name.
        ''' </summary>
        ''' <typeparam name="Tokens"></typeparam>
        ''' <param name="x"></param>
        ''' <returns></returns>
        <Extension> Public Function [TryCast](Of Tokens As IComparable)(x As CodeToken(Of Tokens)) As Object
            Dim typeName As String = Scripting.ToString(x.name)
            Dim type As New Value(Of Type)

            If type = Scripting.GetType(typeName, False) Is Nothing Then
                Return x.text
            Else
                Return CTypeDynamic(x.text, +type)
            End If
        End Function
    End Module
End Namespace
