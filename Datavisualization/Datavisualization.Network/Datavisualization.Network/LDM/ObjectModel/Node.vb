Imports System.Drawing

Public Class Node

    <DumpNode> Public Property Location As Point

    Public ReadOnly Property Id As Integer
        Get
            Return _Id
        End Get
    End Property

    ''' <summary>
    ''' 与本节点相连接的其他节点的<see cref="Node.Id">编号</see>
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <DumpNode> Public Property Neighbours As Integer()

    <DumpNode> Public ReadOnly Property DispName As String
        Get
            Return _DispName
        End Get
    End Property

    <DumpNode> Public Property Weights As Double()
    <DumpNode> Public Property Color As Color
    <DumpNode> Public Property Weight As Double

    Protected Friend _Id As Integer, _DispName As String
    Protected Friend _Visited As Boolean = False

    <DumpNode> Protected Friend _force As Point

    <DumpNode> Public ReadOnly Property Degree As Integer
        Get
            Return Neighbours.Count
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return String.Format("{0}|{1} =>  {2}; degree:= {3}", Id, DispName, Location.ToString, Degree)
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="FrameSize">绘制的区域的中间点</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Protected Friend Function InitializeRandomizeLocation(FrameSize As Size) As Node
        Me.Neighborhood = If(Me.Neighbours.IsNullOrEmpty, 1, Me.Neighbours.Count)

        Dim r = System.Math.Min(FrameSize.Width, FrameSize.Height) / (Neighborhood * 10)

        Call VBMath.Randomize() : Dim X As Integer = (RandomDouble() * r - r) + FrameSize.Width
        Call VBMath.Randomize() : Dim Y As Integer = (RandomDouble() * r - r) + FrameSize.Height

        Me.Location = New Point With {.X = X, .Y = Y}

        Return Me
    End Function

    Protected Friend Function SetLocation(Location As Point) As Node
        Me.Neighborhood = If(Me.Neighbours.IsNullOrEmpty, 1, Me.Neighbours.Count)
        Me.Location = New Point With {
            .X = Location.X,
            .Y = Location.Y
        }
        Return Me
    End Function

    Dim Neighborhood As Integer

    Public ReadOnly Property Neighborhoods As Integer
        Get
            Return Neighborhood
        End Get
    End Property

    ''' <summary>
    ''' 远离目标节点，运动范围被限制在框架内,假设力的大小就是节点所移动的距离
    ''' </summary>
    ''' <param name="F">使用平行四边形法则所合成的力</param>
    ''' <remarks></remarks>
    Protected Friend Sub MoveBackwards(F As Point, FrameSize As Point)
        Dim X = Me.Location.X + 0.1 * F.X / Neighborhood + (2 * RandomDouble() - 2)
        Dim Y = Me.Location.Y + 0.1 * F.Y / Neighborhood + (2 * RandomDouble() - 2)

        If X < 0 OrElse X > FrameSize.X OrElse Y < 0 OrElse Y > FrameSize.Y Then
            X = RandomDouble() * FrameSize.X
            Y = RandomDouble() * FrameSize.Y
        End If

        Me.Location = New Point With {.X = X, .Y = Y}
    End Sub
End Class
