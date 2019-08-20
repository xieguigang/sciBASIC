Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.IO

Module Module1

    Sub mAIN()
        Dim data = DataSet.LoadDataSet("P:\Resources\RABV_24h.csv").ToDictionary(replaceOnDuplicate:=True)

        For Each item In DataSet.LoadDataSet("P:\Resources\RABV_48h.csv")
            If Not data.ContainsKey(item.ID) Then
                data.Add(item.ID, New DataSet With {.ID = item.ID})
            End If

            For Each p In item.Properties
                data(item.ID)($"[48h]{p.Key}") = p.Value
            Next
        Next

        For Each item In DataSet.LoadDataSet("P:\Resources\RABV_72h.csv")
            If Not data.ContainsKey(item.ID) Then
                data.Add(item.ID, New DataSet With {.ID = item.ID})
            End If

            For Each p In item.Properties
                data(item.ID)($"[72h]{p.Key}") = p.Value
            Next
        Next

        Call data.SaveTo("P:\Resources\RABV_Table sample.csv")
    End Sub
End Module
