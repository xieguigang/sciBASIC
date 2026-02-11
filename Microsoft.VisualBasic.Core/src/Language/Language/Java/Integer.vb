#Region "Microsoft.VisualBasic::8d1dad06ac3b80a525677161aa990aee, Microsoft.VisualBasic.Core\src\Language\Language\Java\Integer.vb"

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

    '   Total Lines: 36
    '    Code Lines: 20 (55.56%)
    ' Comment Lines: 8 (22.22%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 8 (22.22%)
    '     File Size: 1.19 KB


    '     Module [Integer]
    ' 
    '         Function: bitCount
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
