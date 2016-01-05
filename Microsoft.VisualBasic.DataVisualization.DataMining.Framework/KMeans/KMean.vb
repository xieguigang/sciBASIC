Imports System.Data
Imports System.Configuration
Imports System.Web
Imports System.Drawing
Imports System.Collections

Namespace KMeans

    Public Structure Point
        Public Cluster As Integer
        Public X As Integer
        Public Y As Integer
        Public Count As Integer

        Public Sub New(pCluster As Integer, pX As Integer, pY As Integer)
            Cluster = pCluster
            X = pX
            Y = pY
            Count = 0
        End Sub

        Public Sub New(p As Point)
            Me.New(p.Cluster, p.X, p.Y)
        End Sub
    End Structure

    ''' <summary>
    ''' K-Means Clustering Used in Intention Based Scoring Projects
    ''' saharkiz, 4 Jan 2009 CPOL
    ''' The use of K-Means clustering for data mining purposes.
    ''' http://www.codeproject.com/Articles/32199/K-Means-Clustering-Used-in-Intention-Based-Scoring
    ''' </summary>
    ''' <remarks></remarks>
    Public Class KMean

        Dim _data As New ArrayList()
        Dim _centres As Point() = New Point(2) {}

        '         * 
        '         *  ###################################################################
        '         *  FUNCTIONS
        '         *  + kMeanCluster:
        '         *  + dist: calculate distance
        '         *  + min2: return minimum value between two numbers
        '         *  ###################################################################
        '         * 
        '         *  input: + Data matrix (0 to 2, 1 to TotalData); Row 0 = cluster, 1 =X, 2= Y; data in columns
        '         *         + numCluster: number of cluster user want the data to be clustered
        '         *         + private variables: Centroid, TotalData
        '         *  ouput: o) update centroid
        '         *         o) assign cluster number to the Data (= row 0 of Data)
        '         * 
        '         

        Private Sub kMeanCluster()
            Dim running As [Boolean] = True
            Dim min As Double = 10000000.0
            ' big number
            Dim distance__1 As Double = 0.0
            Dim cluster As Integer = 0
            Dim p1 As Point = CType(_data(_data.Count - 1), Point)
            For Each p2 As Point In _centres
                distance__1 = Distance(p1, p2)
                If distance__1 < min Then
                    min = distance__1
                    cluster = p2.Cluster
                End If
            Next
            p1 = CType(_data(_data.Count - 1), Point)
            p1.Cluster = cluster
            _data(_data.Count - 1) = p1

            Dim sums As Point()
            Dim c As Integer
            Dim d As Integer

            While running
                'this loop will surely convergent
                'calculate new centroids
                sums = New Point(2) {}
                For c = 0 To _data.Count - 1
                    p1 = CType(_data(c), Point)
                    sums(p1.Cluster).X += p1.X
                    sums(p1.Cluster).Y += p1.Y
                    sums(p1.Cluster).Count += 1
                Next
                For c = 0 To _centres.GetUpperBound(0)
                    If sums(c).Count > 0 Then
                        _centres(c).X = sums(c).X \ sums(c).Count
                        _centres(c).Y = sums(c).Y \ sums(c).Count
                    End If
                Next

                running = False

                For c = 0 To _data.Count - 1
                    min = 10000000.0
                    'big number
                    For d = 0 To _centres.GetUpperBound(0)
                        distance__1 = Distance(CType(_data(c), Point), _centres(d))
                        If distance__1 < min Then
                            min = distance__1
                            cluster = d
                        End If
                    Next
                    p1 = CType(_data(c), Point)
                    If p1.Cluster <> cluster Then
                        p1.Cluster = cluster
                        running = True
                        _data(c) = p1
                    End If
                Next
            End While
        End Sub

        Private Function Dist(pX1 As Integer, pY1 As Integer, pX2 As Integer, pY2 As Integer) As Double
            'calculate Euclidean distance
            Return Math.Sqrt(Math.Pow((pY2 - pY1), 2) + Math.Pow((pX2 - pX1), 2))
        End Function

        Private Function Distance(pP1 As Point, pP2 As Point) As Double
            Return Dist(pP1.X, pP1.Y, pP2.X, pP2.Y)
        End Function
    End Class
End Namespace
