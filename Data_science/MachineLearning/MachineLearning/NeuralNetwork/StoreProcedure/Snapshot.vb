Imports System.Runtime.CompilerServices

Namespace NeuralNetwork.StoreProcedure

    ''' <summary>
    ''' 对于大型的神经网络而言，反复的构建XML数据模型将会额外的消耗掉大量的时间，导致训练的时间过长
    ''' 在这里通过这个持久性的快照对象来减少这种反复创建XML数据快照的问题
    ''' </summary>
    Public Class Snapshot

        ReadOnly snapshot As NeuralNetwork
        ReadOnly source As Network

        Sub New(model As Network)
            source = model
            snapshot = CreateSnapshot.TakeSnapshot(model, 0)
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="error">
        ''' The calculation errors of current snapshot.
        ''' </param>
        ''' <returns></returns>
        Public Function UpdateSnapshot([error] As Double) As Snapshot
            snapshot.errors = [error]
            snapshot.learnRate = source.LearnRate
            snapshot.momentum = source.Momentum



            Return Me
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function WriteIntegralXML(path As String) As Boolean
            Return snapshot.GetXml.SaveTo(path, throwEx:=False)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function WriteScatteredParts(directory As String) As Boolean
            Return snapshot.ScatteredStore(store:=directory)
        End Function

        Public Overrides Function ToString() As String
            Return source.ToString
        End Function
    End Class
End Namespace