#Region "Microsoft.VisualBasic::17de3618826d5b7e5babaf0eb0331233, Data_science\Visualization\test\ROCplotTest.vb"

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

    '   Total Lines: 19
    '    Code Lines: 14 (73.68%)
    ' Comment Lines: 1 (5.26%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 4 (21.05%)
    '     File Size: 1013 B


    ' Module ROCplotTest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Linq
Imports Microsoft.VisualBasic.Data.ChartPlots.Statistics
Imports Microsoft.VisualBasic.Data.Framework.IO
Imports Microsoft.VisualBasic.DataMining.Evaluation

Module ROCplotTest

    Sub Main()
        Dim data = EntityObject.LoadDataSet("D:\smartnucl_integrative\biodeepDB\smartnucl_integrative\build_tools\CVD_kb\duke\out\CHD_20190411_IVD\[ALL-validates]validate-result_4markers.csv").ToArray
        Dim scores As Double() = data.Select(Function(d) d("CHD(predicted)")).ToArray
        Dim labels As Double() = data.Select(Function(d) If(d!CHD > 0, 1.0, 0.0)).ToArray

        ' 统一评估框架入口：分类结果 → EvaluationReport（含 ROC 曲线与 AUC）
        Dim report = ModelEvaluation.Evaluate(ClassificationResult.Create(scores, labels, "CHD"))
        Dim test = report.Curve.Points.CreateSerial

        Call ROCPlot.Plot(test).Save("D:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematica\Plot\data\ROC\identify_ROC2.png")
    End Sub
End Module
