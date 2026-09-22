#Region "Microsoft.VisualBasic::1e411e939d45d3cc55b737f0cf2145e1, Data_science\DataMining\DataMining\Evaluation\LabelEvaluate\ChangePoint.vb"

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

    '   Total Lines: 22
    '    Code Lines: 15 (68.18%)
    ' Comment Lines: 3 (13.64%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 4 (18.18%)
    '     File Size: 737 B


    '     Class ChangePoint
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Evaluation

    ''' <summary>
    ''' 混淆矩阵在排序扫描过程之中的一个变化点（SVM.NET 评估器内部使用）。
    ''' </summary>
    <Obsolete("该类型仅服务于旧的 PerformanceEvaluator，请改用统一的 Validation/RocCurve。", False)>
    Friend Class ChangePoint

        Public Sub New(tp As Integer, fp As Integer, tn As Integer, fn As Integer)
            Me.TP = tp
            Me.FP = fp
            Me.TN = tn
            Me.FN = fn
        End Sub

        Public TP, FP, TN, FN As Integer

        Public Overrides Function ToString() As String
            Return String.Format("{0}:{1}:{2}:{3}", TP, FP, TN, FN)
        End Function
    End Class
End Namespace
