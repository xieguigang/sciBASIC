' /********************************************************************************/
'  Microsoft.VisualBasic.MachineLearning
'  Data_science\MachineLearning\MachineLearning\MultilayerPerceptron\TrainingOptions.vb
' /********************************************************************************/

Namespace MultilayerPerceptron

    ''' <summary>多层感知机所处理的任务类型。</summary>
    Public Enum MlpTask
        ''' <summary>分类任务：输出层附加 Softmax，训练使用交叉熵损失。</summary>
        Classification
        ''' <summary>回归任务：输出层为恒等映射，训练使用均方误差损失。</summary>
        Regression
    End Enum

    ''' <summary>训练阶段使用的参数更新策略。</summary>
    Public Enum OptimizerKind
        ''' <summary>随机梯度下降，可配置动量。</summary>
        SGD
        ''' <summary>Adam 自适应矩估计优化器。</summary>
        Adam
    End Enum

    ''' <summary>多层感知机的训练配置。</summary>
    Public Class TrainingOptions

        ''' <summary>最大训练轮数。</summary>
        Public Property Epochs As Integer = 200

        ''' <summary>学习率。</summary>
        Public Property LearningRate As Double = 0.01

        ''' <summary>小批量大小；小于等于零表示使用全部样本。</summary>
        Public Property BatchSize As Integer = 16

        ''' <summary>优化器类型。</summary>
        Public Property Optimizer As OptimizerKind = OptimizerKind.Adam

        ''' <summary>SGD 的动量系数。</summary>
        Public Property Momentum As Double = 0.9

        ''' <summary>Adam 的一阶矩衰减系数。</summary>
        Public Property Beta1 As Double = 0.9

        ''' <summary>Adam 的二阶矩衰减系数。</summary>
        Public Property Beta2 As Double = 0.999

        ''' <summary>Adam 的数值稳定项。</summary>
        Public Property Epsilon As Double = 0.0000001

        ''' <summary>连续两轮损失变化小于该阈值时提前终止训练。</summary>
        Public Property Tolerance As Double = 0.0000001

        ''' <summary>是否在训练过程中打印损失。</summary>
        Public Property Verbose As Boolean = False

        ''' <summary>每隔多少轮打印一次损失（仅在 <see cref="Verbose"/> 为真时生效）。</summary>
        Public Property LogEvery As Integer = 10

        ''' <summary>创建一个使用默认值的训练配置。</summary>
        Public Sub New()
        End Sub

        ''' <summary>创建一个指定核心超参数的训练配置。</summary>
        Public Sub New(epochs As Integer, learningRate As Double, Optional batchSize As Integer = 16)
            Me.Epochs = epochs
            Me.LearningRate = learningRate
            Me.BatchSize = batchSize
        End Sub

        Friend Sub Validate()
            If Epochs < 1 Then
                Throw New System.ArgumentOutOfRangeException(NameOf(Epochs), "训练轮数必须大于零")
            End If
            If LearningRate <= 0 OrElse Double.IsNaN(LearningRate) Then
                Throw New System.ArgumentOutOfRangeException(NameOf(LearningRate), "学习率必须为正数")
            End If
            If Momentum < 0 OrElse Momentum >= 1 Then
                Throw New System.ArgumentOutOfRangeException(NameOf(Momentum), "动量系数应位于 [0, 1) 区间")
            End If
            If Beta1 <= 0 OrElse Beta1 >= 1 OrElse Beta2 <= 0 OrElse Beta2 >= 1 Then
                Throw New System.ArgumentOutOfRangeException(NameOf(Beta1), "Adam 衰减系数应位于 (0, 1) 区间")
            End If
        End Sub

    End Class

End Namespace
