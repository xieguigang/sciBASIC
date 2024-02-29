Imports Microsoft.VisualBasic.MIME.application.json.JSONLogic

Module logicTest

    Sub Main()
        Dim out As Object

        out = jsonLogic.apply("{'==' : [1, 1]}")
        out = jsonLogic.apply("{'If': [  {'<': [{'var':'temp'}, 0] }, 'freezing',  {'<': [{'var':'temp'}, 100] }, 'liquid',  'gas']}", "{'temp':55}")
        out = jsonLogic.apply("{'if': [ true, 'yes', 'no' ]}")
        out = jsonLogic.apply(
            "{'var': ['a'] }",  ' Logic
            "{'a': 1, 'b': 2 }" ' Data
        )   ' 1

        Pause()
    End Sub
End Module
