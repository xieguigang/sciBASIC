# Microsoft VisualBasic App Runtime

![(๑•̀ㅂ•́)و✧](./etc/badge.png)
![](https://cdn.rawgit.com/LunaGao/BlessYourCodeTag/master/tags/alpaca.svg)
[![Github All Releases](https://img.shields.io/github/downloads/xieguigang/VisualBasic_AppFramework/total.svg?maxAge=2592000?style=flat-square)]()
[![GPL Licence](https://badges.frapsoft.com/os/gpl/gpl.svg?v=103)](https://opensource.org/licenses/GPL-3.0/)

![Microsoft VisualBasic logo](./logo.jpg)

A language feature runtime library for server side CLI application. This framework project includes a lot of utility tools for the enterprises system programming for VisualBasic, and extends the VisualBasic programming language syntax and the utility code function. Makes the VisualBasic programming style more modernization by using this runtime library framework.

> Abount VisualBasic code style guidelines:
> + https://github.com/xieguigang/VisualBasic_AppFramework/tree/master/vb_codestyle

> Guides for using this framework, you can found the document and content index at the [README.md](./guides/README.md)(This guidelines document is currently compiling for users):
> + https://github.com/xieguigang/VisualBasic_AppFramework/blob/master/guides/


+ Install this framework via nuget package:
https://www.nuget.org/packages/VB_AppFramework/

>  PM> Install-Package VB_AppFramework

+ For .NET framework 4.0, install package
https://www.nuget.org/packages/VB_AppFramework_40/

>  PM> Install-Package VB_AppFramework_40

## Framework Gallery
![](./Datavisualization/Datavisualization.Network/tumblr_inline_mqvdlydGCp1qz4rgp.png)

Simple 3D Graphics by [Microsoft.VisualBasic.Imaging](./Datavisualization/Microsoft.VisualBasic.Imaging) 3D engine.

![](./Datavisualization/d3.png)

=========================

###### Modules that Includes in this Framework:

> 1. A document library of read and write Csv document for facility the data exchanges between the GCModeller and R program.
> 2. A distribution computing framework available at repository: https://github.com/xieguigang/Microsoft.VisualBasic.Parallel
> 3. Microsoft VisualBasic Application Framework codes
> 4. Memory pepline services between two client program
> 5. VisualBasic language Feature:  Unix bash command supports in under development which parts of the API is available at namespace Microsoft.VisualBasic.Language

####### Image fast binarization using VisualBasic extension API:
[``Sub Binarization(ByRef curBitmap As Bitmap, Optional style As BinarizationStyles = BinarizationStyles.Binary)``](./Microsoft.VisualBasic.Architecture.Framework/Extensions/Image/Bitmap/hcBitmap.vb)

|Normal|Binary|
|------|------|
|<img src="./Microsoft.VisualBasic.Architecture.Framework/Extensions/Image/f13e6388b975d9434ad9e1a41272d242_1_orig.jpg" width=250 height=250 />|<img src="./Microsoft.VisualBasic.Architecture.Framework/Extensions/Image/lena.binary.jpg" />|