#Region "Microsoft.VisualBasic::4a1707f70c166f9b1bd13e6356e5afd7, ..\VisualBasic_AppFramework\Microsoft.VisualBasic.Architecture.Framework\Scripting\TokenIcer\ParserAPI.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
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

Imports System.Runtime.CompilerServices

Namespace Scripting.TokenIcer

    Public Module ParserAPI

        <Extension>
        Public Function GetTokens(Of Tokens)(parser As TokenParser(Of Tokens), expr As String) As Token(Of Tokens)()
            Dim lstToken As New List(Of Token(Of Tokens))
            Dim tmp As Token(Of Tokens) = Nothing

            parser.InputString = expr
            Do While Not parser.GetToken.ShadowCopy(tmp) Is Nothing
                Call lstToken.Add(tmp)
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
        Public Function TokenParser(Of Tokens)(parser As TokenParser(Of Tokens),
                                               expr As String,
                                               stackT As StackTokens(Of Tokens)) As Func(Of Tokens)

            Dim lstToken As Token(Of Tokens)() = parser.GetTokens(expr)
            Dim whiteSpace As Tokens = stackT.WhiteSpace
            Dim source As Token(Of Tokens)() = (From x As Token(Of Tokens)
                                                In lstToken
                                                Where Not stackT.Equals(x.TokenName, whiteSpace)
                                                Select x).ToArray
            Dim func As Func(Of Tokens) =
                StackParser.Parsing(Of Tokens)(source, stackT)
            Return func
        End Function

        <Extension> Public Function [As](Of Tokens, T)(x As Token(Of Tokens)) As T
            Dim obj As T = InputHandler.CTypeDynamic(Of T)(x.TokenValue)
            Return obj
        End Function

        <Extension> Public Function [CType](Of Tokens)(x As Token(Of Tokens), type As Type) As Object
            Dim obj As Object = InputHandler.CTypeDynamic(x.TokenValue, type)
            Return obj
        End Function

        ''' <summary>
        ''' Try cast the token value to a .NET object based on the token type name.
        ''' </summary>
        ''' <typeparam name="Tokens"></typeparam>
        ''' <param name="x"></param>
        ''' <returns></returns>
        <Extension> Public Function [TryCast](Of Tokens)(x As Token(Of Tokens)) As Object
            Dim typeName As String = Scripting.ToString(x.TokenName)
            Dim type As Type = Nothing
            If Scripting.GetType(typeName, False).ShadowCopy(type) Is Nothing Then
                Return x.TokenValue
            Else
                Return CTypeDynamic(x.TokenValue, type)
            End If
        End Function
    End Module
End Namespace
