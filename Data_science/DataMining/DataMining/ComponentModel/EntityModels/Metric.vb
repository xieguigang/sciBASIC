Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.Correlations
Imports SIMD = Microsoft.VisualBasic.Math.SIMD

Namespace ComponentModel.EntityModels

    ''' <summary>
    ''' 按指定的属性名列表把 <see cref="EntityClusterModel"/> 投影为数值向量，
    ''' 并在其之上计算距离。
    ''' </summary>
    ''' <remarks>
    ''' 旧实现每次调用 <see cref="DistanceTo"/> 都执行两次
    ''' <c>x(propertyNames)</c>（从属性字典逐键取值并新建数组）。
    ''' 在 DBSCAN / KNN 这类 O(n²) 次距离调用的场景里，
    ''' 投影本身的开销会超过距离计算。
    ''' 现在投影结果按实例缓存在 <see cref="ConditionalWeakTable(Of TKey, TValue)"/> 中
    ''' （键被回收时缓存自动释放），因此每个样本只会被投影一次。
    ''' </remarks>
    Public Class Metric

        ReadOnly propertyNames As String()

        ReadOnly _vectors As New ConditionalWeakTable(Of EntityClusterModel, Double())

        Sub New(propertyNames As IEnumerable(Of String))
            Me.propertyNames = propertyNames.ToArray
        End Sub

        ''' <summary>
        ''' 取样本在当前属性投影下的数值向量（带实例级缓存）。
        ''' </summary>
        Public Function Project(entity As EntityClusterModel) As Double()
            Dim vector As Double() = Nothing

            If _vectors.TryGetValue(entity, vector) Then
                Return vector
            End If

            vector = entity(propertyNames)
            _vectors.Add(entity, vector)

            Return vector
        End Function

        ''' <summary>
        ''' 丢弃某个样本的投影缓存（样本属性被修改后调用）。
        ''' </summary>
        Public Sub Invalidate(entity As EntityClusterModel)
            If entity IsNot Nothing Then
                Call _vectors.Remove(entity)
            End If
        End Sub

        ''' <summary>
        ''' 欧氏距离。
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function DistanceTo(x As EntityClusterModel, y As EntityClusterModel) As Double
            Return SIMD.SimdDistance.Euclidean(Project(x), Project(y))
        End Function

        ''' <summary>
        ''' 欧氏距离的平方（聚类比较用平方即可，省掉一次开方）。
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function SquaredDistanceTo(x As EntityClusterModel, y As EntityClusterModel) As Double
            Return SIMD.SimdReduce.DistanceSquared(Project(x), Project(y))
        End Function

    End Class
End Namespace
