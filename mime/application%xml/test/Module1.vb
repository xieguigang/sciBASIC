Imports Microsoft.VisualBasic.MIME.application.xml.MathML

Module Module1

    Sub Main()
        Dim test = "E:\GCModeller\src\runtime\sciBASIC#\mime\etc\kinetics2.xml"
        Dim xml = Microsoft.VisualBasic.MIME.application.xml.XmlParser.ParseXml(test.ReadAllText)
        Dim exp As LambdaExpression = LambdaExpression.FromMathML(test.ReadAllText)

        Console.WriteLine(exp.ToString)

        Pause()
    End Sub

End Module
