#Region "Microsoft.VisualBasic::bc88ea83c1c1346c6195722a26af5b35, mime\application%json\JSONtest\simple_jsonParserTest.vb"

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

    '   Total Lines: 45
    '    Code Lines: 24 (53.33%)
    ' Comment Lines: 16 (35.56%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 5 (11.11%)
    '     File Size: 1.56 KB


    ' Module simple_jsonParserTest
    ' 
    '     Sub: Main, test1
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MIME.application.json
Imports Microsoft.VisualBasic.MIME.application.json.Javascript

Module simple_jsonParserTest

    Sub Main()
        Call test1()
    End Sub

    Sub test1()
        ' Dim str As JsonValue = JsonParser.Parse("'abc'")
        ' Dim null As JsonValue = JsonParser.Parse("null")
        ' Dim vec1 As JsonArray = JsonParser.Parse("[-1,1,2,3,4,5]")
        ' Dim obj As JsonObject = JsonParser.Parse("{'a': true, b: [3,3,4]}")
        ' Dim literal As JsonValue = JsonParser.Parse("false//a scalar boolean value")
        ' Dim empty_array As JsonArray = JsonParser.Parse("[]")
        ' Dim empty_obj As JsonObject = JsonParser.Parse("{}")
        ' Dim escape_str As JsonValue = JsonParser.Parse("'this is \'string\', another ""string block"".'")
        Dim escpae_strVal As JsonArray = JsonParser.Parse("['this is \'string\', \nanother ""string block"".']")
        Dim obj_no_comment = JsonParser.Parse("
        {
            'a': true,
            // is an integer vector
            'v': [1,1,1,3,4,5],
            'empty_array': [],
            // is a string
            'str': 'hello ""world""!',
            // string in multiple lines
            ""text"": '
                line1
                line2
                line3
            ',
            'nest_object': {
                'empty': {},
                scalar: false
            },
            'flag': false// is a single comment line
        }

        ")

        Pause()
    End Sub
End Module
