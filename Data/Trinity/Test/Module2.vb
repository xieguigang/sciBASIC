Imports Microsoft.VisualBasic.Data.NLP.Model
Imports Microsoft.VisualBasic.Serialization.JSON

Module Module2

    Sub Main()
        Dim text As String =
            <str>
                Methylated anthocyanin glycosides were isolated from red Canna indica flower and identified as malvidin 3-O-(6-O-acetyl-beta-d-glucopyranoside)-5-O-beta-d-glucopyranoside (1), malvidin 3,5-O-beta-d-diglucopyranoside (2), cyanidin-3-O-(6''-O-alpha-rhamnopyranosyl-beta-glucopyranoside (3), cyanidin-3-O-(6''-O-alpha-rhamnopyranosyl)-beta-galactopyranoside (4), cyanidin-3-O-beta-glucopyranoside (5) and cyanidin-O-beta-galactopyranoside (6) by HPLC-PDA(http://test.url.com/?q=a,a,b,c+{""aaa"":99999}).
            </str>

        Dim tokens As String() = New SentenceCharWalker(text).GetTokens().ToArray

        Call Console.WriteLine(tokens.GetJson)

        Pause()
    End Sub
End Module
