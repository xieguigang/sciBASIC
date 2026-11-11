#Region "Microsoft.VisualBasic::ce7d552739df42007a367605f5db1cdc, Data_science\Mathematica\Math\DataFittings\Logistic\NamespaceDoc.vb"

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

    '   Total Lines: 8
    '    Code Lines: 4 (50.00%)
    ' Comment Lines: 3 (37.50%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 1 (12.50%)
    '     File Size: 145 B


    '     Module NamespaceDoc
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Logistic

    ''' <summary>
    ''' 二分类逻辑回归（批量梯度下降 + SIMD / 可选分块并行）。
    ''' </summary>
    ''' <remarks>
    ''' <list type="bullet">
    ''' <item>
    ''' <description>
    ''' <b>标签契约</b>：<c>Instance.label</c> 必须是 0 或 1，训练器会显式校验；
    ''' 表入口 <c>NumericTableRegressions.LogisticRegression</c> 按原值传入，
    ''' 不再用 <c>CInt</c> 截断连续标签。
    ''' </description>
    ''' </item>
    ''' <item>
    ''' <description>
    ''' <b>概率语义</b>：<c>Logistic.predict</c>、<c>LogisticFit.GetY</c> 与
    ''' <c>ErrorTest.Yfit</c> 一律返回正类概率 <c>P(y=1|x)</c>，三者共用同一个
    ''' <c>PredictProbability</c> 实现，不存在方向相反的历史问题。
    ''' </description>
    ''' </item>
    ''' <item>
    ''' <description>
    ''' <b>截距与标准化</b>：截距是独立标量 <c>Intercept</c>，不占用
    ''' <c>Polynomial.Factors</c> 的位置，因此系数向量长度始终等于特征数；
    ''' 启用标准化后，均值/标准差由 <c>Standardization</c> 在训练与推理之间共用。
    ''' </description>
    ''' </item>
    ''' <item>
    ''' <description>
    ''' <b>数值稳定</b>：sigmoid 与交叉熵都用 <c>exp(-|z|)</c> 区间约减，
    ''' 成本写成 <c>softplus(z) - y*z</c>，既不会 <c>exp(-z)</c> 上溢，
    ''' 也不会出现 <c>log(0)</c>；每轮检测 NaN/Infinity 并中止发散训练。
    ''' </description>
    ''' </item>
    ''' <item>
    ''' <description>
    ''' <b>训练控制</b>：<c>AdaptiveRate</c> 默认开启（成本下降放大步长、上升回退减半），
    ''' <c>Tolerance</c> 控制早停，<c>CostHistory</c> 与 <c>EffectiveAlpha</c> 可回看训练过程，
    ''' <c>println</c> 回调按固定轮数间隔输出进度。
    ''' </description>
    ''' </item>
    ''' <item>
    ''' <description>
    ''' <b>向量化与可复现性</b>：数值内核集中在 <c>LogisticKernels</c>，
    ''' 前向走 <c>SIMDIntrinsics.DotFmaRow</c>、sigmoid 的指数走
    ''' <c>SimdMath.ExpInPlace</c> 的 AVX2+FMA 多项式路径、梯度走
    ''' <c>SimdEngine.AddScaledInPlace</c>；成本是每轮都要算的监控量，
    ''' 改用单趟标量 <c>Math.Log</c> 累加，避免全数组预扫描反而拖慢 SIMD 路径。
    ''' <c>UseParallel</c> 默认关闭；
    ''' 开启时按工作量阈值分块，每块一份局部梯度，并按块号顺序合并，
    ''' 固定浮点归约次序以保证结果可复现。可用
    ''' <c>SIMDConfiguration.disable</c> 与 <c>SimdParallel.Enable</c>
    ''' 做标量 / SIMD / SIMD+Parallel 三路对拍。
    ''' </description>
    ''' </item>
    ''' <item>
    ''' <description>
    ''' <b>审计</b>：<c>_smoketest/LogisticAudit.vb</c> 使用 <c>DebugAudit</c>
    ''' 记录正确性断言、梯度有限差分检验、三路数值对拍与耗时基准，
    ''' 并生成 <c>logistic-test-report.json</c> 测试报告。
    ''' </description>
    ''' </item>
    ''' </list>
    ''' </remarks>
    Module NamespaceDoc
    End Module
End Namespace
