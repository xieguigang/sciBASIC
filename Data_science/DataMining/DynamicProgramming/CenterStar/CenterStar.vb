#Region "Microsoft.VisualBasic::34285ea8a7eda68ca35e950b25a852bd, Data_science\DataMining\DynamicProgramming\CenterStar\CenterStar.vb"

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

    '   Total Lines: 267
    '    Code Lines: 162 (60.67%)
    ' Comment Lines: 64 (23.97%)
    '    - Xml Docs: 67.19%
    ' 
    '   Blank Lines: 41 (15.36%)
    '     File Size: 9.70 KB


    ' Class CenterStar
    ' 
    '     Properties: NameList
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    '     Function: BuildCenterRow, BuildSequenceRow, CalculateTotalCost, Compute, computeInternal
    ' 
    '     Sub: AlignToCenter, ExtractCenterGaps, FindStarIndex, FindStarIndexExact, FindStarIndexSampled
    '          GetReferenceSet, MultipleAlignment, Tick, ValidateAlignment
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Diagnostics
Imports System.Linq
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar.Tqdm
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.DynamicProgramming
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports std = System.Math

''' <summary>
''' ##### Multiple-sequence-alignment
''' 
''' This program calculates the multiple sequence alignment of k>1 DNA sequences.
''' 
''' The program use the Matrix.txt file For the substitution matrix. The matrix 
''' can be changed, And it used With Default values As: 
''' 
''' + 0 - Match
''' + 1 - Missmatch
''' + 2 - Indel
''' 
''' Algorithm used For this purpose Is Center Star Algotrithm
''' 
''' > https://github.com/EranCohenSW/Multiple-sequence-alignment/blob/master/Project/src/CenterStar.java
''' </summary>
''' <remarks>
''' 实现上分为三个阶段，每个阶段都可以独立并行：
''' 
''' 1. 中心序列选择：选出到其余序列编辑距离之和最小的那一条作为中心（星）序列。
'''    序列条数不超过 <see cref="DefaultExactCenterLimit"/> 时走精确的 O(n^2) 全两两比对，
'''    超过之后退化为确定性的采样近似计算，复杂度降低到 O(n*m)。
''' 2. 空格列合并：先用「原始中心序列」与其余各序列并行两两比对，再把各次比对在中心
'''    序列上诱导出的空格列按照中心坐标一次性合并为等长的对齐矩阵，复杂度 O(n*L)。
''' 3. SP 计分：按列聚合字符频度求和，复杂度 O(n*L + L*A^2)，A 为观测到的字符集大小。
''' </remarks>
Public Class CenterStar

    Dim starIndex%
    Dim multipleAlign$()
    Dim sequence$()
    Dim names$()
    Dim kband As KBandSearch
    Dim editScores As Integer()
    ''' <summary>
    ''' 序列条数不超过该值时使用精确的全两两比对挑选中心序列
    ''' </summary>
    Dim exactCenterLimit%

    ''' <summary>
    ''' 中心的挑选退化为采样近似计算的默认序列条数阈值
    ''' </summary>
    ''' <returns></returns>
    Public Const DefaultExactCenterLimit As Integer = 32

    Public ReadOnly Property NameList As String()
        Get
            Return names
        End Get
    End Property

    Public Const GapChar As Char = "-"c

    ''' <summary>
    ''' create a new multiple sequence alignment model
    ''' </summary>
    ''' <param name="input">the sequence set that should be aligned</param>
    ''' <param name="kband">the k-band width of the internal pairwise alignment</param>
    ''' <param name="exactCenterLimit">
    ''' 序列条数不超过该值时使用精确的全两两比对挑选中心序列，超过之后改用确定性的
    ''' 采样近似计算以提升性能，结果的中心序列可能与精确计算不一致。
    ''' </param>
    Sub New(input As IEnumerable(Of NamedValue(Of String)),
            Optional kband As Integer = 32,
            Optional exactCenterLimit As Integer = DefaultExactCenterLimit)

        With input.ToArray
            sequence = .Select(Function(fa) fa.Value) _
                       .ToArray
            names = .Select(Function(fa) fa.Name) _
                    .ToArray
        End With

        Me.exactCenterLimit = If(exactCenterLimit > 0, exactCenterLimit, DefaultExactCenterLimit)
        Me.kband = New KBandSearch(globalAlign:=New String(2) {}, kband)

        If sequence.Length > 0 Then
            Me.editScores = New Integer(sequence.Length - 1) {}
        Else
            Me.editScores = New Integer() {}
        End If
    End Sub

    ''' <summary>
    ''' auto encode sequence with title in format seq_id
    ''' </summary>
    ''' <param name="input"></param>
    ''' <param name="kband"></param>
    ''' <param name="exactCenterLimit"></param>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Sub New(input As IEnumerable(Of String),
            Optional kband As Integer = 32,
            Optional exactCenterLimit As Integer = DefaultExactCenterLimit)

        Call Me.New(input.Select(Function(seq, i) New NamedValue(Of String)($"seq{i + 1}", seq)), kband, exactCenterLimit)
    End Sub

    ''' <summary>
    ''' Main
    ''' </summary>
    ''' <param name="matrix">得分矩阵</param>
    ''' <returns></returns>
    Public Function Compute(matrix As IScore(Of Char),
                            ByRef alignment As String(),
                            ByRef Optional edits As Integer() = Nothing) As Double
        Dim totalCost#
        Dim n As Integer = sequence.Length

        If n = 0 Then
            alignment = New String() {}
            edits = New Integer() {}

            Return 0
        End If
        If n = 1 Then
            ' 只有一条序列时不存在任何需要比对的列，SP 得分为 0
            alignment = New String() {sequence(Scan0)}
            edits = New Integer() {0}

            Return 0
        End If

        If sequence.All(Function(s) s = sequence(Scan0)) Then
            ' 所输入的序列全部都是一样的？？
            alignment = sequence.ToArray
            totalCost = 0
            edits = New Integer(alignment.Length - 1) {}
        Else
            totalCost = computeInternal(matrix)
            edits = editScores.ToArray
            alignment = multipleAlign.ToArray
        End If

        Return totalCost
    End Function

    Private Function computeInternal(matrix As IScore(Of Char)) As Double
        Dim n As Integer = sequence.Length

        multipleAlign = New String(n - 1) {}

        Call FindStarIndex(n)
        Call MultipleAlignment(n)

        Return calculateTotalCost(matrix, n)
    End Function

    ''' <summary>
    ''' this Function calculate() the total cost
    ''' </summary>
    ''' <returns></returns>
    ''' 
    ''' <remarks>
    ''' 一列的总得分为该列所有序列对的得分之和。由于得分只取决于列内的字符多重集，
    ''' 因此可以先统计字符频度再按字符对聚合，把复杂度从 O(n^2 * L) 降低到 O(n*L + L*A^2)。
    ''' 仅在得分函数于观测字符集上对称时才能走这条快速路径，否则回退到逐对求和。
    ''' </remarks>
    Private Function calculateTotalCost(matrix As IScore(Of Char), n%) As Double
        Dim length As Integer = multipleAlign(Scan0).Length
        Dim rows As Char()() = New Char(n - 1)() {}

        For i As Integer = 0 To n - 1
            rows(i) = multipleAlign(i).ToCharArray()
        Next

        ' 1. 建立字符索引，并惰性填充字符对得分查表
        '    把 O(n^2 * L) 次的接口调用降低为 O(A^2) 次
        Dim index As New Dictionary(Of Char, Integer)()

        For i As Integer = 0 To n - 1
            For Each c As Char In rows(i)
                If Not index.ContainsKey(c) Then
                    index.Add(c, index.Count)
                End If
            Next
        Next

        Dim a As Integer = index.Count

        If a = 0 Then
            Return 0
        End If

        Dim chars As Char() = index.Keys.ToArray()
        Dim scoreTable As Double() = New Double(a * a - 1) {}

        For x As Integer = 0 To a - 1
            For y As Integer = 0 To a - 1
                scoreTable(x * a + y) = matrix.GetSimilarityScore(chars(x), chars(y))
            Next
        Next

        ' 2. 对称性守卫：非对称的得分函数无法用字符频度聚合，回退到逐对求和
        For x As Integer = 0 To a - 1
            For y As Integer = x + 1 To a - 1
                If scoreTable(x * a + y) <> scoreTable(y * a + x) Then
                    Return SumByPairs(rows, index, scoreTable, a, length, n)
                End If
            Next
        Next

        Return SumByColumnFrequency(rows, index, scoreTable, a, length, n)
    End Function

    ''' <summary>
    ''' 按列聚合字符频度计算 SP 得分，复杂度 O(n*L + L*A^2)
    ''' </summary>
    Private Shared Function SumByColumnFrequency(rows As Char()(),
                                                 index As Dictionary(Of Char, Integer),
                                                 scoreTable As Double(),
                                                 a As Integer,
                                                 length As Integer,
                                                 n As Integer) As Double
        Dim counts As Integer() = New Integer(length * a - 1) {}

        For i As Integer = 0 To n - 1
            Dim row As Char() = rows(i)

            For k As Integer = 0 To length - 1
                counts(k * a + index(row(k))) += 1
            Next
        Next

        Dim total# = 0

        For k As Integer = 0 To length - 1
            Dim offset As Integer = k * a

            For x As Integer = 0 To a - 1
                Dim cx As Integer = counts(offset + x)

                If cx = 0 Then
                    Continue For
                End If

                ' 同字符之间的配对数为 C(cx, 2)
                total += CDbl(cx) * (cx - 1) / 2.0R * scoreTable(x * a + x)

                For y As Integer = x + 1 To a - 1
                    Dim cy As Integer = counts(offset + y)

                    If cy > 0 Then
                        total += CDbl(cx) * cy * scoreTable(x * a + y)
                    End If
                Next
            Next
        Next

        Return total
    End Function

    ''' <summary>
    ''' 非对称得分函数的回退路径：按序列对逐列求和，复杂度 O(n^2 * L)，
    ''' 但得分全部来自查表，不再重复调用 <see cref="IScore(Of T).GetSimilarityScore"/>。
    ''' </summary>
    Private Shared Function SumByPairs(rows As Char()(),
                                       index As Dictionary(Of Char, Integer),
                                       scoreTable As Double(),
                                       a As Integer,
                                       length As Integer,
                                       n As Integer) As Double
        Dim codes As Integer()() = New Integer(n - 1)() {}

        For i As Integer = 0 To n - 1
            codes(i) = New Integer(length - 1) {}

            For k As Integer = 0 To length - 1
                codes(i)(k) = index(rows(i)(k))
            Next
        Next

        Dim total# = 0

        For i As Integer = 0 To n - 1
            Dim ci As Integer() = codes(i)

            For j As Integer = i + 1 To n - 1
                Dim cj As Integer() = codes(j)

                For k As Integer = 0 To length - 1
                    total += scoreTable(ci(k) * a + cj(k))
                Next
            Next
        Next

        Return total
    End Function

    ''' <summary>
    ''' 某一条序列与中心序列两两比对之后，在中心坐标上诱导出的空格列表。
    ''' </summary>
    Private Structure CenterGaps

        ''' <summary>
        ''' 中心坐标 p，升序且无重复，取值区间为 [0, |Sc|]，|Sc| 表示中心序列末尾
        ''' </summary>
        Friend positions As Integer()
        ''' <summary>
        ''' 与 <see cref="positions"/> 一一对应的插入字符数 g(p)，恒大于 0
        ''' </summary>
        Friend counts As Integer()
        ''' <summary>
        ''' 比对之后的序列 A_i，与比对之后的中心序列 C_i 等长
        ''' </summary>
        Friend aligned As String
        ''' <summary>
        ''' 该次两两比对的编辑距离
        ''' </summary>
        Friend editDistance As Integer

    End Structure

    ''' <summary>
    ''' The Function do the multiple alignment according to the center string 
    ''' </summary>
    ''' <remarks>
    ''' 经典的中心星定义：先用「原始中心序列」Sc 与其余各序列并行两两比对，
    ''' 再把所有比对在 Sc 上诱导出的空格列按中心坐标合并。
    ''' 
    ''' 合并之后每个中心坐标 p 对应一个列块，块内依次为 gapCount(p) 个插入列
    ''' 与 1 个中心列（p = |Sc| 时只有插入列）。不同序列的插入字符被分配到块内
    ''' 互不重叠的槽位上，因此不会出现两个真实残基被挤进同一列的情况。
    ''' </remarks>
    Private Sub MultipleAlignment(n As Integer)
        Dim center As String = sequence(starIndex)
        Dim clen As Integer = center.Length
        Dim gaps As CenterGaps() = New CenterGaps(n - 1) {}

        ' 1. 并行完成「原始中心序列」与其余各序列的两两比对
        Call AlignToCenter(n, center, clen, gaps)

        ' 2. 汇总每个中心坐标上需要插入的空格列数量
        Dim gapCount As Integer() = New Integer(clen) {}
        Dim totalLen As Integer = clen

        For i As Integer = 0 To n - 1
            If i = starIndex Then
                Continue For
            End If

            Dim g As CenterGaps = gaps(i)

            For t As Integer = 0 To g.positions.Length - 1
                gapCount(g.positions(t)) += g.counts(t)
            Next
        Next

        For p As Integer = 0 To clen
            totalLen += gapCount(p)
        Next

        ' 3. 为每条序列计算其插入字符在块内的起始槽位，使不同序列的插入列互不重叠
        Dim slots As Integer()() = New Integer(n - 1)() {}
        Dim acc As Integer() = New Integer(clen) {}

        For i As Integer = 0 To n - 1
            If i = starIndex Then
                Continue For
            End If

            Dim g As CenterGaps = gaps(i)
            Dim slot As Integer() = New Integer(g.positions.Length - 1) {}

            For t As Integer = 0 To g.positions.Length - 1
                Dim p As Integer = g.positions(t)

                slot(t) = acc(p)
                acc(p) += g.counts(t)
            Next

            slots(i) = slot
        Next

        ' 4. 并行构造每一行
        Dim result As String() = New String(n - 1) {}

        Call System.Threading.Tasks.Parallel.For(0, n,
            Sub(i)
                If i = starIndex Then
                    result(i) = BuildCenterRow(center, gapCount, totalLen)
                Else
                    result(i) = BuildSequenceRow(gaps(i), slots(i), gapCount, clen, totalLen)
                End If
            End Sub)

        multipleAlign = result
        editScores(starIndex) = 0

        For i As Integer = 0 To n - 1
            If i <> starIndex Then
                editScores(i) = gaps(i).editDistance
            End If
        Next

        Call ValidateAlignment(totalLen)
    End Sub

    ''' <summary>
    ''' 并行完成「原始中心序列」与其余各序列的两两比对
    ''' </summary>
    Private Sub AlignToCenter(n As Integer, center As String, clen As Integer, gaps As CenterGaps())
        Dim k As Integer = Me.kband.K
        Dim bar As ProgressBar = TqdmWrapper.Wrap(n, printsPerSecond:=4)
        Dim ticks As Integer() = New Integer() {0}

        ' KBandSearch 持有共享的输出缓冲区，不是线程安全的，
        ' 每个并行分支必须使用各自的实例
        Call System.Threading.Tasks.Parallel.For(0, n,
            Sub(i)
                If i = starIndex Then
                    Return
                End If

                Dim kband As New KBandSearch(globalAlign:=New String(2) {}, k)
                Dim dist As Integer = kband.CalculateEditDistance(center, sequence(i))

                gaps(i) = ExtractCenterGaps(kband.globalAlign(0), kband.globalAlign(1), clen, dist)

                Call Tick(bar, ticks, n, names(i))
            End Sub)

        Call bar.Finish()
    End Sub

    ''' <summary>
    ''' 从中心序列的比对结果中抽出「中心坐标 -> 插入字符数」的稀疏表
    ''' </summary>
    ''' <param name="alignCenter">C_i，比对之后的中心序列</param>
    ''' <param name="alignSeq">A_i，比对之后的第 i 条序列，与 C_i 等长</param>
    ''' <param name="clen">原始中心序列的长度 |Sc|</param>
    ''' <param name="dist">该次两两比对的编辑距离</param>
    ''' <returns></returns>
    ''' <remarks>
    ''' 不变量：C_i 去掉所有 <see cref="GapChar"/> 之后恒等于原始中心序列 Sc，
    ''' 因此按中心坐标顺序扫描一次即可得到全部插入位置。
    ''' </remarks>
    Private Shared Function ExtractCenterGaps(alignCenter As String, alignSeq As String, clen As Integer, dist As Integer) As CenterGaps
        Dim positions As New List(Of Integer)()
        Dim counts As New List(Of Integer)()
        Dim p As Integer = 0
        Dim run As Integer = 0

        For col As Integer = 0 To alignCenter.Length - 1
            If alignCenter(col) = GapChar Then
                run += 1
            Else
                If run > 0 Then
                    positions.Add(p)
                    counts.Add(run)
                    run = 0
                End If

                p += 1
            End If
        Next

        ' 中心序列末尾剩余的空格属于中心坐标 clen
        If run > 0 Then
            positions.Add(clen)
            counts.Add(run)
        End If

        Return New CenterGaps With {
            .positions = positions.ToArray(),
            .counts = counts.ToArray(),
            .aligned = alignSeq,
            .editDistance = dist
        }
    End Function

    ''' <summary>
    ''' 构造中心序列所在的那一行：每个中心坐标 p 上先放 gapCount(p) 个空格，
    ''' 再放中心序列在坐标 p 上的那个字符
    ''' </summary>
    Private Shared Function BuildCenterRow(center As String, gapCount As Integer(), totalLen As Integer) As String
        Dim sb As New StringBuilder(totalLen)

        For p As Integer = 0 To center.Length
            For t As Integer = 1 To gapCount(p)
                sb.Append(GapChar)
            Next
            If p < center.Length Then
                sb.Append(center(p))
            End If
        Next

        Return sb.ToString()
    End Function

    ''' <summary>
    ''' 构造第 i 条序列所在的那一行
    ''' </summary>
    ''' <param name="g">该序列与中心序列的比对结果</param>
    ''' <param name="slot">该序列在每个插入位置上的起始槽位</param>
    ''' <param name="gapCount">每个中心坐标上的插入列总数</param>
    ''' <param name="clen">原始中心序列的长度 |Sc|</param>
    ''' <param name="totalLen">对齐之后的总列数</param>
    ''' <returns></returns>
    Private Shared Function BuildSequenceRow(g As CenterGaps, slot As Integer(),
                                             gapCount As Integer(),
                                             clen As Integer, totalLen As Integer) As String
        Dim sb As New StringBuilder(totalLen)
        Dim src As Integer = 0
        Dim t As Integer = 0

        For p As Integer = 0 To clen
            Dim own As Integer = 0
            Dim start As Integer = 0

            If t < g.positions.Length AndAlso g.positions(t) = p Then
                own = g.counts(t)
                start = slot(t)
                t += 1
            End If

            ' 块内起始槽位之前：其它序列占用的插入列
            For k As Integer = 1 To start
                sb.Append(GapChar)
            Next

            ' 本序列自己插入的字符
            For k As Integer = 1 To own
                sb.Append(g.aligned(src))
                src += 1
            Next

            ' 块内剩余的插入列
            For k As Integer = 1 To (gapCount(p) - start - own)
                sb.Append(GapChar)
            Next

            ' 中心坐标 p 上的那一列
            If p < clen Then
                sb.Append(g.aligned(src))
                src += 1
            End If
        Next

        Return sb.ToString()
    End Function

    ''' <summary>
    ''' 校验不变量：所有行等长，且每一行去掉空格之后必须还原为对应的输入序列。
    ''' 仅在 DEBUG 构建下被调用，Release 构建下不产生任何开销。
    ''' </summary>
    <Conditional("DEBUG")>
    Private Sub ValidateAlignment(expectedLen As Integer)
        Dim n As Integer = multipleAlign.Length
        Dim len As Integer = multipleAlign(Scan0).Length

        System.Diagnostics.Debug.Assert(len = expectedLen, $"alignment length {len} is not the expected {expectedLen}")

        For i As Integer = 0 To n - 1
            System.Diagnostics.Debug.Assert(multipleAlign(i).Length = len, $"row #{i} length {multipleAlign(i).Length} is not {len}")

            Dim raw As String = New String(multipleAlign(i).Where(Function(c) c <> GapChar).ToArray)

            System.Diagnostics.Debug.Assert(raw = sequence(i), $"row #{i} '{names(i)}' does not restore its input sequence")
        Next
    End Sub

    ''' <summary>
    ''' 在并行循环中安全地推进进度条
    ''' </summary>
    Private Shared Sub Tick(bar As ProgressBar, counter As Integer(), total As Integer, label As String)
        Dim done As Integer = Interlocked.Increment(counter(Scan0))

        SyncLock bar
            Call bar.SetLabel(label)
            Call bar.Progress(done, total)
        End SyncLock
    End Sub

    ''' <summary>
    ''' This Function finds the minimum star cost from all sequences
    ''' </summary>
    Private Sub FindStarIndex(n As Integer)
        Dim scores As Double() = New Double(n - 1) {}
        Dim k As Integer = Me.kband.K

        If n <= exactCenterLimit Then
            Call FindStarIndexExact(n, k, scores)
        Else
            Call FindStarIndexSampled(n, k, scores)
        End If

        ' use the index of min score as the star index
        starIndex = which.Min(scores)

        Call VBDebugger.EchoLine($"use [#{starIndex + 1}]{names(starIndex)} sequence as the start center sequence for make alignment!")
    End Sub

    ''' <summary>
    ''' 精确计算每一条序列到其余所有序列的编辑距离之和，需要 O(n^2) 次两两比对
    ''' </summary>
    Private Sub FindStarIndexExact(n As Integer, k As Integer, scores As Double())
        Call System.Threading.Tasks.Parallel.For(0, n,
            Sub(i)
                Dim kband As New KBandSearch(globalAlign:=New String(2) {}, k)
                Dim editDist As Integer = 0

                For j As Integer = 0 To n - 1
                    If j <> i Then
                        ' 编辑距离是对称的，反向的那一次计算纯属浪费
                        editDist += kband.CalculateEditDistance(sequence(i), sequence(j))
                    End If
                Next

                scores(i) = editDist
            End Sub)
    End Sub

    ''' <summary>
    ''' 借助一组确定性的参考序列近似估计每一条序列的中心代价，只需要 O(n*m) 次两两比对
    ''' </summary>
    Private Sub FindStarIndexSampled(n As Integer, k As Integer, scores As Double())
        Dim refs As Integer() = GetReferenceSet(n)

        Call System.Threading.Tasks.Parallel.For(0, n,
            Sub(i)
                Dim kband As New KBandSearch(globalAlign:=New String(2) {}, k)
                Dim editDist As Integer = 0
                Dim count As Integer = 0

                For Each r As Integer In refs
                    If r <> i Then
                        editDist += kband.CalculateEditDistance(sequence(i), sequence(r))
                        count += 1
                    End If
                Next

                If count = 0 Then
                    scores(i) = 0
                Else
                    ' 归一化到 n-1 个距离项，抵消参考序列自身被跳过所带来的偏差
                    scores(i) = editDist * (n - 1) / count
                End If
            End Sub)
    End Sub

    ''' <summary>
    ''' 构造确定性的参考序列集：长度中位数所在的序列 + 按索引等距抽样。
    ''' 不使用随机数，保证同一份输入多次运行得到的结果完全一致。
    ''' </summary>
    Private Function GetReferenceSet(n As Integer) As Integer()
        Dim byLength As Integer() = Enumerable.Range(0, n) _
            .OrderBy(Function(i) sequence(i).Length) _
            .ThenBy(Function(i) i) _
            .ToArray
        Dim m As Integer = std.Min(n, std.Max(16, CInt(std.Sqrt(n)) * 2))
        Dim refs As New List(Of Integer)(m + 1)
        Dim seen As New HashSet(Of Integer)()

        ' 长度居中的序列通常就是不错的中心候选
        Dim median As Integer = byLength(n \ 2)

        refs.Add(median)
        seen.Add(median)

        For t As Integer = 0 To m - 1
            Dim idx As Integer = CInt(std.Floor(CDbl(t) * n / m))

            If seen.Add(idx) Then
                refs.Add(idx)
            End If
        Next

        Return refs.ToArray()
    End Function
End Class
