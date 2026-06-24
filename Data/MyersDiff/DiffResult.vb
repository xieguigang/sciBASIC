#Region "Microsoft.VisualBasic::15dd337ca01f584b97d2fca411de6e5a, Data\MyersDiff\DiffResult.vb"

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

    '   Total Lines: 247
    '    Code Lines: 169 (68.42%)
    ' Comment Lines: 43 (17.41%)
    '    - Xml Docs: 62.79%
    ' 
    '   Blank Lines: 35 (14.17%)
    '     File Size: 9.84 KB


    ' Class DiffResult
    ' 
    '     Properties: DeleteCount, EqualCount, InsertCount, Items, NewCount
    '                 OldCount, Similarity
    ' 
    '     Function: BuildDiffBlocks, ToSideBySide, ToSummary, ToUnifiedDiff, TruncatePad
    ' 
    ' /********************************************************************************/

#End Region

' -----------------------------------------------------------------------
' 差异结果：封装完整的比较结果
' -----------------------------------------------------------------------
Imports System.Text

''' <summary>
''' 表示两个序列比较后的完整差异结果。
''' </summary>
Public Class DiffResult
    ''' <summary>所有差异项的有序列表。</summary>
    Public Property Items As New List(Of DiffItem)()

    ''' <summary>旧序列的元素总数。</summary>
    Public Property OldCount As Integer

    ''' <summary>新序列的元素总数。</summary>
    Public Property NewCount As Integer

    ''' <summary>相同元素的数量。</summary>
    Public ReadOnly Property EqualCount As Integer
        Get
            Dim count As Integer = 0
            For Each item In Items
                If item.Type = EditType.Equal Then count += 1
            Next
            Return count
        End Get
    End Property

    ''' <summary>删除操作的数量。</summary>
    Public ReadOnly Property DeleteCount As Integer
        Get
            Dim count As Integer = 0
            For Each item In Items
                If item.Type = EditType.Delete Then count += 1
            Next
            Return count
        End Get
    End Property

    ''' <summary>插入操作的数量。</summary>
    Public ReadOnly Property InsertCount As Integer
        Get
            Dim count As Integer = 0
            For Each item In Items
                If item.Type = EditType.Insert Then count += 1
            Next
            Return count
        End Get
    End Property

    ''' <summary>相似度（0.0 ~ 1.0），基于相同元素占比计算。</summary>
    Public ReadOnly Property Similarity As Double
        Get
            Dim total As Integer = Math.Max(OldCount, NewCount)
            If total = 0 Then Return 1.0
            Return CDbl(EqualCount) / CDbl(total)
        End Get
    End Property

    ''' <summary>
    ''' 将差异结果转换为统一差异格式(Unified Diff)字符串。
    ''' </summary>
    ''' <param name="oldLabel">旧文件标签（如文件名）。</param>
    ''' <param name="newLabel">新文件标签（如文件名）。</param>
    ''' <param name="contextLines">上下文行数（默认 3 行）。</param>
    ''' <returns>统一差异格式的字符串。</returns>
    Public Function ToUnifiedDiff(oldLabel As String, newLabel As String,
                                   Optional contextLines As Integer = 3) As String
        Dim sb As New StringBuilder()

        ' 如果没有任何差异项，直接返回空字符串
        If Items.Count = 0 Then Return sb.ToString()

        ' 将差异项分组为差异块
        Dim blocks As List(Of DiffBlock) = BuildDiffBlocks(contextLines)

        ' 如果没有任何差异块（完全相同），直接返回
        If blocks.Count = 0 Then Return sb.ToString()

        ' 输出文件头
        sb.AppendFormat("--- {0}", oldLabel).AppendLine()
        sb.AppendFormat("+++ {0}", newLabel).AppendLine()

        ' 逐块输出
        For Each block In blocks
            ' 输出块头 @@ -oldStart,oldCount +newStart,newCount @@
            sb.AppendFormat("@@ -{0},{1} +{2},{3} @@", block.OldStart, block.OldCount,
                            block.NewStart, block.NewCount).AppendLine()

            ' 输出块内每一行
            For Each item In block.Items
                Select Case item.Type
                    Case EditType.Equal
                        sb.AppendFormat(" {0}", item.Value)
                    Case EditType.Delete
                        sb.AppendFormat("-{0}", item.Value)
                    Case EditType.Insert
                        sb.AppendFormat("+{0}", item.Value)
                End Select
                sb.AppendLine()
            Next
        Next

        Return sb.ToString()
    End Function

    ''' <summary>
    ''' 将差异结果转换为简洁的并排对照格式。
    ''' </summary>
    Public Function ToSideBySide(Optional maxWidth As Integer = 50) As String
        Dim sb As New StringBuilder()
        Dim separator As String = " | "
        Dim leftHeader As String = "旧文件".PadRight(maxWidth)
        Dim rightHeader As String = "新文件".PadRight(maxWidth)
        sb.AppendLine(leftHeader & separator & rightHeader)
        sb.AppendLine(New String("-"c, maxWidth) & "-" & New String("-"c, maxWidth))

        For Each item In Items
            Dim leftVal As String
            Dim rightVal As String

            Select Case item.Type
                Case EditType.Equal
                    leftVal = "  " & TruncatePad(item.Value, maxWidth - 2)
                    rightVal = "  " & TruncatePad(item.Value, maxWidth - 2)
                Case EditType.Delete
                    leftVal = "- " & TruncatePad(item.Value, maxWidth - 2)
                    rightVal = "  " & TruncatePad("", maxWidth - 2)
                Case EditType.Insert
                    leftVal = "  " & TruncatePad("", maxWidth - 2)
                    rightVal = "+ " & TruncatePad(item.Value, maxWidth - 2)
                Case Else
                    leftVal = "  " & TruncatePad(item.Value, maxWidth - 2)
                    rightVal = "  " & TruncatePad(item.Value, maxWidth - 2)
            End Select

            sb.AppendLine(leftVal & separator & rightVal)
        Next

        Return sb.ToString()
    End Function

    ''' <summary>
    ''' 生成统计摘要字符串。
    ''' </summary>
    Public Function ToSummary() As String
        Dim sb As New StringBuilder()
        sb.AppendLine("===== 差异比较统计 =====")
        sb.AppendFormat("旧序列元素数: {0}", OldCount).AppendLine()
        sb.AppendFormat("新序列元素数: {0}", NewCount).AppendLine()
        sb.AppendFormat("相同项数量:   {0}", EqualCount).AppendLine()
        sb.AppendFormat("删除项数量:   {0}", DeleteCount).AppendLine()
        sb.AppendFormat("插入项数量:   {0}", InsertCount).AppendLine()
        sb.AppendFormat("相似度:       {0:P2}", Similarity).AppendLine()
        sb.AppendLine("========================")
        Return sb.ToString()
    End Function

    ' ---- 私有辅助方法 ----

    ''' <summary>
    ''' 将差异项分组为差异块，用于统一差异格式输出。
    ''' 合并相邻的上下文行，并在块之间保留指定数量的上下文行。
    ''' </summary>
    Private Function BuildDiffBlocks(contextLines As Integer) As List(Of DiffBlock)
        Dim blocks As New List(Of DiffBlock)()

        ' 首先找出所有包含变更（非 Equal）的差异项索引
        Dim changeIndices As New List(Of Integer)()
        For i As Integer = 0 To Items.Count - 1
            If Items(i).Type <> EditType.Equal Then
                changeIndices.Add(i)
            End If
        Next

        If changeIndices.Count = 0 Then Return blocks

        ' 将变更索引按连续性分组（考虑上下文合并）
        Dim groups As New List(Of List(Of Integer))()
        Dim currentGroup As New List(Of Integer)()
        currentGroup.Add(changeIndices(0))

        For i As Integer = 1 To changeIndices.Count - 1
            ' 如果当前变更与前一变更之间的距离在 2*contextLines 以内，则合并
            Dim gap As Integer = changeIndices(i) - changeIndices(i - 1)
            If gap <= 2 * contextLines + 1 Then
                currentGroup.Add(changeIndices(i))
            Else
                groups.Add(currentGroup)
                currentGroup = New List(Of Integer)()
                currentGroup.Add(changeIndices(i))
            End If
        Next
        groups.Add(currentGroup)

        ' 为每个分组构建差异块
        For Each group In groups
            Dim startIdx As Integer = Math.Max(0, group(0) - contextLines)
            Dim endIdx As Integer = Math.Min(Items.Count - 1, group(group.Count - 1) + contextLines)

            Dim blockItems As New List(Of DiffItem)()
            Dim oldLine As Integer = 0
            Dim newLine As Integer = 0

            ' 计算起始行号
            For i As Integer = 0 To startIdx - 1
                If Items(i).Type = EditType.Equal OrElse Items(i).Type = EditType.Delete Then
                    oldLine += 1
                End If
                If Items(i).Type = EditType.Equal OrElse Items(i).Type = EditType.Insert Then
                    newLine += 1
                End If
            Next

            Dim oldStart As Integer = oldLine + 1  ' 转为 1-based
            Dim newStart As Integer = newLine + 1
            Dim oldCount As Integer = 0
            Dim newCount As Integer = 0

            For i As Integer = startIdx To endIdx
                blockItems.Add(Items(i))
                If Items(i).Type = EditType.Equal OrElse Items(i).Type = EditType.Delete Then
                    oldCount += 1
                End If
                If Items(i).Type = EditType.Equal OrElse Items(i).Type = EditType.Insert Then
                    newCount += 1
                End If
            Next

            blocks.Add(New DiffBlock(oldStart, oldCount, newStart, newCount) With {
                .Items = blockItems
            })
        Next

        Return blocks
    End Function

    Private Shared Function TruncatePad(value As String, width As Integer) As String
        If value Is Nothing Then value = ""
        If value.Length > width Then
            Return value.Substring(0, width - 2) & ".."
        Else
            Return value.PadRight(width)
        End If
    End Function
End Class
