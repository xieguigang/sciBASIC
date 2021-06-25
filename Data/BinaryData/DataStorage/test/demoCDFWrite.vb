Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.IO.netCDF
Imports Microsoft.VisualBasic.Data.IO.netCDF.Components
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Module demoCDFWrite

    Sub Main()
        Using cdf As New CDFWriter("E:\GCModeller\src\R-sharp\tutorials\io\CDF\dataframe.netcdf")
            Dim range1 As DoubleRange = {-99, 99}

            ' create random data vectors as demo data
            Dim a As integers = {2, 2, 3, 4, 5, 1, 1, 1, 1, 1}
            Dim b As doubles = a.Select(Function(any) randf.GetRandomValue(range1)).ToArray
            Dim flags As flags = a.Select(Function(any) randf.NextBoolean).ToArray
            Dim id As integers = a.Select(Function(any, i) i).ToArray

            Dim data_size As New Dimension With {
                .name = "nrow",
                .size = a.Length
            }

            Call cdf.GlobalAttributes(New attribute("time", Now.ToString, CDFDataTypes.CHAR)) _
                    .GlobalAttributes(New attribute("num_of_variables", 4, CDFDataTypes.INT)) _
                    .GlobalAttributes(New attribute("github", "https://github.com/xieguigang/sciBASIC", CDFDataTypes.CHAR))

            Call cdf.AddVariable("a", a, data_size, {New attribute("note", "this is an integer vector", CDFDataTypes.CHAR)})
            Call cdf.AddVariable("b", b, data_size)
            Call cdf.AddVariable("flags", flags, data_size)
            Call cdf.AddVariable("id", id, data_size, {New attribute("note", "this is a unique id vector in asc order", CDFDataTypes.CHAR)})
        End Using

    End Sub
End Module
