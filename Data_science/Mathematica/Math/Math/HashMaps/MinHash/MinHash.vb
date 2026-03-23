#Region "Microsoft.VisualBasic::dbedb566684226db7dcd3f1b17d1acee, Data_science\Mathematica\Math\Math\HashMaps\MinHash\MinHash.vb"

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

    '   Total Lines: 61
    '    Code Lines: 32 (52.46%)
    ' Comment Lines: 19 (31.15%)
    '    - Xml Docs: 73.68%
    ' 
    '   Blank Lines: 10 (16.39%)
    '     File Size: 2.38 KB


    '     Module MinHash
    ' 
    '         Function: CreateSequenceData, GenerateMinHashSignature
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Data.Repository
Imports Microsoft.VisualBasic.Language.Java

Namespace HashMaps.MinHash

    Public Module MinHash

        ''' <summary>
        ''' 生成MinHash签名
        ''' </summary>
        ''' <param name="shingles"></param>
        ''' <param name="Num_HashFunctions">
        ''' MinHash签名长度
        ''' </param>
        ''' <returns></returns>
        Public Function GenerateMinHashSignature(shingles As HashSet(Of String), Num_HashFunctions As Integer) As UInteger()
            Dim signature As UInteger() = New UInteger(Num_HashFunctions - 1) {}.fill(UInteger.MaxValue)

            ' 3. 计算 MinHash
            '    遍历每一个 Shingle 的字节数据
            For Each shingle As String In shingles
                Dim bytesData As Byte() = Encoding.UTF8.GetBytes(shingle)

                ' 遍历每一个哈希函数 (由 seed 0 到 N-1 代表)
                For i As Integer = 0 To Num_HashFunctions - 1
                    ' 使用索引 i 作为种子，相当于第 i 个哈希函数
                    Dim hashVal As UInteger = MurmurHash.MurmurHashCode3_x86_32(bytesData, CUInt(i))

                    ' 更新签名中对应位置的最小值
                    If hashVal < signature(i) Then
                        signature(i) = hashVal
                    End If
                Next
            Next

            Return signature
        End Function

        ''' <summary>
        ''' Compute minhash for a given sequence
        ''' </summary>
        ''' <param name="items">对字符串序列进行n-gram切片后的结果</param>
        ''' <param name="id">sequence id</param>
        ''' <returns></returns>
        <Extension>
        Public Function CreateSequenceData(items As IEnumerable(Of String), id As Integer, Optional Num_HashFunctions As Integer = 100) As SequenceItem
            Dim shingles As New HashSet(Of String)

            For Each item As String In items
                Call shingles.Add(item)
            Next

            Return New SequenceItem With {
                .ID = id,
                .Signature = MinHash.GenerateMinHashSignature(shingles, Num_HashFunctions)
            }
        End Function
    End Module
End Namespace
