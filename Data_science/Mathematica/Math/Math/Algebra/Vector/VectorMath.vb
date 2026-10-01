#Region "Microsoft.VisualBasic::VectorMath.vb, Data_science\Mathematica\Math\Math\Algebra\Vector\VectorMath.vb"

' Copyright (c) 2018 GPL3 Licensed


' /********************************************************************************/

' Code Statistics:

'   Module VectorMath

'   Function: Cross, OuterProduct, Project, GramSchmidt, Orthonormalize

' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports SimdEngine = Microsoft.VisualBasic.Math.SIMD.SimdEngine
Imports std = System.Math

Namespace LinearAlgebra

    ''' <summary>
    ''' 向量数学计算辅助模块
    ''' 
    ''' 提供叉积、外积、投影以及Gram-Schmidt正交化等向量运算辅助函数
    ''' </summary>
    ''' <remarks>
    ''' 所有的运算函数均生成新的结果对象，不会修改参与运算的原始向量对象；
    ''' 点积运算请直接使用<see cref="Vector.DotProduct"/>实例方法或者``Or``运算符
    ''' </remarks>
    <HideModuleName>
    Public Module VectorMath

        ''' <summary>
        ''' 三维向量的叉积运算：``a x b``
        ''' </summary>
        ''' <param name="a">左侧三维向量</param>
        ''' <param name="b">右侧三维向量</param>
        ''' <returns>
        ''' 一个新的三维向量对象，其方向同时垂直于<paramref name="a"/>和<paramref name="b"/>，
        ''' 并且符合右手定则
        ''' </returns>
        Public Function Cross(a As Vector, b As Vector) As Vector
            If a Is Nothing OrElse b Is Nothing Then
                Throw New ArgumentNullException("叉积运算的向量参数不允许为空值")
            ElseIf a.Length <> 3 OrElse b.Length <> 3 Then
                Throw New ArgumentException("叉积运算只支持三维向量")
            End If

            Return New Vector({
                a(1) * b(2) - a(2) * b(1),
                a(2) * b(0) - a(0) * b(2),
                a(0) * b(1) - a(1) * b(0)
            })
        End Function

        ''' <summary>
        ''' 向量的外积运算：``a b^T``，生成一个 ``m x n`` 的矩阵，
        ''' 其中 ``m = |a|``，``n = |b|``，``M(i, j) = a(i) * b(j)``
        ''' </summary>
        ''' <param name="a">列向量</param>
        ''' <param name="b">行向量</param>
        ''' <returns>一个新的一般矩阵对象</returns>
        Public Function OuterProduct(a As Vector, b As Vector) As NumericMatrix
            If a Is Nothing OrElse b Is Nothing Then
                Throw New ArgumentNullException("外积运算的向量参数不允许为空值")
            End If

            Dim m As Integer = a.Length
            Dim n As Integer = b.Length
            Dim buffer As Double()() = New Double(m - 1)() {}
            Dim bRow As Double() = b.Array

            ' SIMD 化：每一行都是标量 a(i) 与连续行向量 b 的数乘，走向量化内核
            For i As Integer = 0 To m - 1
                buffer(i) = SimdEngine.MultiplyScalar(Of Double)(a.Array(i), bRow)
            Next

            Return New NumericMatrix(buffer)
        End Function

        ''' <summary>
        ''' 计算向量<paramref name="a"/>在向量<paramref name="b"/>方向上的投影向量：
        ''' ``proj_b(a) = (a . b / b . b) * b``
        ''' </summary>
        ''' <param name="a">被投影的向量</param>
        ''' <param name="b">投影的目标方向向量</param>
        ''' <returns>
        ''' <paramref name="a"/>在<paramref name="b"/>方向上的投影，一个新的向量对象
        ''' </returns>
        Public Function Project(a As Vector, b As Vector) As Vector
            If a Is Nothing OrElse b Is Nothing Then
                Throw New ArgumentNullException("投影运算的向量参数不允许为空值")
            ElseIf a.Length <> b.Length Then
                Throw New ArgumentException("投影运算的两个向量长度必须相同")
            End If

            Dim bDot As Double = b.DotProduct(b)

            If std.Abs(bDot) < Double.Epsilon Then
                Throw New DivideByZeroException("投影方向向量不能为零向量")
            End If

            Return b * (a.DotProduct(b) / bDot)
        End Function

        ''' <summary>
        ''' Gram-Schmidt正交化：将一组线性无关的向量转化为等张成空间的正交基
        ''' </summary>
        ''' <param name="vectors">一组线性无关的向量</param>
        ''' <returns>
        ''' 与输入向量组张成相同空间的正交基（向量之间两两正交，但模长不一定为1）；
        ''' 当输入的向量组线性相关的时候，相关向量的正交化结果为零向量
        ''' </returns>
        Public Function GramSchmidt(vectors As IEnumerable(Of Vector)) As List(Of Vector)
            If vectors Is Nothing Then
                Throw New ArgumentNullException(NameOf(vectors))
            End If

            Dim basis As New List(Of Vector)

            For Each v As Vector In vectors
                Dim orthogonal As Vector = New Vector(v.ToArray)

                For Each u As Vector In basis
                    orthogonal -= Project(v, u)
                Next

                basis.Add(orthogonal)
            Next

            Return basis
        End Function

        ''' <summary>
        ''' Gram-Schmidt正交化并单位化：将一组线性无关的向量转化为等张成空间的标准正交基
        ''' </summary>
        ''' <param name="vectors">一组线性无关的向量</param>
        ''' <returns>
        ''' 与输入向量组张成相同空间的标准正交基（向量之间两两正交并且模长为1）
        ''' </returns>
        Public Function Orthonormalize(vectors As IEnumerable(Of Vector)) As List(Of Vector)
            Dim basis As List(Of Vector) = GramSchmidt(vectors)

            For i As Integer = 0 To basis.Count - 1
                Dim norm As Double = basis(i).L2Norm()

                If norm < Double.Epsilon Then
                    Throw New ArgumentException($"第{i}个向量与之前的向量线性相关，无法构建标准正交基")
                End If

                basis(i) = basis(i) / norm
            Next

            Return basis
        End Function
    End Module
End Namespace
