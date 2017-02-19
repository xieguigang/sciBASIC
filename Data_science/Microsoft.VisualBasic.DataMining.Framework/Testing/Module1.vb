Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.DataMining.KMeans.NodeTrees
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Module Module1
    Sub Main()
        Dim tree = Network.Load("G:\Xanthomonas_campestris_8004_uid15\genome\palindrome-motifs\palindrome_promoter=-250bp-cut=0.65,minw=6\binary-net").BuildTree
        Dim parts = tree.CutTrees(0.99).ToArray
        Dim json = parts.PartionTable

        Call json.GetJson(True).SaveTo("x:/parts.json")

        Call json.Values.IteratesALL.Count.__DEBUG_ECHO

        Pause()
    End Sub
End Module
