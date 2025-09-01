Namespace ShapleyValue

    Public Class ShapleyValue

        Private cfunction As CharacteristicFunction
        Private output As IList(Of Double)
        Private permutations As PermutationLinkList
        Private currentRange As Long
        Private sizeField As Integer
        Private factorialSize As Long

        Public Sub New(cfunction As CharacteristicFunction)
            Me.cfunction = cfunction
            sizeField = cfunction.NbPlayers
            currentRange = 0
            factorialSize = FactorialUtil.factorial(sizeField)

            output = New List(Of Double)(sizeField + 1)
            For i = 0 To sizeField
                output.Add(0.0)
            Next
        End Sub

        Public Overridable Sub calculate()
            calculate(0, False)
        End Sub

        Public Overridable Sub calculate(sampleSize As Long, randomValue As Boolean)
            If permutations Is Nothing Then
                permutations = New PermutationLinkList(sizeField)
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
                    coalition = RandomPermutations.getRandom(sizeField)
                End If

                currentRange += 1
                ' System.out.println("currentRange "+currentRange);
                count += 1
                Dim [set] As ISet(Of Integer) = New HashSet(Of Integer)()
                Dim prevVal As Double = 0
                For Each element In coalition
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

            Dim res As IList(Of Double) = New List(Of Double)()
            res.Add(0.0)
            Dim total As Double = 0
            For i = 1 To sizeField
                total += output(i) / factorialSize
                res.Add(output(i) / factorialSize)
            Next

            If normalizedValue <> 0 Then
                Dim normalizedRes As IList(Of Double) = New List(Of Double)()
                normalizedRes.Add(0.0)
                For i = 1 To sizeField
                    normalizedRes.Add(res(i) / total)
                Next
                Return normalizedRes
            End If
            Return res
        End Function



        Public Overridable ReadOnly Property LastReached As Boolean
            Get
                If currentRange < FactorialUtil.factorial(sizeField) Then
                    Return False
                Else
                    Return True
                End If
            End Get
        End Property

        Public Overridable ReadOnly Property Size As Integer
            Get
                Return sizeField
            End Get
        End Property

    End Class

End Namespace
