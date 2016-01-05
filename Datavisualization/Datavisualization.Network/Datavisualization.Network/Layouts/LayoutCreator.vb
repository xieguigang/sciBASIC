Imports System.Drawing
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace Layouts

    <PackageNamespace("Network.Layout.ForceDirected", Publisher:="xie.guigang@gmail.com")>
    Public Module LayoutCreator

        ''' <summary>
        ''' 假若两个Node之间存在连接则存在吸引力，否则两个节点之间只存在排斥力
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function CalculateForce(Node_a As Node, OtherNode As Node) As KeyValuePair(Of Double, Point)
            Dim value As Double 'value为力的大小，即为斜边
            Dim p = Array.IndexOf(Node_a.Neighbours, OtherNode.Id)
            Dim fr = CoulombForce(OtherNode.Neighborhoods, Node_a.Neighborhoods, EuclideanDistance(Node_a, OtherNode)) * 1000

            If p > -1 Then '目标节点与本节点还存在着连接，则两个节点之间还存在着吸引力
                value = CoulombForce(-10 * OtherNode.Neighborhoods, 10 * Node_a.Weights(p) * Node_a.Neighborhoods, EuclideanDistance(Node_a, OtherNode))
                value *= Node_a.Weights(p) * 1000
                value = fr - value
            Else
                value = fr
            End If

            If Double.IsInfinity(value) Then
                value = 1000
            End If

            Dim tanA = (Node_a.Location.Y - OtherNode.Location.Y)
            If Not tanA = 0 Then
                Dim d = (Node_a.Location.X - OtherNode.Location.X)
                If Not d = 0 Then
                    tanA = tanA / d
                Else
                    tanA = 100000
                End If
            End If

            Dim Angle = System.Math.Atan(tanA)
            Dim height = value * System.Math.Sin(Angle) / Node_a.Neighborhoods
            Dim width = value * System.Math.Cos(Angle) / Node_a.Neighborhoods

            Dim F = New Point(width, height)
            Return New KeyValuePair(Of Double, Point)(value, F)
        End Function

        Const K = 9 * 10 ^ 5

        Private Function CoulombForce(Q1 As Double, Q2 As Double, R As Double) As Double
            Dim F As Double = K * (Q1 * Q2) / R ^ 2
            Return F
        End Function

        ''' <summary>
        ''' 计算两个点之间的欧氏距离
        ''' </summary>
        ''' <param name="Node_a"></param>
        ''' <param name="Node_b"></param>
        ''' <returns></returns>
        <ExportAPI("Distance.Euclidean")>
        Public Function EuclideanDistance(Node_a As Node, Node_b As Node) As Double
            Dim value As Double = (Node_a.Location.X - Node_b.Location.X) ^ 2 + (Node_a.Location.Y - Node_b.Location.Y) ^ 2
            value = System.Math.Sqrt(value)
            Return value
        End Function

        Public Function Length(Point As Point) As Double
            Dim value As Double = (Point.X) ^ 2 + (Point.Y) ^ 2
            value = System.Math.Sqrt(value)
            Return value
        End Function

        ''' <summary>
        ''' 力的合成
        ''' </summary>
        ''' <param name="Points"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function ForceComposition(Points As Point()) As Point
            If Points.Count = 1 Then '只有一条力则直接返回了 
                Return Points(0)
            End If

            Dim CompositesForce As Point = ParallelogramLaw(Points(0), Points(1))
            For Each F In Points.Skip(2)
                CompositesForce = ParallelogramLaw(F, CompositesForce)
            Next

            Return CompositesForce
        End Function

        ''' <summary>
        ''' 使用平行四边形法则进行力的合成
        ''' </summary>
        ''' <param name="F1"></param>
        ''' <param name="F2"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("ParallelogramLaw")>
        Public Function ParallelogramLaw(F1 As Point, F2 As Point) As Point
            Dim h1 = Length(F1), h2 As Double = Length(F2)
            Dim sinA = F1.Y / h1, sinB = F2.Y / h2, cosA = F1.X / h1, cosB As Double = F2.X / h2
            Dim sinA_B = sinA * cosB - cosA * sinB
            Dim Angle As Double = System.Math.Sinh(sinA_B)  '计算出两个向量之间的夹角

            '两个向量间的夹角加上F2的夹角即为合成的力的夹角
            Angle += System.Math.Asin(sinB)

            Dim C = h1 ^ 2 + h2 ^ 2 + 2 * h1 * h2 * System.Math.Cos(Angle)
            C = System.Math.Sqrt(C)

            '求出小三角形的高
            Dim height = C * System.Math.Sin(Angle)
            Dim width = C * System.Math.Cos(Angle)
            '得到一个大三角形，斜边即为合成的力,即所要返回的向量
            Dim value = New Point(width, height)

            Return value
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Network"></param>
        ''' <param name="cutoff">请尽量选择100左右的数字</param>
        ''' <param name="_DEBUG_EXPORT">A directory for export the debug image.</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 1. 首先对每一个节点分配一个随机的位置
        ''' 2. 执行循环，计算排斥力，直到所有的节点的排斥力都满足一个阈值为止
        ''' </remarks>
        ''' 
        <ExportAPI("Layout.ForceDirected")>
        Public Function ForceDirectedLayout(Network As Network, Optional cutoff As Double = 100, Optional _DEBUG_EXPORT As String = "") As Network
            Dim FrameSize = Network.FrameSize
            Dim Central As Size = New Size(FrameSize.Width / 2, FrameSize.Height / 2)

            Network._NodesInnerList = (From Node In Network.Nodes Select Node.InitializeRandomizeLocation(FrameSize:=Central)).ToList

            Dim MaxWeightNode = (From node In Network Select node Order By node.Neighbours.Count Descending).First
            MaxWeightNode.Location = New Point(Central.Width, Central.Height)

            Dim W = FrameSize.Width, L As Double = FrameSize.Height
            Dim K = System.Math.Sqrt(W * L / Network.Nodes.Count)
            Dim t As Double = 1

            Dim i As Integer = 0

            For i = 0 To 10000 '限定计算制定次数的Loop，以防止出现死循环
                Dim ch As Integer = 0

                For Each Node In Network
                    'On Error Resume Next

                    Dim Direction As KeyValuePair(Of Double, Point)() = (From OtherNode In Network
                                                                         Where Not OtherNode.Equals(Node)
                                                                         Let result = CalculateForce(Node, OtherNode)
                                                                         Select result
                                                                         Order By result.Key Descending).ToArray

                    Dim FCollection As Integer() = (From item In Network.Edges Where item.Length < cutoff Select 1).ToArray
                    If FCollection.IsNullOrEmpty Then
                        ch += 1
                    Else
                        Dim F = ForceComposition((From item In Direction Select item.Value).ToArray)

                        If Node.Equals(MaxWeightNode) Then '尽量减少最大的节点的移动
                            F = New Point(F.X * 0.0085 * t, F.Y * 0.0085 * t)
                        Else
                            F = New Point(F.X * t, F.Y * t)
                        End If

                        Call Node.MoveBackwards(F, FrameSize:=FrameSize)
                    End If
                Next

                If ch = Network.Count Then  '网络中的所有节点已经满足了阈值的条件了
                    Exit For
                Else
                    t = 0.95 * t

                    If Not String.IsNullOrEmpty(_DEBUG_EXPORT) Then
                        Call NetworkVisualizer.DrawImage(Network, FrameSize).Save(String.Format("{0}/{1}.bmp", _DEBUG_EXPORT, i))
                    End If
                End If
            Next

            Return Network
        End Function
    End Module
End Namespace