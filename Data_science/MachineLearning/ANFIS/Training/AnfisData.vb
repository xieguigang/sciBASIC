Option Strict On
Option Explicit On

Imports System
Imports std = System.Math

''' <summary>
''' ANFIS 训练/评估共用的数据整形工具。
''' </summary>
''' <remarks>
''' 输入统一用"行 = 样本"的锯齿数组（与 <see cref="TskNetwork.Predict"/> 一致），
''' 目标统一转成二维矩阵（分块时可以直接切行，不必反复构造锯齿数组）。
''' </remarks>
Friend NotInheritable Class AnfisData

    Private Sub New()
    End Sub

    ''' <summary>校验输入输出矩阵：非空、行列对齐、全部为有限值。</summary>
    Public Shared Sub Validate(x As Double()(), y As Double()())
        If x Is Nothing Then
            Throw New ArgumentNullException(NameOf(x))
        End If
        If y Is Nothing Then
            Throw New ArgumentNullException(NameOf(y))
        End If
        If x.Length = 0 Then
            Throw New ArgumentException("the training set is empty", NameOf(x))
        End If
        If x.Length <> y.Length Then
            Throw New ArgumentException($"the feature matrix has {x.Length} rows but the target has {y.Length}", NameOf(y))
        End If

        Dim n As Integer = x(0).Length
        Dim o As Integer = y(0).Length

        If n = 0 Then
            Throw New ArgumentException("the feature matrix has no column", NameOf(x))
        End If
        If o = 0 Then
            Throw New ArgumentException("the target has no column", NameOf(y))
        End If

        For i As Integer = 0 To x.Length - 1
            If x(i) Is Nothing OrElse x(i).Length <> n Then
                Throw New ArgumentException($"the row {i} of the feature matrix does not have {n} columns", NameOf(x))
            End If
            If y(i) Is Nothing OrElse y(i).Length <> o Then
                Throw New ArgumentException($"the row {i} of the target does not have {o} columns", NameOf(y))
            End If

            For j As Integer = 0 To n - 1
                If x(i)(j).IsNaNImaginary Then
                    Throw New ArgumentException($"the feature [{i}, {j}] is not a finite number", NameOf(x))
                End If
            Next

            For j As Integer = 0 To o - 1
                If y(i)(j).IsNaNImaginary Then
                    Throw New ArgumentException($"the target [{i}, {j}] is not a finite number", NameOf(y))
                End If
            Next
        Next
    End Sub

    ''' <summary>锯齿数组的目标转二维矩阵。</summary>
    Public Shared Function ToMatrix(y As Double()()) As Double(,)
        Dim rows As Integer = y.Length
        Dim cols As Integer = y(0).Length
        Dim matrix(rows - 1, cols - 1) As Double

        For i As Integer = 0 To rows - 1
            For j As Integer = 0 To cols - 1
                matrix(i, j) = y(i)(j)
            Next
        Next

        Return matrix
    End Function

    ''' <summary>按行区间切出二维矩阵的副本。</summary>
    Public Shared Function SliceRows(matrix As Double(,), start As Integer, count As Integer) As Double(,)
        Dim cols As Integer = matrix.GetLength(1)
        Dim result(count - 1, cols - 1) As Double

        For i As Integer = 0 To count - 1
            For j As Integer = 0 To cols - 1
                result(i, j) = matrix(start + i, j)
            Next
        Next

        Return result
    End Function

    ''' <summary>按行下标选集锯齿数组（行内容复制，避免与源数据共享）。</summary>
    Public Shared Function SelectRows(x As Double()(), indices As Integer()) As Double()()
        Dim result(indices.Length - 1)() As Double

        For i As Integer = 0 To indices.Length - 1
            Dim row As Double() = x(indices(i))
            result(i) = CType(row.Clone(), Double())
        Next

        Return result
    End Function

    ''' <summary>按行下标选集二维矩阵。</summary>
    Public Shared Function SelectRows(matrix As Double(,), indices As Integer()) As Double(,)
        Dim cols As Integer = matrix.GetLength(1)
        Dim result(indices.Length - 1, cols - 1) As Double

        For i As Integer = 0 To indices.Length - 1
            For j As Integer = 0 To cols - 1
                result(i, j) = matrix(indices(i), j)
            Next
        Next

        Return result
    End Function

    ''' <summary>用固定种子生成一个打乱的下标排列（Fisher-Yates）。</summary>
    Public Shared Function Shuffle(count As Integer, seed As Integer) As Integer()
        Dim order(count - 1) As Integer

        For i As Integer = 0 To count - 1
            order(i) = i
        Next

        Dim rng As New Random(seed)

        For i As Integer = count - 1 To 1 Step -1
            Dim j As Integer = rng.Next(i + 1)
            Dim swap As Integer = order(i)
            order(i) = order(j)
            order(j) = swap
        Next

        Return order
    End Function

    ''' <summary>
    ''' 按验证集比例划分训练行与验证行（确定性：同一 <paramref name="seed"/> 得到同一划分）。
    ''' </summary>
    Public Shared Sub Split(sampleCount As Integer,
                            validationRatio As Double,
                            seed As Integer,
                            ByRef trainRows As Integer(),
                            ByRef validationRows As Integer())

        If validationRatio <= 0.0 OrElse sampleCount < 5 Then
            trainRows = New Integer(sampleCount - 1) {}

            For i As Integer = 0 To sampleCount - 1
                trainRows(i) = i
            Next

            validationRows = New Integer() {}
            Return
        End If

        Dim count As Integer = CInt(std.Floor(sampleCount * validationRatio))

        If count < 1 Then
            count = 1
        ElseIf count > sampleCount - 2 Then
            count = sampleCount - 2
        End If

        Dim order As Integer() = Shuffle(sampleCount, seed)
        validationRows = New Integer(count - 1) {}
        trainRows = New Integer(sampleCount - count - 1) {}

        For i As Integer = 0 To count - 1
            validationRows(i) = order(i)
        Next

        For i As Integer = count To sampleCount - 1
            trainRows(i - count) = order(i)
        Next
    End Sub

End Class
