#Region "Microsoft.VisualBasic::56ceb9a4319409fe11b0cb13a4d1122f, Data_science\DataMining\DataMining\Evaluation\MetricRegistry.vb"

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

    '   Total Lines: 42
    '    Code Lines: 29 (69.05%)
    ' Comment Lines: 3 (7.14%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 10 (23.81%)
    '     File Size: 1.48 KB


    '  
    ' 
    '     Function: BinaryLabels, External, MatthewsCorrelationCoefficient, Silhouette
    ' 
    ' 
    ' /********************************************************************************/

#End Region

#Region "helpers"

        Private Shared Function BinaryLabels(result As ClassificationResult) As Double()
            Return result.PositiveLabels.Select(Function(b) If(b, 1.0, 0.0)).ToArray
        End Function

        ''' <summary>
        ''' 计算外部聚类指标；当没有提供真值标签的时候返回 ``NaN``。
        ''' </summary>
        Private Shared Function External(result As ClusteringResult, evaluate As Func(Of ClusteringResult, Double)) As Double
            If result.HasGroundTruth Then
                Return evaluate(result)
            Else
                Return Double.NaN
            End If
        End Function

        Private Shared Function Silhouette(result As ClusteringResult) As Double
            Return ClusteringIndices.Silhouette(result.Features, result.ClusterLabels, result.MaxPoints)
        End Function

        Private Shared Function MatthewsCorrelationCoefficient(confusion As Validation) As Double
            Dim tp As Double = confusion.TP
            Dim tn As Double = confusion.TN
            Dim fp As Double = confusion.FP
            Dim fn As Double = confusion.FN
            Dim denominator As Double = std.Sqrt((tp + fp) * (tp + fn) * (tn + fp) * (tn + fn))

            If denominator = 0 Then
                Return 0
            End If

            Return (tp * tn - fp * fn) / denominator
        End Function

#End Region

    End Class

End Namespace

#End Region
