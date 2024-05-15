#Region "Microsoft.VisualBasic::bcf92ea39b2d00d39d400f74de35edd4, Data_science\Mathematica\Math\Math\Scripting\Expression\ExpressionEngine.vb"

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

    '   Total Lines: 128
    '    Code Lines: 100
    ' Comment Lines: 9
    '   Blank Lines: 19
    '     File Size: 5.58 KB


    '     Class ExpressionEngine
    ' 
    '         Function: AddFunction, (+2 Overloads) Evaluate, GetFunction, GetSymbolValue, Parse
    '                   (+2 Overloads) SetSymbol
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl
Imports std = System.Math

Namespace Scripting.MathExpression

    Public Class ExpressionEngine

        ReadOnly symbols As New Dictionary(Of String, Double) From {
            {"PI", std.PI},
            {"E", std.E}
        }

        ''' <summary>
        ''' The mathematics calculation delegates collection with its specific name.
        ''' (具有特定名称的数学计算委托方法的集合) 
        ''' </summary>
        ''' <remarks></remarks>
        ReadOnly functions As New Dictionary(Of String, Func(Of Double(), Double)) From {
                                                                                         _
            {"abs", Function(args) std.Abs(args(Scan0))},
            {"acos", Function(args) std.Acos(args(Scan0))},
            {"asin", Function(args) std.Asin(args(Scan0))},
            {"atan", Function(args) std.Atan(args(Scan0))},
            {"atan2", Function(args) std.Atan2(args(Scan0), args(1))},
            {"bigmul", Function(args) std.BigMul(args(Scan0), args(1))},
            {"ceiling", Function(args) std.Ceiling(args(Scan0))},
            {"cos", Function(args) std.Cos(args(Scan0))},
            {"cosh", Function(args) std.Cosh(args(Scan0))},
            {"exp", Function(args) std.Exp(args(Scan0))},
            {"floor", Function(args) std.Floor(args(Scan0))},
            {"ieeeremainder", Function(args) std.IEEERemainder(args(Scan0), args(1))},
            {"log", Function(args) std.Log(args(Scan0), newBase:=args(1))},
            {"ln", Function(args) std.Log(args(Scan0))},
            {"log10", Function(args) std.Log10(args(Scan0))},
            {"max", Function(args) std.Max(args(Scan0), args(1))},
            {"min", Function(args) std.Min(args(Scan0), args(1))},
            {"pow", Function(args) std.Pow(args(Scan0), args(1))},
            {"round", Function(args) std.Round(args(Scan0))},
            {"sign", Function(args) std.Sign(args(Scan0))},
            {"sin", Function(args) std.Sin(args(Scan0))},
            {"sinh", Function(args) std.Sinh(args(Scan0))},
            {"sqrt", Function(args) std.Sqrt(args(Scan0))},
            {"tan", Function(args) std.Tan(args(Scan0))},
            {"tanh", Function(args) std.Tanh(args(Scan0))},
            {"truncate", Function(args) std.Truncate(args(Scan0))},
            {"rnd", Function(args) Arithmetic.RND(args(Scan0), args(1))},
            {"int", Function(args) CType(args(Scan0), Integer)}
        }

        Default Public Property Symbol(name As String) As Double
            Get
                Return symbols(name)
            End Get
            Set(value As Double)
                symbols(name) = value
            End Set
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="name">函数名</param>
        Public Function AddFunction(name As String, parameters As String(), lambda As String) As ExpressionEngine
            Dim lambdaExpression As Expression = New ExpressionTokenIcer(lambda).GetTokens.ToArray.DoCall(AddressOf BuildExpression)
            Dim func As Func(Of Double(), Double) =
                Function(arguments) As Double
                    Dim env As New ExpressionEngine

                    For Each symbol As KeyValuePair(Of String, Double) In symbols
                        env.symbols(symbol.Key) = symbol.Value
                    Next

                    For i As Integer = 0 To parameters.Length - 1
                        env.symbols(parameters(i)) = arguments(i)
                    Next

                    Return lambdaExpression.Evaluate(env)
                End Function

            functions(name) = func

            Return Me
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetSymbolValue(name As String) As Double
            Return symbols(name)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetFunction(name As String) As Func(Of Double(), Double)
            Return functions(name)
        End Function

        Public Function SetSymbol(symbol As String, value As Double) As ExpressionEngine
            symbols(symbol) = value
            Return Me
        End Function

        Public Function SetSymbol(symbol As String, expression As String) As ExpressionEngine
            symbols(symbol) = Evaluate(expression)
            Return Me
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Evaluate(expression As Expression) As Double
            Return expression.Evaluate(env:=Me)
        End Function

        Public Function Evaluate(expression As String) As Double
            Dim tokens As MathToken() = New ExpressionTokenIcer(expression).GetTokens.ToArray
            Dim exp As Expression = ExpressionBuilder.BuildExpression(tokens)
            Dim result As Double = exp.Evaluate(Me)

            Return result
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Parse(expression As String) As Expression
            Return New ExpressionTokenIcer(expression) _
                .GetTokens _
                .ToArray _
                .DoCall(AddressOf BuildExpression)
        End Function
    End Class
End Namespace
