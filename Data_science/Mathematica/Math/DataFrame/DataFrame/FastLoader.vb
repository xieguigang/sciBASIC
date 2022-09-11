Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Text

Public Module FastLoader

    ''' <summary>
    ''' 在这里仅是针对简单格式的csv文件进行快速文件读取操作，对于包含有复杂格式字符串的csv文件，
    ''' 任然需要通过csv文件模块进行读取，之后再通过相应的API进行对象转换
    ''' </summary>
    ''' <param name="file"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' 简单格式的含义：
    ''' 
    ''' 1. csv文件之中无注释元数据信息
    ''' 2. 单元格的字符串之中无逗号，制表符等分隔符
    ''' </remarks>
    <Extension>
    Public Function ReadCsv(file As Stream,
                            Optional delimiter As Char = ","c,
                            Optional rowHeader As Boolean = True,
                            Optional encoding As Encodings = Encodings.UTF8) As DataFrame

        Dim read As New StreamReader(file, encoding.CodePage)
        Dim rowHeaders As New List(Of String)
        Dim features As New Dictionary(Of String, List(Of String))

        Return New DataFrame With {
            .features = features _
                .ToDictionary(Function(a) a.Key,
                              Function(a)
                                  Return a.Value.ParseFeature
                              End Function),
            .rownames = rowHeaders.ToArray
        }
    End Function

    <Extension>
    Private Function ParseFeature(data As List(Of String)) As FeatureVector

    End Function
End Module
