#Region "Microsoft.VisualBasic::ab524aabf5eceabc36f8dd0af02b69f8, Data_science\DataMining\BinaryTree\DiffusionMap.vb"

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

    '   Total Lines: 137
    '    Code Lines: 52
    ' Comment Lines: 68
    '   Blank Lines: 17
    '     File Size: 5.66 KB


    ' Class DiffusionMap
    ' 
    '     Function: compute_diffusion_map, step_5
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports np = Microsoft.VisualBasic.Math.LinearAlgebra.Matrix.Numpy
Imports std = System.Math

''' <summary>
''' Generate a diffusion map embedding
''' </summary>
Public Class DiffusionMap

    ''' <summary>
    ''' Compute the diffusion maps of a symmetric similarity matrix
    ''' </summary>
    ''' <param name="L">matrix N x N
    ''' L is symmetric and L(x, y) >= 0
    ''' </param>
    ''' <param name="alpha">float [0, 1]
    ''' Setting alpha=1 and the diffusion operator approximates the
    ''' Laplace-Beltrami operator. We then recover the Riemannian geometry
    ''' of the data set regardless of the distribution of the points. To
    ''' describe the Long-term behavior Of the point distribution Of a
    ''' system of stochastic differential equations, we can use alpha=0.5
    ''' And the resulting Markov chain approximates the Fokker-Planck
    ''' diffusion. With alpha=0, it reduces to the classical graph Laplacian
    ''' normalization.
    ''' </param>
    ''' <param name="n_components">
    ''' The number of diffusion map components to return. Due to the
    ''' spectrum decay Of the eigenvalues, only a few terms are necessary To
    ''' achieve a given relative accuracy In the sum M^t.
    ''' </param>
    ''' <param name="diffusion_time">float >= 0
    ''' use the diffusion_time (t) step transition matrix M^t
    ''' 
    ''' t Not only serves as a time parameter, but also has the dual role of
    ''' scale parameter. One Of the main ideas Of diffusion framework Is
    ''' that running the chain forward In time (taking larger And larger
    ''' powers of M) reveals the geometric structure of X at larger And
    ''' larger scales(the diffusion process).
    '''
    ''' t = 0 empirically provides a reasonable balance from a clustering
    ''' perspective. Specifically, the notion of a cluster in the data set
    ''' Is quantified as a region in which the probability of escaping this
    ''' region Is low (within a certain time t).
    ''' </param>
    ''' <param name="skip_checks">Avoid expensive pre-checks on input data. The caller has to make
    ''' sure that input data Is valid Or results will be undefined.</param>
    ''' <param name="overwrite">Optimize memory usage by re-using input matrix L as scratch space.</param>
    ''' <param name="return_result"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' References
    ''' ----------
    '''
    ''' [1] https//en.wikipedia.org/wiki/Diffusion_map
    ''' [2] Coifman, R.R.; S. Lafon. (2006). "Diffusion maps". Applied And
    '''     Computational Harmonic Analysis 21: 5-30. doi:10.1016/j.acha.2006.04.006
    ''' </remarks>
    Public Shared Function compute_diffusion_map(L As GeneralMatrix, Optional alpha As Double = 0.5, Optional n_components As Integer? = Nothing,
                                                Optional diffusion_time As Double = 0,
                       Optional skip_checks As Boolean = False, Optional overwrite As Boolean = False,
                         Optional return_result As Boolean = False)

        Dim ndim As Integer = L.shape(0)
        Dim L_alpha = L
        Dim d_alpha As Vector

        If alpha > 0 Then
            ' Step 2
            Dim d = np.array(L_alpha.sum(axis:=1)).flatten

            d_alpha = np.power(d, -alpha)
            L_alpha = d_alpha.t * L_alpha
            L_alpha = L_alpha * d_alpha.r
        End If

        ' Step 3
        d_alpha = np.power(np.array(L_alpha.sum(axis:=1)).flatten, -1)
        L_alpha = d_alpha.t * L_alpha

        Dim M = L_alpha
        Dim k As Integer

        ' Step 4
        If Not n_components Is Nothing Then
            k = n_components + 1
        Else
            k = std.Max(2, std.Sqrt(ndim))
        End If

        Dim eigen_solver As Func(Of GeneralMatrix, Integer, (lambdas As Double(), vectors As Double()())) = Nothing
        Dim lambdas As Double(), vectors As Double()()

        With eigen_solver(M, k)
            lambdas = .lambdas
            vectors = .vectors
        End With

        ' get max value index
        Dim lambda_idx = np.argsort(lambdas).Last
        Dim max_lambdas = lambdas(lambda_idx)
        Dim max_vector = vectors(lambda_idx)

        Return step_5(max_lambdas, max_vector, ndim, n_components, diffusion_time)
    End Function

    ''' <summary>
    ''' This is a helper function for diffusion map computation.
    ''' 
    ''' The lambdas have been sorted in decreasing order.
    ''' The vectors are ordered according To lambdas.
    ''' </summary>
    ''' <param name="lambdas"></param>
    ''' <param name="vectors"></param>
    ''' <param name="ndim"></param>
    ''' <param name="n_components"></param>
    ''' <param name="diffusion_time"></param>
    ''' <returns></returns>
    Private Shared Function step_5(lambdas As Double, vectors As Vector, ndim As Integer, n_components As Integer, diffusion_time As Double)
        ' psi = vectors/vectors[:, [0]]
        Dim psi = vectors / vectors.First
        Dim diffusion_times = diffusion_time

        If diffusion_time = 0.0 Then
            diffusion_times = std.Exp(1 - std.Log(1 - lambdas) / std.Log(lambdas))
            lambdas = lambdas / (1 - lambdas)
        Else
            lambdas = lambdas ^ diffusion_time
        End If

        Dim lambda_ratio = lambdas / lambdas
        Dim threshold = std.Max(0.05, lambda_ratio)

        Dim n_components_auto
    End Function

End Class
