Imports Microsoft.VisualBasic.MIME.application.json.JSONLogic

Module logicTest

    Sub Main()
        Dim out = jsonLogic.apply(
            "{'var': ['a'] }",  ' Logic
            "{'a': 1, 'b': 2 }" ' Data
        )   ' 1

        Console.WriteLine(out)

        Pause()
    End Sub
End Module
