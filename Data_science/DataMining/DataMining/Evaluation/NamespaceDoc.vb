#Region "Microsoft.VisualBasic::1d355a8201c7ac4f126ebab4657a565e, Data_science\DataMining\DataMining\Evaluation\NamespaceDoc.vb"

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

    '   Total Lines: 38
    '    Code Lines: 4 (10.53%)
    ' Comment Lines: 33 (86.84%)
    '    - Xml Docs: 78.79%
    ' 
    '   Blank Lines: 1 (2.63%)
    '     File Size: 1.90 KB


    '     Module NamespaceDoc
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Evaluation

    ''' <summary>
    ''' 模型质量评估工具模块（统一评估框架）。
    ''' 
    ''' 本模块为三类结果数据提供统一的评估能力：
    ''' 
    ''' + **机器学习分类结果**：<see cref="ClassificationResult"/>（分数 + 标签）
    ''' + **回归结果**：<see cref="RegressionResult"/>（预测值 + 连续真值）
    ''' + **聚类结果**：<see cref="ClusteringResult"/>（特征矩阵 + 簇标签，可带真值标签）
    ''' 
    ''' 使用方式：
    ''' 
    ''' ```vb
    ''' Dim report As EvaluationReport = ModelEvaluation.Evaluate(
    '''     ClassificationResult.Create(scores, labels, "my-model"))
    ''' 
    ''' Dim auc As Double = report.Metric("auc")
    ''' Dim curve As RocCurve = report.Curve
    ''' ```
    ''' 
    ''' 架构说明：
    ''' 
    ''' 1. **唯一 ROC/AUC 核心**：<see cref="RocBuilder"/> 负责曲线生成，
    '''    <see cref="RocAuc"/> 负责面积与最佳阈值计算。模块内不再存在任何重复实现。
    ''' 2. **统一报告与可扩展指标**：<see cref="MetricRegistry"/> 维护
    '''    「结果种类 → 指标」的注册表，并可注册自定义指标；
    '''    <see cref="ModelEvaluation"/> 是唯一的评估入口。
    ''' 3. **兼容层**：<see cref="Validation"/>、<see cref="ROC"/>、<see cref="Validate"/>、
    '''    <see cref="RegressionROC"/>、<see cref="Metric"/> 保留原有公开签名，
    '''    但内部全部委托到上述核心实现。
    ''' 4. **单位约定（重要）**：<see cref="Validation"/> 的所有比率字段
    '''    （Sensibility / Specificity / Accuracy / Precision / FPR）以及 AUC 一律使用
    '''    ``[0, 1]`` 的分数表示，不再使用 ``[0, 100]`` 的百分数。
    ''' </summary>
    Module NamespaceDoc
    End Module
End Namespace
