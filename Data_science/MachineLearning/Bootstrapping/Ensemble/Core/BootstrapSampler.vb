Imports System

Namespace Ensemble

    ''' <summary>
    ''' Bagging 的有放回采样器。
    ''' </summary>
    ''' <remarks>
    ''' 每个基模型应使用独立的采样器实例（例如 <c>New BootstrapSampler(seed + i)</c>），
    ''' 这样调整模型数量不会改变已有模型的采样结果，结果完全可复现。
    ''' </remarks>
    Public NotInheritable Class BootstrapSampler

        Private ReadOnly rnd As Random

        Public Sub New(seed As Integer)
            Me.rnd = New Random(seed)
        End Sub

        ''' <summary>
        ''' 生成 <paramref name="n"/> 个取值范围为 <c>[0, n)</c> 的随机下标（有放回）。
        ''' </summary>
        Public Function Sample(n As Integer) As Integer()
            If n <= 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(n), $"the sample size must be positive, but got {n}!")
            End If

            Dim index As Integer() = New Integer(n - 1) {}

            For i As Integer = 0 To n - 1
                index(i) = rnd.Next(n)
            Next

            Return index
        End Function

        ''' <summary>按给定下标取出子集（用于构造 bootstrap 训练集）</summary>
        Public Shared Function Resample(rows As Double()(), index As Integer()) As Double()()
            If rows Is Nothing Then
                Throw New ArgumentNullException(NameOf(rows))
            End If
            If index Is Nothing Then
                Throw New ArgumentNullException(NameOf(index))
            End If

            Dim result As Double()() = New Double(index.Length - 1)() {}

            For i As Integer = 0 To index.Length - 1
                Dim slot As Integer = index(i)

                If slot < 0 OrElse slot >= rows.Length Then
                    Throw New ArgumentOutOfRangeException(
                        NameOf(index), $"the resample index {slot} is out of range [0, {rows.Length})!")
                End If

                result(i) = rows(slot)
            Next

            Return result
        End Function

        ''' <summary>按给定下标取出子集（值类型数组）</summary>
        Public Shared Function Resample(Of T)(values As T(), index As Integer()) As T()
            If values Is Nothing Then
                Throw New ArgumentNullException(NameOf(values))
            End If
            If index Is Nothing Then
                Throw New ArgumentNullException(NameOf(index))
            End If

            Dim result As T() = New T(index.Length - 1) {}

            For i As Integer = 0 To index.Length - 1
                Dim slot As Integer = index(i)

                If slot < 0 OrElse slot >= values.Length Then
                    Throw New ArgumentOutOfRangeException(
                        NameOf(index), $"the resample index {slot} is out of range [0, {values.Length})!")
                End If

                result(i) = values(slot)
            Next

            Return result
        End Function

    End Class
End Namespace
