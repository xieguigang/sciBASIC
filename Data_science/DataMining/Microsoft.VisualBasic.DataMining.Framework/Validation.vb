#Region "Microsoft.VisualBasic::c4374328cc21ae32675da42084156e5a, Data_science\DataMining\Microsoft.VisualBasic.DataMining.Framework\Validation.vb"

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

    ' Structure Validation
    ' 
    '     Function: Calc, ToDataSet, ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' 验证结果描述
''' 
''' ``灵敏度 = 真阳性人数 / (真阳性人数 + 假阴性人数) * 100%``
''' ``特异度 = 真阴性人数 / (真阴性人数 + 假阳性人数) * 100%``
''' </summary>
Public Structure Validation

    Dim Specificity As Double
    Dim Sensibility As Double
    Dim Accuracy As Double
    Dim All As Integer
    Dim TP As Integer
    Dim FP As Integer
    Dim TN As Integer
    Dim FN As Integer

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function

    Public Function ToDataSet() As Dictionary(Of String, Double)
        Return New Dictionary(Of String, Double) From {
            {NameOf(Specificity), Specificity},
            {NameOf(Sensibility), Sensibility},
            {NameOf(Accuracy), Accuracy},
            {NameOf(All), All},
            {"True Positive", TP},
            {"False Positive", FP},
            {"True Negative", TN},
            {"False Negative", FN}
        }
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="T">
    ''' + ``true`` 表示阳性
    ''' + ``false`` 表示阴性
    ''' </typeparam>
    ''' <param name="entity"></param>
    ''' <param name="getValidate">得到实际的分类结果</param>
    ''' <param name="getPredict">得到预测的分类结果</param>
    ''' <returns></returns>
    Public Shared Function Calc(Of T)(entity As IEnumerable(Of T), getValidate As Func(Of T, Boolean), getPredict As Func(Of T, Boolean)) As Validation
        ' 真阳性人数
        Dim TP As Integer
        ' 假阳性人数
        Dim FP As Integer
        ' 真阴性人数
        Dim TN As Integer
        ' 假阴性人数
        Dim FN As Integer
        Dim All%

        For Each n As T In entity
            Dim validate = getValidate(n)
            Dim predict = getPredict(n)

            If validate = True Then
                ' 真实的结果为阳性
                If predict = True Then
                    ' 预测与真实情况一致
                    TP += 1
                Else
                    ' 但是预测为阴性
                    FN += 1
                End If
            Else
                ' 真实结果为阴性
                If predict = True Then
                    ' 但是预测结果为阳性
                    FP += 1
                Else
                    ' 预测与真实情况一致
                    TN += 1
                End If
            End If

            All += 1
        Next

        Return New Validation With {
            .Sensibility = TP / (TP + FN) * 100,
            .Specificity = TN / (TN + FP) * 100,
            .Accuracy = (TP + TN) / All * 100,
            .All = All,
            .FN = FN,
            .FP = FP,
            .TN = TN,
            .TP = TP
        }
    End Function
End Structure
