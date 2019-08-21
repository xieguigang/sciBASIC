Imports Microsoft.VisualBasic.MachineLearning.Darwinism.NonlinearGridTopology

Module cdfTest

    Sub Main()
        ' Call createCDF()
        Call loaderTest()


        Pause()
    End Sub

    Sub loaderTest()
        Call VBDebugger.BENCHMARK(AddressOf loadCDFTest)
        Call VBDebugger.BENCHMARK(AddressOf loadXmlTest)
    End Sub

    Sub loadCDFTest()
        Dim sys = GridMatrixCDF.LoadFromCDF("D:\non-targeted_ANN\design1_DEM_20190819\20190819_delete_M585T402_1\4_6_trainingSet.stroke_with_Clinical.minError.cdf")
    End Sub

    Sub loadXmlTest()
        Dim sys = "D:\non-targeted_ANN\design1_DEM_20190819\20190819_delete_M585T402_1\4_6_trainingSet.stroke_with_Clinical.minError.Xml".LoadXml(Of GridMatrix).CreateSystem
    End Sub

    Sub createCDF()
        Dim test = "D:\non-targeted_ANN\design1_DEM_20190819\20190819_delete_M585T402_1\4_6_trainingSet.stroke_with_Clinical.minError.Xml".LoadXml(Of GridMatrix)
        Dim sys = test.CreateSystem
        Dim cdf = "D:\non-targeted_ANN\design1_DEM_20190819\20190819_delete_M585T402_1\4_6_trainingSet.stroke_with_Clinical.minError.cdf"

        Call GridMatrixCDF.WriteCDF(sys, cdf, test.samples.names)
    End Sub
End Module
