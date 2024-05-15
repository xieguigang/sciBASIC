#Region "Microsoft.VisualBasic::8d19a5f7569f61e05f1f50ae2de9c97a, Microsoft.VisualBasic.Core\src\Text\Parser\SyntaxTokenlizer.vb"

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

    '   Total Lines: 54
    '    Code Lines: 45
    ' Comment Lines: 0
    '   Blank Lines: 9
    '     File Size: 1.83 KB


    '     Class SyntaxTokenlizer
    ' 
    '         Properties: lastSplashEscape
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetTokens
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.TokenIcer

Namespace Text.Parser

    Public MustInherit Class SyntaxTokenlizer(Of T As IComparable, SyntaxToken As CodeToken(Of T))

        Protected ReadOnly text As CharPtr
        Protected buffer As New CharBuffer

        Protected ReadOnly Property lastSplashEscape As Boolean
            Get
                If buffer = 0 Then
                    Return False
                ElseIf buffer = 1 Then
                    If buffer = "\"c Then
                        Return True
                    Else
                        Return False
                    End If
                Else
                    Return buffer(-1) = "\"c AndAlso buffer(-2) <> "\"c
                End If
            End Get
        End Property

        Sub New(text As [Variant](Of String, CharPtr))
            If text Like GetType(String) Then
                Me.text = New CharPtr(text.TryCast(Of String))
            Else
                Me.text = text.TryCast(Of CharPtr)
            End If
        End Sub

        Public Overridable Iterator Function GetTokens() As IEnumerable(Of SyntaxToken)
            Dim token As New Value(Of SyntaxToken)

            Do While text
                If Not token = walkChar(++text) Is Nothing Then
                    Yield CType(token, SyntaxToken)
                End If
            Loop

            If buffer > 0 Then
                If Not token = popOutToken() Is Nothing Then
                    Yield CType(token, SyntaxToken)
                End If
            End If
        End Function

        Protected MustOverride Function walkChar(c As Char) As SyntaxToken
        Protected MustOverride Function popOutToken() As SyntaxToken
    End Class
End Namespace
