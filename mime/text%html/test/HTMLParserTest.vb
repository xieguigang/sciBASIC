#Region "Microsoft.VisualBasic::5f4a02196e5d904c443739b01603be6d, mime\text%html\test\HTMLParserTest.vb"

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
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MIME.Markup.HTML

Module HTMLParserTest

    ReadOnly testHTML$ =
        (<div style='font-style: normal; font-size: 14; font-family: Microsoft YaHei;' attr2="99999999 + dd">
             <span style="color:red;">Hello</span><span style="color:blue;">world!</span> 
            2<sup>333333</sup> + X<sub>i</sub> = <span style="font-size: 36;">6666666</span>
         </div>).ToString

    Sub Main()

        Dim content = TextAPI.TryParse(testHTML).ToArray

        Pause()
    End Sub
End Module
