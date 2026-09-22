#Region "Microsoft.VisualBasic::0a3e35d29a7a5194b9b0c1f7a4fee566, llm\Trainer\LMBatch.vb"

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

    '   Total Lines: 117
    '    Code Lines: 63 (53.85%)
    ' Comment Lines: 27 (23.08%)
    '    - Xml Docs: 96.30%
    ' 
    '   Blank Lines: 27 (23.08%)
    '     File Size: 4.72 KB


    '     Class LMBatch
    ' 
    '         Properties: BatchSize, LossMask, SeqLen, SupervisedTokens, Targets
    '                     TokenIds, TotalTokens
    ' 
    '         Function: Create
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Trainer


    ''' <summary>
    ''' 一个训练批次：等长的若干条 token 序列（展平存放）+ 逐位置的损失掩码。
    ''' </summary>
    Public Class LMBatch

        ''' <summary>输入 token，展平为 <c>[BatchSize * SeqLen]</c>。</summary>
        Public Property TokenIds As Integer()

        ''' <summary>目标 token，展平后布局与 <see cref="TokenIds"/> 一致。</summary>
        Public Property Targets As Integer()

        ''' <summary>逐位置的损失掩码；<see langword="Nothing"/> 表示全部计入。</summary>
        Public Property LossMask As Boolean()

        ''' <summary>Number of sequences in this batch.</summary>
        Public Property BatchSize As Integer
        ''' <summary>Length of every sequence in this batch.</summary>
        Public Property SeqLen As Integer

        ''' <summary>Total number of tokens in this batch, i.e. <c>BatchSize * SeqLen</c>.</summary>
        Public ReadOnly Property TotalTokens As Integer
            Get
                Return BatchSize * SeqLen
            End Get
        End Property

        ''' <summary>实际计入损失的 token 个数。</summary>
        Public ReadOnly Property SupervisedTokens As Integer
            Get
                If LossMask Is Nothing Then Return TotalTokens

                Dim count As Integer = 0

                For Each flag In LossMask
                    If flag Then count += 1
                Next

                Return count
            End Get
        End Property

        ''' <summary>
        ''' 由 <c>[B][L]</c> 的 token 序列与"该 token 是否要被学习"的掩码构造批次。
        ''' </summary>
        ''' <param name="tokens">B 条等长序列，长度 L &gt;= 2</param>
        ''' <param name="learnMask">
        ''' 与 <paramref name="tokens"/> 同形；<c>learnMask(b)(t)=True</c> 表示"希望模型学会
        ''' 在位置 t 预测 token t"。user 消息与工具返回应为 False。
        ''' </param>
        ''' <param name="nextTokenId">
        ''' 每条序列末位的目标占位 token（通常传 EOS）；因为末位之后没有真实的下一个 token。
        ''' 该位置的掩码会被强制置为 False，因此占位值不会影响损失。
        ''' </param>
        ''' <remarks>
        ''' 这里完成标准的"右移一位"错位：位置 t 的输入是 token t，目标是 token t+1，
        ''' 是否计入损失取 <c>learnMask(t+1)</c>。因此输出序列长度是 L − 1。
        ''' </remarks>
        Public Shared Function Create(tokens As Integer()(), learnMask As Boolean()(),
                                      Optional nextTokenId As Integer = 0) As LMBatch

            If tokens Is Nothing OrElse tokens.Length = 0 Then
                Throw New ArgumentException("批次不能为空")
            End If

            Dim batch = tokens.Length
            Dim length = tokens(0).Length

            If length < 2 Then Throw New ArgumentException("每条序列至少需要 2 个 token 才能构造一个预测目标")

            For b As Integer = 0 To batch - 1
                If tokens(b).Length <> length Then
                    Throw New ArgumentException($"第 {b} 条序列长度 {tokens(b).Length} 与第 0 条的 {length} 不一致")
                End If
            Next

            Dim seqLen = length - 1
            Dim ids(batch * seqLen - 1) As Integer
            Dim targets(batch * seqLen - 1) As Integer
            Dim mask(batch * seqLen - 1) As Boolean

            For b As Integer = 0 To batch - 1
                For t As Integer = 0 To seqLen - 1
                    Dim idx = b * seqLen + t

                    ids(idx) = tokens(b)(t)
                    targets(idx) = If(t + 1 < length, tokens(b)(t + 1), nextTokenId)

                    Dim learn As Boolean = True

                    If learnMask IsNot Nothing AndAlso b < learnMask.Length Then
                        Dim row = learnMask(b)

                        If row IsNot Nothing AndAlso t + 1 < row.Length Then learn = row(t + 1)
                    End If

                    ' 末位的"下一个 token"是人为补的，不参与损失
                    If t + 1 >= length Then learn = False

                    mask(idx) = learn
                Next
            Next

            Return New LMBatch With {
                .TokenIds = ids,
                .Targets = targets,
                .LossMask = mask,
                .BatchSize = batch,
                .SeqLen = seqLen
            }
        End Function

    End Class

End Namespace
