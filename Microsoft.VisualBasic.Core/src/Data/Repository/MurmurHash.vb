#Region "Microsoft.VisualBasic::8f88bdbdf3970d9d5895dd74b82a1462, Microsoft.VisualBasic.Core\src\Data\Repository\MurmurHash.vb"

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

    '   Total Lines: 79
    '    Code Lines: 55 (69.62%)
    ' Comment Lines: 13 (16.46%)
    '    - Xml Docs: 46.15%
    ' 
    '   Blank Lines: 11 (13.92%)
    '     File Size: 2.70 KB


    '     Module MurmurHash
    ' 
    '         Function: MurmurHashCode3_x86_32
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Data.Repository

    Public Module MurmurHash

        ' MurmurHash3 的常量
        Const c1 As UInteger = &HCC9E2D51UI
        Const c2 As UInteger = &H1B873593UI
        Const r1 As Integer = 15
        Const r2 As Integer = 13
        Const m As UInteger = 5UI
        Const n As UInteger = &HE6546B64UI

        ''' <summary>
        ''' 计算给定数据的32位MurmurHash3值。
        ''' </summary>
        ''' <param name="data">输入数据。</param>
        ''' <param name="seed">哈希种子。</param>
        ''' <returns>32位无符号哈希值。</returns>
        Public Function MurmurHashCode3_x86_32(data As Byte(), seed As UInteger) As UInteger
            If data Is Nothing Then Throw New ArgumentNullException(NameOf(data))

            Dim length As Integer = data.Length
            Dim h As UInteger = seed
            Dim i As Integer = 0
            Dim k As UInteger = 0

            ' --- 处理主体部分（4字节块） ---
            While length - i >= 4
                k = CUInt(data(i)) Or
                            CUInt(data(i + 1)) << 8 Or
                            CUInt(data(i + 2)) << 16 Or
                            CUInt(data(i + 3)) << 24

                k *= c1
                k = (k << r1) Or (k >> (32 - r1))
                k *= c2

                h = h Xor k
                h = (h << r2) Or (h >> (32 - r2))
                h = h * m + n
                i += 4
            End While

            ' --- 处理尾部剩余字节（修正部分） ---
            k = 0
            Select Case length - i
                Case 3
                    k = k Xor CUInt(data(i + 2)) << 16
                    ' 注意：这里没有 Exit Select，会继续执行 Case 2
                    k = k Xor CUInt(data(i + 1)) << 8
                    ' 继续执行 Case 1
                    k = k Xor CUInt(data(i))
                Case 2
                    k = k Xor CUInt(data(i + 1)) << 8
                    ' 继续执行 Case 1
                    k = k Xor CUInt(data(i))
                Case 1
                    k = k Xor CUInt(data(i))
            End Select

            If length - i > 0 Then
                k *= c1
                k = (k << r1) Or (k >> (32 - r1))
                k *= c2
                h = h Xor k
            End If

            ' --- 最终混合 ---
            h = h Xor CUInt(length)
            h = h Xor (h >> 16)
            h *= &H85EBCA6BUI
            h = h Xor (h >> 13)
            h *= &HC2B2AE35UI
            h = h Xor (h >> 16)

            Return h
        End Function
    End Module
End Namespace
