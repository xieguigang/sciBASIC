# sciBASIC#: Microsoft VisualBasic for Scientific Computing

![(๑•̀ㅂ•́)و✧](etc/badge.png)
![](https://cdn.rawgit.com/LunaGao/BlessYourCodeTag/master/tags/alpaca.svg)
[![Github All Releases](https://img.shields.io/github/downloads/xieguigang/sciBASIC/total.svg?maxAge=2592000?style=flat-square)]()
[![GPL Licence](https://badges.frapsoft.com/os/gpl/gpl.svg?v=103)](https://opensource.org/licenses/GPL-3.0/)
[![DOI](https://zenodo.org/badge/DOI/10.5281/zenodo.166002.svg)](https://doi.org/10.5281/zenodo.166002)

###### [WARNING] This project is a work in progress and is not recommended for production use.
> Probably some namespace and object name may changes frequently on each commit, and you are feel free to using the **Object Browser** in visual studio to adapted to the object not defined problem which was caused by these changes.....

<hr>

[![sciBASIC# logo](Microsoft.VisualBasic.Core/logo.png)](./vs_solutions/logo/Program.vb)

> Read the project documents: [http://sciBASIC.NET/](http://sciBASIC.NET)

## Scientific Computing runtime for VisualBasic.NET

sciBASIC#: A Microsoft VisualBasic feature runtime for data science application on Windows/Linux/macOS And China Tianhe Super Computing Platform. It was mainly consists with a data frame system, a data science analysis system, a data graphics system and a general application core runtime.

``sciBASIC#``(read as ``scientific visualbasic`` or just ``sciBASIC sharp``) is a Microsoft VisualBasic language feature runtime for your data science application which is running in the CLI environment on Windows/Linux/macOS **Desktop/Server** platform or **supercomputer** platform. This framework project includes a lot of mathematics utility tools and the utility code extension functions for the data sciences application which is programming in VisualBasic language, and it also extends the VisualBasic programming language syntax. Makes the VisualBasic programming style more modernized in the data science industry by using this runtime library framework.

> Abount VisualBasic code style guidelines:
> + https://github.com/xieguigang/sciBASIC/tree/master/docs/vb_codestyle

> Guides for using this framework, you can found the document and content index at the [README.md](./guides/README.md)(This guidelines document is currently compiling for users):
> + https://github.com/xieguigang/sciBASIC/tree/master/docs/guides

![](./etc/README/nodes.gif)
<hr/>

### Runtime Installation

+ Compile &amp; Application Development on this runtime required the latest VisualStudio 2017.
+ If you are running sciBASIC runtime on Windows, please makesure your operating system supports .NET framework 4.6
+ If you are running sciBASIC runtime on Linux/macOS, please install mono runtime at first by following the installation manual on **mono-project** website. And then the ``Microsoft VisualBasic core runtime`` is required, you can find this runtime file in this repository: [Microsoft.VisualBasic.7z](./Microsoft.VisualBasic.7z), just extract this 7z archive and put the **Microsoft.VisualBasic.dll** kernel in the application directory.

### Install this framework via nuget package

For .NET Framework 4.6:

+ https://www.nuget.org/packages/sciBASIC#

```bash
# For install latest stable release version:
PM> Install-Package sciBASIC
# For install latest unstable beta version:
PM> Install-Package sciBASIC -Pre
```

### Contacts

Foundation for VisualBasic.NET scientific computing softwares from China: [foundation@sciBASIC.NET](mailto://foundation@scibasic.net)

### Directory Structure

###### 1. source projects

+ **[/CLI_tools](./CLI_tools/)** : Some small utilities and example tools
+ **[/Data](./Data/)** : *sciBASIC#* data framework system for data science, includes data frame, data I/O, natural language text processing and data object search framework.
+ **[/Data_science](./Data_science/)** : *sciBASIC#* Mathmatica system, data graphics plot system & Data Mining library
+ **[/Microsoft.VisualBasic.Core](./Microsoft.VisualBasic.Core/)** : **Microsoft VisualBasic General App core runtime**
+ **[/mime](./mime/)** : Various mime-type text or Xml doc parsers in VisualBasic.
+ **[/gr](./gr/)** : **sciBASIC# Artists**: (graphic artist) VB.NET imaging graphics system
+ **[/www](./www/)** : Web related utilities code
+ **[/vs_solutions](./vs_solutions/)** : sciBASIC framework Windows installer project. 

###### 2. docs for User

+ **[/guides](./docs/guides/)** : This framework code usage example and manual documents
+ **[/vb_codestyle](./docs/vb_codestyle/)** : sciBASIC# Coding style standard document

---------------------------------------------------------------------------------------------------------------

## Namespace

|  |Namespace                                    |Description                                                                       |
|--|---------------------------------------------|----------------------------------------------------------------------------------|
|  |Microsoft.VisualBasic.ApplicationServices    |Application tool and Utils for build software in a more easy way                  |
|  |Microsoft.VisualBasic.CommandLine            |CLI framework for VisualBasic App                                                 |
|\*|**Microsoft.VisualBasic.Data**               |Raw data processing related code and charting plot library for data visualization |
|\*|**Microsoft.VisualBasic.DataMining**         |``sciBASIC`` data mining library                                                  |
|\*|**Microsoft.VisualBasic.Imaging**            |``sciBASIC`` graphics system based on the GDI+ API from ``System.Drawing``        |
|\*|**Microsoft.VisualBasic.MachineLearning**    |``sciBASIC`` machine learning library                                             |
|\*|**Microsoft.VisualBasic.Math**               |``sciBASIC`` math library                                                         |
|  |Microsoft.VisualBasic.MIME                   |MIME type text document parser                                                    |
|  |Microsoft.VisualBasic.Scripting              |String expression related API for CLI programming and scripting programming       |

sciBASIC runtime can be split as two part: Part1 is the general application runtime, like scripting helper, commandline system, and text document parser; Another part of the runtime is the data science runtime, like math function, cluster function, or charting runtime for data visualization. All of the data science related namespace in sciBASIC are marked with **asterisk**. 

## FAQs

**1. Which platform that I can using this framework?**
For writing x86/x64 server/Desktop Win32 CLI program.

**2. Relationship between Microsoft VisualBasic.NET and sciBASIC?**
sciBASIC is fully compatible with the native VB.NET language, sciBASIC is a kind of dialect derive from the Microsoft VisualBasic.NET language.

**3. Is there any interactive WinForm/WPF control for charting plot?**
One of this framework its primary target is aim at provides a drawing environment for your CLI program create high quality printable content for scientific manuscript. This framework is not too much involved with "interactive".

**4. Any coding style suggestion by using sciBASIC framework?**
``sciBASIC`` framework is a kind of functional programming runtime for VisualBasic language. 

**5. Major features**
Major features of the ``sciBASIC`` are: 
+ Functional programming
+ Fluent style Method cascading
+ Linq pipeline
+ Parallel computing

## ODEs scripting language feature

Example for solving a dynamics system using VisualBasic ODEs scripting language feature, demo created for the [Lorenz system](https://en.wikipedia.org/wiki/Lorenz_system):

```vbnet
Dim x, y, z As var
Dim sigma# = 10
Dim rho# = 28
Dim beta# = 8 / 3
Dim t = (a:=0, b:=120, dt:=0.005)

Call Let$(list:=Function() {x = 1, y = 1, z = 1})
Call {
    x = Function() sigma * (y - x),
    y = Function() x * (rho - z) - y,
    z = Function() x * y - beta * z
}.Solve(dt:=t) _
 .DataFrame _
 .Save($"{App.HOME}/Lorenz_system.csv")
```

![](./Data_science/Mathematica/data/Lorenz_system/Lorenz_system.png)

## Microsoft VisualBasic Trinity Natural Language Processor

### TextRank

PageRank analysis on the text paragraph for find out the keyword, here is the pagerank result of the this example paragraph:

> "the important pagerank. show on pagerank. have significance pagerank. implements pagerank algorithm. textrank base on pagerank."

![](./Data/TextRank/visualize.png)

### GraphQuery

GraphQuery is a query language and execution engine tied to any backend service. It is back-end language independent.

```vbnet
Imports Microsoft.VisualBasic.Data.GraphQuery
Imports Microsoft.VisualBasic.Data.GraphQuery.Language
Imports Microsoft.VisualBasic.MIME.application.json
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports Microsoft.VisualBasic.MIME.Markup.HTML

' define your graph query at here
Dim queryText As String = "..."
Dim query As Query = QueryParser.GetQuery(queryText)
Dim engine As New Engine
' http get of the html document text from web server or local filesystem
Dim url As String = "..."
Dim doc As HtmlDocument = HtmlDocument.LoadDocument(url)
Dim data As JsonElement = engine.Execute(doc, query)
' debug view of the graph query result
Dim json As String = data.BuildJsonString(New JSONSerializerOptions With {.indent = True})

Call Console.WriteLine(json)
```

Read more about the graphquery language: [GraphQuery](https://github.com/xieguigang/sciBASIC/tree/master/Data/GraphQuery)

```bash
graphquery
{
    # parser function pipeline can be 
    # in different line,
    # this will let you write graphquery
    # code in a more graceful style when
    # you needs a lot of pipeline function
    # for parse value data.
    bookID    css("book") 
            | attr("id")

    title     css("title")
    isbn      xpath("//isbn")
    quote     css("quote")
    language  css("title") | attr("lang")

    # another sub query in current graph query
    author css("author") {
        name css("name")
        born css("born")
        dead css("dead")
    }

    # this is a array of type character
    character xpath("//character") [{
        name          css("name")
        born          css("born")
        qualification xpath("qualification")
    }]
}
```

## Image fast binarization using VisualBasic image extension API
[``Sub Binarization(ByRef curBitmap As Bitmap, Optional style As BinarizationStyles = BinarizationStyles.Binary)``](./Microsoft.VisualBasic.Core/src/Extensions/Image/Bitmap/hcBitmap.vb)

```vbnet
Imports Microsoft.VisualBasic.Imaging

Dim bitmap As Image = Image.FromFile("./etc/lena/f13e6388b975d9434ad9e1a41272d242_1_orig.jpg")

Call bitmap.Grayscale().SaveAs("./etc/lena/lena.grayscale.png", ImageFormats.Png)
Call bitmap.GetBinaryBitmap
     .SaveAs("./etc/lena/lena.binary.png", ImageFormats.Png)
Call bitmap.GetBinaryBitmap(BinarizationStyles.SparseGray)
     .SaveAs("./etc/lena/lena.gray.png", ImageFormats.Png)
```

|Normal|Binary|SparseGray|Grayscale|
|------|------|----|---------|
|![](./etc/lena/f13e6388b975d9434ad9e1a41272d242_1_orig.jpg)|![](./etc/lena/lena.binary.png)|![](./etc/lena/lena.gray.png)|![](./etc/lena/lena.grayscale.png)|

## sciBASIC# Graphics Artist

![](./Data_science/Graph/Model/Tree/KdTree/KDtree-visual.png)
> data visualization of the KD-tree based KNN search result

[![](./gr/network-visualization/KEGG-pathway-network-clusters.png)](https://github.com/SMRUCC/GCModeller/blob/master/src/GCModeller/models/Networks/STRING/FunctionalEnrichmentPlot.vb)

+ **[Network Visualization Interface](./gr/network-visualization/)**
+ **[2D Imaging & 3D graphics engine](./gr/Microsoft.VisualBasic.Imaging/)**
+ **[Isometric 3D graphics engine](./gr/Microsoft.VisualBasic.Imaging/Drawing3D/Models/README.md)**

![](./gr/Microsoft.VisualBasic.Imaging/Drawing3D/Models/screenshots/io.fabianterhorst.isometric.screenshot.IsometricViewTest_doScreenshotThree.png)
![](./gr/network-visualization/tumblr_inline_mqvdlydGCp1qz4rgp.png)
![](./gr/build_3DEngine/images/screenshot.png)

## Microsoft VisualBasic Data Science & Data Plots System

+ **[Mathematics & Chart Ploting System](./Data_science/Mathematical/)**
+ **[Darwinism computing module](./Data_science/Darwinism/)**
+ **[Data Mining &amp; Machine Learning](./Data_science/)**
+ **[sciBASIC# DataFrame System](./Data/DataFrame/)**

##### sciBASIC# Chart Plots System

```vbnet
Imports Microsoft.VisualBasic.Data.ChartPlots
```

![](./Data_science/kmeans3D.png)
![](./Data_science/Mathematica/images/DensityPlot.png)
![](./Data_science/DataMining/hierarchical-clustering/test.png)
![](./Data_science/Mathematica/images/identity_pie.png)
![](./Data_science/Mathematica/images/heatmap/Sample.SPCC.png)
![](./Data_science/Mathematica/images/log2FC_heatmap.png)
![](./Data_science/algorithms/CMeans/CMeans.png)
![](./Data_science/Mathematica/images/295022-plots-plots.png)
[![](./Data_science/Mathematica/images/alignment/L26369_%255BM%252BH%255D%252B%2523.%252Flxy-CID30.mzXML%2523294-alignment.png)](./Data_science/Mathematica/Plots/BarPlot/AlignmentPlot.vb)
![](./Data_science/Mathematica/images/boxplot/simpson.png)
![](./Data_science/Mathematica/images/stat/16S-KO-level3-Z-scores.png)

###### 3D heatmap

```vbnet
Dim func As Func(Of Double, Double, (Z#, Color#)) =
_
    Function(x, y) (3 * Math.Sin(x) * Math.Cos(y), Color:=x + y ^ 2)

Call Plot3D.ScatterHeatmap.Plot(
    func, "-3,3", "-3,3",
    New Camera With {
        .screen = New Size(3600, 2500),
        .ViewDistance = -3.3,
        .angleZ = 30,
        .angleX = 30,
        .angleY = -30,
        .offset = New Point(-100, -100)
    }) _
    .SaveAs("./3d-heatmap.png")
```

![](./Data_science/Mathematica/images/3d-heatmap.png)

###### Contour Heatmap

You can using a lambda expression as the plot data source:

```vbnet
Dim f As Func(Of Double, Double, Double) =
    Function(x, y) x ^ 2 + y ^ 3

Call ScatterHeatmap _
    .Plot(f, "(-1,1)", "(-1,1)", legendTitle:="z = x ^ 2 + y ^ 3") _
    .SaveAs("./scatter-heatmap.png")
```

![](./Data_science/Mathematica/images/scatter-heatmap.png)
![](./Data_science/Mathematica/images/256821.654661046-rho_gamma3_1%2C120_0.25%2C20.png)

###### Stacked Barplot

The stacked barplot is a best choice for visualize the sample composition and compares to other samples data:

```vbnet
Imports Microsoft.VisualBasic.Data.ChartPlots

' Plots metagenome taxonomy profiles annotation result using barplot
Dim file = "./FigurePlot-Reference-Unigenes.absolute.level1.csv"
' Using color brewer color profiles
Dim taxonomy As BarDataGroup = csv.LoadBarData(file, "Paired:c8") 

Call BarPlot.Plot(
    taxonomy,
    New Size(2000, 1400),
    stacked:=True,
    legendFont:=New Font(FontFace.BookmanOldStyle, 18)
) _
    .SaveAs("./FigurePlot-Reference-Unigenes.absolute.level1.png")
```

![](./Data_science/Mathematica/images/FigurePlot-Reference-Unigenes.absolute.level1.png)

![](./Data_science/Visualization/data/sample_groups.VolinPlot.png)

###### beta-PDF

```vbnet
Public Function beta(x#, alpha#, _beta#) As Double
    Return Pow(x, alpha - 1) * Pow((1 - x), _beta - 1) *
        Exp(lgamma(alpha + _beta) - lgamma(alpha) - lgamma(_beta))
End Function

Public Function lgamma(x As Double) As Double
    Dim logterm As Double = Math.Log(x * (1.0F + x) * (2.0F + x))
    Dim xp3 As Double = 3.0F + x

    Return -2.081061F - x + 0.0833333F / xp3 -
        logterm + (2.5F + x) * Math.Log(xp3)
End Function
```

![](./Data_science/Mathematica/data/beta-PDF/beta_PDF.png)
> https://en.wikipedia.org/wiki/Beta_distribution

###### Heatmap
![](./Data_science/Mathematica/images/heatmap.png)

```vbnet
Dim data = DataSet.LoadDataSet("./Quick_correlation_matrix_heatmap/mtcars.csv")

Call data.CorrelatesNormalized() _
    .Plot(mapName:="Jet",  ' Using internal color theme 'Jet'
          mapLevels:=20,
          legendFont:=New Font(FontFace.BookmanOldStyle, 32)) _
    .SaveAs("./images/heatmap.png")
```

> ###### Microsoft.VisualBasic.Mathematical.Plots.Heatmap::Plot(IEnumerable(Of NamedValue(Of Dictionary(Of String, Double))), Color(), Integer, String, Boolean, Size, Size, String, String, String) As Bitmap

Heatmap data source from R dataset [``mtcars``](./Data_science/Mathematica/Quick_correlation_matrix_heatmap/mtcars.csv) and calculates [the Pearson correlations](./Microsoft.VisualBasic.Architecture.Framework/Extensions/Math/Correlations.vb):

```R
data(mtcars)
write.csv(mtcars, "./Data_science/Mathematical/Quick_correlation_matrix_heatmap/mtcars.csv")
```

<hr/>

## New VisualBasic Language Syntax in this runtime

First of all, imports the language feature namespace of VisualBasic

```vbnet
#Region "Microsoft VisualBasic.NET language"
' sciBASIC# general application runtime
' Microsoft.VisualBasic.Architecture.Framework_v3.0_22.0.76.201__8da45dcd8060cc9a.dll
#End Region

Imports Microsoft.VisualBasic.Language
```

###### 1. Inline value assign

Old:

```vbnet
Dim s As String = ""

Do While Not s Is Nothing
   s = blablabla

   ' Do other staff
Loop
```

New:

```vbnet
Dim s As New Value(Of String)

Do While Not (s = blablabla) Is Nothing
   ' Do other staff
Loop
```

###### 2. List(Of ) Add

Old:

```vbnet
Dim l As New List(Of String)

Call l.Add("123")
Call l.AddRange(From x In 100.Sequence Select CStr(x))
```

New:

```vbnet
Dim l As New List(Of String)

l += "123"
l += From x As Integer
     In 100.Sequence
     Select CStr(x)
```

###### New Integer(int) type in visualbasic

+ value ranges syntax

```vbnet
Dim min As int = 1
Dim max As int = 200
Dim x% = 199

' Compares
Call println(min <= x < max) ' True
x += 10 ' 209
Call println(min <= x < max) ' False
x = -1
Call println(min <= x < max) ' False
```

+ inline calculation and value assign

```vbnet
Dim bitChunk As Byte() = New Byte(INT64 - 1) {}
Dim p As int = Scan0

Call Array.ConstrainedCopy(rawStream, ++(p + INT64), bitChunk, Scan0, INT64)
ProtocolCategory = BitConverter.ToInt64(bitChunk, Scan0)

Call Array.ConstrainedCopy(rawStream, ++(p + INT64), bitChunk, Scan0, INT64)
Protocol = BitConverter.ToInt64(bitChunk, Scan0)

bitChunk = New Byte(INT64 - 1) {}
Call Array.ConstrainedCopy(rawStream, p = (p + INT64), bitChunk, Scan0, INT64)
BufferLength = BitConverter.ToInt64(bitChunk, Scan0)
```

<hr/>

![](./www/data/github/xieguigang_github-vcard.png)

<img src="https://starchart.cc/xieguigang/sciBASIC.svg" alt="Stargazers over time" style="max-width: 60%">

> Copyleft 2022, [I@xieguigang.me](mailto://I@xieguigang.me) (http://sciBASIC.NET/)
