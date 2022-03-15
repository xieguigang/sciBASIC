#Region "Microsoft.VisualBasic::f59476bc1647bd45bb9921eab52a197f, sciBASIC#\Data_science\Mathematica\Math\MathLambda\MathMLCompiler.vb"

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
    '    Code Lines: 46
    ' Comment Lines: 3
    '   Blank Lines: 5
    '     File Size: 2.86 KB


    ' Module MathMLCompiler
    ' 
    '     Function: (+2 Overloads) CreateBinary, CreateLambda
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Linq.Expressions
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MIME.application.xml
Imports ML = Microsoft.VisualBasic.MIME.application.xml.MathML.BinaryExpression
Imports MLLambda = Microsoft.VisualBasic.MIME.application.xml.MathML.LambdaExpression
Imports MLSymbol = Microsoft.VisualBasic.MIME.application.xml.MathML.SymbolExpression

''' <summary>
''' mathML -> lambda -> linq expression -> compile VB lambda
''' </summary>
Public Module MathMLCompiler

    Public Function CreateLambda(lambda As MLLambda) As LambdaExpression
        Dim parameters = lambda.parameters.Select(Function(name) Expression.Parameter(GetType(Double), name)).ToDictionary(Function(par) par.Name)
        Dim body As Expression = CreateBinary(lambda.lambda, parameters)
        Dim expr As LambdaExpression = Expression.Lambda(body, lambda.parameters.Select(Function(par) parameters(par)).ToArray)

        Return expr
    End Function

    Private Function CreateBinary(member As [Variant](Of ML, MLSymbol), parameters As Dictionary(Of String, ParameterExpression)) As Expression
        If member Like GetType(MLSymbol) Then
            With member.TryCast(Of MLSymbol)
                If .isNumericLiteral Then
                    Return Expression.Constant(ParseDouble(.text), GetType(Double))
                Else
                    Return parameters(.text)
                End If
            End With
        Else
            Return CreateBinary(member.TryCast(Of ML), parameters)
        End If
    End Function

    Private Function CreateBinary(member As ML, parameters As Dictionary(Of String, ParameterExpression)) As Expression
        Select Case MathML.ContentBuilder.SimplyOperator(member.operator)
            Case "+" : Return Expression.Add(CreateBinary(member.applyleft, parameters), CreateBinary(member.applyright, parameters))
            Case "-"
                If member.applyright Is Nothing Then
                    Return Expression.Negate(CreateBinary(member.applyleft, parameters))
                Else
                    Return Expression.Subtract(
                        CreateBinary(member.applyleft, parameters),
                        CreateBinary(member.applyright, parameters)
                    )
                End If
            Case "*" : Return Expression.Multiply(CreateBinary(member.applyleft, parameters), CreateBinary(member.applyright, parameters))
            Case "/" : Return Expression.Divide(CreateBinary(member.applyleft, parameters), CreateBinary(member.applyright, parameters))
            Case "^" : Return Expression.Power(CreateBinary(member.applyleft, parameters), CreateBinary(member.applyright, parameters))
            Case Else
                Throw New InvalidCastException
        End Select
    End Function
End Module
