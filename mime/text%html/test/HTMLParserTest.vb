#Region "Microsoft.VisualBasic::bd9bf22629a5dc616943f2dd7756aea0, mime\text%html\test\HTMLParserTest.vb"

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

    ' Module HTMLParserTest
    ' 
    '     Sub: Main, realHtmlParseTest
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MIME.Html.Language

Module HTMLParserTest

    ReadOnly testHTML$ =
        (<div style='font-style: normal; font-size: 14; font-family: Microsoft YaHei;' attr2="99999999 + dd">
             <span style="color:red;">Hello</span><span style="color:blue;">world!</span> 
            2<sup>333333</sup> + X<sub>i</sub> = <span style="font-size: 36;">6666666</span>
             <br/>
             <img src="aaa.png"/>
             <font></font>
         </div>).ToString

    ReadOnly simpleTagTest$ = (<div>
                                   <span>A</span>
                                   <span>B</span>
                               </div>).ToString
    ReadOnly simpleTag2$ = (<br/>).ToString

    ReadOnly realHtmlTest$ = "
<!DOCTYPE html>
<!--[if IE 9]><html class='lt-ie10' lang='en'><![endif]-->

<img src='/path/folder/aaa.gif' width='100%'>
<br>
<hr>
<br />
<hr/>
<p style='color: red;'>
this is text
</p>
"

    Sub realHtmlParseTest()
        Dim document = HtmlParser.ParseTree("E:\repo\xDoc\Yilia\runtime\sciBASIC#\mime\text%html\Test\test.html".GET)

        Pause()
    End Sub

    Sub Main()
        Call realHtmlParseTest()

        Dim real = HtmlParser.ParseTree(realHtmlTest)
        Dim newline = HtmlParser.ParseTree(simpleTag2)
        Dim simple = HtmlParser.ParseTree(simpleTagTest)
        Dim doc2 = HtmlParser.ParseTree(testHTML)

        ' Dim content = TextAPI.TryParse(testHTML).ToArray

        Pause()
    End Sub
End Module
