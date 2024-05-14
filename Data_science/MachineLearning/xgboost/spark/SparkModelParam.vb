#Region "Microsoft.VisualBasic::1808c0a4191940be084129b134770b3a, Data_science\MachineLearning\xgboost\spark\SparkModelParam.vb"

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

    '   Total Lines: 41
    '    Code Lines: 30
    ' Comment Lines: 4
    '   Blank Lines: 7
    '     File Size: 1.63 KB


    '     Class SparkModelParam
    ' 
    '         Properties: featureCol, labelCol, modelType, predictionCol, rawPredictionCol
    '                     thresholds
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.XGBoost.util

Namespace spark

    <Serializable>
    Public Class SparkModelParam

        Public Const MODEL_TYPE_CLS As String = "_cls_"
        Public Const MODEL_TYPE_REG As String = "_reg_"

        Public Overridable ReadOnly Property modelType As String
        Public Overridable ReadOnly Property featureCol As String
        Public Overridable ReadOnly Property labelCol As String
        Public Overridable ReadOnly Property predictionCol As String

        ''' <summary>
        ''' classification model only
        ''' </summary>
        ''' <returns></returns>
        Public Overridable ReadOnly Property rawPredictionCol As String
        Public Overridable ReadOnly Property thresholds As Double()

        Public Sub New(modelType As String, featureCol As String, reader As ModelReader)
            _modelType = modelType
            _featureCol = featureCol
            _labelCol = reader.readUTF()
            _predictionCol = reader.readUTF()

            If MODEL_TYPE_CLS.Equals(modelType) Then
                _rawPredictionCol = reader.readUTF()
                Dim thresholdLength As Integer = reader.readIntBE()
                _thresholds = If(thresholdLength > 0, reader.readDoubleArrayBE(thresholdLength), Nothing)
            ElseIf MODEL_TYPE_REG.Equals(modelType) Then
                _rawPredictionCol = Nothing
                _thresholds = Nothing
            Else
                Throw New NotSupportedException("Unknown modelType: " & modelType)
            End If
        End Sub
    End Class
End Namespace
