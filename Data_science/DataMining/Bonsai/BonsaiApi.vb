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

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace Microsoft.VisualBasic.DataMining.Bonsai

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
        ''' Fit a Bonsai tree to a high-dimensional point set.
        ''' </summary>
        ''' <param name="means">N x D matrix of sample means.</param>
        ''' <param name="stds">Optional N x D matrix of per-dimension standard deviations.</param>
        ''' <param name="names">Optional N sample labels.</param>
        Public Function Fit(means As Double()(), Optional stds As Double()() = Nothing, Optional names As String() = Nothing) As Bonsai
            Dim data = New PointSet(means, stds, names)
            Me.Data = data
            Me.Tree = BonsaiTree.Build(data, maxMerges, maxTimeIters, verbose)
            Return Me
        End Function

        ''' <summary>
        ''' Fit directly from a <see cref="PointSet"/>.
        ''' </summary>
        Public Function Fit(data As PointSet) As Bonsai
            Me.Data = data
            Me.Tree = BonsaiTree.Build(data, maxMerges, maxTimeIters, verbose)
            Return Me
        End Function

        ''' <summary>
        ''' Low-dimensional embedding for every sample (the D-dimensional effective leaf positions).
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Transform() As Double()()
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
        ''' One-shot fit: build a Bonsai tree and return the low-dimensional coordinates.
        ''' </summary>
        Public Shared Function Embed(means As Double()(), Optional stds As Double()() = Nothing, Optional names As String() = Nothing) As Double()()
            Return New Bonsai().Fit(means, stds, names).Transform()
        End Function
    End Class
End Namespace
