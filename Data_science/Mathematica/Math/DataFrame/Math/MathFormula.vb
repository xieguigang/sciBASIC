#Region "Microsoft.VisualBasic::b25104f28f1129d36998351f37d7d0a5, Data_science\Mathematica\Math\DataFrame\MathFormula.vb"

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

    '   Total Lines: 68
    '    Code Lines: 56 (82.35%)
    ' Comment Lines: 2 (2.94%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 10 (14.71%)
    '     File Size: 2.60 KB


    ' Module MathFormula
    ' 
    '     Function: (+2 Overloads) Evaluate, toNumber
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.Framework
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl

Public Module MathFormula

    <Extension>
    Public Function Evaluate(table As DataFrame, formula As String, Optional imputeNA As Boolean = False) As Double()
        Return table.Evaluate(Expression.Parse(formula), imputeNA)
    End Function

    <Extension>
    Public Function Evaluate(table As DataFrame, formula As Expression, Optional imputeNA As Boolean = False) As Double()
        Dim varNames As String() = formula.GetVariableSymbols.Distinct.ToArray
        Dim vectors As Dictionary(Of String, Func(Of Integer, Object)) = varNames _
            .ToDictionary(Function(name) name,
                          Function(name)
                              Return table(name).Getter
                          End Function)
        Dim nlen As Integer = table.nsamples
        Dim result As Double() = New Double(nlen - 1) {}
        Dim engine As New ExpressionEngine

        For i As Integer = 0 To nlen - 1
            For Each var As String In varNames
                Call engine.SetSymbol(var, toNumber(vectors(var)(i), imputeNA))
            Next

            result(i) = engine.Evaluate(formula)
        Next

        Return result
    End Function

    Private Function toNumber(any As Object, imputeNA As Boolean) As Double
        If any Is Nothing Then
            Return If(imputeNA, 0.0, Double.NaN)
        End If

        If DataFramework.IsNumericType(any.GetType) Then
            Dim x As Double = Conversion.Val(any)

            If x.IsNaNImaginary Then
                Return If(imputeNA, 0, x)
            Else
                Return x
            End If
        ElseIf TypeOf any Is Boolean Then
            Return If(CBool(any), 1, 0)
        Else
            Dim si As String = Microsoft.VisualBasic.Scripting.ToString(any)

            If si.StringEmpty(, True) Then
                Return If(imputeNA, 0.0, Double.NaN)
            ElseIf si.IsNumeric Then
                Return si.ParseDouble
            ElseIf si.StartsWith("<") Then
                ' 20250926 deal with some special situation, example like
                ' LC-MS/MS quantification result: <LOD
                Return 0
            Else
                Return If(imputeNA, 0.0, Double.NaN)
            End If
        End If
    End Function
End Module
