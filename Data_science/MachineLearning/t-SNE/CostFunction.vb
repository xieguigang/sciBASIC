Imports stdNum = System.Math

Friend Class CostFunction

    ReadOnly tSNE As tSNE

    Public ReadOnly Property mN As Integer
        Get
            Return tSNE.mN
        End Get
    End Property

    Sub New(tSNE As tSNE)
        Me.tSNE = tSNE
    End Sub

    ' return cost and gradient, given an arrangement
    Private Sub CostGrad(Y As Double()())
        Dim N = mN
        Dim [dim] = tSNE.mDim ' dim of output space
        Dim P = tSNE.mP
        Dim pmul = If(tSNE.mIter < 100, 4, 1) ' trick that helps with local optima

        ' compute current Q distribution, unnormalized first
        Dim lQu = zeros(N * N)
        Dim qsum = 0.0

        For i = 0 To N - 1

            For j = i + 1 To N - 1
                Dim dsum = 0.0

                For d = 0 To [dim] - 1
                    Dim dhere = Y(i)(d) - Y(j)(d)
                    dsum += dhere * dhere
                Next

                Dim qu = 1.0 / (1.0 + dsum) ' Student t-distribution
                lQu(i * N + j) = qu
                lQu(j * N + i) = qu
                qsum += 2 * qu
            Next
        Next

        ' normalize Q distribution to sum to 1
        Dim NN = N * N
        Dim lQ = zeros(NN)

        For q = 0 To NN - 1
            lQ(q) = stdNum.Max(lQu(q) / qsum, 1.0E-100)
        Next

        Dim cost = 0.0
        Dim grad As List(Of Double()) = New List(Of Double())()

        For i = 0 To N - 1
            Dim gsum = New Double([dim] - 1) {} ' init grad for point i

            For d = 0 To [dim] - 1
                gsum(d) = 0.0
            Next

            For j = 0 To N - 1
                ' accumulate cost (the non-constant portion at least...)
                cost += -P(i * N + j) * stdNum.Log(lQ(i * N + j))
                Dim premult = 4 * (pmul * P(i * N + j) - lQ(i * N + j)) * lQu(i * N + j)

                For d = 0 To [dim] - 1
                    gsum(d) += premult * (Y(i)(d) - Y(j)(d))
                Next
            Next

            grad.Add(gsum)
        Next

        tSNE.mCost = cost
        tSNE.mGrad = New Double(grad.Count - 1)() {}

        For i = 0 To grad.Count - 1
            tSNE.mGrad(i) = grad(i)
        Next
    End Sub

End Class
