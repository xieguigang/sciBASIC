Imports System.Reflection
Imports Microsoft.VisualBasic.DocumentFormat.Csv.DataImports
Imports System.Runtime.CompilerServices


Namespace StorageProvider.Reflection

    Public Module MetaAttributeParser

        Public Function GetEntry(TypeInfo As Type) As Csv.StorageProvider.ComponentModels.MetaAttribute
            Dim attrEntry As Type = GetType(Reflection.MetaAttribute)
            Dim MetaAttr As Csv.StorageProvider.ComponentModels.MetaAttribute =
                (From [PropertyInfo] As System.Reflection.PropertyInfo
                 In TypeInfo.GetProperties(BindingFlags.Public Or BindingFlags.Instance)
                 Let attrs As Object() = PropertyInfo.GetCustomAttributes(attrEntry, inherit:=True)
                 Where Not attrs.IsNullOrEmpty
                 Let MetaAttrEntry = DirectCast(attrs.First, Reflection.MetaAttribute)
                 Select New Csv.StorageProvider.ComponentModels.MetaAttribute(MetaAttrEntry, PropertyInfo)).FirstOrDefault
            Return MetaAttr
        End Function

        ''' <summary>
        ''' 将Csv文档里面的数据加载进入对象数组的字典属性之中
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="ChunkBuffer"></param>
        ''' <param name="DataSource"></param>
        ''' <param name="Schema"></param>
        ''' <returns></returns>
        Public Function LoadData(Of T As Class)(ChunkBuffer As Generic.IEnumerable(Of T),
                                                DataSource As Csv.StorageProvider.ComponentModels.DynamicObjectLoader(),
                                                Schema As Csv.StorageProvider.ComponentModels.SchemaProvider) As Generic.IEnumerable(Of T)
            If Not Schema.HasMetaAttributes Then
                Return ChunkBuffer
            End If

            Dim MetaEntry As Csv.StorageProvider.ComponentModels.MetaAttribute = Schema.MetaAttributes
            Dim TypeCastMethod = Scripting.CasterString(MetaEntry.MetaAttribute.TypeId)
            Dim attrs = (From Name In DataSource.First.Schema Where Not String.IsNullOrEmpty(Name.Key) AndAlso Not Schema.ContainsField(Name.Key) Select Name).ToArray
            Dim hashType As Type = MakeDictionaryType(MetaEntry.MetaAttribute.TypeId)

            For i As Integer = 0 To ChunkBuffer.Count - 1
                Dim hash = DirectCast(Activator.CreateInstance(hashType), IDictionary)
                Dim source = DataSource(i)

                For Each attrName In attrs
                    Call hash.Add(attrName.Key, TypeCastMethod(source.GetValue(attrName.Value)))
                Next

                Call MetaEntry.BindProperty.SetValue(ChunkBuffer(i), hash)
            Next

            Return ChunkBuffer
        End Function

        Public Function MakeDictionaryType(ValueType As Type) As Type
            Dim GenericType As Type = GetType(Generic.Dictionary(Of ,)) 'Type.GetType("System.Collections.Generic.IEnumerable")
            GenericType = GenericType.MakeGenericType({GetType(String), ValueType})
            Return GenericType
        End Function
    End Module
End Namespace