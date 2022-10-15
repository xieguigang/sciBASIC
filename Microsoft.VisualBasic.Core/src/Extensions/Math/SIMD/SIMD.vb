Imports System.Numerics

#If Not NET48 Then
Imports System.Runtime.Intrinsics
Imports System.Runtime.Intrinsics.X86
#End If

Namespace Math.SIMD

    Public Enum SIMDConfiguration
        disable
        enable
        legacy

        ''' <summary>
        ''' disable or SIMD **legacy** mode based on the vector size
        ''' </summary>
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
    Public NotInheritable Class SIMDEnvironment

        ''' <summary>
        ''' This option only works for .NET core runtime
        ''' </summary>
        ''' <returns></returns>
        Public Shared Property config As SIMDConfiguration = SIMDConfiguration.auto

        ''' <summary>
        ''' Vector(Of <see cref="Double"/>).Count
        ''' </summary>
        Public Shared ReadOnly countDouble As Integer = Vector(Of Double).Count
        Public Shared ReadOnly countFloat As Integer = Vector(Of Single).Count
        ''' <summary>
        ''' Vector(Of <see cref="Integer"/>).Count
        ''' </summary>
        Public Shared ReadOnly countInteger As Integer = Vector(Of Integer).Count
        Public Shared ReadOnly countLong As Integer = Vector(Of Long).Count
        Public Shared ReadOnly countShort As Integer = Vector(Of Short).Count

        Private Sub New()
        End Sub
    End Class
End Namespace