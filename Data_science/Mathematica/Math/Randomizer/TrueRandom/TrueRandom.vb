#Region "Microsoft.VisualBasic::TrueRandom, Data_science\Mathematica\Math\Randomizer\TrueRandom\TrueRandom.vb"

    ' Author:
    ' 
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.

    ' /********************************************************************************/

    ' Summaries:

    '   Class TrueRandom
    ' 
    '     Properties: IsThreadSafe
    ' 
    '     Constructor: Sub New
    ' 
    '     Function: (+3 Overloads) [Next], GenerateBytes, GenerateDoubles, GenerateInt32s,
    '               GenerateUInt32, NextDouble, NextFloat
    ' 
    '     Sub: NextBytes, NextFloats
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Security.Cryptography
Imports std = System.Math

''' <summary>
''' 混合真随机数生成器（快、大规模）。
''' 
''' 以操作系统加密级 CSPRNG（<see cref="RandomNumberGenerator"/>，由系统熵池驱动）
''' 做批量展开，并用 <see cref="EntropyMixer"/>（HMAC-SHA256 计数器模式）将其与环境
''' 噪声折叠，周期性刷新密钥，兼顾速度、均匀性与不可预测性。典型用途：蒙特卡洛模拟、
''' 随机抽样、需要不可预测性的统计试验。
''' 
''' 实现 <see cref="IProvideRandomValues"/> 接口，并额外提供批量方法
''' <see cref="GenerateBytes"/>、<see cref="GenerateInt32s"/>、<see cref="GenerateDoubles"/>。
''' 所有状态访问经 <c>SyncLock</c> 保护，<see cref="IsThreadSafe"/> 返回 ``True``。
''' </summary>
Public Class TrueRandom : Implements IProvideRandomValues

    ''' <summary>
    ''' 每展开多少字节后刷新一次密钥（折叠新的 CSPRNG 块与环境噪声）。
    ''' </summary>
    Private Const RefreshIntervalBytes As Integer = 1024

    Private ReadOnly _mixer As EntropyMixer
    Private ReadOnly _entropy As New EnvironmentEntropy
    Private ReadOnly _sync As New Object
    Private _bytesSinceRefresh As Integer

    ''' <summary>
    ''' 全局共享实例，便于直接调用 <c>TrueRandom.SharedInstance.GenerateBytes(n)</c>。
    ''' </summary>
    Public Shared ReadOnly SharedInstance As New TrueRandom()

    ''' <summary>
    ''' 初始化混合生成器：以 CSPRNG 取 32 字节密钥，并折叠一份环境噪声种子。
    ''' </summary>
    Public Sub New()
        Dim key(31) As Byte
        RandomNumberGenerator.Fill(key)

        ' 折叠环境噪声，使初始密钥即使在同一机器上也随时间/负载而不同
        Dim seed As Byte() = _entropy.GetSeed()
        Dim mixed(key.Length + seed.Length - 1) As Byte
        Array.Copy(key, 0, mixed, 0, key.Length)
        Array.Copy(seed, 0, mixed, key.Length, seed.Length)

        Using sha As SHA256 = SHA256.Create()
            _mixer = New EntropyMixer(sha.ComputeHash(mixed))
        End Using
    End Sub

    ''' <summary>
    ''' 本生成器对所有公开方法加锁，可安全地跨线程共享。
    ''' </summary>
    Public ReadOnly Property IsThreadSafe As Boolean Implements IProvideRandomValues.IsThreadSafe
        Get
            Return True
        End Get
    End Property

    ''' <summary>
    ''' 生成 <paramref name="count"/> 字节真随机数据。
    ''' </summary>
    Public Function GenerateBytes(count As Integer) As Byte()
        If count <= 0 Then
            Return New Byte(-1) {}
        End If

        SyncLock _sync
            Dim bytes As Byte() = _mixer.Expand(count)
            _bytesSinceRefresh += count

            If _bytesSinceRefresh >= RefreshIntervalBytes Then
                _bytesSinceRefresh = 0
                RefreshKey()
            End If

            Return bytes
        End SyncLock
    End Function

    ''' <summary>
    ''' 周期性刷新密钥：折叠一块新的 CSPRNG 输出与一次环境噪声采样，实现前向保密。
    ''' 调用方必须已持有 <see cref="_sync"/>。
    ''' </summary>
    Private Sub RefreshKey()
        Dim rng(31) As Byte
        RandomNumberGenerator.Fill(rng)
        _mixer.MixKey(rng)
        _mixer.MixKey(_entropy.GetSeed())
    End Sub

    ''' <summary>
    ''' 生成 <paramref name="count"/> 个均匀分布于全 Int32 范围的 32 位整数
    ''' （由 4 字节密钥流直接构造，无模偏）。
    ''' </summary>
    Public Function GenerateInt32s(count As Integer) As Integer()
        If count <= 0 Then
            Return New Integer(-1) {}
        End If

        Dim bytes As Byte() = GenerateBytes(count * 4)
        Dim out(count - 1) As Integer

        For i As Integer = 0 To count - 1
            out(i) = BitConverter.ToInt32(bytes, i * 4)
        Next

        Return out
    End Function

    ''' <summary>
    ''' 生成 1 个均匀分布于 [0, UInt32.MaxValue] 的 32 位无符号整数（内部使用）。
    ''' </summary>
    Private Function GenerateUInt32() As UInteger
        Return BitConverter.ToUInt32(GenerateBytes(4), 0)
    End Function

    ''' <summary>
    ''' 生成 <paramref name="count"/> 个均匀分布于 [0,1) 的双精度浮点数
    ''' （53-bit 精度，与 <see cref="MersenneTwisterFast.nextDouble"/> 同方案）。
    ''' </summary>
    Public Function GenerateDoubles(count As Integer) As Double()
        If count <= 0 Then
            Return New Double(-1) {}
        End If

        Dim bytes As Byte() = GenerateBytes(count * 8)
        Dim out(count - 1) As Double

        For i As Integer = 0 To count - 1
            Dim u As ULong = BitConverter.ToUInt64(bytes, i * 8)
            ' 取高 53 位，保证 [0,1) 且精度足够
            out(i) = CDbl(u >> 11) / CDbl(1UL << 53)
        Next

        Return out
    End Function

    ''' <summary>
    ''' 生成一个 [0,1) 的双精度浮点数。
    ''' </summary>
    Public Function NextDouble() As Double
        Return GenerateDoubles(1)(0)
    End Function

    ''' <summary>
    ''' 生成一个非负随机整数（[0, Int32.MaxValue)）。
    ''' </summary>
    Public Function [Next]() As Integer
        ' 取 31 位，保证非负
        Return CInt(GenerateUInt32() >> 1)
    End Function

    ''' <summary>
    ''' 生成一个 [0, maxValue) 的非负随机整数。
    ''' </summary>
    Public Function [Next](maxValue As Integer) As Integer
        If maxValue <= 0 Then
            Return 0
        End If

        Return [Next](0, maxValue)
    End Function

    ''' <summary>
    ''' 生成一个 [minValue, maxValue) 的随机整数（maxValue 为独占上界）。
    ''' </summary>
    Public Function [Next](minValue As Integer, maxValue As Integer) As Integer Implements IProvideRandomValues.Next
        If minValue >= maxValue Then
            Return minValue
        End If

        Dim range As Long = CLng(maxValue) - minValue
        Dim d As Double = GenerateDoubles(1)(0)
        Dim r As Long = minValue + CLng(d * range)

        ' d 严格小于 1，理论上 r < maxValue；浮点舍入保底夹紧
        If r >= maxValue Then
            r = maxValue - 1
        End If

        Return CInt(r)
    End Function

    ''' <summary>
    ''' 生成一个 [0,1) 的随机浮点数（接口约定返回 Double）。
    ''' </summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function NextFloat() As Double Implements IProvideRandomValues.NextFloat
        Return NextDouble()
    End Function

    ''' <summary>
    ''' 用随机双精度数填充整个缓冲区（[0,1)）。
    ''' </summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Sub NextFloats(buffer As Double()) Implements IProvideRandomValues.NextFloats
        If buffer Is Nothing OrElse buffer.Length = 0 Then
            Return
        End If

        Dim values As Double() = GenerateDoubles(buffer.Length)
        Array.Copy(values, buffer, buffer.Length)
    End Sub

    ''' <summary>
    ''' 用随机字节填充缓冲区。
    ''' </summary>
    Public Sub NextBytes(buffer As Byte())
        If buffer Is Nothing OrElse buffer.Length = 0 Then
            Return
        End If

        Dim bytes As Byte() = GenerateBytes(buffer.Length)
        Array.Copy(bytes, buffer, buffer.Length)
    End Sub
End Class
