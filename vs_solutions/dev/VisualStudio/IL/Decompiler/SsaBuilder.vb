#Region "Microsoft.VisualBasic::99446bc19a6976348fd08f69eb8678a1, vs_solutions\dev\VisualStudio\IL\Decompiler\SsaBuilder.vb"

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

    '   Total Lines: 304
    '    Code Lines: 187 (61.51%)
    ' Comment Lines: 55 (18.09%)
    '    - Xml Docs: 34.55%
    ' 
    '   Blank Lines: 62 (20.39%)
    '     File Size: 12.65 KB


    '     Class SsaBuilder
    ' 
    '         Properties: BaseNames, DefNames, Phis, SlotTypes, UseNames
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ArgKey, BaseNameOf, Build, CurrentName, FreshName
    '                   LocalKey, ParameterBaseName, TypeOfSlot
    ' 
    '         Sub: AddDefBlock, AddPhi, PlacePhis, RegisterSlots, Rename
    '              RenameBlock
    '         Class PhiInfo
    ' 
    '             Properties: Args, BlockId, Name, SlotKey, VarType
    ' 
    '             Function: ToString
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ---------------------------------------------------------------------------
' SSA（静态单赋值）构造
'
' 只有"参数"与"局部变量"参与 SSA；求值栈槽位不参与（结构化 IL 中跨块残留栈值
' 只可能来自短路布尔，由栈模拟单独处理）。
'
' 流程是标准的 Cytron 式三趟：
'   1) 收集每个变量槽的定义块；
'   2) 在迭代支配边界（iterated dominance frontier）上放置 phi；
'   3) 沿支配树做前序重命名：进入块时把 phi 名字设为当前值，遇定义生成新名字，
'      块结束时用当前名字回填各个后继 phi 的入边参数。
'
' 重命名结果以"指令偏移 -> 名字"两张表（UseNames / DefNames）交给栈模拟消费，
' 因此栈模拟不必再维护符号化的变量环境。
'
' phi 的消解不在这一趟做：本趟只给出名字与入边，真正的赋值插入由栈模拟在
' "前驱块末尾"完成，结构化还原时表现为 if 之前的声明 + 各分支末尾的赋值。
' ---------------------------------------------------------------------------

Imports System.Reflection

Namespace IL

    Public Class SsaBuilder

        ''' <summary>一个 SSA phi 节点</summary>
        Public Class PhiInfo
            ''' <summary>phi 所在的块</summary>
            Public Property BlockId As Integer
            ''' <summary>变量槽键：形参 "a&lt;n&gt;"，局部 "l&lt;n&gt;"</summary>
            Public Property SlotKey As String
            ''' <summary>该 phi 定义的名字（SSA 唯一）</summary>
            Public Property Name As String
            ''' <summary>变量类型（用于生成声明）</summary>
            Public Property VarType As System.Type
            ''' <summary>入边：前驱块编号 -> 该前驱结束时变量的名字</summary>
            Public Property Args As New Dictionary(Of Integer, String)

            Public Overrides Function ToString() As String
                Dim text = String.Join(", ", Args.Select(Function(kv) $"bb{kv.Key}:{kv.Value}"))
                Return $"{Name} = phi({text})"
            End Function
        End Class

        ''' <summary>块编号 -> 该块开头的 phi 列表</summary>
        Public ReadOnly Property Phis As New Dictionary(Of Integer, List(Of PhiInfo))()
        ''' <summary>指令偏移 -> 该处读取变量时应使用的名字</summary>
        Public ReadOnly Property UseNames As New Dictionary(Of Integer, String)()
        ''' <summary>指令偏移 -> 该处定义变量时产生的新名字</summary>
        Public ReadOnly Property DefNames As New Dictionary(Of Integer, String)()

        ''' <summary>槽键 -> 基础名（未加版本号）</summary>
        Public ReadOnly Property BaseNames As New Dictionary(Of String, String)()
        ''' <summary>槽键 -> 静态类型</summary>
        Public ReadOnly Property SlotTypes As New Dictionary(Of String, System.Type)()

        Private ReadOnly _cfg As ControlFlowGraph
        Private ReadOnly _counters As New Dictionary(Of String, Integer)()

        Private Sub New(cfg As ControlFlowGraph)
            _cfg = cfg
        End Sub

        ' ==================================================================
        ' 对外入口
        ' ==================================================================

        ''' <summary>在给定 CFG 上构造 SSA（要求已先算好支配树）</summary>
        Public Shared Function Build(cfg As ControlFlowGraph,
                                     method As MethodInfo,
                                     reader As MethodBodyReader) As SsaBuilder
            If cfg Is Nothing Then Throw New ArgumentNullException(NameOf(cfg))
            If method Is Nothing Then Throw New ArgumentNullException(NameOf(method))
            If reader Is Nothing Then Throw New ArgumentNullException(NameOf(reader))

            Dim ssa As New SsaBuilder(cfg)

            ssa.RegisterSlots(method, reader)
            ssa.PlacePhis()
            ssa.Rename()

            Return ssa
        End Function

        ''' <summary>形参槽键（index 为 ldarg / starg 的下标）</summary>
        Public Shared Function ArgKey(index As Integer) As String
            Return "a" & index
        End Function

        ''' <summary>局部变量槽键（index 为 ldloc / stloc 的下标）</summary>
        Public Shared Function LocalKey(index As Integer) As String
            Return "l" & index
        End Function

        ''' <summary>
        ''' 形参在 AST 里使用的名字。
        ''' 加 p_ 前缀是为了避免与局部变量（V_n）撞名，下游（CUDA 发射器）依赖这个约定。
        ''' </summary>
        Public Shared Function ParameterBaseName(parameterName As String) As String
            If String.IsNullOrEmpty(parameterName) Then Return "p_arg"
            Return "p_" & parameterName
        End Function

        ''' <summary>取槽的静态类型；未知时返回 Nothing</summary>
        Public Function TypeOfSlot(key As String) As System.Type
            Dim t As System.Type = Nothing
            If SlotTypes.TryGetValue(key, t) Then Return t
            Return Nothing
        End Function

        Public Function BaseNameOf(key As String) As String
            Dim n As String = Nothing
            If BaseNames.TryGetValue(key, n) Then Return n
            Return "t_" & key
        End Function

        ' ==================================================================
        ' 第 0 趟：登记变量槽
        ' ==================================================================

        Private Sub RegisterSlots(method As MethodInfo, reader As MethodBodyReader)
            Dim argOffset = If(method.IsStatic, 0, 1)
            Dim parameters = method.GetParameters()

            If Not method.IsStatic Then
                ' ldarg.0 是 this
                BaseNames(ArgKey(0)) = "this"
                SlotTypes(ArgKey(0)) = method.DeclaringType
                _counters(ArgKey(0)) = 0
            End If

            For i As Integer = 0 To parameters.Length - 1
                Dim key = ArgKey(i + argOffset)
                Dim name = parameters(i).Name

                If String.IsNullOrEmpty(name) Then name = "arg" & i

                ' 参数名可能与局部变量重名，统一加 p_ 前缀避免冲突
                BaseNames(key) = ParameterBaseName(name)
                SlotTypes(key) = parameters(i).ParameterType
                _counters(key) = 0
            Next

            For i As Integer = 0 To reader.Locals.Count - 1
                Dim key = LocalKey(i)

                BaseNames(key) = "V_" & i
                SlotTypes(key) = reader.Locals(i).LocalType
                _counters(key) = 0
            Next
        End Sub

        ' ==================================================================
        ' 第 1 趟：phi 放置
        ' ==================================================================

        Private Sub PlacePhis()
            Dim defBlocks As New Dictionary(Of String, HashSet(Of Integer))()

            For Each b As BasicBlock In _cfg.Blocks
                For Each ins As ILInstruction In b.Instructions
                    Dim index As Integer = -1

                    If IlOpcodeInfo.IsStoreLocal(ins, index) Then
                        AddDefBlock(defBlocks, LocalKey(index), b.Id)
                    ElseIf IlOpcodeInfo.IsStoreArg(ins, index) Then
                        AddDefBlock(defBlocks, ArgKey(index), b.Id)
                    End If
                Next
            Next

            For Each pair In defBlocks
                Dim slot = pair.Key
                Dim work As New Queue(Of Integer)(pair.Value)
                Dim hasPhi As New HashSet(Of Integer)(pair.Value)

                While work.Count > 0
                    Dim blockId = work.Dequeue()

                    For Each frontier As Integer In _cfg.Blocks(blockId).DominanceFrontier
                        If hasPhi.Add(frontier) Then
                            AddPhi(frontier, slot)

                            ' 迭代支配边界：phi 本身也是一次定义，要继续向外传播
                            work.Enqueue(frontier)
                        End If
                    Next
                End While
            Next
        End Sub

        Private Shared Sub AddDefBlock(defBlocks As Dictionary(Of String, HashSet(Of Integer)),
                                       slot As String, blockId As Integer)
            If Not defBlocks.ContainsKey(slot) Then
                defBlocks(slot) = New HashSet(Of Integer)()
            End If

            defBlocks(slot).Add(blockId)
        End Sub

        Private Sub AddPhi(blockId As Integer, slot As String)
            If Not Phis.ContainsKey(blockId) Then
                Phis(blockId) = New List(Of PhiInfo)()
            End If

            Phis(blockId).Add(New PhiInfo With {
                .BlockId = blockId,
                .SlotKey = slot,
                .Name = FreshName(slot),
                .VarType = TypeOfSlot(slot)
            })
        End Sub

        Private Function FreshName(slot As String) As String
            Dim n As Integer = 0
            If _counters.ContainsKey(slot) Then n = _counters(slot)

            n += 1
            _counters(slot) = n

            Return BaseNameOf(slot) & "_" & n
        End Function

        ' ==================================================================
        ' 第 2 趟：重命名
        ' ==================================================================

        Private Sub Rename()
            Dim current As New Dictionary(Of String, String)()

            For Each pair In BaseNames
                current(pair.Key) = pair.Value
            Next

            RenameBlock(0, current)
        End Sub

        Private Sub RenameBlock(blockId As Integer, current As Dictionary(Of String, String))
            Dim b = _cfg.Blocks(blockId)
            Dim saved As New Dictionary(Of String, String)(current)

            ' 进入块：phi 定义的新名字成为当前值
            If Phis.ContainsKey(blockId) Then
                For Each phi As PhiInfo In Phis(blockId)
                    current(phi.SlotKey) = phi.Name
                Next
            End If

            For Each ins As ILInstruction In b.Instructions
                Dim index As Integer = -1

                If IlOpcodeInfo.IsStoreLocal(ins, index) Then
                    Dim key = LocalKey(index)
                    Dim name = FreshName(key)

                    DefNames(ins.Offset) = name
                    current(key) = name

                ElseIf IlOpcodeInfo.IsStoreArg(ins, index) Then
                    Dim key = ArgKey(index)
                    Dim name = FreshName(key)

                    DefNames(ins.Offset) = name
                    current(key) = name

                ElseIf IlOpcodeInfo.IsLoadLocal(ins, index) Then
                    UseNames(ins.Offset) = CurrentName(LocalKey(index), current)

                ElseIf IlOpcodeInfo.IsLoadArg(ins, index) Then
                    UseNames(ins.Offset) = CurrentName(ArgKey(index), current)
                End If
            Next

            ' 块结束：用当前名字回填后继 phi 的入边
            For Each succ As Integer In b.Successors
                If Not Phis.ContainsKey(succ) Then Continue For

                For Each phi As PhiInfo In Phis(succ)
                    If Not phi.Args.ContainsKey(blockId) Then
                        phi.Args(blockId) = CurrentName(phi.SlotKey, current)
                    End If
                Next
            Next

            For Each child As Integer In b.DominatorChildren
                RenameBlock(child, current)
            Next

            current.Clear()
            For Each pair In saved
                current(pair.Key) = pair.Value
            Next
        End Sub

        Private Function CurrentName(slot As String, current As Dictionary(Of String, String)) As String
            Dim name As String = Nothing

            If current.TryGetValue(slot, name) Then Return name

            ' 该槽从未登记（例如 ldarg 越界），兜底生成一个名字避免 Nothing 扩散
            Return BaseNameOf(slot)
        End Function
    End Class
End Namespace

