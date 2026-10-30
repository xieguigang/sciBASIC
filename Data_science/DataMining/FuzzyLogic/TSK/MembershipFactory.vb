Option Strict On
Option Explicit On

Imports System

Namespace TSK

    ''' <summary>
    ''' 隶属函数的构造工厂：按类型 + 参数创建，或按"中心 + 间距"做网格初始化。
    ''' </summary>
    Public NotInheritable Class MembershipFactory

        Private Sub New()
        End Sub

        ''' <summary>按类型与参数向量创建隶属函数。</summary>
        Public Shared Function Create(kind As MembershipKind, values As Double()) As IMembershipFunction
            If values Is Nothing Then
                Throw New ArgumentNullException(NameOf(values))
            End If

            Select Case kind
                Case MembershipKind.Gaussian
                    Call Require(values, 2, kind)
                    Return New GaussianMembership(values(0), values(1))
                Case MembershipKind.GeneralizedBell
                    Call Require(values, 3, kind)
                    Return New GeneralizedBellMembership(values(0), values(1), values(2))
                Case MembershipKind.Sigmoid
                    Call Require(values, 2, kind)
                    Return New SigmoidMembership(values(0), values(1))
                Case MembershipKind.Triangular
                    Call Require(values, 3, kind)
                    Return New TriangularMembership(values(0), values(1), values(2))
                Case MembershipKind.Trapezoidal
                    Call Require(values, 4, kind)
                    Return New TrapezoidalMembership(values(0), values(1), values(2), values(3))
                Case Else
                    Throw New ArgumentOutOfRangeException(NameOf(kind), kind, "unsupported membership function kind")
            End Select
        End Function

        ''' <summary>
        ''' 按网格划分的约定参数创建隶属函数：中心在 <paramref name="center"/>，特征宽度为 <paramref name="spacing"/>。
        ''' </summary>
        ''' <remarks>
        ''' 高斯取 <c>sigma = spacing / 2</c>，这等价于 Jang 原文的
        ''' <c>sigma = range/(m-1)/sqrt(8*h)</c> 在重叠度 <c>h = 0.5</c> 时的取值，
        ''' 是 TSK 网络的经典初始化约定（相邻隶属函数在 0.5 处相交）。
        ''' </remarks>
        Public Shared Function CreateGrid(kind As MembershipKind, center As Double, spacing As Double) As IMembershipFunction
            Dim s As Double = If(spacing > 0.0, spacing, 1.0)

            Select Case kind
                Case MembershipKind.Gaussian
                    Return New GaussianMembership(center, s / 2.0)
                Case MembershipKind.GeneralizedBell
                    Return New GeneralizedBellMembership(s / 2.0, 2.0, center)
                Case MembershipKind.Sigmoid
                    Return New SigmoidMembership(4.0 / s, center)
                Case MembershipKind.Triangular
                    Return New TriangularMembership(center - s, center, center + s)
                Case MembershipKind.Trapezoidal
                    Return New TrapezoidalMembership(center - s, center - s / 2.0, center + s / 2.0, center + s)
                Case Else
                    Throw New ArgumentOutOfRangeException(NameOf(kind), kind, "unsupported membership function kind")
            End Select
        End Function

        ''' <summary>参数名称（不创建实例的静态查询，用于诊断与序列化）。</summary>
        Public Shared Function ParameterNames(kind As MembershipKind) As String()
            Select Case kind
                Case MembershipKind.Gaussian
                    Return New String() {"center", "width"}
                Case MembershipKind.GeneralizedBell
                    Return New String() {"a", "b", "c"}
                Case MembershipKind.Sigmoid
                    Return New String() {"slope", "center"}
                Case MembershipKind.Triangular
                    Return New String() {"a", "b", "c"}
                Case MembershipKind.Trapezoidal
                    Return New String() {"a", "b", "c", "d"}
                Case Else
                    Throw New ArgumentOutOfRangeException(NameOf(kind), kind, "unsupported membership function kind")
            End Select
        End Function

        Private Shared Sub Require(values As Double(), expected As Integer, kind As MembershipKind)
            If values.Length <> expected Then
                Throw New ArgumentException($"{kind} expects {expected} parameters but got {values.Length}", NameOf(values))
            End If
        End Sub

    End Class

End Namespace
