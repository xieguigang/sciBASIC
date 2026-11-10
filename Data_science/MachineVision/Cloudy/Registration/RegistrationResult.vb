Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core

Namespace Registration

    ''' <summary>
    ''' 配准结果：变换矩阵 + 质量指标。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 配准算法“跑完了”和“配对正确”是两件事：ICP 收敛后仍可能停在
    ''' 一个局部极小（两块平面互相错位也会有很低的残差）。
    ''' 因此结果对象必须携带足够的质量指标，让上游流水线能够拒绝错误结果：
    ''' </para>
    ''' <list type="bullet">
    ''' <item><description><see cref="Fitness"/>：内点率（有效对应数 / 源点数），低于 0.3 基本可以判失败。</description></item>
    ''' <item><description><see cref="InlierRmse"/>：内点残差的均方根，是“亚毫米级”是否达标的直接依据。</description></item>
    ''' <item><description><see cref="Converged"/>：是否满足收敛判据（而不是耗尽迭代次数）。</description></item>
    ''' <item><description><see cref="Iterations"/>：迭代次数，异常偏大往往意味着几何退化。</description></item>
    ''' </list>
    ''' <para>
    ''' <see cref="Transform"/> 的语义统一为“把<b>源</b>点云变换到<b>目标</b>坐标系”：
    ''' <c>p_target ≈ T · p_source</c>。多站拼接与 CAD 比对都依赖这一点，
    ''' 方向搞反是配准代码最常见的错误。
    ''' </para>
    ''' </remarks>
    Public Class RegistrationResult

        ''' <summary>源 → 目标的刚体变换。</summary>
        Public Property Transform As RigidTransform

        ''' <summary>
        ''' 内点率（重叠度估计）：有效对应数 / 源点总数，取值 <c>[0, 1]</c>。
        ''' </summary>
        Public Property Fitness As Double

        ''' <summary>内点残差的均方根（与点云同单位）。</summary>
        Public Property InlierRmse As Double

        ''' <summary>迭代次数。</summary>
        Public Property Iterations As Integer

        ''' <summary>是否满足收敛判据。</summary>
        Public Property Converged As Boolean

        ''' <summary>使用的算法名称（诊断/报告用）。</summary>
        Public Property Method As String

        ''' <summary>
        ''' 对应点对（源下标、目标下标、距离）；只在大规模统计前的中间结果中填充。
        ''' </summary>
        Public Property Correspondences As (source As Integer, target As Integer, distance As Double)()

        Public Sub New(transform As RigidTransform, fitness As Double, inlierRmse As Double,
                       iterations As Integer, converged As Boolean, method As String)
            Me.Transform = transform
            Me.Fitness = fitness
            Me.InlierRmse = inlierRmse
            Me.Iterations = iterations
            Me.Converged = converged
            Me.Method = method
        End Sub

        ''' <summary>
        ''' 变换源点云（返回新点云）。
        ''' </summary>
        Public Function ApplyTo(source As PointCloudBuffer) As PointCloudBuffer
            If source Is Nothing Then Throw New ArgumentNullException(NameOf(source))

            Return Transform.ApplyToCloud(source)
        End Function

        ''' <summary>
        ''' 是否可用（变换在容差内是刚体，否则说明数值上出了问题）。
        ''' </summary>
        Public Function IsValid(Optional tolerance As Double = 1.0E-4) As Boolean
            Return Transform IsNot Nothing AndAlso Transform.IsRigid(tolerance)
        End Function

        Public Overrides Function ToString() As String
            Return $"{Method}: fitness={Fitness:P1}, rmse={InlierRmse:G4}, iterations={Iterations}, converged={Converged}"
        End Function

    End Class
End Namespace
