Imports Microsoft.VisualBasic.Data.csv.IO

Module Module3

    Sub Main()
        Call Boxplot()
    End Sub

    Sub Boxplot()
        Dim data = DataSet.LoadDataSet("C:\Users\xieguigang\Desktop\8.4\ko-lv3.csv")
        Dim [case] = {"20_1", "18_1", "17_1", "16_1", "15_2", "15_1", "14_1", "13_1", "12_1", "11_1", "11_2", "11_2_1", "11_2_2", "7_4", "6_4", "1_3", "1_4", "1_5"}
        Dim control = data.PropertyNames.AsList - [case]


        Pause()
    End Sub
End Module
