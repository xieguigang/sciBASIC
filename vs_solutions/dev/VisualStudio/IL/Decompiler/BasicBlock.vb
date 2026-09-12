#Region "Microsoft.VisualBasic::3d689fa256de342b4911d735daf08580, vs_solutions\dev\VisualStudio\IL\Decompiler\BasicBlock.vb"

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

    '   Total Lines: 88
    '    Code Lines: 42 (47.73%)
    ' Comment Lines: 32 (36.36%)
    '    - Xml Docs: 50.00%
    ' 
    '   Blank Lines: 14 (15.91%)
    '     File Size: 4.17 KB


    '     Class BasicBlock
    ' 
    '         Properties: Condition, DominanceFrontier, DominatorChildren, ExitStack, FalseSuccessor
    '                     FirstOffset, Id, ImmediateDominator, ImmediatePostDominator, Instructions
    '                     IsLoopHeader, LastInstruction, LoopBody, LoopFollow, LoopLatch
    '                     Offset, PostOrder, Predecessors, ReversePostOrder, Statements
    '                     Successors, Terminates, TrueSuccessor
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ---------------------------------------------------------------------------
' 基本块：CFG 的节点
'
' 一个基本块是一段"只有入口在首指令、只有出口在末指令"的连续指令序列。
' 除了块划分本身，这里还挂载了后续各趟分析的结果：
'   * 前驱 / 后继
'   * 支配树（ImmediateDominator / DominatorChildren）与支配边界
'   * 后支配（用于求 if 的 join 点）
'   * 自然循环（IsLoopHeader / LoopLatch / LoopBody / LoopFollow）
'   * 栈模拟的产出（Statements / Condition / 出口栈）
' ---------------------------------------------------------------------------

Namespace IL

    ''' <summary>控制流图中的一个基本块</summary>
    Public Class BasicBlock

        ''' <summary>块的编号（等于在 Blocks 列表中的下标）</summary>
        Public Property Id As Integer
        ''' <summary>块首指令的 IL 偏移</summary>
        Public Property Offset As Integer

        Public Property Instructions As New List(Of ILInstruction)

        Public Property Predecessors As New List(Of Integer)
        Public Property Successors As New List(Of Integer)

        ' ---------------- 支配树 ----------------
        ''' <summary>直接支配节点（idom）；入口块指向自己，不可达块为 -1</summary>
        Public Property ImmediateDominator As Integer = -1
        Public Property DominatorChildren As New List(Of Integer)
        ''' <summary>支配边界 DF(B)</summary>
        Public Property DominanceFrontier As New HashSet(Of Integer)

        ' ---------------- 排序 ----------------
        Public Property PostOrder As Integer = -1
        ''' <summary>反向后序序号（RPO），越小越靠前</summary>
        Public Property ReversePostOrder As Integer = -1

        ' ---------------- 后支配 ----------------
        ''' <summary>直接后支配节点；为虚拟出口时记 -1</summary>
        Public Property ImmediatePostDominator As Integer = -1

        ' ---------------- 循环 ----------------
        Public Property IsLoopHeader As Boolean
        ''' <summary>回边的源（latch）；多重回边时记最后一条</summary>
        Public Property LoopLatch As Integer = -1
        ''' <summary>循环出口块（while 条件为假时去往的块）</summary>
        Public Property LoopFollow As Integer = -1
        ''' <summary>自然循环的块集合（含 header 与 latch）</summary>
        Public Property LoopBody As New HashSet(Of Integer)

        ' ---------------- 栈模拟产出 ----------------
        ''' <summary>该块归约出的语句序列（不含跳转本身）</summary>
        Public Property Statements As New List(Of Statement)
        ''' <summary>末指令为条件分支时的条件表达式</summary>
        Public Property Condition As Expression
        ''' <summary>条件成立时去往的块</summary>
        Public Property TrueSuccessor As Integer = -1
        ''' <summary>条件不成立时去往的块</summary>
        Public Property FalseSuccessor As Integer = -1
        ''' <summary>块出口处的求值栈（跨块的短路布尔会用到）</summary>
        Public Property ExitStack As New List(Of Expression)

        Public ReadOnly Property FirstOffset As Integer
            Get
                Return If(Instructions.Count = 0, Offset, Instructions(0).Offset)
            End Get
        End Property

        Public ReadOnly Property LastInstruction As ILInstruction
            Get
                Return If(Instructions.Count = 0, Nothing, Instructions(Instructions.Count - 1))
            End Get
        End Property

        ''' <summary>该块是否以 ret / throw 结束（不再有后继）</summary>
        Public ReadOnly Property Terminates As Boolean
            Get
                Return Successors.Count = 0
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"bb{Id}(IL_{Offset.ToString("X4")}, {Instructions.Count} 条)"
        End Function
    End Class
End Namespace
