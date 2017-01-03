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

###### Network Visualize

PageRank analysis was applied on the this example text paragraph for finding out the keyword:

> "the important pagerank. show on pagerank. have significance pagerank. implements pagerank algorithm. textrank base on pagerank."

From this example paragraph that we can known that almost all of the words are point to the word **pagerank**, so that word **pagerank** probably is the keyword of this paragraph, and its network degree is also high. From the network analysis we can find out that the word **algorithm** didn't have the enough link that point to it, so that its network degree is low, but the word **pagerank** point to it, so that it also have the high PageRank value result, almost the same as word **pagerank** it does.  

![](./visualize.png)
> Network visualize of the example text paragraph, the node size is mapping by the word's PageRank result value, and the label font size is mapping by the word's network degree value.
