#Region "Microsoft.VisualBasic::f96fef2bcd799586678881c6646b9a61, llm\Trainer\TrainingConfig.vb"

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

    '   Total Lines: 43
    '    Code Lines: 11 (25.58%)
    ' Comment Lines: 22 (51.16%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 10 (23.26%)
    '     File Size: 2.10 KB


    '     Class TrainingConfig
    ' 
    '         Properties: LearningRate, MaxGradNorm, MaxTrustedGradientNorm, MinLearningRate, TotalSteps
    '                     UseCosineDecay, WarmupSteps
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Trainer

    ''' <summary>训练超参。</summary>
    Public Class TrainingConfig

        ''' <summary>峰值学习率。</summary>
        Public Property LearningRate As Double = 0.0003

        ''' <summary>cosine 衰减的下界。</summary>
        Public Property MinLearningRate As Double = 0.0

        ''' <summary>学习率线性 warmup 的步数。</summary>
        Public Property WarmupSteps As Integer = 10

        ''' <summary>计划的总步数（用于 cosine 衰减）。</summary>
        Public Property TotalSteps As Integer = 100

        ''' <summary>全局梯度范数上限；&lt;= 0 表示不裁剪。</summary>
        Public Property MaxGradNorm As Double = 1.0

        ''' <summary>是否启用 cosine 衰减（关闭则 warmup 之后保持恒定学习率）。</summary>
        Public Property UseCosineDecay As Boolean = True

        ''' <summary>
        ''' Upper bound for the trusted gradient norm; when it is exceeded the step is considered out of control and the
        ''' parameter update is skipped.
        ''' </summary>
        ''' <remarks>
        ''' This is a <b>safety net</b> rather than the normal path. In practice a 200 million parameter model produced a global
        ''' gradient norm spike of 7.8e36 during tool call SFT, and the gradient became NaN on the following step. The forward
        ''' pass was still finite (the loss of that step was normal), only the gradient was broken, so dropping the update of
        ''' that step lets training continue, whereas forcing the update would push the parameters to NaN in one step.
        ''' <para>
        ''' The threshold is intentionally loose (1e6 by default) so that only clearly broken steps are intercepted and normal
        ''' large gradients are left untouched. The number of skipped steps is recorded in <c>LMTrainer.SkippedSteps</c> instead
        ''' of being silently swallowed.
        ''' </para>
        ''' </remarks>
        Public Property MaxTrustedGradientNorm As Double = 1000000.0

    End Class

End Namespace
