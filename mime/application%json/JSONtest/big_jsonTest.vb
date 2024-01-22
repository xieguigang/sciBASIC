Imports Microsoft.VisualBasic.MIME.application.json

Module big_jsonTest

    Sub Main()
        Dim json = "\\192.168.1.254\backup3\项目以外内容\2024\动物器官3D重建测试\test3.0\test20240115\tmp\workflow_tmp\spatial_clustering\traceback.json".ReadAllText
        Dim obj = JSONTextParser.ParseJson(json)

        Pause()
    End Sub
End Module
