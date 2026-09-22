#Region "Microsoft.VisualBasic::69e9f8aacd4ad7abbb9ff1a85788dc46, llm\Trainer\TrainingStepReport.vb"

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

    '   Total Lines: 35
    '    Code Lines: 18 (51.43%)
    ' Comment Lines: 11 (31.43%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (17.14%)
    '     File Size: 1.83 KB


    '     Class TrainingStepReport
    ' 
    '         Properties: [Step], ElapsedMilliseconds, GradientNorm, LearningRate, Loss
    '                     MoEMaxLoadRatio, Perplexity, Skipped
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Trainer

    ''' <summary>一个训练步的结果报告。</summary>
    Public Class TrainingStepReport

        ''' <summary>Zero based index of the training step.</summary>
        Public Property [Step] As Integer
        ''' <summary>Cross entropy loss of this step.</summary>
        Public Property Loss As Double
        ''' <summary>Perplexity of this step, i.e. <c>exp(Loss)</c>.</summary>
        Public Property Perplexity As Double
        ''' <summary>Learning rate applied in this step.</summary>
        Public Property LearningRate As Double
        ''' <summary>裁剪前的全局梯度范数。</summary>
        Public Property GradientNorm As Double
        ''' <summary>Wall clock duration of this step, in milliseconds.</summary>
        Public Property ElapsedMilliseconds As Double
        ''' <summary>本步结束时各 MoE 层中最差的最大负载比（1.0 = 完全均匀；无 MoE 时为 0）。</summary>
        Public Property MoEMaxLoadRatio As Double

        ''' <summary>本步是否因为梯度失控而跳过了参数更新。</summary>
        Public Property Skipped As Boolean

        ''' <summary>Returns a one line summary of this training step.</summary>
        ''' <returns>A text that reports the step index, loss, perplexity, learning rate and gradient norm.</returns>
        Public Overrides Function ToString() As String
            Dim moe = If(MoEMaxLoadRatio > 0, $", moe_load={MoEMaxLoadRatio:F2}x", "")
            Dim skip = If(Skipped, "  [已跳过更新：梯度失控]", "")

            Return $"step {[Step],4}  loss={Loss:F4}  ppl={Perplexity,8:F2}  lr={LearningRate:E3}  " &
                   $"gnorm={GradientNorm,7:F3}  {ElapsedMilliseconds,7:F0}ms{moe}{skip}"
        End Function

    End Class
End Namespace
