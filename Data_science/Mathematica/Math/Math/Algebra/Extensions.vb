#Region "Microsoft.VisualBasic::17526557af990406901ba5ac9f600ac9, Data_science\Mathematica\Math\Math\Algebra\Extensions.vb"

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

    '   Total Lines: 218
    '    Code Lines: 115 (52.75%)
    ' Comment Lines: 72 (33.03%)
    '    - Xml Docs: 69.44%
    ' 
    '   Blank Lines: 31 (14.22%)
    '     File Size: 8.69 KB


    '     Delegate Function
    ' 
    ' 
    '     Module HelperExtensions
    ' 
    '         Function: AsMatrix, IsNaNImaginary, (+2 Overloads) jaccard_coeff, jaccard_coeff_parallel, jaccard_row
    '                   JaccardIndex, NAremove, PrimitiveLinearEquation, Tangent
    '         Class JaccardTask
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Sub: Solve
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.Language.Vectorization
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports Microsoft.VisualBasic.Parallel

Namespace LinearAlgebra

    Public Delegate Function fx(x#) As Double

    <HideModuleName>
    Public Module HelperExtensions

        <Extension>
        Public Function AsMatrix(d As IEnumerable(Of Double())) As NumericMatrix
            Return New NumericMatrix(d.ToArray)
        End Function

        ''' <summary>
        ''' get a logical vector for indicates that which elements inside the given numeric vector is invalid number, like NaN, Inf
        ''' </summary>
        ''' <param name="v"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function IsNaNImaginary(v As IEnumerable(Of Double)) As BooleanVector
            Return New BooleanVector(From xi As Double In v Let flag As Boolean = xi.IsNaNImaginary Select flag)
        End Function

        ''' <summary>
        ''' removes all invalid number from the given numeric vector data
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        <Extension>
        Public Function NAremove(x As IEnumerable(Of Double)) As IEnumerable(Of Double)
            Return From xi As Double In x Where Not xi.IsNaNImaginary
        End Function

        ''' <summary>
        ''' 返回一元一次方程
        ''' </summary>
        ''' <param name="p1"></param>
        ''' <param name="p2"></param>
        ''' <returns>``y=ax+b``</returns>
        Public Function PrimitiveLinearEquation(p1 As PointF, p2 As PointF) As fx
            Dim x1 = p1.X
            Dim x2 = p2.X
            Dim y1 = p1.Y
            Dim y2 = p2.Y
            Dim a# = (y2 - y1) / (x2 - x1)
            Dim b# = y1 - a * x1

            Return Function(x) a * x + b
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="slideWindows">Just contains two points.</param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Tangent(slideWindows As SlideWindow(Of PointF)) As fx
            Return PrimitiveLinearEquation(slideWindows.First, slideWindows.Last)
        End Function

        ''' <summary>
        ''' Compute jaccard coefficient between nearest-neighbor sets
        ''' 
        ''' Weights of both i->j and j->i are recorded if they have intersection. In this case
        ''' w(i->j) should be equal to w(j->i). In some case i->j has weights while j&lt;-i has no
        ''' intersections, only w(i->j) Is recorded. This Is determinded in code `if(u>0)`. 
        ''' In this way, the undirected graph Is symmetrized by halfing the weight 
        ''' in code ``weights(r, 2) = u/(2.0*ncol - u)/2``.
        ''' </summary>
        ''' <param name="idx"></param>
        ''' <returns>
        ''' the weight value of this function will affects by the 
        ''' <paramref name="symmetrize"/> parameters, when 
        ''' 
        ''' + symmetrize = TRUE(default), weight in value range ``(0, 0.5]``,
        ''' + FALSE, weight in value range ``(0, 1]``.
        ''' </returns>
        Public Function jaccard_coeff(idx As Integer()(), Optional symmetrize As Boolean = True) As GeneralMatrix
            Dim nrow As Integer = idx.Length
            Dim weights As New List(Of Double())
            Dim div As Double = If(symmetrize, 2, 1)

            For i As Integer = 0 To nrow - 1
                For Each k As Integer In idx(i)
                    If k < 0 OrElse i = k Then
                        ' removes no links
                        ' or selfloop
                        Continue For
                    End If

                    Dim nodei As Integer() = idx(i)
                    Dim nodej As Integer() = idx(k)
                    Dim u As Integer = nodei.Intersect(nodej).Count

                    If u > 0 Then
                        ' symmetrize the graph
                        ' u is the intersect of the i and j
                        ' so nodei size is greater than u always
                        ' weight value no negative value
                        Call weights.Add({i, k, u / (2.0 * nodei.Length - u) / div})
                    End If
                Next
            Next

            Return New NumericMatrix(weights.ToArray)
        End Function

        Public Function jaccard_coeff_parallel(idx As Integer()(), Optional symmetrize As Boolean = True) As GeneralMatrix
            ' Dim nrow As Integer = idx.Length
            Dim div As Double = If(symmetrize, 2, 1)
            'Dim weightMatrixFold = (From i As Integer
            '                        In Enumerable.Range(0, nrow).AsParallel
            '                        Let node = idx(i).ToArray
            '                        Select node.jaccard_row(i, idx, div).ToArray).ToArray
            Dim par As JaccardTask = New JaccardTask(idx, div).Run
            Dim weightMatrixFold = par.weightMatrixFold

            Return New NumericMatrix(weightMatrixFold.IteratesALL.ToArray)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' use common non-zero index as intersect
        ''' </remarks>
        Public Function JaccardIndex(x As Double(), y As Double()) As Double
            Dim ia As New List(Of Integer)
            Dim ib As New List(Of Integer)

            For i As Integer = 0 To x.Length - 1
                If x(i) <> 0 Then Call ia.Add(i)
                If y(i) <> 0 Then Call ib.Add(i)
            Next

            Dim ni As Integer = ia.Intersect(ib).Count
            Dim nu As Integer = ia.Union(ib).Count

            Return ni / nu
        End Function

        ''' <summary>
        ''' Evaluate the jaccard coeff between two dataset
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <param name="symmetrize"></param>
        ''' <returns></returns>
        <Extension>
        Public Function jaccard_coeff(a As String(), b As String(), Optional symmetrize As Boolean = True) As Double
            Dim u As Double = a.Intersect(b).Count
            Dim size As Double = a.Length
            Dim div As Double = If(symmetrize, 2, 1)
            Dim j As Double = u / (2.0 * size - u) / div

            Return j
        End Function

        Private Class JaccardTask : Inherits VectorTask

            Friend ReadOnly idx As Integer()()
            Friend ReadOnly weightMatrixFold As Double()()()
            Friend ReadOnly div As Double

            Sub New(idx As Integer()(), div As Double)
                Call MyBase.New(idx.Length)

                Me.idx = idx
                Me.weightMatrixFold = New Double(idx.Length - 1)()() {}
                Me.div = div
            End Sub

            Protected Overrides Sub Solve(start As Integer, ends As Integer, cpu_id As Integer)
                For i As Integer = start To ends
                    Dim node = idx(i).ToArray
                    Dim weights = node.jaccard_row(i, idx, div).ToArray

                    weightMatrixFold(i) = weights
                Next
            End Sub
        End Class

        <Extension>
        Private Iterator Function jaccard_row(nodei As Integer(), i As Integer, idx As Integer()(), div As Double) As IEnumerable(Of Double())
            For Each k As Integer In idx(i)
                If k < 0 OrElse i = k Then
                    ' removes no links
                    ' or selfloop
                    Continue For
                End If

                Dim nodej As Integer() = idx(k)
                Dim u As Integer = nodei.Intersect(nodej).Count

                If u > 0 Then
                    ' symmetrize the graph
                    ' u is the intersect of the i and j
                    ' so nodei size is greater than u always
                    ' weight value no negative value
                    Yield {i, k, u / (2.0 * nodei.Length - u) / div}
                End If
            Next
        End Function
    End Module
End Namespace
