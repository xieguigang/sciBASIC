Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.MachineLearning.XGBoost.train

Public Module Conversion

    <Extension>
    Public Function ToTrainingSet(matrix As DoubleTagged(Of Single())(), fieldNames As String()) As TrainData

    End Function

    <Extension>
    Public Function ToValidateSet(matrix As DoubleTagged(Of Single())(), fieldNames As String()) As ValidationData

    End Function

    <Extension>
    Public Function ToTestDataSet(matrix As Single()()) As TestData

    End Function
End Module
