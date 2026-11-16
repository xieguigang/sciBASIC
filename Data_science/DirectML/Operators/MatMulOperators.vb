' ------------------------------------------------------------------------
' 矩阵乘法
'
' DirectML 的 GEMM 描述符本身支持批量维度与广播：
'   A: [batch..., M, K]，B: [batch..., K, N] -> C: [batch..., M, N]
' ONNX 的 MatMul 在 1 维/广播语义上更宽松，桥接层负责把形状整理成 DML 能接受的
' 4 维形式（[N, C, H, W]），必要时插入 Reshape。
' ------------------------------------------------------------------------

Imports Microsoft.VisualBasic.Computing.DirectML.Interop
Imports Microsoft.VisualBasic.Computing.DirectML.Runtime

Namespace Operators

    ''' <summary>GEMM / MatMul 算子工厂。</summary>
    Public Module MatMulOperators

        ''' <summary>
        ''' 通用矩阵乘：Out = alpha * op(A) * op(B) + beta * C。
        ''' </summary>
        ''' <param name="a">左矩阵。</param>
        ''' <param name="b">右矩阵。</param>
        ''' <param name="c">可选的加项矩阵。</param>
        ''' <param name="output">输出。</param>
        ''' <param name="transA">A 是否转置。</param>
        ''' <param name="transB">B 是否转置。</param>
        ''' <param name="alpha">alpha。</param>
        ''' <param name="beta">beta（当 c 为 Nothing 时忽略）。</param>
        ''' <param name="name">调试名称。</param>
        Public Function Gemm(a As DmlTensor,
                             b As DmlTensor,
                             c As DmlTensor,
                             output As DmlTensor,
                             Optional transA As Boolean = False,
                             Optional transB As Boolean = False,
                             Optional alpha As Single = 1.0F,
                             Optional beta As Single = 1.0F,
                             Optional name As String = Nothing) As DmlOperatorDefinition

            If a Is Nothing OrElse b Is Nothing OrElse output Is Nothing Then
                Throw New ArgumentNullException(NameOf(a))
            End If

            Dim inputs As DmlTensor()

            If c Is Nothing Then
                inputs = {a, b}
            Else
                inputs = {a, b, c}
            End If

            Return Build(DML_OPERATOR_TYPE.GEMM, inputs, {output}, name,
                         Function(builder)
                             Dim cPtr As IntPtr = IntPtr.Zero

                             If c IsNot Nothing Then
                                 cPtr = Desc(c, builder)
                             End If

                             Return builder.StructPtr(New DML_GEMM_OPERATOR_DESC With {
                                 .ATensor = Desc(a, builder),
                                 .BTensor = Desc(b, builder),
                                 .CTensor = cPtr,
                                 .OutputTensor = Desc(output, builder),
                                 .TransA = If(transA, DML_MATRIX_TRANSFORM.TRANSPOSE, DML_MATRIX_TRANSFORM.NONE),
                                 .TransB = If(transB, DML_MATRIX_TRANSFORM.TRANSPOSE, DML_MATRIX_TRANSFORM.NONE),
                                 .Alpha = alpha,
                                 .Beta = If(c Is Nothing, 0.0F, beta),
                                 .FusedActivation = IntPtr.Zero
                             })
                         End Function)
        End Function

        ''' <summary>
        ''' 把 ONNX MatMul 的形状规范化成 DML GEMM 需要的 4 维形式。
        ''' </summary>
        ''' <param name="aDims">A 的形状。</param>
        ''' <param name="bDims">B 的形状。</param>
        ''' <returns>(aFormatted, bFormatted, outputDims) 三个 4 维形状。</returns>
        Public Function ResolveMatMulShapes(aDims As Integer(), bDims As Integer()) As Tuple(Of UInteger(), UInteger(), UInteger())
            If aDims Is Nothing OrElse aDims.Length = 0 Then
                Throw New ArgumentException("MatMul 的 A 至少需要 1 维", NameOf(aDims))
            End If

            If bDims Is Nothing OrElse bDims.Length = 0 Then
                Throw New ArgumentException("MatMul 的 B 至少需要 1 维", NameOf(bDims))
            End If

            Dim a() As Integer = aDims
            Dim b() As Integer = bDims

            ' 1 维输入按 ONNX 规则前置/后置一个 1
            Dim aVector As Boolean = a.Length = 1
            Dim bVector As Boolean = b.Length = 1

            If aVector Then
                a = New Integer() {1, a(0)}
            End If

            If bVector Then
                b = New Integer() {b(0), 1}
            End If

            Dim m As Integer = a(a.Length - 2)
            Dim k As Integer = a(a.Length - 1)
            Dim k2 As Integer = b(b.Length - 2)
            Dim n As Integer = b(b.Length - 1)

            If k <> k2 Then
                Throw New ArgumentException($"MatMul 的内维不一致：A 的最后一维是 {k}，B 的倒数第二维是 {k2}")
            End If

            ' 批量维度按 numpy 规则右对齐广播
            Dim aBatch As Integer() = TakeBatch(a)
            Dim bBatch As Integer() = TakeBatch(b)
            Dim batch As Integer() = BroadcastBatch(aBatch, bBatch)

            Dim outShape As New List(Of Integer)

            For Each d As Integer In batch
                Call outShape.Add(d)
            Next

            Call outShape.Add(m)
            Call outShape.Add(n)

            If aVector Then
                Call outShape.RemoveAt(outShape.Count - 2)
            End If

            If bVector Then
                Call outShape.RemoveAt(outShape.Count - 1)
            End If

            Dim outDims As UInteger() = DmlShapeInference.ToUnsigned(outShape.ToArray())

            Return New Tuple(Of UInteger(), UInteger(), UInteger())(
                ToGemm4D(batch, m, k),
                ToGemm4D(batch, k, n),
                ToGemm4D(batch, m, n))
        End Function

        Private Function TakeBatch(dims As Integer()) As Integer()
            If dims.Length <= 2 Then
                Return New Integer() {}
            End If

            Dim result As Integer() = New Integer(dims.Length - 3) {}

            For i As Integer = 0 To result.Length - 1
                result(i) = dims(i)
            Next

            Return result
        End Function

        Private Function BroadcastBatch(a As Integer(), b As Integer()) As Integer()
            Dim rank As Integer = System.Math.Max(a.Length, b.Length)
            Dim result As Integer() = New Integer(rank - 1) {}

            For i As Integer = 0 To rank - 1
                Dim ai As Integer = If(i >= rank - a.Length, a(i - (rank - a.Length)), 1)
                Dim bi As Integer = If(i >= rank - b.Length, b(i - (rank - b.Length)), 1)

                If ai <> bi AndAlso ai <> 1 AndAlso bi <> 1 Then
                    Throw New ArgumentException($"MatMul 的批量维度无法广播：{ai} vs {bi}")
                End If

                result(i) = System.Math.Max(ai, bi)
            Next

            Return result
        End Function

        ''' <summary>把 (batch..., rows, cols) 压成 DML 的 4 维 [batch, 1, rows, cols]。</summary>
        Private Function ToGemm4D(batch As Integer(), rows As Integer, cols As Integer) As UInteger()
            Dim batchSize As Integer = 1

            For Each d As Integer In batch
                batchSize *= d
            Next

            Return New UInteger() {CUInt(batchSize), 1UI, CUInt(rows), CUInt(cols)}
        End Function

    End Module

End Namespace
