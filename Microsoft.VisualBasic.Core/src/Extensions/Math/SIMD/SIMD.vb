Imports System.Numerics

#If Not NET48 Then
Imports System.Runtime.Intrinsics
Imports System.Runtime.Intrinsics.X86
#End If

Imports Microsoft.VisualBasic.Math.SIMDIntrinsics

Namespace Math

    Public Enum SIMDConfiguration
        disable
        enable
        legacy
        auto
    End Enum

    ''' <summary>
    ''' SIMD(Single Instruction Multiple Data)即单指令流多数据流，
    ''' 是一种采用一个控制器来控制多个处理器，同时对一组数据（又称“数据
    ''' 向量”）中的每一个分别执行相同的操作从而实现空间上的并行性的技术。
    ''' 简单来说就是一个指令能够同时处理多个数据。
    ''' </summary>
    ''' <remarks>
    ''' 在这个模块中的代码都不会进行安全检查，默认都是符合计算条件的
    ''' </remarks>
    Public NotInheritable Class SIMD

        ''' <summary>
        ''' This option only works for .NET core runtime
        ''' </summary>
        ''' <returns></returns>
        Public Shared Property config As SIMDConfiguration = SIMDConfiguration.disable

        Friend Shared ReadOnly countDouble As Integer = Vector(Of Double).Count
        Friend Shared ReadOnly countFloat As Integer = Vector(Of Single).Count
        Friend Shared ReadOnly countInteger As Integer = Vector(Of Integer).Count
        Friend Shared ReadOnly countLong As Integer = Vector(Of Long).Count
        Friend Shared ReadOnly countShort As Integer = Vector(Of Short).Count

        Private Sub New()
        End Sub

        Public Shared Function Add(v1 As Double(), v2 As Double()) As Double()
            Select Case SIMD.config
                Case SIMDConfiguration.disable
none:               Dim out As Double() = New Double(v1.Length - 1) {}

                    For i As Integer = 0 To v1.Length - 1
                        out(i) = v1(i) + v2(i)
                    Next

                    Return out
                Case SIMDConfiguration.enable
#If NET48 Then
                    GoTo legacy
#Else
                    If Avx2.IsSupported Then
                        Return SIMDIntrinsics.Vector2(v1, v2, AddressOf Avx2.Add)
                    ElseIf Avx.IsSupported Then
                        Return SIMDIntrinsics.Vector2(v1, v2, AddressOf Avx.Add)
                    Else
                        GoTo legacy
                    End If
#End If
                Case SIMDConfiguration.legacy
legacy:             Dim x1 As Vector(Of Double)
                    Dim x2 As Vector(Of Double)
                    Dim vec As Double() = New Double(v1.Length - 1) {}
                    Dim remaining As Integer = v1.Length Mod SIMD.countDouble
                    Dim ends As Integer = v1.Length - remaining - 1

                    For i As Integer = 0 To ends Step SIMD.countDouble
                        x1 = New Vector(Of Double)(v1, i)
                        x2 = New Vector(Of Double)(v2, i)

                        Call (x1 + x2).CopyTo(vec, i)
                    Next

                    For i As Integer = v1.Length - remaining To v1.Length - 1
                        vec(i) = v1(i) + v2(i)
                    Next

                    Return vec
                Case Else
                    If v1.Length < 10000 Then
                        GoTo none
                    Else
                        GoTo legacy
                    End If
            End Select
        End Function
    End Class
End Namespace