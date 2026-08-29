#Region "Microsoft.VisualBasic::dd6740262a9bd57ac48daa69a470717b, Data_science\DataMining\Bonsai\test\Plot.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 400
    '    Code Lines: 291 (72.75%)
    ' Comment Lines: 64 (16.00%)
    '    - Xml Docs: 26.56%
    ' 
    '   Blank Lines: 45 (11.25%)
    '     File Size: 16.05 KB


    ' Module Plot
    ' 
    '     Function: ColorFromAHSB, ColorScale, CountLeaves, Dot, MatVec
    '               PCA2D, PowerIterate
    ' 
    '     Sub: AssignLayout, CollectAll, DrawLegendBar, PlotBranchTimeHistogram, PlotScatter
    '          PlotTree
    ' 
    ' /********************************************************************************/

#End Region

' /********************************************************************************/
'
'   Bonsai result visualization module (GDI+ / System.Drawing.Common)
'
'   Renders the outputs of the Bonsai high-dimensional reducer into PNG images so
'   the reconstruction can be visually inspected:
'     - PlotScatter            : low-dimensional embedding as a 2-D scatter (PCA-projected)
'     - PlotTree               : the reconstructed tree topology (rooted dendrogram)
'     - PlotBranchTimeHistogram: the 1-D branch-time (pseudotime-like) distribution
'
'   Pure presentation layer: depends only on the public Bonsai API surface
'   (Double()(), Double(), BonsaiNode).
'
' /********************************************************************************/

Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Collections.Generic
Imports Microsoft.VisualBasic.DataMining.Bonsai

Module Plot

    ' =============================================================================
    ' 2-D PCA projection (power iteration, no external linear-algebra dependency)
    ' =============================================================================

    ''' <summary>
    ''' Project an N x D matrix onto its two leading principal components and return
    ''' an N x 2 matrix. Uses covariance power-iteration (deflation) which is more than
    ''' adequate for a visualization projection.
    ''' </summary>
    Public Function PCA2D(X As Double()()) As Double()()
        Dim N = X.Length
        If N = 0 Then Return New Double(-1)() {}
        Dim D = X(0).Length

        If D = 1 Then
            ' Degenerate: pad the second axis with zeros.
            Dim out1(N - 1)() As Double
            For i = 0 To N - 1
                out1(i) = New Double() {X(i)(0), 0.0}
            Next
            Return out1
        End If

        ' column means
        Dim mean(D - 1) As Double
        For i = 0 To N - 1
            For g = 0 To D - 1
                mean(g) += X(i)(g)
            Next
        Next
        For g = 0 To D - 1
            mean(g) /= N
        Next

        ' covariance C = (1/(N-1)) * Xc^T Xc   (D x D)
        Dim C(D - 1)() As Double
        For a = 0 To D - 1
            C(a) = New Double(D - 1) {}
        Next
        For i = 0 To N - 1
            Dim row = X(i)
            For a = 0 To D - 1
                Dim ca = row(a) - mean(a)
                For b = a To D - 1
                    C(a)(b) += ca * (row(b) - mean(b))
                Next
            Next
        Next
        For a = 0 To D - 1
            For b = a To D - 1
                C(a)(b) /= (N - 1)
                If b <> a Then C(b)(a) = C(a)(b)
            Next
        Next

        ' first principal component via power iteration
        Dim v1 = PowerIterate(C)
        Dim lambda1 = Dot(v1, MatVec(C, v1))

        ' deflate and get the second component
        Dim C2(D - 1)() As Double
        For a = 0 To D - 1
            C2(a) = New Double(D - 1) {}
            For b = 0 To D - 1
                C2(a)(b) = C(a)(b) - lambda1 * v1(a) * v1(b)
            Next
        Next
        Dim v2 = PowerIterate(C2)

        ' project
        Dim out(N - 1)() As Double
        For i = 0 To N - 1
            out(i) = New Double() {Dot(X(i), v1), Dot(X(i), v2)}
        Next
        Return out
    End Function

    Private Function PowerIterate(C As Double()()) As Double()
        Dim D = C.Length
        Dim v(D - 1) As Double
        For g = 0 To D - 1
            v(g) = 0.5 - 0.5 * g / D   ' deterministic, non-zero seed
        Next
        For iter = 1 To 50
            Dim Av = MatVec(C, v)
            Dim norm = System.Math.Sqrt(Dot(Av, Av))
            If norm < 1E-12 Then Exit For
            For g = 0 To D - 1
                v(g) = Av(g) / norm
            Next
        Next
        Return v
    End Function

    Private Function MatVec(C As Double()(), v As Double()) As Double()
        Dim D = C.Length
        Dim r(D - 1) As Double
        For a = 0 To D - 1
            Dim s = 0.0
            For b = 0 To D - 1
                s += C(a)(b) * v(b)
            Next
            r(a) = s
        Next
        Return r
    End Function

    Private Function Dot(a As Double(), b As Double()) As Double
        Dim s = 0.0
        For i = 0 To a.Length - 1
            s += a(i) * b(i)
        Next
        Return s
    End Function

    ' =============================================================================
    ' color helper: map t in [0,1] to a blue -> red gradient
    ' =============================================================================

    Private Function ColorScale(t As Double) As Color
        If t < 0 Then t = 0
        If t > 1 Then t = 1
        ' hue 240 (blue) -> 0 (red)
        Return ColorFromAHSB(255, CInt(240 * (1 - t)), 200, 130)
    End Function

    Private Function ColorFromAHSB(alpha As Integer, hue As Integer, saturation As Integer, brightness As Integer) As Color
        ' minimal HSL->RGB (GDI+ Color.FromArgb does not expose HSL directly)
        Dim h = hue Mod 360
        Dim s = saturation / 255.0
        Dim l = brightness / 255.0
        Dim c = (1 - System.Math.Abs(2 * l - 1)) * s
        Dim x = c * (1 - System.Math.Abs((h / 60.0) Mod 2 - 1))
        Dim m = l - c / 2.0
        Dim r = 0.0, g = 0.0, bl = 0.0
        If h < 60 Then
            r = c : g = x
        ElseIf h < 120 Then
            r = x : g = c
        ElseIf h < 180 Then
            g = c : bl = x
        ElseIf h < 240 Then
            g = x : bl = c
        ElseIf h < 300 Then
            r = x : bl = c
        Else
            r = c : bl = x
        End If
        Return Color.FromArgb(alpha,
            CInt((r + m) * 255),
            CInt((g + m) * 255),
            CInt((bl + m) * 255))
    End Function

    ' =============================================================================
    ' 1) scatter plot of the low-dimensional embedding
    ' =============================================================================

    ''' <summary>
    ''' Draw a 2-D scatter of <paramref name="coords2d"/> (the PCA-projected embedding),
    ''' coloring each point by its branch time (normalized over <paramref name="branchTimes"/>).
    ''' </summary>
    Public Sub PlotScatter(coords2d As Double()(), labels As String(), branchTimes As Double(), filePath As String)
        Const W = 900, H = 700, pad = 60
        Using bmp As New Bitmap(W, H)
            Using g As Graphics = Graphics.FromImage(bmp)
                g.SmoothingMode = Drawing2D.SmoothingMode.HighQuality
                g.Clear(Color.White)

                Dim N = coords2d.Length
                Dim minX = Double.MaxValue, maxX = Double.MinValue
                Dim minY = Double.MaxValue, maxY = Double.MinValue
                For i = 0 To N - 1
                    minX = System.Math.Min(minX, coords2d(i)(0))
                    maxX = System.Math.Max(maxX, coords2d(i)(0))
                    minY = System.Math.Min(minY, coords2d(i)(1))
                    maxY = System.Math.Max(maxY, coords2d(i)(1))
                Next
                Dim rx = If(maxX - minX > 0, maxX - minX, 1)
                Dim ry = If(maxY - minY > 0, maxY - minY, 1)

                Dim tMin = Double.MaxValue, tMax = Double.MinValue
                For i = 0 To branchTimes.Length - 1
                    tMin = System.Math.Min(tMin, branchTimes(i))
                    tMax = System.Math.Max(tMax, branchTimes(i))
                Next
                Dim tr = If(tMax - tMin > 0, tMax - tMin, 1)

                ' axes
                g.DrawLine(Pens.Gray, pad, H - pad, W - pad, H - pad)
                g.DrawLine(Pens.Gray, pad, pad, pad, H - pad)
                g.DrawString("PC1", New Font("Consolas", 11), Brushes.Black, W - pad - 30, H - pad + 6)
                g.DrawString("PC2", New Font("Consolas", 11), Brushes.Black, 6, pad - 18)

                ' points
                For i = 0 To N - 1
                    Dim px = pad + (coords2d(i)(0) - minX) / rx * (W - 2 * pad)
                    Dim py = (H - pad) - (coords2d(i)(1) - minY) / ry * (H - 2 * pad)
                    Dim tt = (branchTimes(i) - tMin) / tr
                    Using br As New SolidBrush(ColorScale(tt))
                        g.FillEllipse(br, New RectangleF(CSng(px - 3.5), CSng(py - 3.5), 7, 7))
                    End Using
                Next

                ' title
                g.DrawString("Bonsai low-dim embedding (PCA 2-D, colored by branch-time)",
                    New Font("Consolas", 12, FontStyle.Bold), Brushes.Black, pad, 14)

                ' color legend bar
                DrawLegendBar(g, W - pad - 160, 40, 150, 12, tMin, tMax)
            End Using
            bmp.Save(filePath, ImageFormat.Png)
        End Using
    End Sub

    Private Sub DrawLegendBar(g As Graphics, x As Integer, y As Integer, w As Integer, h As Integer, tMin As Double, tMax As Double)
        For i = 0 To w - 1
            Using br As New SolidBrush(ColorScale(i / w))
                g.FillRectangle(br, x + i, y, 1, h)
            End Using
        Next
        g.DrawRectangle(Pens.Black, x, y, w, h)
        g.DrawString(tMin.ToString("G3"), New Font("Consolas", 9), Brushes.Black, x, y + h + 2)
        g.DrawString(tMax.ToString("G3"), New Font("Consolas", 9), Brushes.Black, x + w - 30, y + h + 2)
        g.DrawString("branch-time", New Font("Consolas", 9, FontStyle.Italic), Brushes.Black, x, y - 14)
    End Sub

    ' =============================================================================
    ' 2) tree topology (rooted dendrogram, depth = cumulative branch time)
    ' =============================================================================

    ''' <summary>
    ''' Render the reconstructed Bonsai tree. Each node's x is its cumulative branch time
    ''' from the root; leaves are laid out top-to-bottom and internal nodes centred on their
    ''' children. Edge lengths thus reflect the optimized diffusion (branch) times.
    ''' </summary>
    Public Sub PlotTree(root As BonsaiNode, filePath As String)
        Const W = 1000, H = 760, padL = 40, padR = 220, padT = 40, padB = 40

        ' layout: assign cumulative x (depth) and leaf y slots
        Dim leafSlots = 0
        AssignLayout(root, 0.0, leafSlots)

        ' gather nodes + bounds
        Dim nodes As New List(Of BonsaiNode)
        CollectAll(root, nodes)
        Dim maxDepth = 0.0
        For Each n In nodes
            If n.x > maxDepth Then maxDepth = n.x
        Next
        If maxDepth <= 0 Then maxDepth = 1

        Using bmp As New Bitmap(W, H)
            Using g As Graphics = Graphics.FromImage(bmp)
                g.SmoothingMode = Drawing2D.SmoothingMode.HighQuality
                g.Clear(Color.White)

                Dim plotW = W - padL - padR
                Dim plotH = H - padT - padB
                Dim nLeaves = CountLeaves(root)
                Dim slotH = plotH / Math.Max(nLeaves, 1)

                ' map helpers
                Dim ToX = Function(d As Double) CSng(padL + d / maxDepth * plotW)
                Dim ToY = Function(s As Double) CSng(padT + (s + 0.5) * slotH)

                ' edges
                Using pen As New Pen(Color.DarkSlateGray, 1.5F)
                    For Each n In nodes
                        If n.par IsNot Nothing Then
                            g.DrawLine(pen, ToX(n.par.x), ToY(n.par.y), ToX(n.x), ToY(n.y))
                        End If
                    Next
                End Using

                ' nodes
                For Each n In nodes
                    Dim px = ToX(n.x), py = ToY(n.y)
                    If n.isLeafNode() Then
                        g.FillEllipse(Brushes.SteelBlue, New RectangleF(px - 3, py - 3, 6, 6))
                    Else
                        g.FillEllipse(Brushes.Black, New RectangleF(px - 3.5, py - 3.5, 7, 7))
                        g.DrawString(n.nodeId, New Font("Consolas", 8), Brushes.Black, px + 4, py - 6)
                    End If
                Next

                g.DrawString("Bonsai tree (x = cumulative branch time)",
                    New Font("Consolas", 12, FontStyle.Bold), Brushes.Black, padL, 12)
            End Using
            bmp.Save(filePath, ImageFormat.Png)
        End Using
    End Sub

    ' depth-first: set n.x (cumulative) and n.y (leaf slot / child average)
    Private Sub AssignLayout(node As BonsaiNode, depthAccum As Double, ByRef leafCounter As Integer)
        node.x = depthAccum
        If node.isLeafNode() Then
            node.y = leafCounter
            leafCounter += 1
        Else
            Dim sum = 0.0
            For Each c In node.childs
                AssignLayout(c, depthAccum + c.tParent, leafCounter)
                sum += c.y
            Next
            node.y = If(node.childs.Count > 0, sum / node.childs.Count, leafCounter)
        End If
    End Sub

    Private Sub CollectAll(n As BonsaiNode, out As List(Of BonsaiNode))
        out.Add(n)
        For Each c In n.childs
            CollectAll(c, out)
        Next
    End Sub

    Private Function CountLeaves(n As BonsaiNode) As Integer
        If n.isLeafNode() Then Return 1
        Dim s = 0
        For Each c In n.childs
            s += CountLeaves(c)
        Next
        Return s
    End Function

    ' =============================================================================
    ' 3) branch-time histogram
    ' =============================================================================

    ''' <summary>
    ''' Histogram of the per-leaf branch-time (tree depth) distribution.
    ''' </summary>
    Public Sub PlotBranchTimeHistogram(times As Double(), filePath As String)
        Const W = 900, H = 560, pad = 60, bins = 30
        Using bmp As New Bitmap(W, H)
            Using g As Graphics = Graphics.FromImage(bmp)
                g.SmoothingMode = Drawing2D.SmoothingMode.HighQuality
                g.Clear(Color.White)

                Dim n = times.Length
                If n = 0 Then
                    bmp.Save(filePath, ImageFormat.Png)
                    Return
                End If
                Dim tMin = times.Min(), tMax = times.Max()
                Dim span = If(tMax - tMin > 0, tMax - tMin, 1)
                Dim counts(bins - 1) As Integer
                For i = 0 To n - 1
                    Dim b = CInt((times(i) - tMin) / span * (bins - 1))
                    If b < 0 Then b = 0
                    If b >= bins Then b = bins - 1
                    counts(b) += 1
                Next
                Dim maxC = 1
                For i = 0 To bins - 1
                    maxC = System.Math.Max(maxC, counts(i))
                Next

                g.DrawLine(Pens.Gray, pad, H - pad, W - pad, H - pad)
                g.DrawLine(Pens.Gray, pad, pad, pad, H - pad)

                Dim bw = (W - 2 * pad) / bins
                For i = 0 To bins - 1
                    Dim bh = counts(i) / maxC * (H - 2 * pad)
                    g.FillRectangle(Brushes.MediumSeaGreen,
                        CSng(pad + i * bw), CSng(H - pad - bh), CSng(bw - 1), CSng(bh))
                Next

                g.DrawString("Bonsai branch-time distribution (bin count = " & bins & ")",
                    New Font("Consolas", 12, FontStyle.Bold), Brushes.Black, pad, 14)
                g.DrawString("branch-time", New Font("Consolas", 10), Brushes.Black, W - pad - 70, H - pad + 6)
                g.DrawString("count", New Font("Consolas", 10), Brushes.Black, 6, pad - 18)
            End Using
            bmp.Save(filePath, ImageFormat.Png)
        End Using
    End Sub

End Module

