Imports Microsoft.VisualBasic.MIME.application.xml.MathML

Module Module1

    Sub Main()
        Dim test = "E:\GCModeller\src\runtime\sciBASIC#\Data_science\Mathematica\Math\MathLambda\mathML.xml"
        Dim xml = Microsoft.VisualBasic.MIME.application.xml.XmlParser.ParseXml(test.ReadAllText)
        Dim exp As LambdaExpression = LambdaExpression.FromMathML(test.ReadAllText)

        Console.WriteLine(exp.ToString)

        Pause()
    End Sub

End Module
