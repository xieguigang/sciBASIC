#Region "Microsoft.VisualBasic::35b163e37b0ff58a64b738d0a4baa0fc, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drawing2D\Math2D\ConcaveHull\DelaunayMesh2d.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region

Imports System.Drawing

Namespace Drawing2D.Math2D.ConcaveHull

    Public Class DelaunayMesh2d
        Public Points As List(Of Point)
        Public Faces As List(Of Triangle)
        Public Edges As EdgeInfo(,)
        Public Sub New()
            Points = New List(Of Point)()
            Faces = New List(Of Triangle)()
        End Sub
        Public Function AddVertex(p As Point) As Integer
            Points.Add(p)
            Return Points.Count - 1
        End Function
        Public Function AddFace(t As Triangle) As Integer
            Faces.Add(t)
            Return Faces.Count - 1
        End Function
        Public Sub InitEdgesInfo()
            Edges = New EdgeInfo(Points.Count - 1, Points.Count - 1) {}
            For i As Integer = 0 To Points.Count - 1
                For j As Integer = 0 To Points.Count - 1
                    Edges(i, j) = New EdgeInfo(0)
                Next
            Next
            For i As Integer = 0 To Faces.Count - 1
                Dim t As Triangle = Faces(i)
                SetEdge(t, i)
            Next

        End Sub
        Private Sub SetEdge(t As Triangle, i As Integer)
            If t.P0Index < t.P1Index Then
                Edges(t.P0Index, t.P1Index).P0Index = t.P0Index
                Edges(t.P0Index, t.P1Index).P1Index = t.P1Index
                Edges(t.P0Index, t.P1Index).AdjTriangle.Add(i)
                Edges(t.P0Index, t.P1Index).Length = BallConcave.GetDistance(Points(t.P0Index), Points(t.P1Index))
            Else
                Edges(t.P1Index, t.P0Index).P0Index = t.P1Index
                Edges(t.P1Index, t.P0Index).P1Index = t.P0Index
                Edges(t.P1Index, t.P0Index).AdjTriangle.Add(i)
                Edges(t.P1Index, t.P0Index).Length = BallConcave.GetDistance(Points(t.P0Index), Points(t.P1Index))
            End If

            If t.P1Index < t.P2Index Then
                Edges(t.P1Index, t.P2Index).P0Index = t.P1Index
                Edges(t.P1Index, t.P2Index).P1Index = t.P2Index
                Edges(t.P1Index, t.P2Index).AdjTriangle.Add(i)
                Edges(t.P1Index, t.P2Index).Length = BallConcave.GetDistance(Points(t.P1Index), Points(t.P2Index))
            Else
                Edges(t.P2Index, t.P1Index).P0Index = t.P2Index
                Edges(t.P2Index, t.P1Index).P1Index = t.P1Index
                Edges(t.P2Index, t.P1Index).AdjTriangle.Add(i)
                Edges(t.P2Index, t.P1Index).Length = BallConcave.GetDistance(Points(t.P1Index), Points(t.P2Index))
            End If

            If t.P0Index < t.P2Index Then
                Edges(t.P0Index, t.P2Index).P0Index = t.P0Index
                Edges(t.P0Index, t.P2Index).P1Index = t.P2Index
                Edges(t.P0Index, t.P2Index).AdjTriangle.Add(i)
                Edges(t.P0Index, t.P2Index).Length = BallConcave.GetDistance(Points(t.P0Index), Points(t.P2Index))
            Else
                Edges(t.P2Index, t.P0Index).P0Index = t.P2Index
                Edges(t.P2Index, t.P0Index).P1Index = t.P0Index
                Edges(t.P2Index, t.P0Index).AdjTriangle.Add(i)
                Edges(t.P2Index, t.P0Index).Length = BallConcave.GetDistance(Points(t.P0Index), Points(t.P2Index))
            End If
        End Sub
        Public Sub ExecuteEdgeDecimation(length As Double)
            Dim queue As New Queue(Of EdgeInfo)()
            For i As Integer = 0 To Points.Count - 1
                For j As Integer = 0 To Points.Count - 1
                    If i < j AndAlso Edges(i, j).IsValid() Then
                        If Edges(i, j).GetEdgeType() = 0 Then
                            Throw New Exception()
                        End If
                        If Edges(i, j).Length > length AndAlso Edges(i, j).GetEdgeType() = 1 Then
                            queue.Enqueue(Edges(i, j))
                        End If
                    End If
                Next
            Next
            Dim opp1Temp As EdgeInfo() = New EdgeInfo(1) {}
            While queue.Count <> 0
                Dim info As EdgeInfo = queue.Dequeue()
                If info.AdjTriangle.Count <> 1 Then
                    Throw New Exception()
                End If
                Dim tindex As Integer = info.AdjTriangle(0)
                Dim t As Triangle = Faces(tindex)
                InitOppEdge(opp1Temp, t, info)
                SetInvalid(info.P0Index, info.P1Index)
                For i As Integer = 0 To 1
                    Dim e As EdgeInfo = opp1Temp(i)
                    e.AdjTriangle.Remove(tindex)
                    If e.GetEdgeType() = 0 Then
                        SetInvalid(e.P0Index, e.P1Index)
                    ElseIf e.GetEdgeType() = 1 AndAlso e.Length > length Then
                        queue.Enqueue(e)
                    End If
                Next
            End While
        End Sub
        Public Function GetBoundaryEdges() As List(Of EdgeInfo)
            Dim list As New List(Of EdgeInfo)()
            For i As Integer = 0 To Points.Count - 1
                For j As Integer = 0 To Points.Count - 1
                    If i < j Then
                        If Edges(i, j).GetEdgeType() = 1 Then
                            list.Add(Edges(i, j))
                        End If
                    End If
                Next
            Next
            Return list
        End Function
        Private Sub SetInvalid(i As Integer, j As Integer)
            Edges(i, j).AdjTriangle.Clear()
            Edges(i, j).Flag = True
            Edges(i, j).P0Index = -1
            Edges(i, j).P1Index = -1
        End Sub
        Private Sub InitOppEdge(opp1Temp As EdgeInfo(), t As Triangle, info As EdgeInfo)
            Dim vindex As Integer = t.P0Index + t.P1Index + t.P2Index - info.P0Index - info.P1Index
            If vindex < info.P0Index Then
                opp1Temp(0) = Edges(vindex, info.P0Index)
            Else
                opp1Temp(0) = Edges(info.P0Index, vindex)
            End If

            If vindex < info.P1Index Then
                opp1Temp(1) = Edges(vindex, info.P1Index)
            Else
                opp1Temp(1) = Edges(info.P1Index, vindex)
            End If
        End Sub
    End Class
End Namespace
