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
    Public Property rate As Double = 0.0001
    ''' <summary>
    ''' the number of iterations 
    ''' </summary>
    Public Property ITERATIONS As Integer = 3000

    ''' <summary>
    ''' the weight to learn 
    ''' </summary>
    Friend weights As Vector

    Dim println As Action(Of String)

    Public Sub New(n As Integer, Optional rate As Double = 0.0001, Optional println As Action(Of String) = Nothing)
        Me.rate = rate
        Me.weights = New Vector(New Double(n - 1) {})
        Me.println = println

        If Me.println Is Nothing Then
            Me.println = AddressOf Console.WriteLine
        End If
    End Sub

    Sub New()
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Friend Shared Function sigmoid(z As Double) As Double
        Return 1.0 / (1.0 + stdNum.Exp(-z))
    End Function

    Public Function train(instances As IEnumerable(Of Instance)) As LogisticFit
        Dim matrix As Instance() = instances.ToArray
        Dim weights As Double() = Me.weights.Array

        For n As Integer = 0 To ITERATIONS - 1
            Dim lik As Double = 0.0

            For i As Integer = 0 To matrix.Length - 1
                Dim x = matrix(i).x
                Dim predicted = classify(x)
                Dim label = matrix(i).label

                For j As Integer = 0 To weights.Length - 1
                    weights(j) = weights(j) + rate * (label - predicted) * x(j)
                Next

                ' not necessary for learning
                lik += label * stdNum.Log(classify(x)) + (1 - label) * stdNum.Log(1 - classify(x))
            Next

            Call println("iteration: " & n & " " & weights.GetJson & " mle: " + lik)
        Next

        Me.weights = New Vector(weights)

        Return LogisticFit.CreateFit(Me, matrix)
    End Function

    Public Function classify(x As Double()) As Double
        Dim logit As Double = (weights * x).Sum
        Dim log = sigmoid(logit)

        Return log
    End Function
End Class