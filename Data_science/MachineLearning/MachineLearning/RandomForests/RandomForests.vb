Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.DecisionTree

Namespace RandomForests

    Public Class RandomForests

        Public ReadOnly Property trees As Tree()

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function CreateForests(trainingSet As DataTable, n%, size%) As RandomForests
            Return trainingSet.Sampling(n, size) _
                .AsParallel _
                .Select(Function(data) New Tree(data)) _
                .ToArray
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(trees As Tree()) As RandomForests
            Return New RandomForests With {._trees = trees}
        End Operator
    End Class
End Namespace