#Region "Microsoft.VisualBasic::5592b289d59ee3d3b5dbcbc4bcdc90ac, Data_science\DataMining\DataMining\test\TestingMain.vb"

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


    ' Code Statistics:

    '   Total Lines: 256
    '    Code Lines: 0 (0.00%)
    ' Comment Lines: 180 (70.31%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 76 (29.69%)
    '     File Size: 10.29 KB


    ' 
    ' /********************************************************************************/

#End Region

'#Region "Microsoft.VisualBasic::e081a9ac42b857cb8ff8bf20072c4deb, Data_science\DataMining\DataMining\test\TestingMain.vb"

'    ' Author:
'    ' 
'    '       asuka (amethyst.asuka@gcmodeller.org)
'    '       xie (genetics@smrucc.org)
'    '       xieguigang (xie.guigang@live.com)
'    ' 
'    ' Copyright (c) 2018 GPL3 Licensed
'    ' 
'    ' 
'    ' GNU GENERAL PUBLIC LICENSE (GPL3)
'    ' 
'    ' 
'    ' This program is free software: you can redistribute it and/or modify
'    ' it under the terms of the GNU General Public License as published by
'    ' the Free Software Foundation, either version 3 of the License, or
'    ' (at your option) any later version.
'    ' 
'    ' This program is distributed in the hope that it will be useful,
'    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
'    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'    ' GNU General Public License for more details.
'    ' 
'    ' You should have received a copy of the GNU General Public License
'    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



'    ' /********************************************************************************/

'    ' Summaries:

'    ' Module TestingMain
'    ' 
'    '     Sub: Main
'    '     Class Student
'    ' 
'    '         Properties: Name
'    ' 
'    '         Function: [New], ToString
'    ' 
'    ' 
'    ' 
'    ' /********************************************************************************/

'#End Region

'Imports Microsoft.VisualBasic.ComponentModel.Algorithm
'Imports Microsoft.VisualBasic.DataMining
'Imports Microsoft.VisualBasic.DataMining.ComponentModel
'Imports Microsoft.VisualBasic.DataMining.KMeans
'Imports Microsoft.VisualBasic.Language
'Imports Microsoft.VisualBasic.Linq
'Imports Microsoft.VisualBasic.Serialization.JSON

'Module TestingMain

'    Class Student : Inherits EntityBase(Of Double)

'        Property Name As String

'        Public Overrides Function ToString() As String
'            Return Name
'        End Function

'        Public Shared Function [New](data As Double()) As Student
'            Return New Student With {.Name = Rnd(), .Properties = data}
'        End Function
'    End Class

'    Sub Main()

'        Dim ea As New IntegerEntity With {.uid = NameOf(ea), .entityVector = {1, 2}}
'        Dim eb As New IntegerEntity With {.uid = NameOf(eb), .entityVector = {1, 2}}
'        Dim ec As New IntegerEntity With {.uid = NameOf(ec), .entityVector = {140, 5}}
'        Dim ed As New IntegerEntity With {.uid = NameOf(ed), .entityVector = {10, 260}}
'        Dim ee As New IntegerEntity With {.uid = NameOf(ee), .entityVector = {10, 250}}
'        Dim ef As New IntegerEntity With {.uid = NameOf(ef), .entityVector = {10, 240}}
'        Dim eg As New IntegerEntity With {.uid = NameOf(eg), .entityVector = {10, 250}}
'        Dim eh As New IntegerEntity With {.uid = NameOf(eh), .entityVector = {10, 250}}
'        Dim ei As New IntegerEntity With {.uid = NameOf(ei), .entityVector = {10, 255}}
'        Dim ej As New IntegerEntity With {.uid = NameOf(ej), .entityVector = {10, 250}}

'        Dim trace As New Dictionary(Of Integer, List(Of KMeans.Entity))
'        '  Dim cccc = {ea, eb, ec, ed, ee, ef, eg, eh, ei, ej}.FuzzyCMeans(3, 0.5, trace:=trace)

'        'Dim g = {ea, eb, ec, ed, ee, ef, eg, eh, ei, ej}.GroupBy(Function(n) n.Fill).ToArray

'        'For Each x In cccc
'        '    Call $"centra {x.uid} =>  {x.Properties.GetJson}".PrintException
'        'Next

'        For Each x In {ea, eb, ec, ed, ee, ef, eg, eh, ei, ej}
'            Call ($"{x.uid}: {x.Properties.GetJson} => " & x.ReadProperty(Of Dictionary(Of String, Double))("tooltip").value.GetJson).__DEBUG_ECHO
'        Next

'        Pause()

'        'Dim ann As New NeuralNetwork.Network(5, 50, 1, 0.01, , New IFuncs.SigmoidFunction)
'        'Dim learn As New NeuralNetwork.TrainingUtils(ann)
'        'Dim map As New Encoder(Of Char)

'        'Call map.AddMap("A", 0)
'        'Call map.AddMap("B", 0.15)
'        'Call map.AddMap("C", 0.2)
'        'Call map.AddMap("D", 0.3)
'        'Call map.AddMap("E", 0.6)
'        'Call map.AddMap("F", 0.7)
'        'Call map.AddMap("G", 0.9)
'        'Call map.AddMap("Z", 1)

'        'learn.Add({10, 20, 15, 33, 65}, {map("B")})
'        'learn.Add({10, 20, 15, 33, 65}, {map("B")})
'        'learn.Add({10, 20, 15, 33, 65}, {map("B")})
'        'learn.Add({10, 20, 15, 33, 65}, {map("B")})
'        'learn.Add({10, 20, 15, 33, 65}, {map("B")})
'        'learn.Add({10, 20, 0, 33, 65}, {map("Z")})
'        'learn.Add({10, 20, 0, 33, 65}, {map("Z")})
'        'learn.Add({10, 20, 0, 33, 65}, {map("Z")})
'        'learn.Add({10, 20, 0, 33, 65}, {map("Z")})
'        'learn.Add({10, 20, 0, 33, 65}, {map("Z")})
'        'learn.Add({10, 20, 0, 33, 0}, {map("D")})
'        'learn.Add({3, 20, 0, 3, 0}, {map("F")})

'        'learn.Train()

'        'map.Decode(learn.NeuronNetwork.Compute({10, 20, 15, 33, 65}).First).GetJson.__DEBUG_ECHO
'        'map.Decode(learn.NeuronNetwork.Compute({10, 20, 0, 33, 65}).First).GetJson.__DEBUG_ECHO
'        'map.Decode(learn.NeuronNetwork.Compute({10, 20, 0, 33, 0}).First).GetJson.__DEBUG_ECHO
'        'map.Decode(learn.NeuronNetwork.Compute({3, 20, 0, 3, 0.2}).First).GetJson.__DEBUG_ECHO

'        'Pause()

'        '  Call 5.0R.Γ.__DEBUG_ECHO
'        '   Call 1.6R.Γ.__DEBUG_ECHO

'        Pause()

'        Dim a() As Char = {"A"c, "B"c, "C"c, "B"c, "D"c, "A"c, "B"c}
'        Dim b() As Char = {"B"c, "D"c, "C"c, "A"c, "B"c, "A"c}

'        Dim c = DynamicProgramming.MaxLengthSubString("ABCBDAB", "BDCABA")

'        'Dim app As New QLearning(Of   )
'        'Try
'        '    app.runLearningLoop()
'        'Catch e As Exception
'        '    Console.WriteLine("Thread.sleep interrupted!")
'        'End Try
'        '    Dim ddd As List(Of Double) = [TypeOf](Of Double)() << OpenHandle("./123.txt")



'        Dim v As New List(Of Value(Of String))

'        v += New Value(Of String)("1234")
'        v += New Value(Of String)("369")
'        v -= v(0)

'        'Dim nnnnet =
'        '    "F:\1.13.RegPrecise_network\MEME_OUT\Regulons.MEME\250.MEME_SW-TOM.OUT\SW-TOM.Hits.VirtualFootprints.Trim.PhenotypeRegulates.TreeNET\Edges.csv".LoadCsv(Of NetworkEdge)
'        '  Dim finderdfff = Dijkstra.DijkstraAPI.CreatePathwayFinder(Dijkstra.DijkstraAPI.ImportsNetwork(nnnnet, 1), True)
'        '  Dim pathssss = Dijkstra.DijkstraAPI.FindAllPath(finderdfff, "xcb_M00552", "xcb_M00024")

'        'Dim parts = TreeAPI.ClusterParts(nnnnet)

'        'Dim ssssst = (From x In parts Select x.Value).IteratesALL.Distinct.ToArray.Length
'        'Call ssssst.__DEBUG_ECHO


'        '    Dim source = "F:\1.13.RegPrecise_network\MEME_OUT\Modules\100.MEME.ClusterMatrix\ClusterMatrix.Csv".LoadCsv(Of EntityLDM)
'        '    Dim net = source.TreeNET

'        '     Call net.Save("./test_tree/", Encodings.ASCII)


'        Dim StudentA As New Student With {.Properties = New Double() {15, 32, 35.6}, .Name = "A"},
'        StudentB As New Student With {.Properties = New Double() {55, 60, 65.1}, .Name = "B"},
'        StudentC As New Student With {.Properties = New Double() {52, 57, 65.6}, .Name = "C"},
'          StudentD As New Student With {.Properties = New Double() {14, 32, 35.6}, .Name = "D"},
'            StudentE As New Student With {.Properties = New Double() {18, 32, 35.6}, .Name = "E"}


'        Dim clusters As ClusterCollection(Of Student)

'        Dim data = {StudentA, StudentB, StudentC, StudentD, StudentE}.Shuffles

'        clusters = KMeans.ClusterDataSet(data, 2)


'        Dim distance = KMeans.EuclideanDistance(StudentA.Properties, StudentB.Properties)

'        Dim distances = KMeans.ManhattanDistance(StudentA.Properties, StudentB.Properties)

'        Dim cluster#(,) = {
'            {15, 32, 35.6},
'            {19, 54, 65.1}
'        }
'        Dim centroid#() = KMeans.ClusterMean(cluster)

'        Call $"<br/>Cluster mean Calc: {centroid}".__DEBUG_ECHO












'        Dim nnn = 1.Sequence
'        nnn = 0.Sequence

'        nnn = (-1).Sequence

'        Dim Data0 = Microsoft.VisualBasic.Data.csv.IO.File.FastLoad("E:\xcb_vcell\xcb_model\Result\MAT_OUT.csv")
'        '  Dim MAT = Microsoft.VisualBasic.DataMining.Serials.PeriodAnalysis.SerialsVarialble.Load(Data0)

'        ' Dim datad = Microsoft.VisualBasic.Math.Interpolation.BezierExtensions.BezierSmoothInterpolation(MAT(1).SerialsData, 100)

'        'Call datad.SaveTo("./Bezier.csv")

'        '    Dim DFT = New Microsoft.VisualBasic.Math.Signals.TFftAlgorithm(datad)
'        '   Call MAT(1).SerialsData.SaveTo("./vec.csv")

'        '  Call DFT.FourierTransformation()

'        '   Call DFT.a.SaveTo("./dft.a.csv")
'        '   Call DFT.b.SaveTo("./dft.b.csv")
'        '   Call DFT.cosine.SaveTo("./dft.cosine.csv")
'        '   Call DFT.sine.SaveTo("./dft.sine.csv")
'        '   Call DFT.xw.SaveTo("./dft.xw.csv")
'        '   Call DFT.y.SaveTo("./dft.y.csv")


'        ' Call Microsoft.VisualBasic.Math.Signals.WaveletTransform.FWT(datad)
'        '  Call datad.SaveTo("./wat.csv")

'        Dim Factors = New List(Of Microsoft.VisualBasic.DataMining.DFL_Driver.I_FactorElement)
'        Call Factors.Add(New DataMining.DFL_Driver.I_FactorElement() With {.Weight = 0.5}.set_Quantity(2))
'        Call Factors.Add(New DataMining.DFL_Driver.I_FactorElement() With {.Weight = 0.6}.set_Quantity(3))
'        Call Factors.Add(New DataMining.DFL_Driver.I_FactorElement() With {.Weight = -0.99}.set_Quantity(1))
'        Call Factors.Add(New DataMining.DFL_Driver.I_FactorElement() With {.Weight = -0.1}.set_Quantity(8))
'        Call Factors.Add(New DataMining.DFL_Driver.I_FactorElement() With {.Weight = 0.2}.set_Quantity(2))
'        Call Factors.Add(New DataMining.DFL_Driver.I_FactorElement() With {.Weight = 1}.set_Quantity(0.4))

'        Dim node = New DataMining.DFL_Driver.dflNode(Factors)

'        Call Console.WriteLine(node.State)
'        Call Console.Read()
'    End Sub
'End Module
