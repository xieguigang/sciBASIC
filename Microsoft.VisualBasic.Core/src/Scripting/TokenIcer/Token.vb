#Region "Microsoft.VisualBasic::26d1dc834a3ee048d6cd1d9646830ed8, Microsoft.VisualBasic.Core\src\Scripting\TokenIcer\Token.vb"

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

    '     Class CodeToken
    ' 
    '         Properties: isNumeric, name, span, text
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: ToString
    '         Operators: (+2 Overloads) <>, (+2 Overloads) =
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Language

Namespace Scripting.TokenIcer

    ''' <summary>
    ''' a Token object class, This defines the Token object
    ''' </summary>
    ''' <typeparam name="Tokens">应该是枚举类型</typeparam>
    ''' <remarks>
    ''' A Token object holds the token and token value.
    ''' </remarks>
    Public Class CodeToken(Of Tokens As IComparable) : Implements Value(Of String).IValueOf

        ''' <summary>
        ''' Token type
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute("name")>
        Public Property name As Tokens
        Public Property span As CodeSpan

        ''' <summary>
        ''' The text that makes up the token.
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute>
        Public Property text As String Implements Value(Of String).IValueOf.Value

        ''' <summary>
        ''' Returns a Boolean value indicating whether an expression can be evaluated as
        ''' a number.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property isNumeric As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return text.IsNumeric
            End Get
        End Property

        Public Sub New(name As Tokens, value$)
            Me.name = name
            Me.text = value
        End Sub

        Sub New(name As Tokens)
            Me.name = name
        End Sub

        Sub New()
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return $"[{name}] {text}"
        End Function

#If NET_48 Or netcore5 = 1 Then

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator =(token As CodeToken(Of Tokens), element As (Tokens, String())) As Boolean
            Return token.name.Equals(element.Item1) AndAlso (element.Item2.IndexOf(token.text) > -1)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator <>(token As CodeToken(Of Tokens), element As (Tokens, String())) As Boolean
            Return Not token = element
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator =(token As CodeToken(Of Tokens), element As (Tokens, String)) As Boolean
            Return token.name.Equals(element.Item1) AndAlso token.text = element.Item2
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator <>(token As CodeToken(Of Tokens), element As (Tokens, String)) As Boolean
            Return Not token = element
        End Operator

#End If
    End Class
End Namespace
