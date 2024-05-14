#Region "Microsoft.VisualBasic::749193b3f43ab201a94bff410cb295c2, mime\application%json\JSONtest\logicTest.vb"

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
    '    Code Lines: 14
    ' Comment Lines: 0
    '   Blank Lines: 4
    '     File Size: 567 B


    ' Module logicTest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MIME.application.json.JSONLogic

Module logicTest

    Sub Main()
        Dim out As Object

        out = jsonLogic.apply("{'==' : [1, 1]}")
        out = jsonLogic.apply("{'If': [  {'<': [{'var':'temp'}, 0] }, 'freezing',  {'<': [{'var':'temp'}, 100] }, 'liquid',  'gas']}", "{'temp':55}")
        out = jsonLogic.apply("{'if': [ true, 'yes', 'no' ]}")
        out = jsonLogic.apply(
            "{'var': ['a'] }",  ' Logic
            "{'a': 1, 'b': 2 }" ' Data
        )   ' 1

        Pause()
    End Sub
End Module
