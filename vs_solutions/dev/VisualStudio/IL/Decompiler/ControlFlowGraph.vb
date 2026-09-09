#Region "Microsoft.VisualBasic::b660f705d76f5eb7c3abc5a7e352fb50, vs_solutions\dev\VisualStudio\IL\Decompiler\ControlFlowGraph.vb"

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

    '   Total Lines: 549
    '    Code Lines: 374 (68.12%)
    ' Comment Lines: 64 (11.66%)
    '    - Xml Docs: 46.88%
    ' 
    '   Blank Lines: 111 (20.22%)
    '     File Size: 20.74 KB


    '     Class ControlFlowGraph
    ' 
    '         Properties: Blocks, EntryBlock
    ' 
    '         Function: BlockIdAtOffset, Build, ComputeIdomOn, Dominates, Intersect
    '                   IntersectOn, ReachableBlocks, ReversePostOrderIds
    ' 
    '         Sub: AddEdge, CollectLoopBody, ComputeDominanceFrontier, ComputeDominators, ComputeNaturalLoops
    '              ComputePostDominators, ComputePostOrder, DepthFirstPostOrder, LinkBlocks, SplitBlocks
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ---------------------------------------------------------------------------
' 控制流图：基本块切分 + 前驱/后继 + 支配树 + 支配边界 + 后支配 + 自然循环
'
' 切块规则：
'   1) 首指令必然是块的开始；
'   2) 任何跳转目标都是块的开始；
'   3) 任何跳转指令的下一条指令都是块的开始；
'   4) ret / throw 的下一条指令也切一刀（防御性，避免把死代码并进上一个块）；
'   5) try / handler / filter 的入口也切一刀。
'
' 支配树用 Cooper-Harvey-Kennedy 迭代算法；后支配把图反向并挂一个虚拟出口节点后
' 复用同一套算法。
' ---------------------------------------------------------------------------

Namespace IL

    ''' <summary>一个方法的控制流图</summary>
    Public Class ControlFlowGraph

        ''' <summary>虚拟出口节点在后支配计算图中的编号；对外一律用 -1 表示"汇聚到出口"</summary>
        Public Const VirtualExit As Integer = -1

        Private ReadOnly _blocks As New List(Of BasicBlock)
        Private ReadOnly _blockByOffset As New Dictionary(Of Integer, Integer)()

        Public ReadOnly Property Blocks As IReadOnlyList(Of BasicBlock)
            Get
                Return _blocks
            End Get
        End Property

        ''' <summary>入口块（编号固定为 0）</summary>
        Public ReadOnly Property EntryBlock As BasicBlock
            Get
                Return If(_blocks.Count = 0, Nothing, _blocks(0))
            End Get
        End Property

        Public Function BlockIdAtOffset(offset As Integer) As Integer
            Dim id As Integer = -1
            If _blockByOffset.TryGetValue(offset, id) Then Return id
            Return -1
        End Function

        ''' <summary>按 RPO 顺序返回全部可达块</summary>
        Public Function ReachableBlocks() As List(Of BasicBlock)
            Return _blocks.Where(Function(b) b.ReversePostOrder >= 0) _
                          .OrderBy(Function(b) b.ReversePostOrder).ToList()
        End Function

        ' ==================================================================
        ' 构建
        ' ==================================================================

        ''' <summary>由已解析好的方法体构建控制流图</summary>
        Public Shared Function Build(reader As MethodBodyReader) As ControlFlowGraph
            If reader Is Nothing Then Throw New ArgumentNullException(NameOf(reader))

            Dim cfg As New ControlFlowGraph()

            cfg.SplitBlocks(reader)
            cfg.LinkBlocks()

            If cfg._blocks.Count = 0 Then
                Throw New DecompileException("方法体为空，无法构建控制流图")
            End If

            cfg.ComputePostOrder()

            Return cfg
        End Function

        ''' <summary>扫描全部跳转目标，把指令序列切成基本块</summary>
        Private Sub SplitBlocks(reader As MethodBodyReader)
            Dim instrs = reader.Instructions
            If instrs Is Nothing OrElse instrs.Count = 0 Then Return

            Dim leaders As New SortedSet(Of Integer)()
            leaders.Add(instrs(0).Offset)

            For Each ins As ILInstruction In instrs
                If ins.IsBranch Then
                    For Each target As Integer In ins.BranchTargets
                        If reader.IndexByOffset(target) >= 0 Then leaders.Add(target)
                    Next

                    If ins.CanFallThrough Then leaders.Add(ins.NextOffset)
                ElseIf ins.IsReturn OrElse ins.Code.FlowControl = Reflection.Emit.FlowControl.Throw Then
                    ' ret 之后的字节可能是数据或不可达代码，切一刀避免被并入本块
                    leaders.Add(ins.NextOffset)
                End If
            Next

            For Each clause In reader.ExceptionClauses
                If reader.IndexByOffset(clause.TryOffset) >= 0 Then leaders.Add(clause.TryOffset)
                If reader.IndexByOffset(clause.HandlerOffset) >= 0 Then leaders.Add(clause.HandlerOffset)

                If clause.Flags = Reflection.ExceptionHandlingClauseOptions.Filter AndAlso
                   reader.IndexByOffset(clause.FilterOffset) >= 0 Then
                    leaders.Add(clause.FilterOffset)
                End If
            Next

            Dim leaderList = leaders _
                .Where(Function(o) reader.IndexByOffset(o) >= 0) _
                .OrderBy(Function(o) o) _
                .ToList()

            For i As Integer = 0 To leaderList.Count - 1
                Dim startIndex = reader.IndexByOffset(leaderList(i))
                Dim endIndex = If(i + 1 < leaderList.Count,
                                  reader.IndexByOffset(leaderList(i + 1)),
                                  instrs.Count)

                Dim block As New BasicBlock With {
                    .Id = _blocks.Count,
                    .Offset = leaderList(i)
                }

                For k As Integer = startIndex To endIndex - 1
                    block.Instructions.Add(instrs(k))
                Next

                _blockByOffset(block.Offset) = block.Id
                _blocks.Add(block)
            Next
        End Sub

        ''' <summary>按块末指令连边，并标记条件分支的 true/false 后继</summary>
        Private Sub LinkBlocks()
            For Each b As BasicBlock In _blocks
                Dim last = b.LastInstruction
                If last Is Nothing Then Continue For

                If last.IsReturn OrElse last.Code.FlowControl = Reflection.Emit.FlowControl.Throw Then
                    Continue For
                End If

                If last.IsSwitch Then
                    For Each t As Integer In last.BranchTargets
                        AddEdge(b, t)
                    Next
                ElseIf last.IsUnconditionalBranch Then
                    If TypeOf last.Operand Is Integer Then AddEdge(b, CInt(last.Operand))
                ElseIf last.IsConditionalBranch Then
                    If TypeOf last.Operand Is Integer Then
                        b.TrueSuccessor = BlockIdAtOffset(CInt(last.Operand))
                        b.FalseSuccessor = BlockIdAtOffset(last.NextOffset)
                        AddEdge(b, CInt(last.Operand))
                        AddEdge(b, last.NextOffset)
                    End If
                Else
                    AddEdge(b, last.NextOffset)
                End If
            Next
        End Sub

        Private Sub AddEdge(fromBlock As BasicBlock, targetOffset As Integer)
            Dim targetId = BlockIdAtOffset(targetOffset)
            If targetId < 0 Then Return

            If Not fromBlock.Successors.Contains(targetId) Then
                fromBlock.Successors.Add(targetId)
            End If

            Dim target = _blocks(targetId)

            If Not target.Predecessors.Contains(fromBlock.Id) Then
                target.Predecessors.Add(fromBlock.Id)
            End If
        End Sub

        ' ==================================================================
        ' 后序 / 反向后序
        ' ==================================================================

        Private Sub ComputePostOrder()
            Dim visited As New HashSet(Of Integer)()
            Dim order As New List(Of Integer)()

            DepthFirstPostOrder(0, visited, order)

            For i As Integer = 0 To order.Count - 1
                _blocks(order(i)).PostOrder = i
            Next

            ' RPO = 后序的逆序
            For i As Integer = 0 To order.Count - 1
                Dim id = order(order.Count - 1 - i)
                _blocks(id).ReversePostOrder = i
            Next
        End Sub

        Private Sub DepthFirstPostOrder(id As Integer, visited As HashSet(Of Integer), order As List(Of Integer))
            If visited.Contains(id) Then Return

            visited.Add(id)

            For Each s As Integer In _blocks(id).Successors
                DepthFirstPostOrder(s, visited, order)
            Next

            order.Add(id)
        End Sub

        ''' <summary>按 RPO 升序返回可达块的编号</summary>
        Public Function ReversePostOrderIds() As List(Of Integer)
            Dim ids = _blocks.Where(Function(b) b.ReversePostOrder >= 0) _
                             .OrderBy(Function(b) b.ReversePostOrder) _
                             .Select(Function(b) b.Id) _
                             .ToList()
            Return ids
        End Function

        ' ==================================================================
        ' 支配树 / 支配边界
        ' ==================================================================

        ''' <summary>
        ''' 迭代式计算直接支配节点（Cooper-Harvey-Kennedy）。
        ''' 同时填好 DominatorChildren 与 DominanceFrontier。
        ''' </summary>
        Public Sub ComputeDominators()
            Dim rpo = ReversePostOrderIds()
            If rpo.Count = 0 Then Return

            Dim rpoIndex = New Integer(_blocks.Count - 1) {}
            For i As Integer = 0 To rpo.Count - 1
                rpoIndex(rpo(i)) = i
            Next

            For Each b In _blocks
                b.ImmediateDominator = -1
                b.DominatorChildren.Clear()
                b.DominanceFrontier.Clear()
            Next

            _blocks(rpo(0)).ImmediateDominator = rpo(0)

            Dim changed As Boolean = True

            While changed
                changed = False

                For Each id As Integer In rpo
                    If id = rpo(0) Then Continue For

                    Dim b = _blocks(id)
                    Dim newIdom As Integer = -1

                    For Each p As Integer In b.Predecessors
                        If _blocks(p).ImmediateDominator < 0 Then Continue For

                        If newIdom < 0 Then
                            newIdom = p
                        Else
                            newIdom = Intersect(p, newIdom, rpoIndex)
                        End If
                    Next

                    If newIdom >= 0 AndAlso b.ImmediateDominator <> newIdom Then
                        b.ImmediateDominator = newIdom
                        changed = True
                    End If
                Next
            End While

            For Each b In _blocks
                If b.ImmediateDominator >= 0 AndAlso b.ImmediateDominator <> b.Id Then
                    _blocks(b.ImmediateDominator).DominatorChildren.Add(b.Id)
                End If
            Next

            ComputeDominanceFrontier(rpo)
        End Sub

        Private Function Intersect(a As Integer, b As Integer, rpoIndex As Integer()) As Integer
            Dim f1 = a
            Dim f2 = b

            While f1 <> f2
                While rpoIndex(f1) > rpoIndex(f2)
                    f1 = _blocks(f1).ImmediateDominator
                End While
                While rpoIndex(f2) > rpoIndex(f1)
                    f2 = _blocks(f2).ImmediateDominator
                End While
            End While

            Return f1
        End Function

        ''' <summary>
        ''' DF(X) = { Y | X 支配 Y 的某个前驱，但 X 并不严格支配 Y }
        ''' </summary>
        Private Sub ComputeDominanceFrontier(rpo As List(Of Integer))
            For Each id As Integer In rpo
                Dim b = _blocks(id)

                If b.Predecessors.Count >= 2 Then
                    For Each p As Integer In b.Predecessors
                        If _blocks(p).ImmediateDominator < 0 Then Continue For

                        Dim runner = p

                        While runner <> b.ImmediateDominator AndAlso runner >= 0
                            _blocks(runner).DominanceFrontier.Add(b.Id)
                            runner = _blocks(runner).ImmediateDominator
                        End While
                    Next
                End If
            Next
        End Sub

        ' ==================================================================
        ' 后支配
        ' ==================================================================

        ''' <summary>
        ''' 计算直接后支配节点。做法是把整张图反向，并额外挂一个"虚拟出口"节点
        ''' （所有没有后继的块都连到它），然后在反向图上跑一遍支配算法。
        ''' 结果落在 <see cref="BasicBlock.ImmediatePostDominator"/>，
        ''' 指向虚拟出口时记为 <see cref="VirtualExit"/>。
        ''' </summary>
        Public Sub ComputePostDominators()
            Dim n = _blocks.Count
            Dim exitId = n
            Dim succs As New List(Of List(Of Integer))()

            For i As Integer = 0 To n - 1
                succs.Add(New List(Of Integer)())
            Next

            ' 反向图：原图的 pred 变成 rev 图的 succ
            For i As Integer = 0 To n - 1
                For Each p As Integer In _blocks(i).Predecessors
                    succs(i).Add(p)
                Next
            Next

            ' 虚拟出口：原图中没有后继的块，在反向图中连到虚拟出口
            Dim exitSuccs As New List(Of Integer)()
            For i As Integer = 0 To n - 1
                If _blocks(i).Successors.Count = 0 Then exitSuccs.Add(i)
            Next
            succs.Add(exitSuccs)

            Dim idom = ComputeIdomOn(succs, exitId)

            For i As Integer = 0 To n - 1
                If idom(i) = exitId Then
                    _blocks(i).ImmediatePostDominator = VirtualExit
                Else
                    _blocks(i).ImmediatePostDominator = idom(i)
                End If
            Next
        End Sub

        ''' <summary>
        ''' 在给定邻接表上跑 Cooper-Harvey-Kennedy 支配算法。
        ''' </summary>
        Friend Shared Function ComputeIdomOn(succs As List(Of List(Of Integer)), entry As Integer) As Integer()
            Dim n = succs.Count
            Dim preds As New List(Of List(Of Integer))()

            For i As Integer = 0 To n - 1
                preds.Add(New List(Of Integer)())
            Next

            For i As Integer = 0 To n - 1
                For Each s As Integer In succs(i)
                    If Not preds(s).Contains(i) Then preds(s).Add(i)
                Next
            Next

            ' 反向图上的后序（以 entry 为根做 DFS），再取逆序得 RPO
            Dim visited As New HashSet(Of Integer)()
            Dim post As New List(Of Integer)()

            Dim stack As New Stack(Of Integer)()
            Dim iter As New Stack(Of Integer)()

            ' 迭代式 DFS，避免深方法体递归爆栈
            stack.Push(entry)
            iter.Push(0)

            While stack.Count > 0
                Dim top = stack.Peek()
                Dim idx = iter.Pop()

                If idx = 0 AndAlso Not visited.Contains(top) Then
                    visited.Add(top)
                End If

                If idx < succs(top).Count Then
                    iter.Push(idx + 1)
                    Dim nxt = succs(top)(idx)
                    If Not visited.Contains(nxt) Then
                        stack.Push(nxt)
                        iter.Push(0)
                    End If
                Else
                    post.Add(top)
                    stack.Pop()
                End If
            End While

            Dim rpo As New List(Of Integer)()
            For i As Integer = post.Count - 1 To 0 Step -1
                rpo.Add(post(i))
            Next

            Dim rpoIndex = New Integer(n - 1) {}
            For i As Integer = 0 To n - 1
                rpoIndex(i) = -1
            Next
            For i As Integer = 0 To rpo.Count - 1
                rpoIndex(rpo(i)) = i
            Next

            Dim idom = New Integer(n - 1) {}
            For i As Integer = 0 To n - 1
                idom(i) = -1
            Next

            If rpo.Count = 0 Then Return idom

            idom(entry) = entry

            Dim changed As Boolean = True

            While changed
                changed = False

                For Each id As Integer In rpo
                    If id = entry Then Continue For

                    Dim newIdom As Integer = -1

                    For Each p As Integer In preds(id)
                        If idom(p) < 0 Then Continue For

                        If newIdom < 0 Then
                            newIdom = p
                        Else
                            newIdom = IntersectOn(p, newIdom, idom, rpoIndex)
                        End If
                    Next

                    If newIdom >= 0 AndAlso idom(id) <> newIdom Then
                        idom(id) = newIdom
                        changed = True
                    End If
                Next
            End While

            Return idom
        End Function

        Private Shared Function IntersectOn(a As Integer, b As Integer, idom As Integer(), rpoIndex As Integer()) As Integer
            Dim f1 = a
            Dim f2 = b

            While f1 <> f2
                While rpoIndex(f1) > rpoIndex(f2)
                    f1 = idom(f1)
                End While
                While rpoIndex(f2) > rpoIndex(f1)
                    f2 = idom(f2)
                End While
            End While

            Return f1
        End Function

        ' ==================================================================
        ' 自然循环
        ' ==================================================================

        ''' <summary>
        ''' 识别回边（A -> B 且 B 支配 A）并由此算出自然循环：
        ''' header = B，latch = A，body = 所有"不经过 B 就能到达 A"的块 ∪ {B}。
        ''' 同时给每个循环头填 LoopFollow（header 的后继中不在循环体内的那个）。
        ''' </summary>
        Public Sub ComputeNaturalLoops()
            For Each b As BasicBlock In _blocks
                b.IsLoopHeader = False
                b.LoopLatch = -1
                b.LoopFollow = -1
                b.LoopBody.Clear()
            Next

            For Each a As BasicBlock In _blocks
                If a.ReversePostOrder < 0 Then Continue For

                For Each succ As Integer In a.Successors
                    If Dominates(succ, a.Id) Then
                        Dim header = succ
                        Dim hb = _blocks(header)

                        hb.IsLoopHeader = True
                        hb.LoopLatch = a.Id

                        Dim body As New HashSet(Of Integer)()
                        body.Add(header)
                        CollectLoopBody(a.Id, header, body)
                        hb.LoopBody = body

                        For Each s As Integer In hb.Successors
                            If Not body.Contains(s) Then
                                hb.LoopFollow = s
                                Exit For
                            End If
                        Next
                    End If
                Next
            Next
        End Sub

        Private Sub CollectLoopBody(node As Integer, header As Integer, body As HashSet(Of Integer))
            If node = header Then Return
            If Not body.Add(node) Then Return

            For Each p As Integer In _blocks(node).Predecessors
                CollectLoopBody(p, header, body)
            Next
        End Sub

        ''' <summary>a 是否支配 b（含 a = b）</summary>
        Public Function Dominates(a As Integer, b As Integer) As Boolean
            If a < 0 OrElse b < 0 Then Return False

            Dim runner = b
            Dim guard As Integer = 0

            While runner >= 0
                If runner = a Then Return True
                If runner = _blocks(runner).ImmediateDominator Then Exit While

                runner = _blocks(runner).ImmediateDominator
                guard += 1

                If guard > _blocks.Count + 1 Then Exit While
            End While

            Return False
        End Function
    End Class
End Namespace

