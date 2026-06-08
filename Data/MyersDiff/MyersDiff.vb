#Region "Microsoft.VisualBasic::46d56b3c166df9607a9b952261c6cebf, Data\MyersDiff\MyersDiff.vb"

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

    '   Total Lines: 399
    '    Code Lines: 195 (48.87%)
    ' Comment Lines: 141 (35.34%)
    '    - Xml Docs: 35.46%
    ' 
    '   Blank Lines: 63 (15.79%)
    '     File Size: 16.73 KB


    ' Class MyersDiff
    ' 
    '     Function: Backtrack, BuildDiffItems, Compare, CompareChars, CompareFiles
    '               ComputeEditPath, ReadFileLines
    '     Class EditStep
    ' 
    '         Properties: NewIndex, OldIndex, Type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
' ============================================================================

Imports System.IO
Imports System.Text



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

        ' 执行 Myers 算法，获取编辑路径
        Dim editPath As List(Of EditStep) = ComputeEditPath(oldLines, newLines)

        ' 从编辑路径生成差异项
        result.Items = BuildDiffItems(oldLines, newLines, editPath)

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

        Dim oldChars As String() = New String(oldText.Length - 1) {}
        For i As Integer = 0 To oldText.Length - 1
            oldChars(i) = oldText(i).ToString()
        Next

        Dim newChars As String() = New String(newText.Length - 1) {}
        For i As Integer = 0 To newText.Length - 1
            newChars(i) = newText(i).ToString()
        Next

        Return Compare(oldChars, newChars)
    End Function

    ''' <summary>
    ''' 比较两个文本文件，返回差异结果。
    ''' </summary>
    ''' <param name="oldFilePath">旧文件路径。</param>
    ''' <param name="newFilePath">新文件路径。</param>
    ''' <param name="encoding">文件编码（默认 UTF-8）。</param>
    ''' <returns>包含所有差异项的 DiffResult 对象。</returns>
    Public Function CompareFiles(oldFilePath As String, newFilePath As String,
                                  Optional encoding As Encoding = Nothing) As DiffResult
        If encoding Is Nothing Then encoding = Encoding.UTF8

        Dim oldLines As String() = ReadFileLines(oldFilePath, encoding)
        Dim newLines As String() = ReadFileLines(newFilePath, encoding)

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
    ''' 空间复杂度: O((N+M) * D)
    ''' </summary>
    Private Function ComputeEditPath(oldSeq As String(), newSeq As String()) As List(Of EditStep)
        Dim N As Integer = oldSeq.Length
        Dim M As Integer = newSeq.Length
        Dim maxD As Integer = N + M  ' 编辑距离的上界

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

        ' trace 数组：记录每个 (d, k) 对应的最远 x 坐标，用于回溯
        Dim trace As New List(Of Integer())()

        Dim found As Boolean = False

        ' 外层循环：编辑距离 d 从 0 递增
        For d As Integer = 0 To maxD
            ' 保存当前 d 轮次的 V 数组快照
            Dim snapshot As Integer() = New Integer(2 * maxD) {}
            Array.Copy(V, snapshot, 2 * maxD + 1)
            trace.Add(snapshot)

            ' 内层循环：对角线 k 从 -d 到 d，步长 2
            For k As Integer = -d To d Step 2
                ' 确定是向下走（插入）还是向右走（删除）
                Dim x As Integer
                Dim kIdx As Integer = k + maxD

                If k = -d OrElse (k <> d AndAlso V(kIdx - 1) < V(kIdx + 1)) Then
                    ' 向下走：从对角线 k+1 的最远点向下移动（插入操作）
                    ' 选择 V(k+1) 对应的 x，因为 V(k+1) > V(k-1)
                    x = V(kIdx + 1)
                Else
                    ' 向右走：从对角线 k-1 的最远点向右移动（删除操作）
                    x = V(kIdx - 1) + 1
                End If

                Dim y As Integer = x - k

                ' 沿对角线前进（跳过相同元素）
                While x < N AndAlso y < M AndAlso oldSeq(x) = newSeq(y)
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

        ' 回溯编辑路径
        Return Backtrack(trace, oldSeq, newSeq, maxD)
    End Function

    ' ================================================================
    ' 回溯阶段：从 trace 数组还原编辑路径
    ' ================================================================
    ''' <summary>
    ''' 从算法执行过程中保存的 trace 数组回溯，还原完整的编辑步骤序列。
    ''' 
    ''' 回溯过程从终点 (N, M) 开始，逆向追踪每一步是如何到达的：
    ''' - 如果前一步在对角线 k-1 上且 x' = x-1，则为删除操作
    ''' - 如果前一步在对角线 k+1 上且 x' = x，则为插入操作
    ''' - 对角线移动（相同元素）不消耗编辑距离
    ''' </summary>
    Private Function Backtrack(trace As List(Of Integer()), oldSeq As String(),
                                newSeq As String(), maxD As Integer) As List(Of EditStep)
        Dim N As Integer = oldSeq.Length
        Dim M As Integer = newSeq.Length
        Dim steps As New List(Of EditStep)()

        Dim x As Integer = N
        Dim y As Integer = M

        ' 从最后一个 trace 开始逆向回溯
        For d As Integer = trace.Count - 1 To 1 Step -1
            Dim k As Integer = x - y
            Dim kIdx As Integer = k + maxD

            Dim prevV As Integer() = trace(d - 1)

            ' 判断前一步是从哪条对角线来的
            Dim prevX As Integer
            Dim prevK As Integer
            Dim stepType As EditType

            If k = -d Then
                ' 只能从对角线 k+1 向下走来
                prevX = prevV(kIdx + 1)
                prevK = k + 1
                stepType = EditType.Insert
            ElseIf k = d Then
                ' 只能从对角线 k-1 向右走来
                prevX = prevV(kIdx - 1)
                prevK = k - 1
                stepType = EditType.Delete
            ElseIf prevV(kIdx - 1) < prevV(kIdx + 1) Then
                ' V(k-1) < V(k+1)，从对角线 k+1 向下走
                prevX = prevV(kIdx + 1)
                prevK = k + 1
                stepType = EditType.Insert
            Else
                ' V(k-1) >= V(k+1)，从对角线 k-1 向右走
                prevX = prevV(kIdx - 1)
                prevK = k - 1
                stepType = EditType.Delete
            End If

            ' 记录对角线移动（相同元素），从 (prevX, prevX - prevK) 到 (x, y) 之前
            ' 先处理对角线部分
            Dim diagStartX As Integer = prevX
            Dim diagStartY As Integer = prevX - prevK

            ' 对角线移动（从蛇尾到蛇头）
            ' 但我们需要先处理非对角线移动（插入或删除），再处理对角线移动
            ' 实际上回溯顺序：先记录对角线移动，再记录非对角线移动

            ' 对角线移动：从 (prevX, prevX - prevK) 到蛇的起点
            ' 蛇的起点是非对角线移动后的位置
            ' 但在回溯中，我们是从终点往回走

            ' 让我重新理清逻辑：
            ' 在前向搜索中，对于编辑距离 d 和对角线 k：
            '   1. 先做非对角线移动（删除或插入），到达 (midX, midY)
            '   2. 然后沿对角线移动（相同元素），到达 (x, y)
            ' 
            ' 回溯时，我们从 (x, y) 开始：
            '   1. 先回退对角线移动，到达 (midX, midY)
            '   2. 再回退非对角线移动，到达 (prevX, prevY)

            ' midX 是非对角线移动后的 x 坐标
            Dim midX As Integer
            Dim midY As Integer

            If stepType = EditType.Delete Then
                ' 从 (prevX, prevX - prevK) 向右走一步到 (prevX + 1, prevX - prevK)
                ' 然后沿对角线走到 (x, y)
                midX = prevX + 1
                midY = prevX - prevK  ' y 不变
            Else
                ' 从 (prevX, prevX - prevK) 向下走一步到 (prevX, prevX - prevK + 1)
                ' 然后沿对角线走到 (x, y)
                midX = prevX
                midY = prevX - prevK + 1
            End If

            ' 记录对角线移动（从 (midX, midY) 到 (x, y)），逆序添加
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
            y = prevX - prevK
        Next

        ' 处理 d = 0 的情况：只有对角线移动
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
    ''' 将编辑步骤序列转换为差异项列表，合并连续的对角线移动为单个 Equal 项。
    ''' </summary>
    Private Function BuildDiffItems(oldSeq As String(), newSeq As String(),
                                     editPath As List(Of EditStep)) As List(Of DiffItem)
        Dim items As New List(Of DiffItem)()

        For Each [step] As EditStep In editPath
            Select Case [step].Type
                Case EditType.Equal
                    items.Add(New DiffItem(EditType.Equal, [step].OldIndex, [step].NewIndex,
                                           oldSeq([step].OldIndex)))
                Case EditType.Delete
                    items.Add(New DiffItem(EditType.Delete, [step].OldIndex, -1,
                                           oldSeq([step].OldIndex)))
                Case EditType.Insert
                    items.Add(New DiffItem(EditType.Insert, -1, [step].NewIndex,
                                           newSeq([step].NewIndex)))
            End Select
        Next

        ' 合并连续的相同类型操作（可选优化，使输出更紧凑）
        ' 这里我们保持原始粒度，不做合并

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

    ' ================================================================
    ' 辅助：读取文件行
    ' ================================================================
    ''' <summary>
    ''' 读取文件的所有行，保留行尾空白但去除行尾换行符。
    ''' </summary>
    Private Function ReadFileLines(filePath As String, encoding As Encoding) As String()
        If Not File.Exists(filePath) Then
            Throw New FileNotFoundException(String.Format("文件未找到: {0}", filePath), filePath)
        End If

        Dim lines As New List(Of String)()

        Using reader As New StreamReader(filePath, encoding)
            Dim line As String = reader.ReadLine()
            While line IsNot Nothing
                lines.Add(line)
                line = reader.ReadLine()
            End While
        End Using

        Return lines.ToArray()
    End Function

End Class


