Imports System.Drawing

Namespace Drawing2D.Math2D.ConcaveHull

    Public Structure TriangleIndex
        Public vv0 As Long
        Public vv1 As Long
        Public vv2 As Long

        Public Overrides Function ToString() As String
            Return {vv0, vv1, vv2}.JoinBy(" - ")
        End Function
    End Structure

    Public Structure TriangleVertex
        Public p0 As Point
        Public p1 As Point
        Public p2 As Point
    End Structure

    Public Structure Triangle

        Public P0Index As Integer
        Public P1Index As Integer
        Public P2Index As Integer
        Public Index As Integer

        Public Sub New(p0index As Integer, p1index As Integer, p2index As Integer)
            Me.P0Index = p0index
            Me.P1Index = p1index
            Me.P2Index = p2index
            Me.Index = -1
        End Sub

        Public Sub New(p0index As Integer, p1index As Integer, p2index As Integer, index As Integer)
            Me.P0Index = p0index
            Me.P1Index = p1index
            Me.P2Index = p2index
            Me.Index = index
        End Sub
    End Structure

    Public Structure EdgeInfo

        Public P0Index As Integer
        Public P1Index As Integer
        Public AdjTriangle As List(Of Integer)
        Public Flag As Boolean
        Public Length As Double

        Public Function GetEdgeType() As Integer
            Return AdjTriangle.Count
        End Function

        Public Function IsValid() As Boolean
            Return P0Index <> -1
        End Function

        Public Sub New(d As Integer)
            P0Index = -1
            P1Index = -1
            Flag = False
            AdjTriangle = New List(Of Integer)()
            Length = -1
        End Sub
    End Structure
End Namespace