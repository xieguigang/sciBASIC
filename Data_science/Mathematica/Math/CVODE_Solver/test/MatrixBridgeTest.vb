Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix

''' <summary>
''' <see cref="NumericMatrix"/> 就地赋值与矩形数组互转能力的自检用例。
''' 
''' 这些能力是在删除求解器自带的私有稠密矩阵之后，为了让调用点可以 1:1 平移而
''' 补充到核心库之中的，因此这里对它们做一次覆盖测试。
''' 
''' ```shell
''' dotnet test.dll --matrix
''' ```
''' </summary>
''' <remarks>
''' 断言与报告统一通过 <see cref="TestAudit"/> 记录（API 与核心库的 DebugAudit 模块一致），
''' 并在指定路径生成 JSON 审计报告。
''' </remarks>
Public Module MatrixBridgeTest

    Private Const EPS As Double = 0.0000000001

    ''' <summary>
    ''' 运行全部矩阵自检用例，返回值即进程退出码（0 表示全部通过）。
    ''' </summary>
    ''' <param name="reportPath">审计报告的 JSON 输出路径，为空的时候不生成报告文件。</param>
    ''' <returns></returns>
    Public Function Run(Optional reportPath As String = Nothing) As Integer
        Call TestAudit.Reset()

        Console.WriteLine()
        Console.WriteLine("=== NumericMatrix in-place assignment / rectangular interop self test ===")

        Call CheckToRectangular()
        Call CheckRectangularRoundTrip()
        Call CheckCopyFromMatrix()
        Call CheckCopyFromDimensionMismatch()
        Call CheckSetConstant()
        Call CheckSetDiagonal()
        Call CheckIsSquare()

        Console.WriteLine()
        Console.WriteLine($"failure(s): {TestAudit.FailureCount}")

        If Not String.IsNullOrEmpty(reportPath) Then
            Call TestAudit.SaveReport(reportPath)
            Console.WriteLine($"audit report: {reportPath}")
        End If

        If TestAudit.FailureCount = 0 Then
            Console.WriteLine("PASS: 0 failure(s)")
            Return 0
        Else
            Console.WriteLine(TestAudit.Dump())
            Return 1
        End If
    End Function

    ''' <summary>
    ''' 构建一个 ``i * 10 + j`` 填充的测试矩阵。
    ''' </summary>
    Private Function Sample(rows As Integer, cols As Integer) As NumericMatrix
        Dim m As New NumericMatrix(rows, cols)

        For i As Integer = 0 To rows - 1
            For j As Integer = 0 To cols - 1
                m(i, j) = i * 10 + j
            Next
        Next

        Return m
    End Function

    ''' <summary>
    ''' <see cref="NumericMatrix.ToRectangular"/>：形状与数值一致，且与内部行数组不共享引用。
    ''' </summary>
    Private Sub CheckToRectangular()
        Dim m As NumericMatrix = Sample(2, 3)
        Dim rect As Double(,) = m.ToRectangular()

        Call TestAudit.AreEqual(2, rect.GetLength(0), "ToRectangular 的行数应与矩阵行数一致")
        Call TestAudit.AreEqual(3, rect.GetLength(1), "ToRectangular 的列数应与矩阵列数一致")
        Call TestAudit.AreEqual(12.0, rect(1, 2), "ToRectangular 的元素值应与矩阵一致")

        ' 导出的是副本：改写矩形数组不应反过来影响矩阵内部的行数组
        rect(0, 0) = -1

        Call TestAudit.AreEqual(0.0, m(0, 0), "ToRectangular 的返回值应与矩阵内部行数组不共享引用")
    End Sub

    ''' <summary>
    ''' ``ToRectangular(buffer)`` + ``CopyFrom(buffer)`` 的往返：数据写回生效，且内部行数组引用保持不变。
    ''' </summary>
    Private Sub CheckRectangularRoundTrip()
        Dim m As NumericMatrix = Sample(3, 3)
        Dim buffer As Double(,) = New Double(2, 2) {}
        Dim rowsBefore As Double()() = m.ArrayPack(deepcopy:=False).ToArray
        Dim changed As Boolean = False

        Call m.ToRectangular(buffer)

        For i As Integer = 0 To 2
            For j As Integer = 0 To 2
                buffer(i, j) = i + j
            Next
        Next

        Call m.CopyFrom(buffer)

        For i As Integer = 0 To 2
            For j As Integer = 0 To 2
                If System.Math.Abs(m(i, j) - (i + j)) > EPS Then
                    changed = True
                End If
            Next
        Next

        Call TestAudit.Check(Not changed, "CopyFrom(Double(,)) 之后矩阵的元素值应为矩形数组的内容")

        Dim rowsAfter As Double()() = m.ArrayPack(deepcopy:=False)

        For i As Integer = 0 To 2
            Call TestAudit.Check(rowsBefore(i) Is rowsAfter(i),
                                  $"CopyFrom(Double(,)) 不应该替换第 {i} 行的内部数组引用")
        Next
    End Sub

    ''' <summary>
    ''' <see cref="NumericMatrix.CopyFrom(GeneralMatrix)"/>：另一个矩阵的数据整块复制。
    ''' </summary>
    Private Sub CheckCopyFromMatrix()
        Dim source As New NumericMatrix(New Double()() {
            New Double() {1.0, 2.0},
            New Double() {3.0, 4.0}
        })
        Dim target As New NumericMatrix(2, 2)
        Dim rowsBefore As Double()() = target.ArrayPack(deepcopy:=False).ToArray

        Call target.CopyFrom(source)

        Call TestAudit.AreEqual(1.0, target(0, 0), "CopyFrom(GeneralMatrix) 应复制左上角元素")
        Call TestAudit.AreEqual(4.0, target(1, 1), "CopyFrom(GeneralMatrix) 应复制右下角元素")

        Dim rowsAfter As Double()() = target.ArrayPack(deepcopy:=False)

        For i As Integer = 0 To 1
            Call TestAudit.Check(rowsBefore(i) Is rowsAfter(i),
                                  $"CopyFrom(GeneralMatrix) 不应该替换第 {i} 行的内部数组引用")
        Next

        ' 源矩阵依然可以独立使用
        Call TestAudit.AreEqual(3.0, source(1, 0), "CopyFrom 不应修改源矩阵")
    End Sub

    ''' <summary>
    ''' 维度不一致的时候必须抛出 <see cref="ArgumentException"/>，而不是静默越界或截断。
    ''' </summary>
    Private Sub CheckCopyFromDimensionMismatch()
        Call TestAudit.CheckThrows(Of ArgumentException)(
            Sub() Call (New NumericMatrix(2, 2)).CopyFrom(New Double(1, 2) {}),
            "维度不一致的矩形数组拷贝应该抛出 ArgumentException")

        Call TestAudit.CheckThrows(Of ArgumentException)(
            Sub() Call (New NumericMatrix(2, 2)).CopyFrom(New NumericMatrix(3, 3)),
            "维度不一致的矩阵拷贝应该抛出 ArgumentException")
    End Sub

    ''' <summary>
    ''' <see cref="NumericMatrix.SetConstant"/>：全部元素就地置为同一个常量。
    ''' </summary>
    Private Sub CheckSetConstant()
        Dim m As NumericMatrix = Sample(2, 4)

        Call m.SetConstant(-2.5)

        Dim all As Boolean = True

        For i As Integer = 0 To m.RowDimension - 1
            For j As Integer = 0 To m.ColumnDimension - 1
                If System.Math.Abs(m(i, j) - (-2.5)) > EPS Then
                    all = False
                End If
            Next
        Next

        Call TestAudit.Check(all, "SetConstant 之后所有元素都应该是 -2.5")
    End Sub

    ''' <summary>
    ''' <see cref="NumericMatrix.SetDiagonal"/>：仅主对角线（非方阵取 ``min(m, n)`` 个）被赋值。
    ''' </summary>
    Private Sub CheckSetDiagonal()
        Dim square As NumericMatrix = Sample(3, 3)

        Call square.SetDiagonal(9.0)

        Call TestAudit.AreEqual(9.0, square(0, 0), "方阵的第 0 个对角元素应为 9")
        Call TestAudit.AreEqual(9.0, square(2, 2), "方阵的最后一个对角元素应为 9")
        Call TestAudit.AreEqual(1.0, square(0, 1), "非对角元素不应被 SetDiagonal 修改")

        Dim wide As NumericMatrix = Sample(2, 4)

        Call wide.SetDiagonal(5.0)

        Call TestAudit.AreEqual(5.0, wide(0, 0), "非方阵的第 0 个对角元素应为 5")
        Call TestAudit.AreEqual(5.0, wide(1, 1), "非方阵的第 1 个对角元素应为 5")
        Call TestAudit.AreEqual(12.0, wide(1, 2), "非对角元素不应被 SetDiagonal 修改")
    End Sub

    ''' <summary>
    ''' <see cref="NumericMatrix.IsSquare"/>：行列相等的判定。
    ''' </summary>
    Private Sub CheckIsSquare()
        Call TestAudit.Check(New NumericMatrix(3, 3).IsSquare, "3x3 的矩阵应被判定为方阵")
        Call TestAudit.Check(Not New NumericMatrix(2, 3).IsSquare, "2x3 的矩阵不应被判定为方阵")
        Call TestAudit.Check(Not New NumericMatrix(3, 2).IsSquare, "3x2 的矩阵不应被判定为方阵")
    End Sub
End Module
