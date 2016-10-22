Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Mathematical

Module Module1

    Sub Main()

        Dim b = 10.Sequence.ToArray(Function(x) Distributions.Beta.beta(x, 10, 100))

        b = Distributions.Beta.beta(Mathematical.Extensions.seq(0, 1, 0.01), 0.5, 0.5).ToArray

        Call b.FlushAllLines("x:\dddd.csv")

        Call b.GetJson.__DEBUG_ECHO

        Pause()
    End Sub
End Module
