Imports System.Math
Imports System.Runtime.CompilerServices
Imports std = System.Math

Namespace Math3D

    ''' <summary>
    ''' 3x3 实矩阵。3D 刚体用它表示惯性张量（局部系）与随姿态旋转后的世界系逆惯性张量
    ''' <c>R · I⁻¹ · Rᵀ</c>。
    ''' </summary>
    ''' <remarks>
    ''' 元素命名 <c>mij</c> 表示第 i 行第 j 列，与 <see cref="Quaternion.ToMatrix3x3"/> 的
    ''' 行向量约定（<c>v' = v · M</c>）保持一致。
    ''' </remarks>
    Public Structure Matrix3x3

        Public m11 As Double
        Public m12 As Double
        Public m13 As Double
        Public m21 As Double
        Public m22 As Double
        Public m23 As Double
        Public m31 As Double
        Public m32 As Double
        Public m33 As Double

        Sub New(m11 As Double, m12 As Double, m13 As Double,
                m21 As Double, m22 As Double, m23 As Double,
                m31 As Double, m32 As Double, m33 As Double)
            Me.m11 = m11 : Me.m12 = m12 : Me.m13 = m13
            Me.m21 = m21 : Me.m22 = m22 : Me.m23 = m23
            Me.m31 = m31 : Me.m32 = m32 : Me.m33 = m33
        End Sub

        ''' <summary>零矩阵。</summary>
        Public Shared ReadOnly Property Zero As Matrix3x3
            Get
                Return New Matrix3x3(0, 0, 0, 0, 0, 0, 0, 0, 0)
            End Get
        End Property

        ''' <summary>单位矩阵。</summary>
        Public Shared ReadOnly Property Identity As Matrix3x3
            Get
                Return New Matrix3x3(1, 0, 0, 0, 1, 0, 0, 0, 1)
            End Get
        End Property

        ''' <summary>由对角线元素构造对角矩阵。</summary>
        Public Shared Function Diagonal(x As Double, y As Double, z As Double) As Matrix3x3
            Return New Matrix3x3(x, 0, 0, 0, y, 0, 0, 0, z)
        End Function

        ''' <summary>转置。</summary>
        Public Function Transpose() As Matrix3x3
            Return New Matrix3x3(
                m11, m21, m31,
                m12, m22, m32,
                m13, m23, m33)
        End Function

        ''' <summary>行列式。</summary>
        Public Function Determinant() As Double
            Return m11 * (m22 * m33 - m23 * m32) -
                   m12 * (m21 * m33 - m23 * m31) +
                   m13 * (m21 * m32 - m22 * m31)
        End Function

        ''' <summary>
        ''' 求逆矩阵。奇异（行列式接近 0）时返回零矩阵，表示该物体不参与转动（等效无限惯量）。
        ''' </summary>
        Public Function Inverse() As Matrix3x3
            Dim det As Double = Determinant()

            If std.Abs(det) < 1.0E-12 Then
                Return Matrix3x3.Zero
            End If

            Dim inv As Double = 1.0 / det

            Return New Matrix3x3(
                (m22 * m33 - m23 * m32) * inv,
                (m13 * m32 - m12 * m33) * inv,
                (m12 * m23 - m13 * m22) * inv,
                (m23 * m31 - m21 * m33) * inv,
                (m11 * m33 - m13 * m31) * inv,
                (m13 * m21 - m11 * m23) * inv,
                (m21 * m32 - m22 * m31) * inv,
                (m12 * m31 - m11 * m32) * inv,
                (m11 * m22 - m12 * m21) * inv)
        End Function

        ''' <summary>行向量约定下的矩阵-向量乘：<c>v' = v · M</c>。</summary>
        Public Function Multiply(v As Vector3) As Vector3
            Return New Vector3(
                v.x * m11 + v.y * m21 + v.z * m31,
                v.x * m12 + v.y * m22 + v.z * m32,
                v.x * m13 + v.y * m23 + v.z * m33)
        End Function

        ''' <summary>列向量约定下的矩阵-向量乘：<c>v' = M · v</c>。</summary>
        Public Function MultiplyLeft(v As Vector3) As Vector3
            Return New Vector3(
                m11 * v.x + m12 * v.y + m13 * v.z,
                m21 * v.x + m22 * v.y + m23 * v.z,
                m31 * v.x + m32 * v.y + m33 * v.z)
        End Function

        ''' <summary>矩阵乘：<c>this · other</c>。</summary>
        Public Function Multiply(other As Matrix3x3) As Matrix3x3
            Dim r As New Matrix3x3()

            r.m11 = m11 * other.m11 + m12 * other.m21 + m13 * other.m31
            r.m12 = m11 * other.m12 + m12 * other.m22 + m13 * other.m32
            r.m13 = m11 * other.m13 + m12 * other.m23 + m13 * other.m33

            r.m21 = m21 * other.m11 + m22 * other.m21 + m23 * other.m31
            r.m22 = m21 * other.m12 + m22 * other.m22 + m23 * other.m32
            r.m23 = m21 * other.m13 + m22 * other.m23 + m23 * other.m33

            r.m31 = m31 * other.m11 + m32 * other.m21 + m33 * other.m31
            r.m32 = m31 * other.m12 + m32 * other.m22 + m33 * other.m32
            r.m33 = m31 * other.m13 + m32 * other.m23 + m33 * other.m33

            Return r
        End Function

        Public Shared Operator *(a As Matrix3x3, b As Matrix3x3) As Matrix3x3
            Return a.Multiply(b)
        End Operator

        Public Shared Operator *(m As Matrix3x3, s As Double) As Matrix3x3
            Return New Matrix3x3(
                m.m11 * s, m.m12 * s, m.m13 * s,
                m.m21 * s, m.m22 * s, m.m23 * s,
                m.m31 * s, m.m32 * s, m.m33 * s)
        End Operator

        Public Shared Operator +(a As Matrix3x3, b As Matrix3x3) As Matrix3x3
            Return New Matrix3x3(
                a.m11 + b.m11, a.m12 + b.m12, a.m13 + b.m13,
                a.m21 + b.m21, a.m22 + b.m22, a.m23 + b.m23,
                a.m31 + b.m31, a.m32 + b.m32, a.m33 + b.m33)
        End Operator

        ''' <summary>
        ''' 外积（并矢）<c>a ⊗ b</c>，用于把接触点的贡献叠加到有效质量矩阵上。
        ''' </summary>
        Public Shared Function Outer(a As Vector3, b As Vector3) As Matrix3x3
            Return New Matrix3x3(
                a.x * b.x, a.x * b.y, a.x * b.z,
                a.y * b.x, a.y * b.y, a.y * b.z,
                a.z * b.x, a.z * b.y, a.z * b.z)
        End Function

        ''' <summary>
        ''' 由向量构造反对称（叉积）矩阵 <c>[v]×</c>，满足 <c>Cross(v,w) = [v]× · w</c>。
        ''' </summary>
        Public Shared Function SkewSymmetric(v As Vector3) As Matrix3x3
            Return New Matrix3x3(
                0, -v.z, v.y,
                v.z, 0, -v.x,
                -v.y, v.x, 0)
        End Function

        Public Overrides Function ToString() As String
            Return $"[{m11:F3} {m12:F3} {m13:F3}; {m21:F3} {m22:F3} {m23:F3}; {m31:F3} {m32:F3} {m33:F3}]"
        End Function
    End Structure
End Namespace
