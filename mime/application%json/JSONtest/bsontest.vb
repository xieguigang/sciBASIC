#Region "Microsoft.VisualBasic::017e80907f9fb2362205ae2d14df52d1, mime\application%json\JSONtest\bsontest.vb"

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

    '   Total Lines: 31
    '    Code Lines: 22
    ' Comment Lines: 0
    '   Blank Lines: 9
    '     File Size: 821 B


    ' Module bsontest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.MIME.application.json
Imports Microsoft.VisualBasic.MIME.application.json.Javascript

Module bsontest

    Sub Main()
        Dim obj As New JsonObject()
        obj("hello") = New JsonValue(123)

        Dim where As New JsonObject()

        obj("where") = where
        where("Korea") = New JsonValue("Asia")
        where("USA") = New JsonValue("America")
        obj("bytes") = New JsonValue(New Byte(128) {})
        obj("yes?") = New JsonValue(True)

        Dim bsonPath = "./test.bson"

        Using file As Stream = bsonPath.Open
            Call BSON.WriteBuffer(obj, buffer:=file)
        End Using

        Dim load = BSON.Load(bsonPath.ReadBinary)

        Dim json = load.BuildJsonString

        Pause()
    End Sub
End Module
