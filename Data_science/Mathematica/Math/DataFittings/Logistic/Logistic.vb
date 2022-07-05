Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Serialization.JSON
Imports stdNum = System.Math

''' <summary>
''' Performs simple logistic regression.
''' User: tpeng
''' Date: 6/22/12
''' Time: 11:01 PM
''' 
''' @author tpeng
''' @author Matthieu Labas
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
    ''' 1.0 / (1.0 + e ^ -z)
    ''' </summary>
    ''' <param name="z"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Friend Shared Function sigmoid(z As Double) As Double
        Return 1.0 / (1.0 + stdNum.E ^ -z)
    End Function

    Public Function train(instances As IEnumerable(Of Instance)) As LogisticFit
        Dim matrix As Instance() = instances.ToArray
        Dim theta As Double() = Me.theta.Array
        Dim m As Double = matrix.Length

        For n As Integer = 0 To ITERATIONS - 1
            Dim grad As Double = 0.0
            Dim cost As Double = 0.0

            For i As Integer = 0 To matrix.Length - 1
                Dim x = matrix(i).x
                Dim h = predict(x, theta)
                Dim y = matrix(i).label

                For j As Integer = 0 To theta.Length - 1
                    theta(j) = theta(j) - (ALPHA / m) * ((h - y) * x(j))
                Next

                'h = predict(x, theta)
                'cost += (1 / m) * (-y * stdNum.Log(h) - (1 - y) * stdNum.Log(1 - h))
                'grad += x.Select(Function(xi) xi * (1 / m) * (h - y)).Sum
            Next

            If Not println Is Nothing Then
                ' Call println("iteration: " & n & " " & theta.GetJson & " grad: " & grad & " cost: " & cost)
            End If
        Next

        Me.theta = New Vector(theta)

        Return LogisticFit.CreateFit(Me, matrix)
    End Function

    Private Function predict(x As Double(), theta As Double()) As Double
        Dim logit As Double = theta.Select(Function(wi, i) wi * x(i)).Sum
        Dim p = sigmoid(logit)

        Return p
    End Function

    Public Function predict(x As Double()) As Double
        Dim logit As Double = (theta * x).Sum
        Dim p = sigmoid(logit)

        Return 1 - p
    End Function
End Class