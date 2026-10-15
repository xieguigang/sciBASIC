#Region "Microsoft.VisualBasic::dc8296fa23af677a26002bc4da1011bd, Data\MyersDiff\MyersDiff.vb"

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

    '   Total Lines: 371
    '    Code Lines: 181 (48.79%)
    ' Comment Lines: 135 (36.39%)
    '    - Xml Docs: 34.81%
    ' 
    '   Blank Lines: 55 (14.82%)
    '     File Size: 15.77 KB


    ' Class MyersDiff
    ' 
    '     Function: Backtrack, BuildDiffItems, Compare, CompareChars, CompareFiles
    '               ComputeEditPath
    '     Class EditStep
    ' 
    '         Properties: NewIndex, OldIndex, Type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text

' ============================================================================
' MyersDiff.vb — 基于 Myers 差异算法的文本比较模块
' ============================================================================
' 算法参考: Eugene W. Myers, "An O(ND) Difference Algorithm and Its Variations",
'           Algorithmica, Vol. 1, No. 2, 1986, pp. 251-266.
'
' 功能说明:
'   1. 使用 Myers 算法计算两个文本文件/字符串序列之间的最短编辑脚本(SES)
'   2. 支持行级(Line-level)与字符级(Character-level)两种粒度的差异比较
'   3. 输出统一差异格式(Unified Diff Format)及结构化差异结果
'   4. 纯 VB.NET 基础函数库实现，无第三方依赖
'
' 使用示例:
'   Dim diff As New MyersDiff()
'   Dim result As DiffResult = diff.CompareFiles("old.txt", "new.txt")
'   Console.WriteLine(result.ToUnifiedDiff("old.txt", "new.txt"))
'
' 实现要点(2026-10 修正):
'   a) 回溯阶段原先取 trace(d - 1)，与前向搜索的轮次语义差 1（前向第 d 轮的
'      决策依据应是 V(d-1) = trace(d)），导致编辑脚本错乱；已修正为 trace(d)。
'   b) 回溯表原先每轮都保存长度 2*(N+M)+1 的完整快照，内存 O(D*(N+M))；
'      现在每轮只保存 k ∈ [-d, d] 窗口（长度 2d+1），内存降为 O(D^2)。
'   c) 比较前先裁剪公共前后缀，典型版本差异场景（大文件 + 少量改动）可大幅
'      缩短计算时间，同时避免公共部分被误判为改动。
'   d) CompareChars 对空字符串构造 String(-1) 数组会抛异常，已修正。
'   e) 新增 MaxEditDistance 上界，超过上界时退化为「全删 + 全插」，避免
'      极端场景下 O(D^2) 的内存爆炸。
' ============================================================================

' -----------------------------------------------------------------------
' Myers 差异算法核心实现
' -----------------------------------------------------------------------
''' <summary>
''' Myers 差异算法的 VB.NET 实现。
''' 该算法以 O((N+M)D) 的时间复杂度计算两个序列之间的最短编辑脚本(SES)，
''' 其中 N、M 分别为两个序列的长度，D 为编辑距离。
''' </summary>
Public Class MyersDiff

    ''' <summary>
    ''' 编辑距离（插入 + 删除的数量）的上界。Myers 算法的回溯表需要 O(D^2) 的
    ''' 存储空间，当两个序列差异极大时（例如完全不相关的两个文件），时间与内存
    ''' 开销都会急剧上升。超过此上界时比较结果退化为「全部删除 + 全部插入」
    ''' （<see cref="DiffResult.Truncated"/> = TRUE）。
    '''
    ''' 设为 0 或负数表示不限制（此时最坏情况下可能占用大量内存）。
    ''' </summary>
    Public Property MaxEditDistance As Integer = 5000

    ''' <summary>
    ''' 比较两个字符串数组（行序列），返回差异结果。
    ''' </summary>
    ''' <param name="oldLines">旧文本的行数组。</param>
    ''' <param name="newLines">新文本的行数组。</param>
    ''' <returns>包含所有差异项的 DiffResult 对象。</returns>
    Public Function Compare(oldLines As String(), newLines As String()) As DiffResult
        If oldLines Is Nothing Then oldLines = New String() {}
        If newLines Is Nothing Then newLines = New String() {}

        Dim result As New DiffResult()
        result.OldCount = oldLines.Length
        result.NewCount = newLines.Length

        ' 特殊情况：某一侧为空
        If oldLines.Length = 0 AndAlso newLines.Length = 0 Then
            Return result
        End If

        If oldLines.Length = 0 Then
            For i As Integer = 0 To newLines.Length - 1
                result.Items.Add(New DiffItem(EditType.Insert, -1, i, newLines(i)))
            Next
            Return result
        End If

        If newLines.Length = 0 Then
            For i As Integer = 0 To oldLines.Length - 1
                result.Items.Add(New DiffItem(EditType.Delete, i, -1, oldLines(i)))
            Next
            Return result
        End If

        Dim N As Integer = oldLines.Length
        Dim M As Integer = newLines.Length
        Dim items As New List(Of DiffItem)()

        ' ------------------------------------------------------------
        ' 裁剪公共前后缀：版本比较场景中绝大部分内容都是相同的，先裁掉
        ' 这部分可以显著降低 Myers 算法的输入规模。
        ' ------------------------------------------------------------
        Dim prefix As Integer = 0
        Dim maxPrefix As Integer = System.Math.Min(N, M)

        While prefix < maxPrefix AndAlso String.Equals(oldLines(prefix), newLines(prefix), StringComparison.Ordinal)
            prefix += 1
        End While

        Dim suffix As Integer = 0
        Dim maxSuffix As Integer = System.Math.Min(N - prefix, M - prefix)

        While suffix < maxSuffix AndAlso String.Equals(oldLines(N - 1 - suffix), newLines(M - 1 - suffix), StringComparison.Ordinal)
            suffix += 1
        End While

        ' 头部的公共行
        For i As Integer = 0 To prefix - 1
            items.Add(New DiffItem(EditType.Equal, i, i, oldLines(i)))
        Next

        Dim midN As Integer = N - prefix - suffix
        Dim midM As Integer = M - prefix - suffix

        If midN = 0 AndAlso midM = 0 Then
            ' 两个序列完全相同
        ElseIf midN = 0 Then
            ' 只有插入
            For j As Integer = 0 To midM - 1
                items.Add(New DiffItem(EditType.Insert, -1, prefix + j, newLines(prefix + j)))
            Next
        ElseIf midM = 0 Then
            ' 只有删除
            For i As Integer = 0 To midN - 1
                items.Add(New DiffItem(EditType.Delete, prefix + i, -1, oldLines(prefix + i)))
            Next
        Else
            Dim midOld As String() = New String(midN - 1) {}
            Dim midNew As String() = New String(midM - 1) {}

            Call Array.ConstrainedCopy(oldLines, prefix, midOld, 0, midN)
            Call Array.ConstrainedCopy(newLines, prefix, midNew, 0, midM)

            Dim editPath As List(Of EditStep) = ComputeEditPath(midOld, midNew)

            If editPath Is Nothing Then
                ' 编辑距离超过上界：退化为全删 + 全插
                result.Truncated = True

                For i As Integer = 0 To midN - 1
                    items.Add(New DiffItem(EditType.Delete, prefix + i, -1, oldLines(prefix + i)))
                Next
                For j As Integer = 0 To midM - 1
                    items.Add(New DiffItem(EditType.Insert, -1, prefix + j, newLines(prefix + j)))
                Next
            Else
                items.AddRange(BuildDiffItems(midOld, midNew, editPath, prefix))
            End If
        End If

        ' 尾部的公共行
        For s As Integer = 0 To suffix - 1
            Dim i As Integer = N - suffix + s
            Dim j As Integer = M - suffix + s

            items.Add(New DiffItem(EditType.Equal, i, j, oldLines(i)))
        Next

        result.Items = items

        Return result
    End Function

    ''' <summary>
    ''' 比较两个字符串（字符级），返回差异结果。
    ''' </summary>
    ''' <param name="oldText">旧文本字符串。</param>
    ''' <param name="newText">新文本字符串。</param>
    ''' <returns>包含所有差异项的 DiffResult 对象。</returns>
    Public Function CompareChars(oldText As String, newText As String) As DiffResult
        If oldText Is Nothing Then oldText = ""
        If newText Is Nothing Then newText = ""

        Return Compare(ToChars(oldText), ToChars(newText))
    End Function

    ''' <summary>
    ''' 字符串拆分为字符数组（空字符串返回空数组，避免 New String(-1) 越界）
    ''' </summary>
    Private Shared Function ToChars(text As String) As String()
        If text.Length = 0 Then
            Return New String() {}
        End If

        Dim chars As String() = New String(text.Length - 1) {}

        For i As Integer = 0 To text.Length - 1
            chars(i) = text(i).ToString()
        Next

        Return chars
    End Function

    ''' <summary>
    ''' 比较两个文本文件，返回差异结果。
    ''' </summary>
    ''' <param name="oldFilePath">旧文件路径。</param>
    ''' <param name="newFilePath">新文件路径。</param>
    ''' <param name="encoding">文件编码（默认 UTF-8）。</param>
    ''' <returns>包含所有差异项的 DiffResult 对象。</returns>
    Public Function CompareFiles(oldFilePath As String, newFilePath As String, Optional encoding As Encoding = Nothing) As DiffResult
        If encoding Is Nothing Then
            encoding = Encoding.UTF8
        End If

        Dim oldLines As String() = oldFilePath.ReadAllLines(encoding)
        Dim newLines As String() = newFilePath.ReadAllLines(encoding)

        Return Compare(oldLines, newLines)
    End Function

    ' ================================================================
    ' Myers 算法核心：计算最短编辑路径
    ' ================================================================
    ''' <summary>
    ''' 执行 Myers 算法的前向搜索阶段，计算从 (0,0) 到 (N,M) 的最短编辑路径。
    '''
    ''' 算法核心思想：
    ''' - 编辑图(Edit Graph)中，对角线 k = x - y
    ''' - 对于每个编辑距离 d（从 0 到最大），搜索每条对角线 k（从 -d 到 d，步长 2）
    ''' - 记录每条对角线上能到达的最远 x 坐标（即 furthest reaching point）
    ''' - 当到达 (N, M) 时，当前 d 即为最小编辑距离
    '''
    ''' 时间复杂度: O((N+M) * D)，其中 D 为编辑距离
    ''' 空间复杂度: O(D^2)（回溯表只保留每轮实际使用的对角线窗口）
    ''' </summary>
    ''' <returns>
    ''' 编辑步骤序列；当编辑距离超过 <see cref="MaxEditDistance"/> 时返回 Nothing。
    ''' </returns>
    Private Function ComputeEditPath(oldSeq As String(), newSeq As String()) As List(Of EditStep)
        Dim N As Integer = oldSeq.Length
        Dim M As Integer = newSeq.Length
        Dim maxD As Integer = N + M  ' 编辑距离的上界

        If MaxEditDistance > 0 AndAlso MaxEditDistance < maxD Then
            maxD = MaxEditDistance
        End If

        ' V 数组：V(k) 表示对角线 k 上能到达的最远 x 坐标
        ' 对角线 k 的范围是 [-maxD, maxD]，使用偏移量 maxD 来映射到数组索引
        Dim V As Integer() = New Integer(2 * maxD) {}
        Dim NEG_INF As Integer = Integer.MinValue

        ' 初始化：在 d = -1 时，对角线 k = 1 上 x = 0
        ' 这相当于从 (0, -1) 开始，第一步向下走（插入）
        For i As Integer = 0 To 2 * maxD
            V(i) = NEG_INF
        Next
        V(maxD + 1) = 0  ' V[1] = 0，即对角线 k=1 上 x=0

        ' trace 数组：记录每一轮开始前（即上一轮结束后）的 V 状态，用于回溯。
        ' 第 d 轮只需要 k ∈ [-d, d] 的窗口，所以快照长度取 2d+1（偏移量 d），
        ' 相比保存完整的 2*maxD+1 可节省大量内存。
        Dim trace As New List(Of Integer())()

        Dim found As Boolean = False

        ' 外层循环：编辑距离 d 从 0 递增
        For d As Integer = 0 To maxD
            Dim snapshot As Integer() = New Integer(2 * d) {}

            For k As Integer = -d To d
                snapshot(k + d) = V(k + maxD)
            Next

            trace.Add(snapshot)

            ' 内层循环：对角线 k 从 -d 到 d，步长 2
            For k As Integer = -d To d Step 2
                ' 确定是向下走（插入）还是向右走（删除）
                Dim x As Integer
                Dim kIdx As Integer = k + maxD

                If k = -d OrElse (k <> d AndAlso V(kIdx - 1) < V(kIdx + 1)) Then
                    ' 向下走：从对角线 k+1 的最远点向下移动（插入操作）
                    x = V(kIdx + 1)
                Else
                    ' 向右走：从对角线 k-1 的最远点向右移动（删除操作）
                    x = V(kIdx - 1) + 1
                End If

                Dim y As Integer = x - k

                ' 沿对角线前进（跳过相同元素）
                While x < N AndAlso y < M AndAlso String.Equals(oldSeq(x), newSeq(y), StringComparison.Ordinal)
                    x += 1
                    y += 1
                End While

                ' 更新对角线 k 上的最远 x 坐标
                V(kIdx) = x

                ' 检查是否到达终点 (N, M)
                If x >= N AndAlso y >= M Then
                    found = True
                    Exit For
                End If
            Next

            If found Then Exit For
        Next

        If Not found Then
            ' 编辑距离超过上界，交由调用方做降级处理
            Return Nothing
        End If

        ' 回溯编辑路径
        Return Backtrack(trace, oldSeq, newSeq)
    End Function

    ' ================================================================
    ' 回溯阶段：从 trace 数组还原编辑路径
    ' ================================================================
    ''' <summary>
    ''' 从算法执行过程中保存的 trace 数组回溯，还原完整的编辑步骤序列。
    '''
    ''' 回溯从终点 (N, M) 逆向进行：第 d 轮的位置是 (x, y)，该轮的移动决策依据
    ''' 的是第 d-1 轮结束时的 V 状态，也就是 trace(d)（注意不是 trace(d-1)，
    ''' 旧实现在这里差了一轮，会还原出错误的编辑脚本）。
    '''
    ''' - 前一步在对角线 k-1 上：向右走一步，即删除 old(prevX)
    ''' - 前一步在对角线 k+1 上：向下走一步，即插入 new(prevY)
    ''' - 对角线移动（相同元素）不消耗编辑距离
    ''' </summary>
    Private Function Backtrack(trace As List(Of Integer()), oldSeq As String(),
                                newSeq As String()) As List(Of EditStep)
        Dim N As Integer = oldSeq.Length
        Dim M As Integer = newSeq.Length
        Dim steps As New List(Of EditStep)()

        Dim x As Integer = N
        Dim y As Integer = M

        ' 从最后一个 trace 开始逆向回溯
        For d As Integer = trace.Count - 1 To 1 Step -1
            ' trace(d) 是第 d 轮开始时的 V 状态（偏移量为 d，覆盖 k ∈ [-d, d]）
            Dim prevV As Integer() = trace(d)
            Dim k As Integer = x - y

            ' 判断前一步是从哪条对角线来的
            Dim prevX As Integer
            Dim prevY As Integer
            Dim stepType As EditType

            If k = -d OrElse (k <> d AndAlso prevV(k + d - 1) < prevV(k + d + 1)) Then
                ' 从对角线 k+1 向下走来（插入）
                prevX = prevV(k + 1 + d)
                stepType = EditType.Insert
            Else
                ' 从对角线 k-1 向右走来（删除）
                prevX = prevV(k - 1 + d)
                stepType = EditType.Delete
            End If

            prevY = prevX - (If(stepType = EditType.Insert, k + 1, k - 1))

            ' 非对角线移动之后的落点（蛇的起点）
            Dim midX As Integer
            Dim midY As Integer

            If stepType = EditType.Delete Then
                ' 从 (prevX, prevY) 向右走一步
                midX = prevX + 1
                midY = prevY
            Else
                ' 从 (prevX, prevY) 向下走一步
                midX = prevX
                midY = prevY + 1
            End If

            ' 回退对角线移动（从 (x, y) 退到蛇的起点），逆序记录
            Dim diagX As Integer = x
            Dim diagY As Integer = y

            While diagX > midX AndAlso diagY > midY
                diagX -= 1
                diagY -= 1
                steps.Add(New EditStep(EditType.Equal, diagX, diagY))
            End While

            ' 记录非对角线移动
            If stepType = EditType.Delete Then
                steps.Add(New EditStep(EditType.Delete, midX - 1, -1))
            Else
                steps.Add(New EditStep(EditType.Insert, -1, midY - 1))
            End If

            ' 更新 x, y 为前一轮的位置
            x = prevX
            y = prevY
        Next

        ' 处理 d = 0 的情况：只有对角线移动（公共前缀部分）
        While x > 0 AndAlso y > 0
            x -= 1
            y -= 1
            steps.Add(New EditStep(EditType.Equal, x, y))
        End While

        ' 反转步骤列表（因为回溯是逆序的）
        steps.Reverse()

        Return steps
    End Function

    ' ================================================================
    ' 从编辑步骤构建差异项列表
    ' ================================================================
    ''' <summary>
    ''' 将编辑步骤序列转换为差异项列表。
    ''' </summary>
    ''' <param name="offset">
    ''' 公共前缀的长度：中间部分的索引需要加上该偏移量才是原始序列的索引。
    ''' </param>
    Private Function BuildDiffItems(oldSeq As String(), newSeq As String(),
                                     editPath As List(Of EditStep),
                                     offset As Integer) As List(Of DiffItem)
        Dim items As New List(Of DiffItem)()

        For Each [step] As EditStep In editPath
            Select Case [step].Type
                Case EditType.Equal
                    items.Add(New DiffItem(EditType.Equal, offset + [step].OldIndex, offset + [step].NewIndex,
                                           oldSeq([step].OldIndex)))
                Case EditType.Delete
                    items.Add(New DiffItem(EditType.Delete, offset + [step].OldIndex, -1,
                                           oldSeq([step].OldIndex)))
                Case EditType.Insert
                    items.Add(New DiffItem(EditType.Insert, -1, offset + [step].NewIndex,
                                           newSeq([step].NewIndex)))
            End Select
        Next

        Return items
    End Function

    ' ================================================================
    ' 辅助：编辑步骤内部表示
    ' ================================================================
    ''' <summary>
    ''' 内部使用的编辑步骤记录，用于回溯阶段。
    ''' </summary>
    Private Class EditStep
        Public Property Type As EditType
        Public Property OldIndex As Integer
        Public Property NewIndex As Integer

        Public Sub New(type As EditType, oldIndex As Integer, newIndex As Integer)
            Me.Type = type
            Me.OldIndex = oldIndex
            Me.NewIndex = newIndex
        End Sub
    End Class
End Class
