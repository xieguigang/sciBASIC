Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MachineLearning.CNN.Layer
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions
Imports std = System.Math

Namespace CNN

    Friend Class Util

        Public Delegate Function [Operator](value As Double) As Double

        Public Shared ReadOnly one_value As [Operator] = Function(value) 1 - value


        Public Shared ReadOnly digmod As [Operator] = Function(value) 1 / (1 + std.Pow(std.E, -value))


        Public Delegate Function OperatorOnTwo(a As Double, b As Double) As Double


        Public Shared ReadOnly plus As OperatorOnTwo = Function(a, b) a + b

        Public Shared multiply As OperatorOnTwo = Function(a, b) a * b

        Public Shared minus As OperatorOnTwo = Function(a, b) a - b

        Public Shared Sub printMatrix(matrix As Double()())
            For i = 0 To matrix.Length - 1
                Dim line = String.Join(", ", matrix(i))
                line = line.Replace(", ", vbTab)
                Console.WriteLine(line)
            Next
            Console.WriteLine()
        End Sub

        Public Shared Function rot180(matrix As Double()()) As Double()()
            matrix = cloneMatrix(matrix)
            Dim m = matrix.Length
            Dim n = matrix(0).Length

            For i = 0 To m - 1
                For j As Integer = 0 To n / 2 - 1
                    Dim tmp = matrix(i)(j)
                    matrix(i)(j) = matrix(i)(n - 1 - j)
                    matrix(i)(n - 1 - j) = tmp
                Next
            Next

            For j = 0 To n - 1
                For i As Integer = 0 To m / 2 - 1
                    Dim tmp = matrix(i)(j)
                    matrix(i)(j) = matrix(m - 1 - i)(j)
                    matrix(m - 1 - i)(j) = tmp
                Next
            Next
            Return matrix
        End Function

        Public Shared Function randomMatrix(x As Integer, y As Integer, b As Boolean) As Double()()
            Dim matrix = RectangularArray.Matrix(Of Double)(x, y)
            Dim tag = 1
            For i = 0 To x - 1
                For j = 0 To y - 1
                    matrix(i)(j) = (randf.NextDouble() - 0.05) / 10
                    '				matrix[i][j] = tag * 0.5;
                    '				if (b)
                    '					matrix[i][j] *= 1.0*(i + j + 2) / (i + 1) / (j + 1);
                    '				tag *= -1;
                Next
            Next
            ' printMatrix(matrix);
            Return matrix
        End Function

        Public Shared Function randomPerm(size As Integer, batchSize As Integer) As Integer()
            Dim [set] As ISet(Of Integer?) = New HashSet(Of Integer?)()
            While [set].Count < batchSize
                [set].Add(randf.Next(size))
            End While
            Dim randPerm = New Integer(batchSize - 1) {}
            Dim i As i32 = 0
            For Each value In [set]
                randPerm(++i) = value.Value
            Next
            Return randPerm
        End Function

        Public Shared Function cloneMatrix(matrix As Double()()) As Double()()
            Dim m = matrix.Length
            Dim n = matrix(0).Length
            Dim outMatrix = RectangularArray.Matrix(Of Double)(m, n)

            For i = 0 To m - 1
                For j = 0 To n - 1
                    outMatrix(i)(j) = matrix(i)(j)
                Next
            Next
            Return outMatrix
        End Function

        Public Shared Function matrixOp(ma As Double()(), [operator] As [Operator]) As Double()()
            Dim m = ma.Length
            Dim n = ma(0).Length
            For i = 0 To m - 1
                For j = 0 To n - 1
                    ma(i)(j) = [operator](ma(i)(j))
                Next
            Next
            Return ma

        End Function

        Public Shared Function matrixOp(ma As Double()(),
                                        mb As Double()(),
                                        operatorA As [Operator],
                                        operatorB As [Operator],
                                        [operator] As OperatorOnTwo) As Double()()
            Dim m = ma.Length
            Dim n = ma(0).Length
            If m <> mb.Length OrElse n <> mb(0).Length Then
                Throw New Exception("ma.length:" & ma.Length.ToString() & "  mb.length:" & mb.Length.ToString())
            End If

            For i = 0 To m - 1
                For j = 0 To n - 1
                    Dim a = ma(i)(j)
                    If operatorA IsNot Nothing Then
                        a = operatorA(a)
                    End If
                    Dim b = mb(i)(j)
                    If operatorB IsNot Nothing Then
                        b = operatorB(b)
                    End If
                    mb(i)(j) = [operator](a, b)
                Next
            Next
            Return mb
        End Function

        Public Shared Function kronecker(matrix As Double()(), scale As Size) As Double()()
            Dim m = matrix.Length
            Dim n = matrix(0).Length
            Dim outMatrix = RectangularArray.Matrix(Of Double)(m * scale.x, n * scale.y)

            For i = 0 To m - 1
                For j = 0 To n - 1
                    For ki = i * scale.x To (i + 1) * scale.x - 1
                        For kj = j * scale.y To (j + 1) * scale.y - 1
                            outMatrix(ki)(kj) = matrix(i)(j)
                        Next
                    Next
                Next
            Next
            Return outMatrix
        End Function

        Public Shared Function scaleMatrix(matrix As Double()(), scale As Size) As Double()()
            Dim m = matrix.Length
            Dim n = matrix(0).Length
            Dim sm As Integer = m / scale.x
            Dim sn As Integer = n / scale.y
            Dim outMatrix = RectangularArray.Matrix(Of Double)(sm, sn)

            If sm * scale.x <> m OrElse sn * scale.y <> n Then
                Call $"scale is mis-matched with matrix?".Warning
            End If

            Dim size = scale.x * scale.y

            For i = 0 To sm - 1
                For j = 0 To sn - 1
                    Dim sum = 0.0
                    For si = i * scale.x To (i + 1) * scale.x - 1
                        For sj = j * scale.y To (j + 1) * scale.y - 1
                            sum += matrix(si)(sj)
                        Next
                    Next
                    outMatrix(i)(j) = sum / size
                Next
            Next

            Return outMatrix
        End Function

        Public Shared Function convnFull(matrix As Double()(), kernel As Double()()) As Double()()
            Dim m = matrix.Length
            Dim n = matrix(0).Length
            Dim km = kernel.Length
            Dim kn = kernel(0).Length
            Dim extendMatrix = RectangularArray.Matrix(Of Double)(m + 2 * (km - 1), n + 2 * (kn - 1))
            For i = 0 To m - 1
                For j = 0 To n - 1
                    extendMatrix(i + km - 1)(j + kn - 1) = matrix(i)(j)
                Next
            Next
            Return convnValid(extendMatrix, kernel)
        End Function

        Public Shared Function convnValid(matrix As Double()(), kernel As Double()()) As Double()()
            'kernel = rot180(kernel);
            Dim m = matrix.Length
            Dim n = matrix(0).Length
            Dim km = kernel.Length
            Dim kn = kernel(0).Length
            Dim kns = n - kn + 1
            Dim kms = m - km + 1
            Dim outMatrix = RectangularArray.Matrix(Of Double)(kms, kns)

            For i = 0 To kms - 1
                For j = 0 To kns - 1
                    Dim sum = 0.0
                    For ki = 0 To km - 1
                        For kj = 0 To kn - 1
                            sum += matrix(i + ki)(j + kj) * kernel(ki)(kj)
                        Next
                    Next
                    outMatrix(i)(j) = sum
                Next
            Next

            Return outMatrix
        End Function

        Public Shared Function convnValid(matrix As Double()()()(), mapNoX As Integer, kernel As Double()()()(), mapNoY As Integer) As Double()()
            Dim m = matrix.Length
            Dim n = matrix(0)(mapNoX).Length
            Dim h = matrix(0)(mapNoX)(0).Length
            Dim km = kernel.Length
            Dim kn = kernel(0)(mapNoY).Length
            Dim kh = kernel(0)(mapNoY)(0).Length
            Dim kms = m - km + 1
            Dim kns = n - kn + 1
            Dim khs = h - kh + 1

            If matrix.Length <> kernel.Length Then
                Throw New Exception($"the size of matrix should equals to the kernel size!")
            End If

            Dim outMatrix = RectangularArray.Cubic(Of Double)(kms, kns, khs)

            For i = 0 To kms - 1
                For j = 0 To kns - 1
                    For k = 0 To khs - 1
                        Dim sum = 0.0
                        For ki = 0 To km - 1
                            For kj = 0 To kn - 1
                                For kk = 0 To kh - 1
                                    sum += matrix(i + ki)(mapNoX)(j + kj)(k + kk) * kernel(ki)(mapNoY)(kj)(kk)
                                Next
                            Next
                        Next
                        outMatrix(i)(j)(k) = sum
                    Next
                Next
            Next
            Return outMatrix(0)
        End Function

        Public Shared Function sigmod(x As Double) As Double
            Return 1 / (1 + std.Pow(std.E, -x))
        End Function

        Public Shared Function sum([error] As Double()()) As Double
            Dim m = [error].Length
            Dim n = [error](0).Length
            Dim lSum = 0.0
            For i = 0 To m - 1
                For j = 0 To n - 1
                    lSum += [error](i)(j)
                Next
            Next
            Return lSum
        End Function

        Public Shared Function sum(errors As Double()()()(), j As Integer) As Double()()
            Dim m = errors(0)(j).Length
            Dim n = errors(0)(j)(0).Length
            Dim result = RectangularArray.Matrix(Of Double)(m, n)
            For mi = 0 To m - 1
                For nj = 0 To n - 1
                    Dim lSum As Double = 0
                    For i = 0 To errors.Length - 1
                        lSum += errors(i)(j)(mi)(nj)
                    Next
                    result(mi)(nj) = lSum
                Next
            Next
            Return result
        End Function

        Public Shared Function getMaxIndex(out As Double()) As Integer
            Dim max = out(0)
            Dim index = 0
            For i = 1 To out.Length - 1
                If out(i) > max Then
                    max = out(i)
                    index = i
                End If
            Next
            Return index
        End Function
    End Class
End Namespace
