#Region "Microsoft.VisualBasic::81f17644b4da4a590e266287553a56b1, mime\application%json\JSONtest\writerTest.vb"

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

    '   Total Lines: 37
    '    Code Lines: 20 (54.05%)
    ' Comment Lines: 11 (29.73%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 6 (16.22%)
    '     File Size: 979 B


    ' Module writerTest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MIME.application.json
Imports Microsoft.VisualBasic.MIME.application.json.Javascript

Module writerTest

    Const json As String = "{
    'vec': [23,4,44,23,42,34,2,3,42,3,4,2.34],
    'strs': [
        'dsadsadad',
'asdadasdsd','qeqeqweqwewq','4444444444','3333'
],
'chars':['a','s','m','d','k','j','a','d','b','a','s','d','a','s','d'],
'flags':[true,true,true,false,false,true],
'nest_obj':{
    'a': true,
    'b': [5,23,53,2.4244,2.323],
    'c': 'xxxxxxxxxxxxxxxxxx'
},
'objs': [

{'a':1,'c':false},
{'a':-1,'c':false,'v':[23,4,32,43,24]},
{'a':1,'c':true}
]
}"

    Sub Main()
        Dim json As JsonElement = JsonParser.Parse(writerTest.json)
        Dim json_str As String = json.BuildJsonString(indent:=True)
        Dim json_line As String = json.BuildJsonString(indent:=False)

        Call Console.WriteLine(json_str)
        Call Console.WriteLine(json_line)

        Pause()
    End Sub
End Module
