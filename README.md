# Microsoft VisualBasic App Runtime

![(๑•̀ㅂ•́)و✧](./etc/badge.png)
![](https://cdn.rawgit.com/LunaGao/BlessYourCodeTag/master/tags/alpaca.svg)
[![Github All Releases](https://img.shields.io/github/downloads/xieguigang/VisualBasic_AppFramework/total.svg?maxAge=2592000?style=flat-square)]()
[![GPL Licence](https://badges.frapsoft.com/os/gpl/gpl.svg?v=103)](https://opensource.org/licenses/GPL-3.0/)

![Microsoft VisualBasic logo](./logo.jpg)

#### Directory Structure

+ **[/CLI_tools](./CLI_tools/)** : Some small utilities and example tools
+ **[/Data](./Data/)** : VisualBasic data framework system for data science
+ **[/Data_science](./Data_science/)** : VBMath system
+ **[/Microsoft.VisualBasic.Architecture.Framework](./Microsoft.VisualBasic.Architecture.Framework/)** : Microsoft VisualBasic App Runtime core
+ **[/doc](./doc/)** : VisualBasic doc parsers
+ **[/gr](./gr/)** : VisualBasic data graphics system
+ **[/win32_api](./win32_api/)** : Win32 API collection
+ **[/www](./www/)** : Web related codes

###### docs for User

+ **[/guides](./guides/)**
+ **[/vb_codestyle](./vb_codestyle/)**


---------------------------------------------------------------------------------------------------------------

A language feature runtime library for server side CLI application. This framework project includes a lot of utility tools for the enterprises system programming for VisualBasic, and extends the VisualBasic programming language syntax and the utility code function. Makes the VisualBasic programming style more modernization by using this runtime library framework.

> Abount VisualBasic code style guidelines:
> + https://github.com/xieguigang/VisualBasic_AppFramework/tree/master/vb_codestyle

> Guides for using this framework, you can found the document and content index at the [README.md](./guides/README.md)(This guidelines document is currently compiling for users):
> + https://github.com/xieguigang/VisualBasic_AppFramework/blob/master/guides/


##### Install this framework via nuget package

+ https://www.nuget.org/packages/VB_AppFramework/

>  PM> Install-Package VB_AppFramework

+ For .NET framework 4.0, install package
https://www.nuget.org/packages/VB_AppFramework_40/

>  PM> Install-Package VB_AppFramework_40

## Microsoft VisualBasic Mathematics System for Data Science

+ **[Mathematics System](./Data_science/Mathematical)** <<<
+ **[DataFrame System for VisualBasic Data Science](./DocumentFormats/VB_DataFrame)** <<<
+ **[Network Visualization Interface](./Datavisualization/Datavisualization.Network)** <<<

![](./gr/Datavisualization.Network/tumblr_inline_mqvdlydGCp1qz4rgp.png)

##### Plots System
```vbnet
Imports Microsoft.VisualBasic.Mathematical.Plots
```

![](./Data_science/Mathematical/images/Bubble.png)
![](./Data_science/Mathematical/images/37_number_of_observation_on_barplot.png)

## What's new of VisualBasic language Syntax from this runtime library?

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
Imports Microsoft.VisualBasic.Language

Dim s As New Value(Of String)

Do While Not (s = blablabla) Is Nothing
   ' Do other staff
Loop
```

###### 2. List(Of )

Old:

```vbnet
Dim l As New List(Of String)

Call l.Add("123")
Call l.AddRange(From x In 100.Sequence Select CStr(x))
```

New:

```vbnet
Imports Microsoft.VisualBasic

Dim l As New List(Of String)

l += "123"
l += From x As Integer
     In 100.Sequence
     Select CStr(x)
```

###### int Type

```vbnet
Imports Microsoft.VisualBasic.Language

Dim min As int = 1
Dim max As int = 200
Dim x As Integer = 199

Console.WriteLine(min <= x < max) ' True
x += 10 ' 209
Console.WriteLine(min <= x < max) ' False
x = -1
Console.WriteLine(min <= x < max) ' False
```

## Framework Gallery
Simple 3D Graphics by [Microsoft.VisualBasic.Imaging](./gr/Microsoft.VisualBasic.Imaging) 3D engine.

![](./gr/d3.png)

Chart plot system

![](./Data_science/Mathematical/images/pie_chart.png)

=========================

###### Modules that Includes in this Framework:

> 1. A document library of read and write Csv document for facility the data exchanges between the GCModeller and R program.
> 2. A distribution computing framework available at repository: https://github.com/xieguigang/Microsoft.VisualBasic.Parallel
> 3. Microsoft VisualBasic Application Framework codes
> 4. Memory pepline services between two client program
> 5. VisualBasic language Feature:  Unix bash command supports in under development which parts of the API is available at namespace Microsoft.VisualBasic.Language

###### Image fast binarization using VisualBasic extension API
[``Sub Binarization(ByRef curBitmap As Bitmap, Optional style As BinarizationStyles = BinarizationStyles.Binary)``](./Microsoft.VisualBasic.Architecture.Framework/Extensions/Image/Bitmap/hcBitmap.vb)

|Normal|Binary|
|------|------|
|<img src="./Microsoft.VisualBasic.Architecture.Framework/Extensions/Image/f13e6388b975d9434ad9e1a41272d242_1_orig.jpg" width=250 height=250 />|<img src="./Microsoft.VisualBasic.Architecture.Framework/Extensions/Image/lena.binary.jpg" />|
