#Region "Microsoft.VisualBasic::39ac962561dd3d5a9e7a478e52d74f50, Data_science\NLP\LDA\ParallelGibbsLda\GibbsWorker.vb"

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

    '   Total Lines: 90
    '    Code Lines: 64 (71.11%)
    ' Comment Lines: 7 (7.78%)
    '    - Xml Docs: 42.86%
    ' 
    '   Blank Lines: 19 (21.11%)
    '     File Size: 3.03 KB


    '     Class GibbsWorker
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ToString
    ' 
    '         Sub: gibbsSampling, run
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace LDA

    ''' <summary>
    ''' Created by chenjianfeng on 2018/1/21.
    ''' </summary>
    Public Class GibbsWorker

        Private pGL As ParallelGibbsLda
        Private start As Integer
        Private [end] As Integer
        Friend nw As Integer()()
        Friend nwsum As Integer()

        Public Sub New(pGL As ParallelGibbsLda, start As Integer, [end] As Integer)
            Me.pGL = pGL
            Me.start = start
            Me.end = [end]

            nw = RectangularArray.Matrix(Of Integer)(pGL.V, pGL.K)
            nwsum = New Integer(pGL.K - 1) {}
        End Sub

        Public Overridable Sub run()
            gibbsSampling()
        End Sub

        Public Overrides Function ToString() As String
            Return $"&{start}"
        End Function

        Public Overridable Sub gibbsSampling()
            Dim N As Integer

            Array.ConstrainedCopy(pGL.nwsum, 0, nwsum, 0, nwsum.Length)

            ' copy global nw, nwsum array
            For topic As Integer = 0 To pGL.K - 1
                For word As Integer = 0 To pGL.V - 1
                    nw(word)(topic) = pGL.nw(word)(topic)
                Next
            Next

            For m As Integer = start To [end]
                N = pGL.documents(m).Length

                For i As Integer = 0 To N - 1
                    ' remove z_i
                    pGL.nd(m)(pGL.z(m)(i)) -= 1
                    pGL.ndsum(m) -= 1
                    nw(pGL.documents(m)(i))(pGL.z(m)(i)) -= 1
                    nwsum(pGL.z(m)(i)) -= 1

                    ' perform gibbs sampling formula and assign a topic to z[m][i]
                    Dim prob = New Double(pGL.K - 1) {}
                    Dim p As Double

                    For topic = 0 To pGL.K - 1
                        p = (pGL.nd(m)(topic) + pGL.alpha) / (pGL.ndsum(m) + pGL.K * pGL.alpha) *
                            (nw(pGL.documents(m)(i))(topic) + pGL.beta) /
                            (nwsum(topic) + pGL.V * pGL.beta)
                        prob(topic) = p
                    Next

                    For idx = 1 To pGL.K - 1
                        prob(idx) += prob(idx - 1)
                    Next

                    Dim r As Double = randf.NextDouble() * prob(pGL.K - 1)

                    For topic = 0 To pGL.K - 1
                        If r < prob(topic) Then
                            pGL.z(m)(i) = topic
                            Exit For
                        End If
                    Next

                    ' update count variables
                    pGL.nd(m)(pGL.z(m)(i)) += 1
                    pGL.ndsum(m) += 1
                    nw(pGL.documents(m)(i))(pGL.z(m)(i)) += 1
                    nwsum(pGL.z(m)(i)) += 1
                Next
            Next
        End Sub
    End Class

End Namespace
