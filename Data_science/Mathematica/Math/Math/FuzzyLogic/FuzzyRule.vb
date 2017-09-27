#Region "Microsoft.VisualBasic::e4be7af0c54441fc98d181208919afe9, ..\sciBASIC#\Data_science\Mathematica\Math\Math\FuzzyLogic\FuzzyRule.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
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

#Region "GNU Lesser General Public License"
'
'This file is part of DotFuzzy.
'
'DotFuzzy is free software: you can redistribute it and/or modify
'it under the terms of the GNU Lesser General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.
'
'DotFuzzy is distributed in the hope that it will be useful,
'but WITHOUT ANY WARRANTY; without even the implied warranty of
'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'GNU Lesser General Public License for more details.
'
'You should have received a copy of the GNU Lesser General Public License
'along with DotFuzzy.  If not, see <http://www.gnu.org/licenses/>.
'

#End Region

Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.Scripting.Logical
Imports Microsoft.VisualBasic.Scripting.TokenIcer

Namespace Logical.FuzzyLogic

    ''' <summary>
    ''' Represents a rule.
    ''' </summary>
    Public Class FuzzyRule : Inherits BaseClass

#Region "Private Properties"

        Dim m_text As String = [String].Empty
        Dim m_conditions As Token(Of Tokens)()
        Dim m_conclusion As Token(Of Tokens)()

#End Region

#Region "Private Methods"

        Private Function Validate(text As String) As String
            Dim count As Integer = 0
            Dim position As Integer = text.IndexOf("(")
            Dim tokens As String() = text.Replace("(", "").Replace(")", "").Split()

            While position >= 0
                count += 1
                position = text.IndexOf("(", position + 1)
            End While

            position = text.IndexOf(")")
            While position >= 0
                count -= 1
                position = text.IndexOf(")", position + 1)
            End While

            If count > 0 Then
                Throw New Exception("missing right parenthesis: " & text)
            ElseIf count < 0 Then
                Throw New Exception("missing left parenthesis: " & text)
            End If

            If tokens(0) <> "IF" Then
                Throw New Exception("'IF' not found: " & text)
            End If

            If tokens(tokens.Length - 4) <> "THEN" Then
                Throw New Exception("'THEN' not found: " & text)
            End If

            If tokens(tokens.Length - 2) <> "IS" Then
                Throw New Exception("'IS' not found: " & text)
            End If

            Dim i As Integer = 2
            While i < (tokens.Length - 5)
                If (tokens(i) <> "IS") AndAlso (tokens(i) <> "AND") AndAlso (tokens(i) <> "OR") Then
                    Throw New Exception("Syntax error: " & tokens(i))
                End If
                i = i + 2
            End While

            Return text
        End Function

#End Region

#Region "Constructors"

        ''' <summary>
        ''' Default constructor.
        ''' </summary>
        Public Sub New()
        End Sub

        ''' <param name="text">The text of the rule.</param>
        Public Sub New(text As String)
            Me.Text = text
        End Sub

#End Region

#Region "Public Properties"

        ''' <summary>
        ''' The text of the rule.
        ''' </summary>
        Public Property Text() As String
            Get
                Return m_text
            End Get
            Set(value As String)
                m_text = Validate(value)
                m_conditions = TokenIcer.TryParse(Me.Conditions)
                m_conclusion = TokenIcer.TryParse(Regex.Split(value, " THEN ", RegexOptions.IgnoreCase).Last)
            End Set
        End Property

        ''' <summary>
        ''' The value of the rule after the evaluation process.
        ''' </summary>
        Public Property Value() As Double
#End Region

#Region "Public Methods"

        ''' <summary>
        ''' Returns the conditions of the rule.
        ''' The part of the rule between IF and THEN.
        ''' </summary>
        ''' <returns>The conditions of the rule.</returns>
        Public Function Conditions() As String
            Return Me.m_text.Substring(Me.m_text.IndexOf("IF ") + 3, Me.m_text.IndexOf(" THEN") - 3)
        End Function

#End Region
    End Class
End Namespace
