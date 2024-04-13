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

        Public Overridable Sub gibbsSampling()
            ' copy global nw, nwsum array
            For topic = 0 To pGL.K - 1
                nwsum(topic) = pGL.nwsum(topic)
                For word = 0 To pGL.V - 1
                    nw(word)(topic) = pGL.nw(word)(topic)
                Next
            Next

            For m = start To [end]
                Dim N = pGL.documents(m).Length
                For i = 0 To N - 1
                    ' remove z_i
                    pGL.nd(m)(pGL.z(m)(i)) -= 1
                    pGL.ndsum(m) -= 1
                    nw(pGL.documents(m)(i))(pGL.z(m)(i)) -= 1
                    nwsum(pGL.z(m)(i)) -= 1

                    ' perform gibbs sampling formula and assign a topic to z[m][i]
                    Dim prob = New Double(pGL.K - 1) {}

                    For topic = 0 To pGL.K - 1
                        Dim p = (pGL.nd(m)(topic) + pGL.alpha) / (pGL.ndsum(m) + pGL.K * pGL.alpha) * (nw(pGL.documents(m)(i))(topic) + pGL.beta) / (nwsum(topic) + pGL.V * pGL.beta)
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
