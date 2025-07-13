﻿#Region "Microsoft.VisualBasic::4467fbc2496521ab61e69b72ae1e53f9, Data_science\Mathematica\Math\Math\Scripting\Expression\ExpressionCompiler.vb"

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

'   Total Lines: 95
'    Code Lines: 76 (80.00%)
' Comment Lines: 5 (5.26%)
'    - Xml Docs: 100.00%
' 
'   Blank Lines: 14 (14.74%)
'     File Size: 4.96 KB


'     Delegate Function
' 
' 
'     Module ExpressionCompiler
' 
'         Function: CreateExpression, CreateLambda, MakeSymbolReference
' 
' 
' 
' /********************************************************************************/

#End Region

Imports System.Linq.Expressions
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.DynamicProgramming.Levenshtein
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl
Imports ScriptExpression = Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl.Expression

Namespace Scripting.MathExpression

    ''' <summary>
    ''' An abstract function interface for make math formula expression evaluation
    ''' </summary>
    ''' <param name="eval">evaluate the variable as numeric value.</param>
    ''' <returns></returns>
    Public Delegate Function DynamicInvoke(eval As Func(Of String, Double)) As Double

    Public Module ExpressionCompiler

        Public Function CreateLambda(arguments As String(), model As ScriptExpression) As LambdaExpression
            Dim parameters As Dictionary(Of String, ParameterExpression) = arguments _
                .ToDictionary(Function(name) name,
                              Function(name)
                                  Return Expressions.Expression.Parameter(GetType(Double), name)
                              End Function)
            Dim body As Expressions.Expression = CreateExpression(parameters, model, {})
            Dim lambda As LambdaExpression = Expressions.Expression.Lambda(body, parameters.Values.ToArray)

            Return lambda
        End Function

        Private Function CreateExpression(parameters As Dictionary(Of String, ParameterExpression),
                                          model As ScriptExpression,
                                          applied As Index(Of String)) As Expressions.Expression
            Select Case model.GetType
                Case GetType(Literal)
                    ' a constant
                    Return Expressions.Expression.Constant(DirectCast(model, Literal).Evaluate(Nothing), GetType(Double))
                Case GetType(Factorial)
                    ' a! factorial expression
                    Return Expressions.Expression.Constant(DirectCast(model, Factorial).Evaluate(Nothing), GetType(Double))
                Case GetType(SymbolExpression)
                    ' x
                    Return DirectCast(model, SymbolExpression).MakeSymbolReference(parameters, applied)
                Case GetType(Impl.BinaryExpression)
                    ' a - b
                    Dim bin As Impl.BinaryExpression = DirectCast(model, Impl.BinaryExpression)
                    Dim left As Expressions.Expression = CreateExpression(parameters, bin.left, applied)
                    Dim right As Expressions.Expression = CreateExpression(parameters, bin.right, applied)

                    Select Case bin.operator
                        Case "+"c : Return Expressions.Expression.Add(left, right)
                        Case "-"c : Return Expressions.Expression.Subtract(left, right)
                        Case "*"c : Return Expressions.Expression.Multiply(left, right)
                        Case "/"c : Return Expressions.Expression.Divide(left, right)
                        Case "^"c : Return Expressions.Expression.Power(left, right)
                        Case "%"c : Return Expressions.Expression.Modulo(left, right)
                        Case Else
                            Throw New InvalidCastException(bin.ToString)
                    End Select
                Case GetType(Impl.FunctionInvoke)
                    Dim fx As Impl.FunctionInvoke = DirectCast(model, Impl.FunctionInvoke)
                    Dim name As String = fx.funcName
                    Dim args As Expressions.Expression() = fx.parameters _
                        .SafeQuery _
                        .Select(Function(r) CreateExpression(parameters, r, applied)) _
                        .ToArray

                    Select Case name.ToLower
                        Case "abs"
                            Dim absMethod As MethodInfo = GetType(System.Math).GetMethod("Abs", {GetType(Double)})
                            Dim callAbs As MethodCallExpression = Expressions.Expression.Call(absMethod, args(0))
                            Return callAbs
                        Case Else
                            Throw New NotImplementedException($"the function call of '{name}' is not implemented!")
                    End Select


                Case Else
                    Throw New InvalidProgramException(model.GetType.FullName)
            End Select
        End Function

        <Extension>
        Private Function MakeSymbolReference(model As SymbolExpression,
                                             parameters As Dictionary(Of String, ParameterExpression),
                                             applied As Index(Of String)) As Expressions.Expression

            Dim symbolName As String = DirectCast(model, SymbolExpression).symbolName

            If parameters.ContainsKey(symbolName) Then
                If Not symbolName Like applied Then
                    applied += symbolName
                End If

                Return parameters(symbolName)
            Else
                With parameters _
                    .Where(Function(t) Not t.Key Like applied) _
                    .OrderByDescending(Function(t)
                                           Return LevenshteinDistance.ComputeDistance(t.Key, symbolName)?.MatchSimilarity >= 0.5
                                       End Function) _
                    .FirstOrDefault

                    If Not .Value Is Nothing Then
                        Call $"the missing {symbolName} is replaced by un-applied '{ .Key}'!".Warning

                        applied += .Key
                        Return .Value
                    Else
                        Throw New EntryPointNotFoundException(symbolName)
                    End If
                End With
            End If
        End Function
    End Module
End Namespace
