Imports System
Imports System.Runtime.CompilerServices

Namespace PaCMAP
    ''' <summary>
    ''' 归一化距离计算
    ''' Normalized distance calculation
    ''' 
    ''' 原始JavaScript代码:
    ''' export function normalizedDistance(distances) {
    '''   return tf.tidy(() => {
    '''     const k = 7;
    '''     const topKNearestDistances = tf.topk(distances.neg(), k, true).values;
    '''     const fourthToSixthNearestDistances = topKNearestDistances.slice([0, 4], [-1, -1]);
    '''     const averageFourthToSixthNearestDistances = fourthToSixthNearestDistances
    '''       .neg()
    '''       .sum(-1)
    '''       .div(tf.scalar(3))
    '''       .expandDims(1);
    '''     const sigma = averageFourthToSixthNearestDistances.matMul(averageFourthToSixthNearestDistances, false, true);
    '''     return distances.square().div(sigma);
    '''   });
    ''' }
    ''' </summary>
    Public Module NormalizedDistanceExtensions
        <Extension()>
        Public Function NormalizedDistance(distances As Tensor) As Tensor
            Return NormalizedDistanceExtensions.Compute(distances)
        End Function

        ''' <summary>
        ''' 计算归一化距离
        ''' Calculate normalized distance
        ''' 
        ''' 算法步骤:
        ''' 1. 取距离矩阵每行的最小的7个值（即最近的7个邻居）
        ''' 2. 取其中第4-6小的值（索引4,5,6，即第5,6,7近的邻居）
        ''' 3. 计算这三个值的平均值
        ''' 4. 计算 sigma = avg * avg^T (外积)
        ''' 5. 返回 distances^2 / sigma
        ''' </summary>
        ''' <param name="distances">距离矩阵，形状 [N, N]</param>
        ''' <returns>归一化距离矩阵，形状 [N, N]</returns>
        Public Function Compute(distances As Tensor) As Tensor
            Const k = 7

            If distances.Rank <> 2 Then
                Throw New ArgumentException("Distances must be a 2D tensor")
            End If

            Dim n As Integer = distances.Shape(0)

            ' 1. 取负值后进行TopK（相当于取最小的k个值）
            ' tf.topk(distances.neg(), k, true).values
            Dim negDistances = distances.Neg()
            Dim topKResult = negDistances.TopK(k, True)
            Dim topKNearestDistances = topKResult.Values ' [N, 7]

            ' 2. 取第4-6个值（索引4,5,6）
            ' topKNearestDistances.slice([0, 4], [-1, -1])
            Dim fourthToSixthNearestDistances = topKNearestDistances.Slice(New Integer() {0, 4}, New Integer() {-1, -1}) ' [N, 3]

            ' 3. 计算平均值
            ' fourthToSixthNearestDistances.neg().sum(-1).div(tf.scalar(3)).expandDims(1)
            Dim avgFourthToSixth = TensorExtensions.Div(TensorExtensions.Sum(TensorExtensions.Neg(fourthToSixthNearestDistances), -1), 3.0).ExpandDims(1)           ' 取反恢复原始距离值
            ' 沿最后一维求和 [N]
            ' 除以3得到平均值
            ' [N, 1]

            ' 4. 计算sigma（外积）
            ' sigma = avg * avg^T
            Dim sigma = avgFourthToSixth.MatMul(avgFourthToSixth, False, True) ' [N, N]

            ' 5. 返回归一化距离
            ' distances.square().div(sigma)
            Dim result = TensorExtensions.Square(distances).Div(sigma)

            Return result
        End Function
    End Module
End Namespace
