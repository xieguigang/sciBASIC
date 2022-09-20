Imports System.Numerics

#If Not NET48 Then
Imports System.Runtime.Intrinsics
Imports System.Runtime.Intrinsics.X86
#End If

Namespace Math

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
        Public Shared Property enable As Boolean = True

        Shared ReadOnly countDouble As Integer = Vector(Of Double).Count
        Shared ReadOnly countFloat As Integer = Vector(Of Single).Count
        Shared ReadOnly countInteger As Integer = Vector(Of Integer).Count
        Shared ReadOnly countLong As Integer = Vector(Of Long).Count
        Shared ReadOnly countShort As Integer = Vector(Of Short).Count

        Private Sub New()
        End Sub

        Public Shared Function Add(v1 As Double(), v2 As Double()) As Double()
            Dim out As Double() = New Double(v1.Length - 1) {}
            Dim remaining As Integer = v1.Length Mod countDouble

            If enable Then
#If NET48 Then
                Dim x1 As Vector(Of Double)
                Dim x2 As Vector(Of Double)

                For i As Integer = 0 To v1.Length - remaining - 1
                    x1 = New Vector(Of Double)(v1, i)
                    x2 = New Vector(Of Double)(v2, i)

                    Call (x1 + x2).CopyTo(out, i)
                Next
#Else
                Dim i1, i2, i3 As Integer
                Dim a, b, c As Vector256(Of Double)

                For i As Integer = 0 To v1.Length - 1 Step 4
                    i1 = i + 1
                    i2 = i + 2
                    i3 = i + 3

                    a = Vector256.Create(v1(i), v1(i1), v1(i2), v1(i3))
                    b = Vector256.Create(v2(i), v2(i1), v2(i2), v2(i3))
                    c = Avx.Add(a, b)

                    out(i) = c.GetElement(0)
                    out(i1) = c.GetElement(1)
                    out(i2) = c.GetElement(2)
                    out(i3) = c.GetElement(3)
                Next
#End If
                For i As Integer = v1.Length - remaining To v1.Length - 1
                    out(i) = v1(i) + v2(i)
                Next
            Else
                For i As Integer = 0 To v1.Length - 1
                    out(i) = v1(i) + v2(i)
                Next
            End If

            Return out
        End Function
    End Class
End Namespace