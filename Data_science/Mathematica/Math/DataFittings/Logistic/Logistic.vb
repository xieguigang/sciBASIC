#Region "Microsoft.VisualBasic::9591d64384ccf462256fd63e332873e2, Data_science\Mathematica\Math\DataFittings\Logistic\Logistic.vb"

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

    '   Total Lines: 102
    '    Code Lines: 59 (57.84%)
    ' Comment Lines: 24 (23.53%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 19 (18.63%)
    '     File Size: 3.86 KB


    '     Class Logistic
    ' 
    '         Properties: ALPHA, ITERATIONS
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: computeCost, predict, (+2 Overloads) sigmoid, train
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports std = System.Math

Namespace Logistic

    ''' <summary>
    ''' This method uses Gradient descent algorithm. It uses number of small
    ''' steps (iterations) And with each step use New theta values which
    ''' results in smaller cost function value. After a while it comes to 
    ''' local minima (minimum cost function).
    ''' </summary>
    Public Class Logistic

        ''' <summary>
        ''' the learning rate 
        ''' </summary>
        Public Property ALPHA As Double = 0.0001
        ''' <summary>
        ''' the number of iterations 
        ''' </summary>
        Public Property ITERATIONS As Integer = 3000

        ''' <summary>
        ''' the weight to learn 
        ''' </summary>
        Friend theta As Vector

        Dim println As Action(Of String)

        Public Sub New(n As Integer, Optional rate As Double = 0.0001, Optional println As Action(Of String) = Nothing)
            Me.ALPHA = rate
            Me.theta = Vector.rand(n) * n
            Me.println = println
        End Sub

        Sub New()
        End Sub

        ''' <summary>
        ''' Sigmoid function. Formula: g = 1 ./ (1 + (exp(-1 .* z)));
        ''' </summary>
        ''' <param name="z">
        ''' Matrix, for which elements sigmoid function is calculated.
        ''' </param>
        ''' <returns>
        ''' Matrix with elements from sigmoid function.
        ''' </returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Shared Function sigmoid(z As NumericMatrix) As NumericMatrix
            Return 1.0 / (1.0 + std.E ^ -z)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Shared Function sigmoid(z As Double) As Double
            Return 1.0 / (1.0 + std.E ^ -z)
        End Function

        Friend Shared Function computeCost(features As NumericMatrix, values As NumericMatrix, theta As NumericMatrix) As Double
            Dim size As Integer = values.RowDimension
            Dim one As NumericMatrix = NumericMatrix.One(values.ColumnDimension, values.RowDimension)
            Dim first As NumericMatrix = (-values) * sigmoid(features * theta).Log
            Dim second As NumericMatrix = (one - values) * (one - sigmoid(features * theta)).Log

            Return (first - second).Sum(Function(v) v.Sum) / size
        End Function

        Public Function train(instances As IEnumerable(Of Instance)) As LogisticFit
            Dim raw As Instance() = instances.ToArray
            Dim size As Integer = raw.Length
            Dim dims As Integer = raw(Scan0).featureSize
            Dim features As Double()() = New Double(size - 1)() {}
            Dim labels As Double() = New Double(size - 1) {}
            Dim weights As Double() = New Double(dims - 1) {}
            Dim gradient As Double() = New Double(dims - 1) {}
            Dim hypothesis As Double() = New Double(size - 1) {}

            For i As Integer = 0 To size - 1
                features(i) = raw(i).x
                labels(i) = raw(i).label
            Next

            ' 标准的批量梯度下降：
            '   theta := theta - alpha * X^T (sigmoid(X * theta) - y) / m
            ' 
            ' 这里使用普通的 Double 数组直接计算，避免依赖 NumericMatrix 的
            ' 运算符语义（其 ``*`` 运算符为逐元素乘法，直接用于 X^T*y 会出错）。
            For iteration As Integer = 0 To ITERATIONS - 1
                For i As Integer = 0 To size - 1
                    Dim z As Double = 0
                    Dim xi As Double() = features(i)

                    For j As Integer = 0 To dims - 1
                        z += xi(j) * weights(j)
                    Next

                    hypothesis(i) = sigmoid(z)
                Next

                For j As Integer = 0 To dims - 1
                    gradient(j) = 0
                Next

                For i As Integer = 0 To size - 1
                    Dim delta As Double = hypothesis(i) - labels(i)
                    Dim xi As Double() = features(i)

                    For j As Integer = 0 To dims - 1
                        gradient(j) += delta * xi(j)
                    Next
                Next

                For j As Integer = 0 To dims - 1
                    weights(j) -= ALPHA * gradient(j) / size
                Next
            Next

            Me.theta = New Vector(weights)

            Return LogisticFit.CreateFit(Me, raw)
        End Function

        Public Function predict(x As Double()) As Double
            Dim logit As Double = (theta * x).Sum
            Dim p = sigmoid(logit)

            Return 1 - p
        End Function
    End Class
End Namespace
