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
'   along with this program. If not, see <http://www.gnu.org/licenses/>.

' /********************************************************************************/

Imports System.Runtime.CompilerServices

''' <summary>
''' Public entry point for the Bonsai high-dimensional data visualisation. The API mirrors the
''' fit/transform style used by the sibling UMAP and t-SNE projects in this solution so that the three
''' reducers can be swapped transparently.
''' </summary>
Public Class Bonsai

    ''' <summary>
    ''' Reconstructed tree (available after <see cref="Fit"/>).
    ''' </summary>
    Public ReadOnly Property Tree As BonsaiTree

    ''' <summary>
    ''' Input point set (available after <see cref="Fit"/>).
    ''' </summary>
    Public ReadOnly Property Data As PointSet

    ''' <summary>
    ''' Maximum number of merge rounds during construction (0 = unlimited).
    ''' </summary>
    Public Property maxMerges As Integer = -1

    ''' <summary>
    ''' Maximum iterations for each <see cref="BonsaiTree.optTimes"/> call.
    ''' </summary>
    Public Property maxTimeIters As Integer = 20

    ''' <summary>
    ''' Verbosity flag for construction diagnostics.
    ''' </summary>
    Public Property verbose As Boolean = False

    ''' <summary>
    ''' When true (default), dimensions whose signal-to-noise ratio S_g &lt; <see cref="snrThreshold"/> are
    ''' dropped before tree construction, following the Bonsai paper's default of keeping only S_g >= 1 genes.
    ''' </summary>
    Public Property filterLowSNR As Boolean = True

    ''' <summary>
    ''' Signal-to-noise ratio cutoff used by <see cref="filterLowSNR"/>. Bonsai keeps S_g >= 1 by default.
    ''' </summary>
    Public Property snrThreshold As Double = 1.0

    ''' <summary>
    ''' When true, the likelihood uses the global gene-variance prior v_g as a diffusion scaling
    ''' (vars + v_g * tParent) instead of the default measurement-error-only transition variance.
    ''' </summary>
    Public Property useGlobalVariance As Boolean = False

    ''' <summary>
    ''' 2D layout style returned by <see cref="Transform"/> / <see cref="Get2DLayout"/>. Either "dendrogram"
    ''' (rectangular tree) or "radial".
    ''' </summary>
    Public Property layout As String = "dendrogram"

    ''' <summary>
    ''' The full (pre-filtering) point set, retained so downstream callers can still access the original
    ''' high-dimensional means alongside the reduced coordinates.
    ''' </summary>
    Public ReadOnly Property RawData As PointSet

    ''' <summary>
    ''' Fit a Bonsai tree to a high-dimensional point set.
    ''' </summary>
    ''' <param name="means">N x D matrix of sample means.</param>
    ''' <param name="stds">Optional N x D matrix of per-dimension standard deviations.</param>
    ''' <param name="names">Optional N sample labels.</param>
    Public Function Fit(means As Double()(), Optional stds As Double()() = Nothing, Optional names As String() = Nothing) As Bonsai
        Return FitCore(New PointSet(means, stds, names))
    End Function

    ''' <summary>
    ''' Fit directly from a <see cref="PointSet"/>.
    ''' </summary>
    Public Function Fit(data As PointSet) As Bonsai
        Return FitCore(data)
    End Function

    Private Function FitCore(raw As PointSet) As Bonsai
        raw.useGlobalVariance = Me.useGlobalVariance
        Me._RawData = raw

        Dim data = raw
        If filterLowSNR Then
            data = raw.FilterBySNR(snrThreshold)
            ' Preserve the variance-prior switch on the (possibly reduced) point set.
            data.useGlobalVariance = Me.useGlobalVariance
        End If
        Me._Data = data

        Me._Tree = BonsaiTree.Build(data, maxMerges, maxTimeIters, verbose)
        Return Me
    End Function

    ''' <summary>
    ''' Two-dimensional embedding for every sample: the tree layout produced from the topology and branch
    ''' lengths (the distortion-free 2D visualisation that distinguishes Bonsai from UMAP / t-SNE). The order
    ''' of the returned rows matches <see cref="GetHighDimStates"/> and the input samples.
    ''' </summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Transform() As Double()()
        Return Get2DLayout()
    End Function

    ''' <summary>
    ''' Two-dimensional coordinates of every leaf from the selected tree layout.
    ''' </summary>
    Public Function Get2DLayout() As Double()()
        If layout = "radial" Then
            Return TreeLayout.RadialLayout(Tree.root)
        Else
            Return TreeLayout.DendrogramLayout(Tree.root)
        End If
    End Function

    ''' <summary>
    ''' High-dimensional effective state of every sample (the per-dimension leaf positions <see cref="BonsaiNode.ltqs"/>,
    ''' in the gene space after any SNR filtering). Intended for downstream analysis; for the 2D visualisation
    ''' use <see cref="Transform"/>.
    ''' </summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function GetHighDimStates() As Double()()
        Return Tree.GetLowDimCoords()
    End Function

    ''' <summary>
    ''' One-dimensional branch-time coordinate (tree depth) per sample, useful as a pseudotime-like axis.
    ''' </summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function BranchTimeCoords() As Double()
        Return Tree.GetBranchTimeCoords()
    End Function

    ''' <summary>
    ''' Newick string of the reconstructed tree (edge lengths are branch times).
    ''' </summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function ToNewick() As String
        Return Tree.ToNewick()
    End Function

    ''' <summary>
    ''' Final complete tree log-likelihood.
    ''' </summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function LogLikelihood() As Double
        Return Tree.calcLogLComplete()
    End Function

    ' ----- static convenience -----

    ''' <summary>
    ''' One-shot fit: build a Bonsai tree and return the 2D layout coordinates.
    ''' </summary>
    Public Shared Function Embed(means As Double()(), Optional stds As Double()() = Nothing, Optional names As String() = Nothing) As Double()()
        Return New Bonsai().Fit(means, stds, names).Transform()
    End Function
End Class

