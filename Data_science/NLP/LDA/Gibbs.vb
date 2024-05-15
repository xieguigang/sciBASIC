#Region "Microsoft.VisualBasic::2e60192f55a98a42700b1b696a464dbd, Data_science\NLP\LDA\Gibbs.vb"

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

    '   Total Lines: 146
    '    Code Lines: 102
    ' Comment Lines: 22
    '   Blank Lines: 22
    '     File Size: 5.31 KB


    '     Class GibbsSamplingTask
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: gibbs_sampling
    ' 
    '         Sub: Solve
    '         Structure gibbs_pars
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Parallel
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace LDA

    Friend Class GibbsSamplingTask : Inherits VectorTask

        ReadOnly v As Integer()
        ReadOnly zi As Integer
        ReadOnly gibbs As LdaGibbsSampler
        ReadOnly K As Integer
        ReadOnly beta As Double
        ReadOnly alpha As Double
        ReadOnly voca_size As Integer

        Sub New(v As Integer(), zi As Integer, gibbs As LdaGibbsSampler)
            Call MyBase.New(v.Length)

            Me.alpha = gibbs.alpha
            Me.beta = gibbs.beta
            Me.K = gibbs.K
            Me.v = v
            Me.zi = zi
            Me.gibbs = gibbs
            Me.voca_size = gibbs.V
        End Sub

        Protected Overrides Sub Solve(start As Integer, ends As Integer, cpu_id As Integer)
            Dim nw As Integer()
            Dim nd As Integer()
            Dim nwsum As Integer()
            Dim ndsum As Integer()
            Dim topic As Integer
            Dim pars As New gibbs_pars With {
                .alpha = alpha,
                .beta = beta,
                .K = K,
                .voca_size = voca_size
            }

            If sequenceMode Then
                'nw = gibbs.nw
                'nd = gibbs.nd
                nwsum = gibbs.nwsum
                ndsum = gibbs.ndsum
                nd = gibbs.nd(zi)
                nw = Nothing
            Else
                nw = New Integer(K - 1) {}
                nd = New Integer(K - 1) {}
                nwsum = New Integer(K - 1) {}
                ndsum = New Integer(gibbs.ndsum.Length - 1) {}

                Array.ConstrainedCopy(gibbs.nwsum, Scan0, nwsum, Scan0, nwsum.Length)
                Array.ConstrainedCopy(gibbs.ndsum, Scan0, ndsum, Scan0, ndsum.Length)
                Array.ConstrainedCopy(gibbs.nd(zi), Scan0, nd, Scan0, nd.Length)
            End If

            For n As Integer = start To ends
                topic = v(n)

                If sequenceMode Then
                    nw = gibbs.nw(gibbs.documents(zi)(n))
                Else
                    Call Array.ConstrainedCopy(
                        gibbs.nw(gibbs.documents(zi)(n)), Scan0,
                        nw, Scan0,
                        nw.Length
                    )
                End If

                ' (z_i = z[m][n])
                ' sample from p(z_i|z_-i, w)
                topic = gibbs_sampling(topic, zi, nw, nd, nwsum, ndsum, pars)
                v(n) = topic

                ' add newly estimated z_i to count variables
                ' 将重新估计的该词语加入计数器
                gibbs.nw(gibbs.documents(zi)(n))(topic) += 1
                gibbs.nd(zi)(topic) += 1
                gibbs.nwsum(topic) += 1
                gibbs.ndsum(zi) += 1
            Next
        End Sub

        ''' <summary>
        ''' Sample a topic z_i from the full conditional distribution: p(z_i = j |
        ''' z_-i, w) = (n_-i,j(w_i) + beta)/(n_-i,j(.) + W * beta) * (n_-i,j(d_i) +
        ''' alpha)/(n_-i,.(d_i) + K * alpha) 
        ''' 根据上述公式计算文档m中第n个词语的主题的完全条件分布，输出最可能的主题
        ''' </summary>
        ''' <param name="m"> document </param>
        ''' <returns>
        ''' sampling and assign new topic index
        ''' </returns>
        Private Shared Function gibbs_sampling(topic As Integer, m As Integer,
                                               nw As Integer(), nd As Integer(),
                                               nwsum As Integer(), ndsum As Integer(),
                                               gibbs_pars As gibbs_pars) As Integer

            ' remove z_i from the count variables
            ' 先将这个词从计数器中抹掉
            nw(topic) -= 1
            nd(topic) -= 1
            nwsum(topic) -= 1
            ndsum(m) -= 1

            ' do multinomial sampling via cumulative method: 通过多项式方法采样多项式分布
            Dim p = New Double(gibbs_pars.K - 1) {}

            For K As Integer = 0 To gibbs_pars.K - 1
                p(K) = (nw(K) + gibbs_pars.beta) / (nwsum(K) +
                    gibbs_pars.voca_size * gibbs_pars.beta) * (nd(K) + gibbs_pars.alpha) /
                    (ndsum(m) + gibbs_pars.K * gibbs_pars.alpha)
            Next

            ' cumulate multinomial parameters
            ' 累加多项式分布的参数
            For K As Integer = 1 To p.Length - 1
                p(K) += p(K - 1)
            Next

            ' scaled sample because of unnormalised p[] 正则化
            Dim u As Double = randf.NextDouble * p(gibbs_pars.K - 1)

            topic = 0

            Do While topic < p.Length - 1
                If u < p(topic) Then
                    Exit Do
                Else
                    topic += 1
                End If
            Loop

            Return topic
        End Function

        Private Structure gibbs_pars
            Dim K As Integer
            Dim beta As Double
            Dim voca_size As Integer
            Dim alpha As Double
        End Structure
    End Class
End Namespace
