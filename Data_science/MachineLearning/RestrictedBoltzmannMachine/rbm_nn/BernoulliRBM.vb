Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.math
Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.nn.rbm.learn

Namespace nn.rbm

    ''' <summary>
    ''' 伯努利-伯努利受限玻尔兹曼机：可见层与隐藏层的单元状态都是二值的<c>{0, 1}</c>。
    ''' </summary>
    ''' <remarks>
    ''' 这个类型是<see cref="RBM"/>的一个带有更友好的训练接口的子类，
    ''' <see cref="Train"/>方法直接接受数据集、训练轮数、学习速率以及CD步数。
    ''' </remarks>
    Public Class BernoulliRBM
        Inherits RBM

        Public Sub New(visibleUnits As Integer, hiddenUnits As Integer)
            MyBase.New(visibleUnits, hiddenUnits)
        End Sub

        ''' <summary>
        ''' 使用对比散度(CD-k)算法训练当前的RBM。
        ''' </summary>
        ''' <param name="data">训练数据：每一个行向量代表一个样本，元素取值为<c>0</c>或者<c>1</c></param>
        ''' <param name="epochs">训练轮数</param>
        ''' <param name="learningRate">学习速率</param>
        ''' <param name="k">CD算法之中的Gibbs采样步数，默认值为<c>1</c></param>
        ''' <param name="log">是否输出训练过程的日志</param>
        ''' <returns>训练结束之后的重构均方误差</returns>
        Public Function Train(data As DenseMatrix,
                              Optional epochs As Integer = 10,
                              Optional learningRate As Double = 0.01,
                              Optional k As Integer = 1,
                              Optional log As Boolean = False) As Double

            Dim parameters As New LearningParameters()
            Call parameters.setEpochs(epochs).setLearningRate(learningRate).setK(k).setLog(log)

            Dim trainer As New ContrastiveDivergence(parameters)
            Call trainer.learn(Me, data)

            Return ReconstructionError(data)
        End Function

    End Class

End Namespace
