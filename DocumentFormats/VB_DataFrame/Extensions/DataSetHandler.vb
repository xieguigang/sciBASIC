Imports System.Text
Imports Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider.Reflection

Public Module DataSetHandler

    Public Sub InitHandle()
        Call CollectionIO.SetHandle(AddressOf ISaveCsv)
        Call VBDebugger.Warning($"Collection IO extension default IO handle has been changes to {CollectionIO.DefaultHandle.ToString}...")
    End Sub

    Public Function ISaveCsv(source As IEnumerable, path As String, encoding As Encoding) As Boolean
        Dim o As Object = (From x In source Select x).FirstOrDefault
        Dim type As Type = o.GetType

        path = FileIO.FileSystem.GetFileInfo(path).FullName

        Call Console.WriteLine("[CSV.Reflector::{0}]" & vbCrLf & "Save data to file:///{1}", type.FullName, path)
        Call Reflector.__save(source, type, False).Save(path, LazySaved:=False, encoding:=encoding)
        Call Console.WriteLine("CSV saved!")

        Return True
    End Function
End Module
