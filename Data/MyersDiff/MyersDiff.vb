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
' 编辑操作类型枚举
' -----------------------------------------------------------------------
''' <summary>
''' 表示差异项的编辑操作类型。
''' </summary>
Public Enum EditType As Integer
    ''' <summary>两序列中相同的元素，无需编辑。</summary>
    Equal = 0
    ''' <summary>元素在旧序列中存在但在新序列中被删除。</summary>
    Delete = 1
    ''' <summary>元素在新序列中存在但旧序列中没有，属于插入操作。</summary>
    Insert = 2
End Enum

' -----------------------------------------------------------------------
' 差异项：记录单个编辑操作
' -----------------------------------------------------------------------
''' <summary>
''' 表示一个差异项，包含编辑类型、旧序列索引和新序列索引。
''' </summary>
Public Class DiffItem
    ''' <summary>编辑操作类型。</summary>
    Public Property Type As EditType

    ''' <summary>旧序列中的索引（从 0 开始）；对于 Insert 操作为 -1。</summary>
    Public Property OldIndex As Integer

    ''' <summary>新序列中的索引（从 0 开始）；对于 Delete 操作为 -1。</summary>
    Public Property NewIndex As Integer

    ''' <summary>涉及的元素值（行文本或字符）。</summary>
    Public Property Value As String

    Public Sub New(type As EditType, oldIndex As Integer, newIndex As Integer, value As String)
        Me.Type = type
        Me.OldIndex = oldIndex
        Me.NewIndex = newIndex
        Me.Value = value
    End Sub

    Public Overrides Function ToString() As String
        Select Case Type
            Case EditType.Equal
                Return String.Format("  {0}", Value)
            Case EditType.Delete
                Return String.Format("- {0} [old:{1}]", Value, OldIndex)
            Case EditType.Insert
                Return String.Format("+ {0} [new:{1}]", Value, NewIndex)
            Case Else
                Return Value
        End Select
    End Function
End Class

' -----------------------------------------------------------------------
' 差异块：连续相同类型差异项的分组，用于统一差异格式输出
' -----------------------------------------------------------------------
''' <summary>
''' 表示一个差异块，即连续相同类型差异项的分组。
''' </summary>
Public Class DiffBlock
    ''' <summary>该块中旧序列的起始行号（从 1 开始，用于显示）。</summary>
    Public Property OldStart As Integer

    ''' <summary>该块中旧序列的行数。</summary>
    Public Property OldCount As Integer

    ''' <summary>该块中新序列的起始行号（从 1 开始，用于显示）。</summary>
    Public Property NewStart As Integer

    ''' <summary>该块中新序列的行数。</summary>
    Public Property NewCount As Integer

    ''' <summary>该块包含的差异项列表。</summary>
    Public Property Items As New List(Of DiffItem)()

    Public Sub New(oldStart As Integer, oldCount As Integer,
                   newStart As Integer, newCount As Integer)
        Me.OldStart = oldStart
        Me.OldCount = oldCount
        Me.NewStart = newStart
        Me.NewCount = newCount
    End Sub
End Class

' -----------------------------------------------------------------------
' 差异结果：封装完整的比较结果
' -----------------------------------------------------------------------
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

        For Each step In editPath
            Select Case step.Type
                    Case EditType.Equal
                    items.Add(New DiffItem(EditType.Equal, step.OldIndex, step.NewIndex,
                                           oldSeq(step.OldIndex)))
                Case EditType.Delete
                    items.Add(New DiffItem(EditType.Delete, step.OldIndex, -1,
                                           oldSeq(step.OldIndex)))
                Case EditType.Insert
                    items.Add(New DiffItem(EditType.Insert, -1, step.NewIndex,
                                           newSeq(step.NewIndex)))
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

' -----------------------------------------------------------------------
' 便捷工具类：提供静态方法快速调用
' -----------------------------------------------------------------------
''' <summary>
''' 提供静态便捷方法的差异比较工具类。
''' </summary>
Public NotInheritable Class DiffUtils

    Private Sub New()
        ' 禁止实例化
    End Sub

    ''' <summary>
    ''' 比较两个文本文件并返回统一差异格式字符串。
    ''' </summary>
    ''' <param name="oldFilePath">旧文件路径。</param>
    ''' <param name="newFilePath">新文件路径。</param>
    ''' <param name="contextLines">上下文行数（默认 3）。</param>
    ''' <returns>统一差异格式字符串。</returns>
    Public Shared Function DiffFiles(oldFilePath As String, newFilePath As String,
                                      Optional contextLines As Integer = 3) As String
        Dim differ As New MyersDiff()
        Dim result As DiffResult = differ.CompareFiles(oldFilePath, newFilePath)
        Return result.ToUnifiedDiff(oldFilePath, newFilePath, contextLines)
    End Function

    ''' <summary>
    ''' 比较两个字符串数组并返回统一差异格式字符串。
    ''' </summary>
    Public Shared Function DiffLines(oldLines As String(), newLines As String(),
                                      Optional oldLabel As String = "a",
                                      Optional newLabel As String = "b",
                                      Optional contextLines As Integer = 3) As String
        Dim differ As New MyersDiff()
        Dim result As DiffResult = differ.Compare(oldLines, newLines)
        Return result.ToUnifiedDiff(oldLabel, newLabel, contextLines)
    End Function

    ''' <summary>
    ''' 比较两个字符串（字符级）并返回差异结果。
    ''' </summary>
    Public Shared Function DiffChars(oldText As String, newText As String) As DiffResult
        Dim differ As New MyersDiff()
        Return differ.CompareChars(oldText, newText)
    End Function

    ''' <summary>
    ''' 比较两个文本文件并返回完整的差异结果对象。
    ''' </summary>
    Public Shared Function CompareFiles(oldFilePath As String, newFilePath As String) As DiffResult
        Dim differ As New MyersDiff()
        Return differ.CompareFiles(oldFilePath, newFilePath)
    End Function

    ''' <summary>
    ''' 比较两个字符串数组并返回完整的差异结果对象。
    ''' </summary>
    Public Shared Function CompareLines(oldLines As String(), newLines As String()) As DiffResult
        Dim differ As New MyersDiff()
        Return differ.Compare(oldLines, newLines)
    End Function

End Class

End Namespace

' ============================================================================
' 演示程序（控制台应用入口）
' ============================================================================
' 以下代码展示如何使用 MyersDiff 模块。如果将此文件集成到自己的项目中，
' 可以删除此 Module。
' ============================================================================

Module MyersDiffDemo

    Sub Main()
        Console.WriteLine("============================================")
        Console.WriteLine("  Myers 差异算法模块 — 演示程序")
        Console.WriteLine("============================================")
        Console.WriteLine()

        ' ---- 演示 1：行级差异比较 ----
        DemoLineDiff()

        ' ---- 演示 2：字符级差异比较 ----
        DemoCharDiff()

        ' ---- 演示 3：文件差异比较 ----
        DemoFileDiff()

        Console.WriteLine()
        Console.WriteLine("按任意键退出...")
        Console.ReadKey()
    End Sub

    ''' <summary>
    ''' 演示行级差异比较
    ''' </summary>
    Private Sub DemoLineDiff()
        Console.WriteLine("──────────────────────────────────────")
        Console.WriteLine("  演示 1：行级差异比较")
        Console.WriteLine("──────────────────────────────────────")
        Console.WriteLine()

        Dim oldLines As String() = {
            "Imports System",
            "Imports System.Collections",
            "",
            "Public Class Calculator",
            "    Private value As Integer = 0",
            "",
            "    Public Sub New()",
            "        value = 0",
            "    End Sub",
            "",
            "    Public Function Add(x As Integer) As Integer",
            "        value += x",
            "        Return value",
            "    End Function",
            "",
            "    Public Function Subtract(x As Integer) As Integer",
            "        value -= x",
            "        Return value",
            "    End Function",
            "End Class"
        }

        Dim newLines As String() = {
            "Imports System",
            "Imports System.Collections.Generic",
            "",
            "Public Class Calculator",
            "    Private value As Integer = 0",
            "    Private history As New List(Of Integer)()",
            "",
            "    Public Sub New()",
            "        value = 0",
            "        history.Clear()",
            "    End Sub",
            "",
            "    Public Function Add(x As Integer) As Integer",
            "        value += x",
            "        history.Add(value)",
            "        Return value",
            "    End Function",
            "",
            "    Public Function Subtract(x As Integer) As Integer",
            "        value -= x",
            "        history.Add(value)",
            "        Return value",
            "    End Function",
            "",
            "    Public Function GetHistory() As List(Of Integer)",
            "        Return history",
            "    End Function",
            "",
            "End Class"
        }

        Dim differ As New MyersDiffAlgorithm.MyersDiff()
        Dim result As MyersDiffAlgorithm.DiffResult = differ.Compare(oldLines, newLines)

        ' 输出统计摘要
        Console.WriteLine(result.ToSummary())
        Console.WriteLine()

        ' 输出统一差异格式
        Console.WriteLine("统一差异格式输出:")
        Console.WriteLine(result.ToUnifiedDiff("Calculator.vb (旧)", "Calculator.vb (新)", 2))
        Console.WriteLine()

        ' 输出并排对照格式
        Console.WriteLine("并排对照格式输出:")
        Console.WriteLine(result.ToSideBySide(40))
        Console.WriteLine()
    End Sub

    ''' <summary>
    ''' 演示字符级差异比较
    ''' </summary>
    Private Sub DemoCharDiff()
        Console.WriteLine("──────────────────────────────────────")
        Console.WriteLine("  演示 2：字符级差异比较")
        Console.WriteLine("──────────────────────────────────────")
        Console.WriteLine()

        Dim oldText As String = "The quick brown fox jumps over the lazy dog."
        Dim newText As String = "The quick red fox leaps over the sleepy dog."

        Dim differ As New MyersDiffAlgorithm.MyersDiff()
        Dim result As MyersDiffAlgorithm.DiffResult = differ.CompareChars(oldText, newText)

        Console.WriteLine("旧文本: {0}", oldText)
        Console.WriteLine("新文本: {0}", newText)
        Console.WriteLine()

        ' 字符级差异输出
        Console.Write("差异: ")
        For Each item In result.Items
            Select Case item.Type
                Case MyersDiffAlgorithm.EditType.Equal
                    Console.Write(item.Value)
                Case MyersDiffAlgorithm.EditType.Delete
                    Console.Write("[-" & item.Value & "-]")
                Case MyersDiffAlgorithm.EditType.Insert
                    Console.Write("[+" & item.Value & "+]")
            End Select
        Next
        Console.WriteLine()
        Console.WriteLine()

        Console.WriteLine(result.ToSummary())
        Console.WriteLine()
    End Sub

    ''' <summary>
    ''' 演示文件差异比较
    ''' </summary>
    Private Sub DemoFileDiff()
        Console.WriteLine("──────────────────────────────────────")
        Console.WriteLine("  演示 3：文件差异比较")
        Console.WriteLine("──────────────────────────────────────")
        Console.WriteLine()

        ' 创建临时测试文件
        Dim tempDir As String = Path.GetTempPath()
        Dim oldFile As String = Path.Combine(tempDir, "myers_diff_test_old.txt")
        Dim newFile As String = Path.Combine(tempDir, "myers_diff_test_new.txt")

        Try
            ' 写入测试内容
            File.WriteAllText(oldFile,
                "第一行：这是原始文件" & vbCrLf &
                "第二行：内容保持不变" & vbCrLf &
                "第三行：这一行将被修改" & vbCrLf &
                "第四行：这一行将被删除" & vbCrLf &
                "第五行：内容保持不变" & vbCrLf,
                Encoding.UTF8)

            File.WriteAllText(newFile,
                "第一行：这是原始文件" & vbCrLf &
                "第二行：内容保持不变" & vbCrLf &
                "第三行：这一行已被修改" & vbCrLf &
                "第五行：内容保持不变" & vbCrLf &
                "新增行：这是插入的新行" & vbCrLf,
                Encoding.UTF8)

            ' 执行文件比较
            Dim diffText As String = MyersDiffAlgorithm.DiffUtils.DiffFiles(oldFile, newFile, 2)

            Console.WriteLine("旧文件: {0}", oldFile)
            Console.WriteLine("新文件: {0}", newFile)
            Console.WriteLine()
            Console.WriteLine(diffText)

            ' 获取详细结果
            Dim result As MyersDiffAlgorithm.DiffResult =
                MyersDiffAlgorithm.DiffUtils.CompareFiles(oldFile, newFile)
            Console.WriteLine(result.ToSummary())

        Finally
            ' 清理临时文件
            If File.Exists(oldFile) Then File.Delete(oldFile)
            If File.Exists(newFile) Then File.Delete(newFile)
        End Try
    End Sub

End Module
