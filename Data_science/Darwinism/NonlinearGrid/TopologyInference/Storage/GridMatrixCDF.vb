Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.IO.netCDF
Imports Microsoft.VisualBasic.Data.IO.netCDF.Components
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' 对于一个很大的矩阵而言, 因为XML文件是字符串格式, 序列化和反序列化都需要大量进行字符串的解析或者生成等操作, 
''' I/O性能会非常差, 并且生成的模型文件会很大 
''' 
''' 所以对于大矩阵而言, 在这里使用CDF文件来存储, 从而减小模型文件以及提升IO性能
''' </summary>
Public Module GridMatrixCDF

    ReadOnly doubleFullName$ = GetType(Double).FullName

    Const cor_factor$ = "cor.factor"

    ''' <summary>
    ''' 从CDF文件之中加载计算模型
    ''' </summary>
    ''' <param name="cdf"></param>
    ''' <returns></returns>
    Public Function LoadFromCDF(cdf As String) As GridSystem
        Using reader As New netCDFReader(cdf)
            Dim size As Integer = reader("size")
            Dim factor = reader.getFactor(cor_factor)
            Dim system As New GridSystem With {
                .A = factor.factors,
                .AC = factor.const
            }
            Dim names$() = CStr(reader("variables")).LoadJSON(Of String())
            Dim cor As Correlation() = names _
                .Select(Function(name) As Correlation
                            If Not reader.dataVariableExists(name) Then
                                Return Nothing
                            Else
                                factor = reader.getFactor(name)
                            End If

                            Return New Correlation With {
                                .B = factor.factors,
                                .BC = factor.const
                            }
                        End Function) _
                .Where(Function(c) Not c Is Nothing) _
                .ToArray

            ' 进行数据校验
            If cor.Length <> size OrElse size <> system.Width Then
                Throw New InvalidProgramException
            End If

            Return system _
                .With(Sub()
                          system.C = cor
                      End Sub)
        End Using
    End Function

    <Extension>
    Private Function getFactor(cdf As netCDFReader, var As String) As ([const] As Double, factors As Vector)
        Dim vector As CDFData = cdf.getDataVariable(var)
        Dim factor_const# = cdf.getDataVariableEntry(var) _
            .FindAttribute("const") _
           ?.value

        Return (factor_const, vector.numerics)
    End Function

    <Extension>
    Public Sub WriteCDF(genome As GridSystem, cdf$, Optional names$() = Nothing)
        Dim attr As [Variant](Of attribute, attribute())
        Dim cor As Correlation

        If names.IsNullOrEmpty Then
            names = genome.A _
                .Sequence _
                .Select(Function(i) $"X{i + 1}") _
                .ToArray
        End If

        Using cdfWriter = New CDFWriter(cdf).Dimensions(
            Dimension.Double,
            Dimension.Integer,
            Dimension.Text(4096)
        )

            Call cdfWriter.GlobalAttributes(
                New attribute With {.name = "dataset", .type = CDFDataTypes.CHAR, .value = GetType(GridSystem).FullName},
                New attribute With {.name = "size", .type = CDFDataTypes.INT, .value = names.Length},
                New attribute With {.name = "create-time", .type = CDFDataTypes.CHAR, .value = Now.ToString},
                New attribute With {.name = "variables", .type = CDFDataTypes.CHAR, .value = names.GetJson},
                New attribute With {.name = "github", .type = CDFDataTypes.CHAR, .value = LICENSE.githubURL}
            )

            attr = New attribute With {
                .name = "const",
                .type = CDFDataTypes.DOUBLE,
                .value = genome.AC
            }

            Call cdfWriter.AddVariable("cor.factor", genome.A, doubleFullName, attr)

            For i As Integer = 0 To names.Length - 1
                cor = genome.C(i)
                attr = New attribute With {
                    .name = "const",
                    .type = CDFDataTypes.DOUBLE,
                    .value = cor.BC
                }

                Call cdfWriter.AddVariable(names(i), cor.B, doubleFullName, attr)
            Next
        End Using
    End Sub
End Module
