#Region "Microsoft.VisualBasic::27b9d298c52b33b41bf11c8e1d8d5149, Data_science\Mathematica\Math\Math\Scripting\Expression\ExpressionBuilder.vb"

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

    '   Total Lines: 211
    '    Code Lines: 160 (75.83%)
    ' Comment Lines: 23 (10.90%)
    '    - Xml Docs: 73.91%
    ' 
    '   Blank Lines: 28 (13.27%)
    '     File Size: 8.34 KB


    '     Module ExpressionBuilder
    ' 
    '         Function: AsCallFunction, AsExpression, BuildExpression, isFunctionInvoke, isOperator
    ' 
    '         Sub: joinNegatives, processOperators
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl

Namespace Scripting.MathExpression

    Public Module ExpressionBuilder

        ReadOnly operatorPriority As String() = {"^", "*/%", "+-"}

        <Extension>
        Private Function AsExpression(token As MathToken) As Expression
            Select Case token.name
                Case MathTokens.Literal
                    If token.text.Last = "!"c Then
                        Return New Factorial(token.text)
                    Else
                        Return New Literal(token.text)
                    End If
                Case MathTokens.Symbol
                    Return New SymbolExpression(token.text)
                Case MathTokens.Open, MathTokens.Close, MathTokens.Invalid, MathTokens.Operator, MathTokens.Terminator
                    Throw New SyntaxErrorException
                Case Else
                    Throw New NotImplementedException
            End Select
        End Function

        <Extension>
        Private Function AsCallFunction(tokens As MathToken()) As Expression
            Dim funSymbol As MathToken = tokens(Scan0)
            Dim argVals As Expression() = tokens _
                .Skip(2) _
                .Take(tokens.Length - 3) _
                .SplitByTopLevelDelimiter(MathTokens.Comma) _
                .Where(Function(b) Not (b.Length = 1 AndAlso b(Scan0).name = MathTokens.Comma)) _
                .Select(AddressOf BuildExpression) _
                .ToArray
            Dim funCall As New FunctionInvoke(funSymbol.text, argVals)

            Return funCall
        End Function

        Public Function BuildExpression(tokens As MathToken()) As Expression
            Dim blocks = tokens.SplitByTopLevelDelimiter(MathTokens.Operator)

            If blocks = 1 Then
                If blocks(Scan0).Length > 1 Then
                    If blocks(Scan0).isFunctionInvoke Then
                        Return blocks(Scan0).AsCallFunction
                    Else
                        ' (....)
                        tokens = blocks(Scan0)
                        tokens = tokens.Skip(1).Take(tokens.Length - 2).ToArray
                        blocks = tokens.SplitByTopLevelDelimiter(MathTokens.Operator)
                    End If
                Else
                    With blocks(Scan0)(Scan0)
                        Return .AsExpression
                    End With
                End If
            End If

            If blocks = 1 Then
                Return BuildExpression(blocks(Scan0))
            End If

            Dim buf As New List(Of [Variant](Of Expression, String))
            Dim oplist As New List(Of String)

            Call blocks.joinNegatives(buf, oplist)
            ' 算数操作符以及字符串操作符按照操作符的优先度进行构建
            Call buf.processOperators(oplist, operatorPriority, test:=Function(op, o) op.IndexOf(o) > -1)

            If buf > 1 Then
                Throw New SyntaxErrorException
            Else
                Return buf(Scan0)
            End If
        End Function

        ''' <summary>
        ''' *
        ''' </summary>
        ''' <param name="tokens"></param>
        ''' <returns></returns>
        <Extension>
        Public Function isFunctionInvoke(tokens As MathToken()) As Boolean
            If tokens(Scan0).name = MathTokens.Symbol AndAlso
               tokens(1).name = MathTokens.Open AndAlso
               tokens.Last.name = MathTokens.Close Then

                Return True
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' *
        ''' </summary>
        ''' <param name="tokens"></param>
        ''' <param name="operators"></param>
        ''' <returns></returns>
        <Extension>
        Public Function isOperator(tokens As MathToken(), ParamArray operators As String()) As Boolean
            If tokens.Length = 1 AndAlso tokens(Scan0).name = MathTokens.Operator Then
                If operators.Length > 0 Then
                    Dim op$ = tokens(Scan0).text

                    For Each test As String In operators
                        If op = test Then
                            Return True
                        End If
                    Next

                    Return False
                Else
                    Return True
                End If
            Else
                Return False
            End If
        End Function

        <Extension>
        Private Sub joinNegatives(tokenBlocks As List(Of MathToken()), ByRef buf As List(Of [Variant](Of Expression, String)), ByRef oplist As List(Of String))
            Dim syntaxResult As Expression
            Dim index As i32 = Scan0

            If tokenBlocks(Scan0).Length = 1 AndAlso tokenBlocks(Scan0)(Scan0) = (MathTokens.Operator, {"-", "+"}) Then
                If tokenBlocks(1).Length = 1 AndAlso tokenBlocks(1)(Scan0).isNumeric Then
                    tokenBlocks.RemoveAt(Scan0)
                    tokenBlocks(0)(0).text = "-" & tokenBlocks(0)(0).text
                Else
                    ' insert a ZERO before
                    tokenBlocks.Insert(Scan0, {MathToken.ZERO})
                End If
            End If

            For i As Integer = Scan0 To tokenBlocks.Count - 1
                If ++index Mod 2 = 0 Then
                    If tokenBlocks(i).isOperator("+", "-") Then
                        syntaxResult = ExpressionBuilder.BuildExpression(tokenBlocks(i + 1))
                        syntaxResult = New BinaryExpression(
                            left:=New Literal(0),
                            right:=syntaxResult,
                            op:=tokenBlocks(i)(Scan0).text
                        )

                        i += 1
                    Else
                        syntaxResult = ExpressionBuilder.BuildExpression(tokenBlocks(i))
                    End If

                    Call buf.Add(syntaxResult)
                Else
                    Call buf.Add(tokenBlocks(i)(Scan0).text)
                    Call oplist.Add(buf.Last.VB)
                End If
            Next
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="buf"></param>
        ''' <param name="oplist"></param>
        ''' <param name="operators$"></param>
        ''' <param name="test">test(op, o)</param>
        <Extension>
        Private Sub processOperators(buf As List(Of [Variant](Of Expression, String)), oplist As List(Of String), operators$(), test As Func(Of String, String, Boolean))
            Dim binExp As Expression
            Dim a As Expression
            Dim b As Expression

            For Each op As String In operators
                Dim nop As Integer = oplist _
                    .AsEnumerable _
                    .Count(Function(o) test(op, o))

                ' 从左往右计算
                For i As Integer = 0 To nop - 1
                    For j As Integer = 0 To buf.Count - 1
                        If buf(j) Like GetType(String) AndAlso test(op, buf(j).VB) Then
                            ' j-1 and j+1
                            If j - 1 < 0 Then
                                Throw New SyntaxErrorException
                            Else
                                a = buf(j - 1).TryCast(Of Expression)
                            End If
                            If j + 1 > buf.Count Then
                                Throw New SyntaxErrorException
                            Else
                                b = buf(j + 1).TryCast(Of Expression)
                            End If

                            binExp = New BinaryExpression(a, b, buf(j).VB)

                            Call buf.RemoveRange(j - 1, 3)
                            Call buf.Insert(j - 1, binExp)

                            Exit For
                        End If
                    Next
                Next
            Next
        End Sub
    End Module
End Namespace
