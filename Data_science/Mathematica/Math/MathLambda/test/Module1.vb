Imports Microsoft.VisualBasic.MIME.application.rdf_xml.MathML

Module Module1

    Sub Main()
        Dim exp As BinaryExpression = BinaryExpression.FromMathML("E:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematica\Math\MathLambda\mathML.xml".ReadAllText)

        Pause()
    End Sub

End Module
