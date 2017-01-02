```vbnet
Imports Microsoft.VisualBasic.Data.NLP
Imports Microsoft.VisualBasic.Data.visualize.Network
Imports Microsoft.VisualBasic.Serialization.JSON

Dim s$ = "the important pagerank. show on pagerank. have significance pagerank. implements pagerank algorithm. textrank base on pagerank."

Dim ps = TextRank.Sentences(s.TrimNewLine)
Dim g As GraphMatrix = ps.TextGraph
Dim pr As New PageRank(g)
Dim result = g.TranslateVector(pr.ComputePageRank, True)

Call result.GetJson(True)
```

###### Output

```json
{
    "pagerank": 0.26708992178741547,
    "algorithm": 0.260770594647373,
    "on": 0.11565500311551431,
    "base": 0.062514670852759385,
    "important": 0.062514670852759385,
    "significance": 0.062514670852759385,
    "have": 0.033788093578283718,
    "implements": 0.033788093578283718,
    "show": 0.033788093578283718,
    "textrank": 0.033788093578283718,
    "the": 0.033788093578283718
}
```