#Region "Microsoft.VisualBasic::8c3aefdec3dffe0d7a7f353a550c35df, Data\BinaryData\HDF5\test\SanityCheck.vb"

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

    '   Total Lines: 83
    '    Code Lines: 54 (65.06%)
    ' Comment Lines: 14 (16.87%)
    '    - Xml Docs: 78.57%
    ' 
    '   Blank Lines: 15 (18.07%)
    '     File Size: 3.25 KB


    '     Module SanityCheck
    ' 
    '         Function: Check, Hint
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text

Namespace test

    ''' <summary>
    ''' 针对 10x Genomics 数据语义的数值合理性校验。
    ''' 用于识别「不抛异常但数值错误」的静默解码问题（如缺失 shuffle 过滤器）。
    ''' </summary>
    Public Module SanityCheck

        ''' <summary>
        ''' 对读出的抽样值字符串做启发式合理性检查。
        ''' 返回告警文本（无告警则返回空字符串）。
        ''' </summary>
        Public Function Check(path As String, sample As String) As String
            If String.IsNullOrEmpty(sample) Then
                Return ""
            End If

            ' 字符串类：检查是否含大量不可打印字符（乱码）
            If sample.IndexOf("?"c) >= 0 OrElse sample.IndexOf("�"c) >= 0 Then
                Return "字符串含替换字符，疑似编码/全局堆解析错误"
            End If

            ' 数值类：检查是否全是 0 或异常负值（静默解码错误的常见表象）
            Dim numericParts = sample _
                .Replace("[", "").Replace("]", "").Replace("{", "").Replace("}", "") _
                .Split(New Char() {" "c, ","c, vbTab, vbCr, vbLf}, StringSplitOptions.RemoveEmptyEntries)

            If numericParts.Length = 0 Then
                Return ""
            End If

            Dim allZero As Boolean = True
            Dim negativeCount As Integer = 0
            Dim parsed As New List(Of Long)()

            For Each p In numericParts
                Dim v As Long
                If Long.TryParse(p, v) Then
                    parsed.Add(v)
                    If v <> 0 Then allZero = False
                    If v < 0 Then negativeCount += 1
                Else
                    ' 含非整数字段（浮点/字符串），放弃数值启发式
                    Return ""
                End If
            Next

            If parsed.Count > 3 Then
                If allZero Then
                    Return "采样值全部为 0，疑似解压/反混洗缺失导致的静默解码错误"
                End If

                If negativeCount > parsed.Count \ 2 Then
                    Return "多数采样值为负数，疑似字节序或解码错误"
                End If
            End If

            Return ""
        End Function

        ''' <summary>
        ''' 依据对象路径给出语义化的范围提示（仅用于报告，不强制断言）。
        ''' </summary>
        Public Function Hint(path As String) As String
            Dim lower = path.ToLower()

            If lower.Contains("barcode") OrElse lower.Contains("feature_idx") OrElse lower.Contains("barcode_idx") Then
                Return "索引类，应为非负整数"
            End If
            If lower.Contains("count") OrElse lower.Contains("umi") Then
                Return "计数类，应为非负整数"
            End If
            If lower.Contains("name") OrElse lower.Contains("genome") OrElse lower.Contains("id") Then
                Return "文本类，应为可读字符串"
            End If

            Return ""
        End Function
    End Module

End Namespace
