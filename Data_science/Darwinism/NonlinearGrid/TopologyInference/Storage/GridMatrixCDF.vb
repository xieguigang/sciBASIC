Imports Microsoft.VisualBasic.Data.IO.netCDF
Imports Microsoft.VisualBasic.Data.IO.netCDF.Components

''' <summary>
''' 对于一个很大的矩阵而言, 因为XML文件是字符串格式, 序列化和反序列化都需要大量进行字符串的解析或者生成等操作, 
''' I/O性能会非常差, 并且生成的模型文件会很大 
''' 
''' 所以对于大矩阵而言, 在这里使用CDF文件来存储, 从而减小模型文件以及提升IO性能
''' </summary>
Public Module GridMatrixCDF

    Public Function WriteCDF(genome As GridSystem, cdf$)
        Using cdfWriter = New CDFWriter(cdf).Dimensions(
            Dimension.Double,
            Dimension.Integer
        )
            Call cdfWriter.AddVariable("A", New CDFData With {.numerics = genome.A}, GetType(Double).FullName, {New attribute With {.name = "const", .type = CDFDataTypes.DOUBLE, .value = genome.AC}})
        End Using
    End Function
End Module
