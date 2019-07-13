#Region "Microsoft.VisualBasic::067f9367f812385e656985211acdf33d, Data\DataFrame\IO\MetaData\GenericMeta.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Module GenericMeta
    ' 
    '         Function: GetMetaDataRows, TryParseMetaDataRows
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language

Namespace IO

    Public Module GenericMeta

        ''' <summary>
        ''' 通用的meta元数据的解析函数
        ''' </summary>
        ''' <param name="file$"></param>
        ''' <param name="prefix$"></param>
        ''' <returns></returns>
        Public Function GetMetaDataRows(file$, Optional prefix$ = "##") As String()
            Dim out As New List(Of String)

            For Each line$ In file.IterateAllLines
                If InStr(line, prefix, CompareMethod.Text) = 1 Then
                    out += line
                Else
                    ' 已经找不到起始字符串了，则不会是元数据了
                    Exit For
                End If
            Next

            Return out.ToArray
        End Function

        Public Function TryParseMetaDataRows(file$, Optional delimiter$ = "=", Optional prefix$ = "##") As Dictionary(Of String, String())
            Dim rows$() = GetMetaDataRows(file, prefix)
            Dim pl% = prefix.Length
            Dim tagsData As NamedValue(Of String)() = rows _
                .Select(Function(s) Mid(s, pl + 1)) _
                .Select(Function(s)
                            Return s.GetTagValue(delimiter, trim:=True)
                        End Function) _
                .ToArray

            Return tagsData.GroupBy(Function(o) o.Name) _
                .ToDictionary(Function(k) k.Key,
                              Function(s)
                                  Return s.Select(Function(v) v.Value).ToArray
                              End Function)
        End Function
    End Module
End Namespace
