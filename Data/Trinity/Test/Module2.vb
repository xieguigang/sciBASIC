#Region "Microsoft.VisualBasic::d44e04cece63bbad1e6a11019e1848e2, Data\Trinity\Test\Module2.vb"

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

    '   Total Lines: 18
    '    Code Lines: 13
    ' Comment Lines: 0
    '   Blank Lines: 5
    '     File Size: 896 B


    ' Module Module2
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.NLP.Model
Imports Microsoft.VisualBasic.Serialization.JSON

Module Module2

    Sub Main()
        Dim text As String =
            <str>
                Methylated anthocyanin glycosides were isolated from red Canna indica flower and identified as malvidin 3-O-(6-O-acetyl-beta-d-glucopyranoside)-5-O-beta-d-glucopyranoside (1), malvidin 3,5-O-beta-d-diglucopyranoside (2), cyanidin-3-O-(6''-O-alpha-rhamnopyranosyl-beta-glucopyranoside (3), cyanidin-3-O-(6''-O-alpha-rhamnopyranosyl)-beta-galactopyranoside (4), cyanidin-3-O-beta-glucopyranoside (5) and cyanidin-O-beta-galactopyranoside (6) by HPLC-PDA(http://test.url.com/?q=a,a,b,c+{""aaa"":99999}).
            </str>

        Dim tokens As String() = New SentenceCharWalker(text).GetTokens().ToArray

        Call Console.WriteLine(tokens.GetJson)

        Pause()
    End Sub
End Module
