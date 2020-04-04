#Region "Microsoft.VisualBasic::e9563c58807900351b0fd757c2f37468, Data_science\Mathematica\Math\Math\Scripting\Arithmetic.Expression\SimpleParser.vb"

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

    '     Module SimpleParser
    ' 
    '         Function: ClearOverlapOperator, (+2 Overloads) TryParse
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Math.Scripting.Types
Imports Microsoft.VisualBasic.Scripting.TokenIcer

Namespace Scripting

    ''' <summary>
    ''' Parser for simple expression
    ''' </summary>
    Public Module SimpleParser

        Public Function TryParse(s As String) As SimpleExpression
            Dim tokens = TokenIcer.TryParse(s.ClearOverlapOperator) 'Get all of the number that appears in this expression including factoral operator.

            If tokens.Count = 1 Then
                Dim token As Token(Of ExpressionTokens) = tokens.First

                If token.Type = ExpressionTokens.Number Then
                    Return New SimpleExpression(Val(token.Text))
                Else  ' Syntax error
                    Throw New SyntaxErrorException(s)
                End If
            Else
                Return New Pointer(Of Token(Of ExpressionTokens))(tokens).TryParse
            End If
        End Function

        <Extension>
        Public Function TryParse(tokens As Pointer(Of Token(Of ExpressionTokens))) As SimpleExpression
            Dim sep As New SimpleExpression 'New object to return for this function
            Dim s As Token(Of ExpressionTokens)
            Dim n As Double
            Dim o As Char

            Do While Not tokens.EndRead
                s = +tokens

                If tokens.EndRead Then
                    Call sep.Add(Val(s.Text), "+c")
                Else
                    o = (++tokens).Text.First
                    n = Val(s.Text)

                    If o = "!"c Then
                        n = VBMath.Factorial(n)

                        If tokens.EndRead Then
                            Call sep.Add(n, "+"c)
                            Exit Do
                        Else
                            o = (++tokens).Text.First
                        End If
                    End If

                    Call sep.Add(n, o)
                End If
            Loop

            Return sep
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="s"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Extension> Public Function ClearOverlapOperator(ByRef s As String) As String
            Dim sBuilder As StringBuilder = New StringBuilder(value:="0+" & s)

            's = "0+" & sbr.ToString '0a=a; 0-a=-a; 0+a=a

            Call sBuilder.Replace("++", "+")
            Call sBuilder.Replace("--", "+")
            Call sBuilder.Replace("+-", "-")

            Return sBuilder.ToString
        End Function
    End Module
End Namespace
