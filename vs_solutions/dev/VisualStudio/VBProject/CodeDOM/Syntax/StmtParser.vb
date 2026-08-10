#Region "Microsoft.VisualBasic::d3da5eab9285e82fb56ad9fd9ae3b1fd, vs_solutions\dev\VisualStudio\VBProject\CodeDOM\Syntax\StmtParser.vb"

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

    '   Total Lines: 80
    '    Code Lines: 65 (81.25%)
    ' Comment Lines: 4 (5.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 11 (13.75%)
    '     File Size: 2.53 KB


    '     Class StmtParser
    ' 
    '         Properties: Current, Eof
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ReadAttributeBlock
    ' 
    '         Sub: CollectLeading
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text

Namespace VBProj.CodeDOM.Syntax

    ' ------------------------------------------------------------------
    ' statement cursor : skips leading attributes and modifiers
    ' ------------------------------------------------------------------
    Friend Class StmtParser

        Public Tokens As List(Of Token)
        Public Pos As Integer
        Public Attributes As New List(Of String)
        Public Modifiers As String = ""

        Public Sub New(tk As List(Of Token), Optional p As Integer = 0)
            Tokens = tk
            Pos = p
        End Sub

        Public ReadOnly Property Eof As Boolean
            Get
                Return Pos >= Tokens.Count
            End Get
        End Property

        Public ReadOnly Property Current As Token
            Get
                If Eof Then
                    Return New Token With {.Kind = TokenKind.Punctuation, .Text = ""}
                End If
                Return Tokens(Pos)
            End Get
        End Property

        Public Sub CollectLeading()
            Do
                If Not Eof AndAlso Current.Text = "<"c Then
                    Attributes.Add(ReadAttributeBlock())
                ElseIf Not Eof AndAlso IsModifier(Current.Text.ToLowerInvariant()) Then
                    If Modifiers.Length > 0 Then
                        Modifiers &= " "
                    End If
                    Modifiers &= Current.Text
                    Pos += 1
                Else
                    Exit Do
                End If
            Loop
        End Sub

        Public Function ReadAttributeBlock() As String
            ' Current is "<"
            Pos += 1
            Dim sb As New StringBuilder()
            Dim depth As Integer = 0

            While Not Eof
                Dim tk As Token = Current
                If tk.Text = "("c Then
                    depth += 1
                    sb.Append(tk.Text)
                    Pos += 1
                ElseIf tk.Text = ")"c Then
                    depth -= 1
                    sb.Append(tk.Text)
                    Pos += 1
                ElseIf tk.Text = ">"c AndAlso depth = 0 Then
                    Pos += 1
                    Exit While
                Else
                    sb.Append(tk.Text)
                    Pos += 1
                End If
            End While

            Return sb.ToString().Trim()
        End Function
    End Class

End Namespace
