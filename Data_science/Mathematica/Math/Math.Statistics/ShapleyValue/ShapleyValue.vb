Namespace ShapleyValue

    Public Class ShapleyValue

        Private cfunction As CharacteristicFunction
        Private output As IList(Of Double)
        Private permutations As PermutationLinkList
        Private currentRange As Long
        Private factorialSize As Long

        Public Overridable ReadOnly Property LastReached As Boolean
            Get
                If currentRange < FactorialUtil.factorial(Size) Then
                    Return False
                Else
                    Return True
                End If
            End Get
        End Property

        Public Overridable ReadOnly Property Size As Integer

        Public Sub New(cfunction As CharacteristicFunction)
            Me.cfunction = cfunction
            Size = cfunction.NbPlayers
            currentRange = 0
            factorialSize = FactorialUtil.factorial(Size)
            output = New List(Of Double)(Size + 1)

            For i As Integer = 0 To Size
                Call output.Add(0.0)
            Next
        End Sub

        Public Overridable Sub calculate()
            calculate(0, False)
        End Sub

        Public Overridable Sub calculate(sampleSize As Long, randomValue As Boolean)
            If permutations Is Nothing Then
                permutations = New PermutationLinkList(Size)
            End If

            Dim count = 1
            If sampleSize <= 0 Then
                sampleSize = factorialSize
            End If

            While Not LastReached AndAlso count <= sampleSize
                Dim coalition As IList(Of Integer) = Nothing

                If Not randomValue Then
                    coalition = permutations.NextPermutation
                Else
                    coalition = RandomPermutations.getRandom(Size)
                End If

                currentRange += 1
                ' System.out.println("currentRange "+currentRange);
                count += 1
                Dim [set] As New HashSet(Of Integer)()
                Dim prevVal As Double = 0

                For Each element As Integer In coalition
                    [set].Add(element)
                    Dim val = cfunction.getValue([set]) - prevVal
                    output(element) = val + output(element)
                    prevVal += val
                Next
            End While
        End Sub

        Public Overridable ReadOnly Property Result As IList(Of Double)
            Get
                Return getResult(0)
            End Get
        End Property

        Public Overridable Function getResult(normalizedValue As Integer) As IList(Of Double)
            Dim res As New List(Of Double)() From {0.0}
            Dim total As Double = 0

            For i As Integer = 1 To Size
                total += output(i) / factorialSize
                res.Add(output(i) / factorialSize)
            Next

            If normalizedValue <> 0 Then
                Dim normalizedRes As New List(Of Double)() From {0.0}

                For i As Integer = 1 To Size
                    normalizedRes.Add(res(i) / total)
                Next

                Return normalizedRes
            End If

            Return res
        End Function
    End Class

End Namespace
