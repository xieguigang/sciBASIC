#Region "Microsoft.VisualBasic::c780b9067ddb0ca443b576eee48a68b3, Data_science\Mathematica\Math\DataFrame\Correlation\CorrelationMatrix.vb"

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

    '   Total Lines: 162
    '    Code Lines: 121 (74.69%)
    ' Comment Lines: 20 (12.35%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 21 (12.96%)
    '     File Size: 5.57 KB


    ' Class CorrelationMatrix
    ' 
    '     Properties: (+2 Overloads) pvalue
    ' 
    '     Constructor: (+2 Overloads) Sub New
    '     Function: GetCorrelationQuantile, GetPvalueMatrix, GetPvalueQuantile, GetUniqueTuples, PositiveMatrix
    '               Power, Sign
    '     Operators: *
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports Microsoft.VisualBasic.Math.Quantile
Imports std = System.Math

''' <summary>
''' the correlation matrix join the pvalue matrix
''' </summary>
Public Class CorrelationMatrix : Inherits DataMatrix

    ReadOnly pvalueMat As Double()()

    Public Property pvalue(a$, b$) As Double
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return pvalue(names(a), names(b))
        End Get
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Set
            pvalue(names(a), names(b)) = Value
        End Set
    End Property

    Public Overridable Property pvalue(i%, j%) As Double
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return pvalueMat(j)(i)
        End Get
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Set(value As Double)
            pvalueMat(j)(i) = value
        End Set
    End Property

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Sub New(names As IEnumerable(Of String))
        Call MyBase.New(names)
    End Sub

    Sub New(names As Index(Of String), matrix As Double()(), pvalue As Double()())
        Call MyBase.New(names, matrix)

        Me.pvalueMat = pvalue
    End Sub

    Public Function GetPvalueMatrix() As GeneralMatrix
        Return New NumericMatrix(pvalueMat)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function GetUniqueTuples() As IEnumerable(Of (a$, b$))
        Return keys _
            .Select(Function(a)
                        Return keys _
                            .Where(Function(b) a <> b) _
                            .Select(Function(b) (a, b))
                    End Function) _
            .IteratesALL _
            .GroupBy(Function(t)
                         Return {t.a, t.b} _
                            .OrderBy(Function(str) str) _
                            .JoinBy("-")
                     End Function) _
            .Select(Function(t) t.First)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function GetCorrelationQuantile() As FastRankQuantile
        Return GetUniqueTuples _
            .Select(Function(t)
                        Return std.Abs(Me(t.a, t.b))
                    End Function) _
            .DoCall(Function(q)
                        Return New FastRankQuantile(q)
                    End Function)
    End Function

    ''' <summary>
    ''' note: this method returns the -log10(p.value)
    ''' </summary>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function GetPvalueQuantile() As FastRankQuantile
        Return GetUniqueTuples _
            .Select(Function(t)
                        Return -std.Log10(pvalue(t.a, t.b))
                    End Function) _
            .DoCall(Function(q)
                        Return New FastRankQuantile(q)
                    End Function)
    End Function

    ''' <summary>
    ''' cor ^ exp
    ''' </summary>
    ''' <param name="exp"></param>
    ''' <returns></returns>
    Public Function Power(exp As Double) As CorrelationMatrix
        Dim cor As Double()() = matrix.ToArray
        Dim pow As Double()() = cor _
            .Select(Function(c)
                        Return c.Select(Function(ci) ci ^ exp).ToArray
                    End Function) _
            .ToArray

        Return New CorrelationMatrix(names, pow, pvalueMat)
    End Function

    ''' <summary>
    ''' get the sign of the correlation value
    ''' </summary>
    ''' <returns></returns>
    Public Function Sign() As Double()()
        Return matrix _
            .Select(Function(c)
                        Return c.Select(Function(ci) CDbl(std.Sign(ci))).ToArray
                    End Function) _
            .ToArray
    End Function

    ''' <summary>
    ''' just get the positive correlation part of the matrix
    ''' </summary>
    ''' <returns></returns>
    Public Function PositiveMatrix() As CorrelationMatrix
        Dim cor As Double()() = New Double(matrix.Length - 1)() {}
        Dim pval As Double()() = New Double(matrix.Length - 1)() {}

        For i As Integer = 0 To matrix.Length - 1
            Dim row As Double() = matrix(i)
            Dim p As Double() = pvalueMat(i)
            Dim cor_v As Double() = New Double(row.Length - 1) {}
            Dim p_v As Double() = New Double(row.Length - 1) {}

            For j As Integer = 0 To row.Length - 1
                If row(j) > 0 Then
                    cor_v(j) = row(j)
                    p_v(j) = p(j)
                End If
            Next

            cor(i) = cor_v
            pval(i) = p_v
        Next

        Return New CorrelationMatrix(names, cor, pval)
    End Function

    Public Shared Operator *(x As Double()(), y As CorrelationMatrix) As CorrelationMatrix
        Dim cor As Double()() = y.matrix
        Dim mul As Double()() = x _
            .Select(Function(xi, i)
                        Return (New Vector(xi) * New Vector(cor(i))).ToArray
                    End Function) _
            .ToArray

        Return New CorrelationMatrix(y.names, mul, y.pvalueMat)
    End Operator
End Class
