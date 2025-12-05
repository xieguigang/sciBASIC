Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Language

Module qgramTestSearch

    Sub Run()
        Dim q As New QGramIndex(3)
        Dim i As i32 = 0

        Call q.AddString("", ++i)
        Call q.AddString("", ++i)
        Call q.AddString("", ++i)
        Call q.AddString("test ATP token", ++i)
        Call q.AddString("", ++i)
        Call q.AddString("", ++i)
        Call q.AddString("ATP", ++i)
        Call q.AddString("test acid", ++i)
        Call q.AddString("ATP", ++i)
        Call q.AddString("ATP+", ++i)
        Call q.AddString("Hello world", ++i)
        Call q.AddString("test", ++i)

        Dim find1 = q.FindSimilar("ATP").ToArray
        Dim find2 = q.FindSimilar("atp").ToArray

        Pause()
    End Sub
End Module
