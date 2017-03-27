Imports System.Reflection
Imports System.Text
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text
Imports RowTokens = System.Collections.Generic.IEnumerable(Of System.String)

Namespace ComponentModel.DataSourceModel

    ''' <summary>
    ''' 将文件读取出来然后对每一行数据进行分割，由于没有使用自定义属性来标记列的名称，所以这个很简单的tsv加载器要求属性的名称与列名称要完全一致。
    ''' 而且，还不能够为非初始数据类型，这个模块之中提供了简单的数据类型转换操作，这个只是一个简单的内置TSv文件读取模块
    ''' </summary>
    ''' <remarks></remarks>
    Public Module TsvFileIO

        ''' <summary>
        ''' 自动将tsv文件数据之中的行解析反序列化加载为一个Class对象
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="Path"></param>
        ''' <returns></returns>
        Public Iterator Function Load(Of T As Class)(path$, Optional encoding As Encodings = Encodings.UTF8) As IEnumerable(Of T)
            Dim data = TsvFileIO.LoadFile(path, encoding.CodePage)
            Dim header$() = data.First.ToArray
            Dim schemaOrdinals As New Dictionary(Of String, Integer)

            For i As Integer = 0 To header.Length - 1
                Call schemaOrdinals.Add(header(i), i)
            Next

            Dim tableSchema = DataFramework.Schema(Of T)(PropertyAccess.ReadWrite, True)
            Dim type As Type = GetType(T)

            For Each line As String() In data.Skip(1).Select(Function(r) DirectCast(r, String()))
                Dim o As Object = Activator.CreateInstance(type)

                For Each field As KeyValuePair(Of String, PropertyInfo) In tableSchema
                    Dim index As Integer = schemaOrdinals(field.Key)
                    Dim s$ = line(index)
                    Dim value As Object = Scripting.CTypeDynamic(s, field.Value.PropertyType)

                    Call field.Value.SetValue(o, value)
                Next

                Yield DirectCast(o, T)
            Next
        End Function

        ''' <summary>
        ''' 读取文件并且按照TAb进行分割
        ''' </summary>
        ''' <param name="path"></param>
        ''' <returns></returns>
        Private Function LoadFile(path As String, encoding As Encoding) As RowTokens()
            Dim lines As String() = ReadAllLines(path, encoding)
            Dim LQuery = LinqAPI.Exec(Of RowTokens) <=
                From strLine As String
                In lines
                Let t As String() = Strings.Split(strLine, vbTab) ' 跳过标题行
                Select DirectCast(t, RowTokens)

            Return LQuery
        End Function
    End Module
End Namespace