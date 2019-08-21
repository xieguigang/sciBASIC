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
