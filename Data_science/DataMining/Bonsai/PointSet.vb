#Region "Microsoft.VisualBasic::db126736053df3b43ec381a6846b53fd, Data_science\DataMining\Bonsai\PointSet.vb"

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

    '   Total Lines: 235
    '    Code Lines: 117 (49.79%)
    ' Comment Lines: 90 (38.30%)
    '    - Xml Docs: 71.11%
    ' 
    '   Blank Lines: 28 (11.91%)
    '     File Size: 8.80 KB


    ' Class PointSet
    ' 
    '     Properties: useGlobalVariance
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: EstimateGeneVariance, FilterBySNR, FromMatrix, GetMean, GetSNR
    '               GetStd, GetVar
    ' 
    ' /********************************************************************************/

#End Region

' /********************************************************************************/

'   Author:
'
'       xie (genetics@smrucc.org)
'
'   Copyright (c) 2026 GPL3 Licensed
'
'
'   GNU GENERAL PUBLIC LICENSE (GPL3)
'
'
'   This program is free software: you can redistribute it and/or modify
'   it under the terms of the GNU General Public License as published by
'   the Free Software Foundation, either version 3 of the License, or
'   (at your option) any later version.
'
'   This program is distributed in the hope that it will be useful,
'   but WITHOUT ANY WARRANTY; without even the implied warranty of
'   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'   GNU General Public License for more details.
'
'   You should have received a copy of the GNU General Public License
'   along with this program. If not,, see <http://www.gnu.org/licenses/>.

' /********************************************************************************/

Imports System.Runtime.CompilerServices

''' <summary>
''' A point set of high-dimensional observations, stripped of all single-cell biology
''' semantics. Each sample is represented by a mean vector (coords) and a per-dimension
''' standard deviation (uncertainty). This mirrors the Bonsai ``SCData`` abstraction but
''' only keeps the pure numeric content required by the tree-reconstruction core.
''' </summary>
Public Class PointSet

    ''' <summary>
    ''' N x D matrix of sample means. Row i is the D-dimensional coordinate of sample i.
    ''' </summary>
    Public ReadOnly means As Double()()

    ''' <summary>
    ''' N x D matrix of per-dimension standard deviations. <see cref="stds"/>(i, g) is the
    ''' uncertainty on <see cref="means"/>(i, g). When nothing is known these default to 1.
    ''' </summary>
    Public ReadOnly stds As Double()()

    ''' <summary>
    ''' N sample labels (optional, used for Newick export and result annotation).
    ''' </summary>
    Public ReadOnly names As String()

    ''' <summary>
    ''' Number of samples (rows).
    ''' </summary>
    Public ReadOnly nSamples As Integer

    ''' <summary>
    ''' Number of dimensions per sample (columns).
    ''' </summary>
    Public ReadOnly nGenes As Integer

    ''' <summary>
    ''' Per-dimension variances, i.e. <see cref="stds"/> squared. Pre-computed for speed.
    ''' </summary>
    Public ReadOnly vars As Double()()

    ''' <summary>
    ''' Per-dimension global gene variance v_g, estimated as the sample variance of each dimension across
    ''' all cells: v_g = (1/(N-1)) * Σ_i (means(i,g) - x_g)^2. Used as the optional diffusion prior
    ''' (which assumes the diffusion magnitude along dimension g is proportional to v_g) described in the
    ''' Bonsai paper (equation 4). When <see cref="useGlobalVariance"/> is off, the likelihood reverts to
    ''' the default behaviour of using the per-cell measurement error only.
    ''' </summary>
    Public ReadOnly geneVariance As Double()

    ''' <summary>
    ''' When true, the likelihood uses v_g * tParent (global gene variance scaled diffusion) instead of the
    ''' plain measurement-error variance in the transition variance. Default off, so existing numerical
    ''' results are preserved unless explicitly enabled.
    ''' </summary>
    Public Property useGlobalVariance As Boolean = False

    Sub New(means As Double()(), Optional stds As Double()() = Nothing, Optional names As String() = Nothing)
        Me.means = means
        Me.nSamples = means.Length
        Me.nGenes = If(nSamples > 0, means(0).Length, 0)

        If stds Is Nothing Then
            Me.stds = means.Select(Function(row) row.Select(Function(r) 1.0).ToArray).ToArray
        Else
            Me.stds = stds
        End If

        Me.vars = Me.stds _
            .Select(Function(row) row.Select(Function(s) s * s).ToArray) _
            .ToArray

        Me.geneVariance = EstimateGeneVariance(means)

        If names Is Nothing Then
            Me.names = Enumerable.Range(0, nSamples) _
                .Select(Function(i) "s" & i) _
                .ToArray
        Else
            Me.names = names
        End If
    End Sub

    ''' <summary>
    ''' Per-dimension global variance v_g = (1/(N-1)) Σ_i (means(i,g) - x_g)^2, the empirical spread of each
    ''' gene across all samples. Mirrors the gene-variance prior used by Bonsai.
    ''' </summary>
    Private Shared Function EstimateGeneVariance(means As Double()()) As Double()
        Dim N = means.Length
        Dim D = If(N > 0, means(0).Length, 0)
        Dim vg(D - 1) As Double
        If N < 2 OrElse D = 0 Then
            For g = 0 To D - 1
                vg(g) = 1.0
            Next
            Return vg
        End If

        For g = 0 To D - 1
            Dim meanG = 0.0
            For i = 0 To N - 1
                meanG += means(i)(g)
            Next
            meanG /= N
            Dim ss = 0.0
            For i = 0 To N - 1
                Dim delta = means(i)(g) - meanG
                ss += delta * delta
            Next
            vg(g) = ss / (N - 1)
            If vg(g) <= 0.0 Then vg(g) = 1.0
        Next
        Return vg
    End Function

    ''' <summary>
    ''' Signal-to-noise ratio of each dimension:
    '''   S_g = (1/C) * Σ_i (means(i,g) - x_g)^2 / ε²_gi
    ''' where ε²_gi is the measurement variance (stds squared) of cell i on gene g. Following the Bonsai
    ''' paper, only dimensions with S_g >= <paramref name="threshold"/> are kept for tree construction.
    ''' </summary>
    Public Function GetSNR(Optional threshold As Double = 1.0) As Double()
        Dim N = nSamples
        Dim D = nGenes
        Dim snr(D - 1) As Double
        For g = 0 To D - 1
            Dim meanG = 0.0
            For i = 0 To N - 1
                meanG += means(i)(g)
            Next
            meanG /= N
            Dim acc = 0.0
            For i = 0 To N - 1
                Dim num = (means(i)(g) - meanG) ^ 2
                Dim den = vars(i)(g)
                If den <= 0.0 Then den = Likelihood.EPS
                acc += num / den
            Next
            snr(g) = acc / N
        Next
        Return snr
    End Function

    ''' <summary>
    ''' Return a copy of this point set that keeps only the dimensions whose signal-to-noise ratio S_g is at
    ''' least <paramref name="threshold"/> (Bonsai keeps S_g >= 1 by default). Returns the original set
    ''' unchanged when every dimension passes.
    ''' </summary>
    Public Function FilterBySNR(Optional threshold As Double = 1.0) As PointSet
        Dim snr = GetSNR(threshold)
        Dim keep As New List(Of Integer)
        For g = 0 To nGenes - 1
            If snr(g) >= threshold Then
                keep.Add(g)
            End If
        Next

        If keep.Count = nGenes Then
            Return Me
        End If

        ' If every dimension is rejected, fall back to keeping the original set so the tree can still be
        ' built (better a noisier tree than an empty one).
        If keep.Count = 0 Then
            Return Me
        End If

        Dim newMeans As Double()() = New Double(nSamples - 1)() {}
        Dim newStds As Double()() = New Double(nSamples - 1)() {}
        For i = 0 To nSamples - 1
            Dim ii = i
            newMeans(i) = keep.Select(Function(g) means(ii)(g)).ToArray
            newStds(i) = keep.Select(Function(g) stds(ii)(g)).ToArray
        Next
        Return New PointSet(newMeans, newStds, names)
    End Function

    ''' <summary>
    ''' Build a point set directly from a flat row-major array.
    ''' </summary>
    Public Shared Function FromMatrix(data As Double()(), Optional sd As Double()() = Nothing, Optional names As String() = Nothing) As PointSet
        Return New PointSet(data, sd, names)
    End Function

    ''' <summary>
    ''' Extract the mean vector of a single sample as a D-length array.
    ''' </summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function GetMean(i As Integer) As Double()
        Return means(i)
    End Function

    ''' <summary>
    ''' Extract the variance vector of a single sample as a D-length array.
    ''' </summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function GetVar(i As Integer) As Double()
        Return vars(i)
    End Function

    ''' <summary>
    ''' Convenience accessor for the standard deviation matrix.
    ''' </summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function GetStd(i As Integer) As Double()
        Return stds(i)
    End Function
End Class
