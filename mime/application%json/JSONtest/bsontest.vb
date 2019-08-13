Imports System.IO
Imports Microsoft.VisualBasic.MIME.application.json
Imports Microsoft.VisualBasic.MIME.application.json.Parser

Module bsontest

    Sub Main()
        Dim obj As New JsonObject()
        obj("hello") = New JsonValue(123)

        Dim where As New JsonObject()

        obj("where") = where
        where("Korea") = New JsonValue("Asia")
        where("USA") = New JsonValue("America")
        obj("bytes") = New JsonValue(New Byte(128) {})

        Using file As Stream = "./test.bson".Open
            Call BSON.WriteBuffer(obj, buffer:=file)
        End Using
    End Sub
End Module
