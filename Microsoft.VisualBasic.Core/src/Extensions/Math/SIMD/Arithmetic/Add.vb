Imports System.Numerics

#If Not NET48 Then
Imports System.Runtime.Intrinsics
Imports System.Runtime.Intrinsics.X86
#End If

Namespace Math.SIMD

    Public Class Add

        Public Shared Function f64_op_add_f64_scalar(v1 As Double(), v2 As Double) As Double()
            Select Case SIMDEnvironment.config
                Case SIMDConfiguration.disable
none:               Dim out As Double() = New Double(v1.Length - 1) {}

                    For i As Integer = 0 To v1.Length - 1
                        out(i) = v1(i) + v2
                    Next

                    Return out
                Case SIMDConfiguration.enable
                    '#If NET48 Then
                    '                    GoTo legacy
                    '#Else
                    '                    If Avx2.IsSupported Then
                    '                        Return SIMDIntrinsics.Vector2(v1, v2, AddressOf Avx2.Add)
                    '                    ElseIf Avx.IsSupported Then
                    '                        Return SIMDIntrinsics.Vector2(v1, v2, AddressOf Avx.Add)
                    '                    Else
                    '                        GoTo legacy
                    '                    End If
                    '#End If
                    GoTo legacy
                Case SIMDConfiguration.legacy
legacy:
                    Dim array_v2 As Double() = New Double(SIMDEnvironment.countDouble - 1) {}

                    For i As Integer = 0 To array_v2.Length - 1
                        array_v2(i) = v2
                    Next

                    Dim x1 As Vector(Of Double)
                    Dim x2 As Vector(Of Double) = New Vector(Of Double)(array_v2, Scan0)
                    Dim vec As Double() = New Double(v1.Length - 1) {}
                    Dim remaining As Integer = v1.Length Mod SIMDEnvironment.countDouble
                    Dim ends As Integer = v1.Length - remaining - 1

                    For i As Integer = 0 To ends Step SIMDEnvironment.countDouble
                        x1 = New Vector(Of Double)(v1, i)
                        ' x2 = New Vector(Of Double)(v2, i)

                        Call (x1 + x2).CopyTo(vec, i)
                    Next

                    For i As Integer = v1.Length - remaining To v1.Length - 1
                        vec(i) = v1(i) + v2
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

        ''' <summary>
        ''' data unchecked add two f64 vector
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <param name="v2"></param>
        ''' <returns></returns>
        Public Shared Function f64_op_add_f64(v1 As Double(), v2 As Double()) As Double()
            Select Case SIMDEnvironment.config
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
                    Dim remaining As Integer = v1.Length Mod SIMDEnvironment.countDouble
                    Dim ends As Integer = v1.Length - remaining - 1

                    For i As Integer = 0 To ends Step SIMDEnvironment.countDouble
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