#Region "Microsoft.VisualBasic::4dcba88feef6ecee3a82589acb06a0d0, mime\text%markdown\test\Program.vb"

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

    '   Total Lines: 148
    '    Code Lines: 95 (64.19%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 53 (35.81%)
    '     File Size: 2.72 KB


    ' Module Program
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MIME.text.markdown

Module Program

    Const markdown_demo As String = "

# header1

## *header2*

this has a ``code`` span. **yes it** is!

```md
not a **bold** word
---------------------------
``aaa``

```

-------------------------------

### head*er3*

***

this is *alt* __bold__ style
this is _alt italic_ style

_________
_________


|a|b|c|
|--|--|--|
|*1*|2|<span style='color: red;'>3</span>|
|4|*5*|6|
|7|**8**|*9*|

#### head``e``r4

###### tittle level 6

+ item1
+ item2
+ item2
+ item3: ``a+b=c``
+ item4

> a **fox**
>
> a ![image](aa/bb.png) [url](aaa.txt)


--------

Valid links:

 [this is a link]()
 [this is a link](<http://something.example.com/foo/bar>)
 [this is a link](http://something.example.com/foo/bar 'test')
 ![this is an image]()
 ![this is an image](<http://something.example.com/foo/bar>)
 ![this is an image](http://something.example.com/foo/bar 'test')
 
 [escape test](<\>\>\>\>\>\>\>\>\>\>\>\>\>\>> '\'\'\'\'\'\'\'\'\'\'\'\'\'\'')
 [escape test \]\]\]\]\]\]\]\]\]\]\]\]\]\]\]\]](\)\)\)\)\)\)\)\)\)\)\)\)\)\))



- alter list 1
- alter list2
- alter list3
- alter list4
- alter list``5``


Invalid links:

 [this is not a link

 [this is not a link](

 [this is not a link](http://something.example.com/foo/bar 'test'
 
 [this is not a link](((((((((((((((((((((((((((((((((((((((((((((((
 
 [this is not a link]((((((((((()))))))))) (((((((((()))))))))))


*this* *is* *your* *basic* *boring* *emphasis*

_this_ _is_ _your_ _basic_ _boring_ _emphasis_

**this** **is** **your** **basic** **boring** **emphasis**


- alter list 1
- alter list2
- alter list3

- alter list4
- alter list``5``

1.  a
20.  b
30. x
40. d
500. . g``gg``


```
- alter list 1
- alter list2
- alter list3

10. a
20. b
30. c
```

"

    Const quote_demo = "
# reference

> aaaaa
> bbbbbbb
> ccccc
> dddddddd

"

    Const quote_links = "

### Help Documentation and Literature Citation

> 1. Abdelmoula WM, Lopez BG, Randall EC, Kapur T, Sarkaria JN, White FM, Agar JN, Wells WM, Agar NYR. Peak learning of mass spectrometry imaging data using artificial neural networks. *Nat Commun*. 2021 Sep 20;12(1):5544. doi: 10.1038/s41467-021-25744-8. PMID: 34545087; PMCID: PMC8452737.
> 2. Parse the SMILES molecule structre string - https://mzkit.org/vignettes/mzkit/mzkit/SMILES/parse.html
> 3. as.formula - https://mzkit.org/vignettes/mzkit/mzkit/SMILES/as.formula.html
> 4. get atoms table from the SMILES structure data - https://mzkit.org/vignettes/mzkit/mzkit/SMILES/atoms.html
> 5. create graph embedding result for a specific molecular strucutre data - https://mzkit.org/vignettes/mzkit/mzkit/SMILES/links.html
"

    Private Sub tableTest()
        Dim md = "E:\GCModeller\src\runtime\sciBASIC#\mime\text%markdown\test\test_table.md".ReadAllText
        Call New MarkdownRender().Transform(md).SaveTo("./test_table.html")
    End Sub

    Sub Main(args As String())
        tableTest()
        Console.WriteLine(New MarkdownRender().Transform(quote_links))

        Call New MarkdownRender().Transform(quote_links).SaveTo("./test_demo.html")
    End Sub
End Module
