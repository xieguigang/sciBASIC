Imports Microsoft.VisualBasic.Math.LinearAlgebra.LinearProgramming

Module Module2

    Sub Main()

        Dim model = "E:\repo\xDoc\Yilia\runtime\sciBASIC#\Data_science\Mathematica\data\LP\map00220_lpp.XML".LoadXml(Of LPPModel)
        Dim lpp As New LPP(model)
        Dim solution = lpp.solve


        Pause()

    End Sub

End Module
