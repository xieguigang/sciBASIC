#Region "Microsoft.VisualBasic::ModelPrediction, MachineLearning\ML_SHAP\Evaluation\ModelPrediction.vb"

Imports System.Linq

''' <summary>
''' 一个模型在一个数据集之上的预测结果与常用的评估指标。
''' </summary>
Public Class ModelPrediction

    Public Property ModelName As String
    Public Property DatasetName As String
    Public Property IsClassification As Boolean
    Public Property IDs As String()
    Public Property Labels As Double()
    Public Property Predictions As Double()

    Public ReadOnly Property Size As Integer
        Get
            Return If(Predictions Is Nothing, 0, Predictions.Length)
        End Get
    End Property

    ''' <summary>
    ''' 均方误差。
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property MSE As Double
        Get
            If Size = 0 Then
                Return 0
            End If

            Dim sum As Double = 0

            For i As Integer = 0 To Size - 1
                Dim d As Double = Predictions(i) - Labels(i)
                sum += d * d
            Next

            Return sum / Size
        End Get
    End Property

    ''' <summary>
    ''' 均方根误差。
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property RMSE As Double
        Get
            Return System.Math.Sqrt(MSE)
        End Get
    End Property

    ''' <summary>
    ''' 决定系数 R^2。
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property R2 As Double
        Get
            If Size = 0 Then
                Return 0
            End If

            Dim mean As Double = Labels.Average()
            Dim sse As Double = 0
            Dim sst As Double = 0

            For i As Integer = 0 To Size - 1
                Dim d As Double = Predictions(i) - Labels(i)
                Dim t As Double = Labels(i) - mean
                sse += d * d
                sst += t * t
            Next

            If sst = 0 Then
                Return 0
            Else
                Return 1.0 - sse / sst
            End If
        End Get
    End Property

    ''' <summary>
    ''' 分类准确率（以 0.5 为判定阈值）。
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Accuracy As Double
        Get
            If Size = 0 Then
                Return 0
            End If

            Dim correct As Integer = 0

            For i As Integer = 0 To Size - 1
                If Predict(Predictions(i)) = CInt(System.Math.Round(Labels(i))) Then
                    correct += 1
                End If
            Next

            Return correct / Size
        End Get
    End Property

    Private Shared Function Predict(score As Double) As Integer
        Return If(score > 0.5, 1, 0)
    End Function

    Public Overrides Function ToString() As String
        If IsClassification Then
            Return $"{ModelName} @ {DatasetName}: accuracy={Accuracy.ToString("P2")}, mse={MSE.ToString("G4")}"
        Else
            Return $"{ModelName} @ {DatasetName}: rmse={RMSE.ToString("G4")}, r2={R2.ToString("G4")}"
        End If
    End Function

End Class

#End Region
