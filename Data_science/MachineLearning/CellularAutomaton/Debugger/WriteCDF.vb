Imports Microsoft.VisualBasic.Data.IO.netCDF
Imports Microsoft.VisualBasic.Data.IO.netCDF.Components
Imports Microsoft.VisualBasic.Language

Module WriteCDF

    Public Sub Flush(path As String, cache As Integer()()(), type As Type)
        Using file As New CDFWriter(path)
            Dim w = cache(Scan0).Length
            Dim h = cache.Length
            Dim data As CDFData
            Dim id As i32 = Scan0
            Dim dims As Dimension()
            Dim attrs As attribute()

            file.GlobalAttributes(
                New attribute With {.name = "schema", .type = CDFDataTypes.CHAR, .value = type.FullName},
                New attribute With {.name = "size\width", .type = CDFDataTypes.INT, .value = w},
                New attribute With {.name = "size\height", .type = CDFDataTypes.INT, .value = h}
            )
            file.Dimensions(Dimension.Integer)

            For j As Integer = 0 To w - 1
                For i As Integer = 0 To h - 1
                    data = cache(i)(j).ToArray
                    dims = {Dimension.Integer}
                    attrs = {
                        New attribute With {.name = "i", .type = CDFDataTypes.INT, .value = i},
                        New attribute With {.name = "j", .type = CDFDataTypes.INT, .value = j}
                    }
                    file.AddVariable((++id).ToHexString, data, dims, attrs)
                Next
            Next
        End Using
    End Sub
End Module
