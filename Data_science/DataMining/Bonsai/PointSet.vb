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

Imports Microsoft.VisualBasic.DataMining.Bonsai.MathExtensions
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace Microsoft.VisualBasic.DataMining.Bonsai

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

        Sub New(means As Double()(), Optional stds As Double()() = Nothing, Optional names As String() = Nothing)
            Me.means = means
            Me.nSamples = means.Length
            Me.nGenes = If(nSamples > 0, means(0).Length, 0)

            If stds Is Nothing Then
                Me.stds = means.Select(Function(row) row.Select(Function(_) 1.0).ToArray).ToArray
            Else
                Me.stds = stds
            End If

            Me.vars = Me.stds _
                .Select(Function(row) row.Select(Function(s) s * s).ToArray) _
                .ToArray

            If names Is Nothing Then
                Me.names = Enumerable.Range(0, nSamples) _
                    .Select(Function(i) "s" & i) _
                    .ToArray
            Else
                Me.names = names
            End If
        End Sub

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
End Namespace
