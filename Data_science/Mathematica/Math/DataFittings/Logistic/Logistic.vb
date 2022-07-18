Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports Microsoft.VisualBasic.Serialization.JSON
Imports stdNum = System.Math

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
        Return 1.0 / (1.0 + stdNum.E ^ -z)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Friend Shared Function sigmoid(z As Double) As Double
        Return 1.0 / (1.0 + stdNum.E ^ -z)
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
        Dim values As New Vector(raw.Select(Function(r) r.label))
        Dim size As Integer = values.Length
        Dim features As New NumericMatrix(raw.Length, raw(Scan0).featureSize)
        Dim theta As NumericMatrix = NumericMatrix.Zero(1, features.ColumnDimension)

        For i As Integer = 0 To raw.Length - 1
            features.Array(i) = raw(i).x
        Next

        For i As Integer = 0 To ITERATIONS - 1
            Dim featuresTranspose As NumericMatrix = features.Transpose
            Dim hx = sigmoid(features * theta)
            Dim A = featuresTranspose * hx
            Dim B = (featuresTranspose * values) / size
            Dim delta = A - B

            theta = theta - (delta * ALPHA)
        Next

        Me.theta = theta.ColumnVector(0)

        Return LogisticFit.CreateFit(Me, raw)
    End Function

    Public Function predict(x As Double()) As Double
        Dim logit As Double = (theta * x).Sum
        Dim p = sigmoid(logit)

        Return 1 - p
    End Function
End Class