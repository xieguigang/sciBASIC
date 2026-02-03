Imports System.Numerics
Imports System.Runtime.CompilerServices

Namespace Language.Java

    Public Module [Integer]

        <Extension>
        Public Function bitCount(i As Integer) As Integer
#If NET48 Then
            ' 将有符号整数转换为无符号整数以模拟 Java 的无符号移位操作 (>>>)
            Dim n As UInteger = CUInt(i)

            ' Java 源码算法: 并行位计数
            ' 第一步：每2位计算1的个数
            n = n - ((n >> 1) And &H55555555UI)

            ' 第二步：每4位计算1的个数
            n = (n And &H33333333UI) + ((n >> 2) And &H33333333UI)

            ' 第三步：每8位计算1的个数
            n = (n + (n >> 4)) And &HF0F0F0FUI

            ' 第四步：横向相加
            n = n + (n >> 8)
            n = n + (n >> 16)

            ' 最后取低6位即可（最大32位，所以结果不超过 32，二进制 100000）
            Return CInt(n And &H3FUI)
#Else
            ' BitOperations.PopCount 接收 Unsigned 类型，所以需要转换
            Return BitOperations.PopCount(CUInt(i))
#End If
        End Function
    End Module
End Namespace