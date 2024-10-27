Imports Microsoft.VisualBasic.DataMining.Evaluation
Imports Microsoft.VisualBasic.Scripting.Runtime

Module roc_test2

    Sub Main()
        Dim predicts = "E:\biodeep\biodeepdb_v3\biodeepdb_v3\workspace\202410-mslearn\networking_pos_roc\predicts.txt".ReadAllLines.AsDouble
        Dim labels = "E:\biodeep\biodeepdb_v3\biodeepdb_v3\workspace\202410-mslearn\networking_pos_roc\label.txt".ReadAllLines.AsDouble

        Dim test = RegressionROC.ROC(predicts, labels, n:=500).ToArray
        Dim auc As Double = test.AUC

        Pause()
    End Sub
End Module
