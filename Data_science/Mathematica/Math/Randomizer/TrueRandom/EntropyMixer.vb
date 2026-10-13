#Region "Microsoft.VisualBasic::21c166116fc71d427ad4adf392fd85fa, Data_science\Mathematica\Math\Randomizer\TrueRandom\EntropyMixer.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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


    ' Code Statistics:

    '   Total Lines: 73
    '    Code Lines: 42 (57.53%)
    ' Comment Lines: 19 (26.03%)
    '    - Xml Docs: 94.74%
    ' 
    '   Blank Lines: 12 (16.44%)
    '     File Size: 2.68 KB


    ' Class EntropyMixer
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: Expand
    ' 
    '     Sub: MixKey
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Security.Cryptography
Imports std = System.Math

''' <summary>
''' 基于 HMAC-SHA256 计数器（CTR）模式的密钥流混合器。
''' 
''' 将 256-bit 密钥展开为任意长度密钥流：第 i 块 = HMAC-SHA256(密钥, 计数器[i])。
''' 密钥可通过 <see cref="MixKey"/> 周期性折叠新的熵（CSPRNG 输出或环境噪声），
''' 实现前向保密与抗回溯。本类本身不加锁，线程安全由调用方（<see cref="TrueRandom"/>）
''' 通过 <c>SyncLock</c> 保证。
''' </summary>
Public Class EntropyMixer

    Private _key As Byte()
    Private _counter As ULong

    ''' <summary>
    ''' 使用给定 32 字节密钥初始化混合器，计数器从 0 开始。
    ''' </summary>
    Public Sub New(key As Byte())
        If key Is Nothing OrElse key.Length <> 32 Then
            Throw New System.ArgumentException("EntropyMixer 需要一个 32 字节（256-bit）密钥。", NameOf(key))
        End If

        _key = CType(key.Clone, Byte())
        _counter = 0
    End Sub

    ''' <summary>
    ''' 将附加熵折叠进当前密钥：新密钥 = SHA256(旧密钥 ‖ 附加熵)。
    ''' 用于周期性刷新，使已产出的密钥流无法反推后续输出。
    ''' </summary>
    Public Sub MixKey(additional As Byte())
        If additional Is Nothing OrElse additional.Length = 0 Then
            Return
        End If

        Dim input(_key.Length + additional.Length - 1) As Byte
        Array.Copy(_key, 0, input, 0, _key.Length)
        Array.Copy(additional, 0, input, _key.Length, additional.Length)

        Using sha As SHA256 = SHA256.Create()
            _key = sha.ComputeHash(input)
        End Using
    End Sub

    ''' <summary>
    ''' 展开 <paramref name="count"/> 字节密钥流。每 32 字节为一次 HMAC 计算，
    ''' 计数器随块递增，保证输出均匀且不可预测。
    ''' </summary>
    Public Function Expand(count As Integer) As Byte()
        If count <= 0 Then
            Return New Byte(-1) {}
        End If

        Dim out As Byte() = New Byte(count - 1) {}
        Dim offset As Integer = 0

        Using hmac As New HMACSHA256(_key)
            Do
                Dim ctr As Byte() = BitConverter.GetBytes(_counter)
                _counter += 1UL

                Dim block As Byte() = hmac.ComputeHash(ctr)
                Dim take As Integer = std.Min(block.Length, count - offset)
                Array.Copy(block, 0, out, offset, take)
                offset += take
            Loop While offset < count
        End Using

        Return out
    End Function
End Class
