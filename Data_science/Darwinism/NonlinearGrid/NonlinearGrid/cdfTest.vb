#Region "Microsoft.VisualBasic::3b73d7e83d06995f6c72bcb493a8844b, Data_science\Darwinism\NonlinearGrid\NonlinearGrid\cdfTest.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:

    ' Module cdfTest
    ' 
    '     Sub: createCDF, loadCDFTest, loaderTest, loadXmlTest, Main
    ' 
    ' /********************************************************************************/

#End Region

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
