#Region "Microsoft.VisualBasic::7e2833c57a5dfeea1966b9f06417e450, Data_science\Mathematica\Math\MathLambda\MathMLCompiler.vb"

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

    '   Total Lines: 91
    '    Code Lines: 75
    ' Comment Lines: 3
    '   Blank Lines: 13
    '     File Size: 4.00 KB


    ' Class MathMLCompiler
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: CastExpression, CreateBinary, CreateLambda, CreateLiteral, CreateMathCalls
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Linq.Expressions
Imports System.Reflection
Imports Microsoft.VisualBasic.MIME.application.xml
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports ML = Microsoft.VisualBasic.MIME.application.xml.MathML.BinaryExpression
Imports MLLambda = Microsoft.VisualBasic.MIME.application.xml.MathML.LambdaExpression
Imports MLSymbol = Microsoft.VisualBasic.MIME.application.xml.MathML.SymbolExpression

''' <summary>
''' mathML -> lambda -> linq expression -> compile VB lambda
''' </summary>
Public Class MathMLCompiler

    Dim symbols As SymbolIndex

    Shared ReadOnly clr_mathFuncs As New Dictionary(Of String, MethodInfo)

    Shared Sub New()
        Dim math As Type = GetType(System.Math)
        Dim funcs = From f As MethodInfo
                    In math.GetMethods
                    Let rtvl = f.ReturnType
                    Where f.IsStatic AndAlso f.IsPublic
                    Where rtvl IsNot Nothing AndAlso rtvl IsNot GetType(Void)
                    Where f.GetParameters.All(Function(a) a.ParameterType Is GetType(Double))
                    Select f
                    Group By f.Name Into Group

        For Each func In funcs
            Call clr_mathFuncs.Add(func.Name.ToLower, func.Group.FirstOrDefault)
        Next
    End Sub

    Public Shared Function CreateLambda(lambda As MLLambda) As LambdaExpression
        Dim parameters = SymbolIndex.FromLambda(lambda)
        Dim compiler As New MathMLCompiler With {.symbols = parameters}
        Dim body As Expression = compiler.CastExpression(lambda.lambda)
        Dim expr As LambdaExpression = Expression.Lambda(body, parameters.Alignments)

        Return expr
    End Function

    Private Function CastExpression(lambda As MathML.MathExpression) As Expression
        Select Case lambda.GetType
            Case GetType(MLSymbol) : Return CreateLiteral(lambda)
            Case GetType(MathML.MathFunctionExpression) : Return CreateMathCalls(lambda)
            Case GetType(ML) : Return CreateBinary(TryCast(lambda, ML))
            Case Else
                Throw New NotImplementedException(lambda.GetType.FullName)
        End Select
    End Function

    Private Function CreateMathCalls(func As MathML.MathFunctionExpression) As Expression
        Dim args As New List(Of Expression)
        Dim f As MethodInfo = clr_mathFuncs(func.name)

        For Each arg In func.parameters
            Call args.Add(CastExpression(arg))
        Next

        Return Expression.Call(f, args.ToArray)
    End Function

    Private Function CreateLiteral(symbol As MLSymbol) As Expression
        If symbol.isNumericLiteral Then
            Return Expression.Constant(ParseDouble(symbol.text), GetType(Double))
        Else
            Return symbols(symbol.text)
        End If
    End Function

    Private Function CreateBinary(member As ML) As Expression
        Select Case MathML.ContentBuilder.SimplyOperator(member.operator)
            Case "+" : Return Expression.Add(CastExpression(member.applyleft), CastExpression(member.applyright))
            Case "-"
                If member.applyright Is Nothing Then
                    Return Expression.Negate(CastExpression(member.applyleft))
                Else
                    Return Expression.Subtract(
                        CastExpression(member.applyleft),
                        CastExpression(member.applyright)
                    )
                End If
            Case "*" : Return Expression.Multiply(CastExpression(member.applyleft), CastExpression(member.applyright))
            Case "/" : Return Expression.Divide(CastExpression(member.applyleft), CastExpression(member.applyright))
            Case "^" : Return Expression.Power(CastExpression(member.applyleft), CastExpression(member.applyright))
            Case Else
                Throw New InvalidCastException(member.operator)
        End Select
    End Function
End Class
