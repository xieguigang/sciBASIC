Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core

Module sqliteTest
    Sub Main()
        Using fs = "C:\Users\Administrator\Downloads\xcb.db".Open(doClear:=False), db As New Sqlite3Database(fs)
            Dim tbl = db.GetTable("Pathways")
            Dim rows = tbl.EnumerateRows.ToArray

            Pause()
        End Using
    End Sub
End Module
