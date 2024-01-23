Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Math.Correlations

Namespace Clustering

    ''' <summary>
    ''' use canopy method for measure the k for kmeans clustering
    ''' </summary>
    Public Class Canopy

        ''' <summary>
        ''' 进行聚类的点
        ''' </summary>
        ReadOnly m_points As List(Of ClusterEntity)
        ''' <summary>
        ''' 存储簇
        ''' </summary>
        ReadOnly clusters As List(Of Bisecting.Cluster)

        ''' <summary>
        ''' 阈值
        ''' </summary>
        Dim T2 As Double = -1
        Dim clnm As Integer

        ''' <summary>
        ''' 获取阈值T2
        ''' </summary>
        Public Overridable ReadOnly Property Threshold As Double
            Get
                Return T2
            End Get
        End Property

        Public ReadOnly Property K As Integer
            Get
                Return clusters.Count
            End Get
        End Property

        Public Sub New(dataSet As IEnumerable(Of ClusterEntity))
            m_points = dataSet.ToList
        End Sub

        ''' <summary>
        ''' 进行聚类，按照Canopy算法进行计算，将所有点进行聚类
        ''' </summary>
        ''' <returns></returns>
        Public Overridable Function cluster() As Integer
            Dim index As Integer

            T2 = AverageDistance(m_points)

            While m_points.Count <> 0 'point不为空
                Dim lCluster As New Bisecting.Cluster
                Dim basePoint As ClusterEntity = m_points(0) ' 基准点

                index = 0
                lCluster.addPoint(basePoint)
                m_points.RemoveAt(0)

                While index < m_points.Count
                    Dim anotherPoint = m_points(index)
                    Dim distance As Single = basePoint.DistanceTo(anotherPoint)

                    If distance <= T2 Then
                        lCluster.addPoint(anotherPoint)
                        m_points.RemoveAt(index)
                    Else
                        index += 1
                    End If
                End While

                clusters.Add(lCluster)
                clnm = clusters.Count
            End While

            Return clnm
        End Function

        ''' <summary>
        ''' 得到平均距离
        ''' </summary>
        ''' <param name="points"></param>
        ''' <returns></returns>
        Private Shared Function AverageDistance(points As List(Of ClusterEntity)) As Double
            Dim sum As Double = 0
            Dim pointSize = points.Count

            For i As Integer = 0 To pointSize - 1
                For j As Integer = 0 To pointSize - 1
                    If i = j Then
                        Continue For
                    End If

                    sum += points(i).DistanceTo(points(j)) ^ 2
                Next
            Next

            Dim distanceNumber As Integer = pointSize * (pointSize + 1) / 2
            ' 平均距离的1/8
            Dim T2 As Double = sum / distanceNumber / 32

            Return T2
        End Function
    End Class
End Namespace