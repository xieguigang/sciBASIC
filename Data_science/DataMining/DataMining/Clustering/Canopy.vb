Namespace Clustering

    Public Class Canopy

        ''' <summary>
        ''' 进行聚类的点
        ''' </summary>
        Private pointsField As List(Of Single()) = CType(New List(Of Single())(), List(Of Single()))
        ''' <summary>
        ''' 存储簇
        ''' </summary>
        Private clusters As List(Of List(Of Single())) = CType(New List(Of List(Of Single()))(), List(Of List(Of Single())))

        ''' <summary>
        ''' 阈值
        ''' </summary>
        Private T2 As Double = -1
        Friend clnm As Integer

        Public Sub New(dataSet As List(Of Single()))
            pointsField = New List(Of Single())(dataSet)
        End Sub
        Public Sub New()

        End Sub

        ''' <summary>
        ''' 进行聚类，按照Canopy算法进行计算，将所有点进行聚类
        ''' </summary>
        ''' <returns></returns>
        Public Overridable Function cluster() As Integer
            '        T2 = 100;
            T2 = getAverageDistance(pointsField)
            Console.WriteLine(T2)
            While pointsField.Count <> 0 'point不为空

                Dim lCluster As List(Of Single()) = CType(New List(Of Single())(), List(Of Single()))

                Dim basePoint = pointsField(0) ' 基准点
                lCluster.Add(basePoint)
                pointsField.RemoveAt(0)
                Dim index = 0
                While index < pointsField.Count
                    Dim anotherPoint = pointsField(index)

                    ' double distance = Math.sqrt((basePoint.x – anotherPoint.x)
                    ' 
                    '  (basePoint.x – anotherPoint.x)
                    ' 
                    ' + (basePoint.y – anotherPoint.y)
                    ' 
                    '  (basePoint.y – anotherPoint.y));
                    Dim sum As Single = 0
                    For i = 1 To pointsField(0).Length - 1
                        Dim temp As Single = 0
                        temp = basePoint(i) - anotherPoint(i)
                        sum += temp * temp

                    Next
                    Dim distance As Single = Math.Sqrt(sum)

                    If distance <= T2 Then
                        lCluster.Add(anotherPoint)
                        pointsField.RemoveAt(index)
                    Else
                        index += 1
                    End If
                End While
                clusters.Add(lCluster)
                clnm = lCluster.Count
            End While
            Return clnm
        End Function

        ''' <summary>
        ''' 获取Cluster对应的中心点(各点相加求平均)
        ''' </summary>
        Public Overridable ReadOnly Property ClusterCenterPoints As List(Of Single())
            Get

                Dim centerPoints As List(Of Single()) = New List(Of Single())()
                ' for (i=0;i<centerPoints.get(0).length;i++) {
                ' 
                ' centerPoints.add(getCenterPoint((ArrayList<float[]>) cluster));
                ' 
                ' }
                Return centerPoints

            End Get
        End Property

        ''' <summary>
        ''' 得到平均距离
        ''' </summary>
        ''' <paramname="points"></param>
        ''' <returns></returns>
        Private Function getAverageDistance(points As List(Of Single())) As Double

            Dim sum As Double = 0
            Dim pointSize = points.Count
            For i = 0 To pointSize - 1
                For j = 0 To pointSize - 1
                    If i = j Then
                        Continue For
                    End If
                    Dim pointA = points(i)
                    Dim pointB = points(j)
                    For k = 1 To points(0).Length - 1
                        Dim temp As Single
                        temp = pointA(k) - pointB(k)
                        sum += temp * temp
                    Next
                    ' sum += Math.sqrt((pointA.x – pointB.x) * (pointA.x – pointB.x)
                    ' + (pointA.y – pointB.y) * (pointA.y – pointB.y));
                Next
            Next

            Dim distanceNumber As Integer = pointSize * (pointSize + 1) / 2

            Dim T2 = sum / distanceNumber / 32 ' 平均距离的1/8

            Return T2

        End Function

        ''' <summary>
        ''' 得到的中心点(各点相加求平均)
        ''' </summary>
        ''' <paramname="points"></param>
        ''' <returns></returns>
        Private Function getCenterPoint(points As List(Of Single())) As Single()

            ' double sumX = 0;
            ' 
            ' double sumY = 0;
            ' 
            ' for (flout[] point : points) {
            ' 
            ' sumX += point.x;
            ' 
            ' sumY += point.y;
            ' 
            ' }
            ' 
            ' int clusterSize = points.size();
            ' 
            ' Point centerPoint = new Point(sumX / clusterSize, sumY / clusterSize);
            Dim pointlengths = points(0).Length
            Dim centerPoint = New Single(pointlengths - 1) {}
            For i = 0 To points.Count - 1
                Dim sum = New Single(pointlengths - 1) {}
                For j = 1 To pointlengths - 1
                    sum(j) += points(i)(j)

                Next
                While i = pointlengths
                    For k = 1 To pointlengths - 1
                        centerPoint(k) = sum(k) / points.Count

                    Next

                End While
            Next
            Return centerPoint
        End Function

        ''' <summary>
        ''' 获取阈值T2
        ''' </summary>
        Public Overridable ReadOnly Property Threshold As Double
            Get

                Return T2

            End Get
        End Property
        Public Overridable Property Points As List(Of Single())
            Get
                Return pointsField
            End Get
            Set(value As List(Of Single()))
                pointsField = value
            End Set
        End Property


    End Class
End Namespace