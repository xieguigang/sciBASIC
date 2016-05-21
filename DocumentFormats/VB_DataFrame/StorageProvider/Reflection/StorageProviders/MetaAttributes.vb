Imports System.Reflection
Imports Microsoft.VisualBasic.DocumentFormat.Csv.DataImports
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider.ComponentModels
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace StorageProvider.Reflection

    Public Module MetaAttributeParser

        Public Function GetEntry(type As Type) As ComponentModels.MetaAttribute
            Dim attrEntry As Type = GetType(Reflection.MetaAttribute)
            Dim MetaAttr As ComponentModels.MetaAttribute =
                LinqAPI.DefaultFirst(Of ComponentModels.MetaAttribute) <=
                    From prop As PropertyInfo
                    In type.GetProperties(BindingFlags.Public Or BindingFlags.Instance)
                    Let attrs As Object() =
                        prop.GetCustomAttributes(attrEntry, inherit:=True)
                    Where Not attrs.IsNullOrEmpty
                    Let mattr As MetaAttribute = DirectCast(attrs.First, Reflection.MetaAttribute)
                    Select New ComponentModels.MetaAttribute(mattr, prop)
            Return MetaAttr
        End Function

        ''' <summary>
        ''' 将Csv文档里面的数据加载进入对象数组的字典属性之中
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="buf"></param>
        ''' <param name="DataSource"></param>
        ''' <param name="Schema"></param>
        ''' <returns></returns>
        Public Function LoadData(Of T As Class)(buf As IEnumerable(Of T), DataSource As DynamicObjectLoader(), Schema As SchemaProvider) As IEnumerable(Of T)
            If Not Schema.HasMetaAttributes Then
                Return buf
            End If

            Dim MetaEntry As ComponentModels.MetaAttribute = Schema.MetaAttributes
            Dim typeCast As Func(Of String, Object) =
                Scripting.CasterString(MetaEntry.MetaAttribute.TypeId)
            Dim attrs As KeyValuePair(Of String, Integer)() =
                LinqAPI.Exec(Of KeyValuePair(Of String, Integer)) <=
                    From Name As KeyValuePair(Of String, Integer)
                    In DataSource.First.Schema
                    Where Not String.IsNullOrEmpty(Name.Key) AndAlso
                        Not Schema.ContainsField(Name.Key)
                    Select Name
            Dim hashType As Type =
                MakeDictionaryType(MetaEntry.MetaAttribute.TypeId)
            Dim o As Object

            For Each x As SeqValue(Of T) In buf.SeqIterator
                Dim hash As IDictionary =
                    DirectCast(Activator.CreateInstance(hashType), IDictionary)
                Dim source As DynamicObjectLoader = DataSource(x.i)

                For Each attrName As KeyValuePair(Of String, Integer) In attrs
                    o = typeCast(source.GetValue(attrName.Value))
                    Call hash.Add(attrName.Key, o)
                Next

                Call MetaEntry.BindProperty.SetValue(x.obj, hash, Nothing)
            Next

            Return buf
        End Function

        ''' <summary>
        ''' Function returns type of <see cref="Dictionary(Of String, ValueType)"/>
        ''' </summary>
        ''' <param name="ValueType">Type of the value in the dictionary, and the key type is <see cref="String"/></param>
        ''' <returns></returns>
        Public Function MakeDictionaryType(ValueType As Type) As Type
            Dim GenericType As Type = GetType(Dictionary(Of ,)) ' Type.GetType("System.Collections.Generic.IEnumerable")
            GenericType = GenericType.MakeGenericType({GetType(String), ValueType})
            Return GenericType
        End Function
    End Module
End Namespace