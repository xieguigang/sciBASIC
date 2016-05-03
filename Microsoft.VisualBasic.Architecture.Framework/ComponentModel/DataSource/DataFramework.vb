Imports System.Reflection
Imports Microsoft.VisualBasic.Serialization.ConfigurationMappings

Namespace ComponentModel.DataSourceModel

    Public Interface IObjectModel_Driver
        Function Run() As Integer
    End Interface

    ''' <summary>
    ''' 在目标对象中必须要具有一个属性有自定义属性<see cref="DataFrameColumnAttribute"></see>
    ''' </summary>
    ''' <remarks></remarks>
    Public Module DataFramework

        Public Enum PropertyAccessibilityControls
            ReadWrite
            Readable
            Writeable
        End Enum

        'Public Function LoadSchema(Of T As Class)(AccessibilityControl As PropertyAccessibilityControls)
        '    Dim PropertyCollection = (From [property] As System.Reflection.PropertyInfo
        '                              In GetType(T).GetProperties(Reflection.BindingFlags.Public Or Reflection.BindingFlags.Instance)
        '                              Where BasicTypes.ContainsKey([property].PropertyType) OrElse IsBasicTypeEnumerator([property].PropertyType)
        '                              Select [property]).ToArray    '仅解析出简单类型的属性值

        'End Function

        'Public Function IsBasicTypeEnumerator(TypeInfo As Type) As Boolean
        '    If TypeInfo.IsArray Then
        '        Return BasicTypes.ContainsKey(TypeInfo.GetElementType)
        '    End If

        '    If Not TypeInfo.IsGenericType Then
        '        Return False
        '    End If


        'End Function

#If NET_40 = 0 Then

        ''' <summary>
        ''' 将字符串数据类型转换为其他的数据类型
        ''' </summary>
        ''' <remarks></remarks>
        Public ReadOnly Property BasicTypesLoading As Dictionary(Of System.Type, __StringTypeCaster) =
            New Dictionary(Of Type, __StringTypeCaster) From {
 _
                {GetType(String), Function(strValue As String) strValue},
                {GetType(Boolean), AddressOf getBoolean},
                {GetType(DateTime), Function(strValue As String) CType(strValue, DateTime)},
                {GetType(Double), AddressOf Val},
                {GetType(Integer), Function(strValue As String) CInt(strValue)},
                {GetType(Long), Function(strValue As String) CLng(strValue)},
                {GetType(Single), Function(s) CSng(Val(s))},
                {GetType(Char), Function(s) s.FirstOrDefault}
        }

        Public ReadOnly Property BasicTypesFlushs As Dictionary(Of Type, __LDMStringTypeCastHandler) =
            New Dictionary(Of Type, __LDMStringTypeCastHandler) From {
 _
                {GetType(String), AddressOf DataFramework.__toStringInternal},
                {GetType(Boolean), AddressOf DataFramework.__toStringInternal},
                {GetType(DateTime), AddressOf DataFramework.__toStringInternal},
                {GetType(Double), AddressOf DataFramework.__toStringInternal},
                {GetType(Integer), AddressOf DataFramework.__toStringInternal},
                {GetType(Long), AddressOf DataFramework.__toStringInternal},
                {GetType(Byte), AddressOf DataFramework.__toStringInternal},
                {GetType(ULong), AddressOf DataFramework.__toStringInternal},
                {GetType(UInteger), AddressOf DataFramework.__toStringInternal},
                {GetType(Short), AddressOf DataFramework.__toStringInternal},
                {GetType(UShort), AddressOf DataFramework.__toStringInternal},
                {GetType(Char), AddressOf DataFramework.__toStringInternal},
                {GetType(Single), AddressOf DataFramework.__toStringInternal},
                {GetType(SByte), AddressOf DataFramework.__toStringInternal}
        }

        Public Delegate Function CTypeDynamics(obj As Object, ConvertType As Type) As Object
#End If

        ''' <summary>
        ''' 出现错误的时候总是会返回空字符串的
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        Friend Function __toStringInternal(obj As Object) As String
            If obj Is Nothing Then
                Return ""
            Else
                Try
                    Return obj.ToString
                Catch ex As Exception
                    Call App.LogException(ex)
                    Return ""
                End Try
            End If
        End Function

        ''' <summary>
        ''' Convert target data object collection into a datatable for the data source of the <see cref="System.Windows.Forms.DataGridView"></see>>.
        ''' (将目标对象集合转换为一个数据表对象，用作DataGridView控件的数据源)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="DataCollection"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function CreateObject(Of T)(DataCollection As IEnumerable(Of T)) As DataTable
            Dim Columns = __initSchema(GetType(T))
            Dim DataTable As DataTable = New DataTable
            For Each column In Columns
                Call DataTable.Columns.Add(column.Key.Name, column.Value.PropertyType)
            Next

            For Each row In DataCollection
                Dim LQuery As Object() = (From column In Columns Select column.Value.GetValue(row, Nothing)).ToArray
                Call DataTable.Rows.Add(LQuery)
            Next

            Return DataTable
        End Function

        ''' <summary>
        ''' Retrive data from a specific datatable object.(从目标数据表中获取数据)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="DataTable"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetValue(Of T)(DataTable As DataTable) As T()
            Dim Columns = __initSchema(GetType(T))
            Dim rtvlData As T() = New T(DataTable.Rows.Count - 1) {}
            Dim i As Integer = 0

            Dim Schema As List(Of KeyValuePair(Of Integer, PropertyInfo)) =
                New List(Of KeyValuePair(Of Integer, PropertyInfo))
            For Each column As DataColumn In DataTable.Columns
                Dim LQuery = (From schemaColumn In Columns
                              Where String.Equals(schemaColumn.Key.Name, column.ColumnName)
                              Select schemaColumn.Value).FirstOrDefault
                If Not LQuery Is Nothing Then
                    Call Schema.Add(New KeyValuePair(Of Integer, PropertyInfo)(column.Ordinal, LQuery))
                End If
            Next

            For Each row As DataRow In DataTable.Rows
                Dim obj As T = Activator.CreateInstance(Of T)()
                For Each column In Schema
                    Dim value = row.Item(column.Key)
                    If IsDBNull(value) OrElse value Is Nothing Then
                        Continue For
                    End If
                    Call column.Value.SetValue(obj, value, Nothing)
                Next

                rtvlData(i) = obj
                i += 1
            Next
            Return rtvlData
        End Function

        Private Function __initSchema(type As Type) As Dictionary(Of DataFrameColumnAttribute, PropertyInfo)
            Dim DataColumnType As Type = GetType(DataFrameColumnAttribute)
            Dim props As PropertyInfo() = type.GetProperties
            Dim Columns = (From [property] As PropertyInfo
                           In props
                           Let attrs As Object() = [property].GetCustomAttributes(DataColumnType, True)
                           Where Not attrs.IsNullOrEmpty
                           Select colMaps =
                               DirectCast(attrs.First, DataFrameColumnAttribute), [property]
                           Order By colMaps.Index Ascending).ToList

            For Each column In Columns
                If String.IsNullOrEmpty(column.colMaps.Name) Then
                    Call column.colMaps.SetNameValue(column.property.Name)
                End If
            Next

            Dim UnIndexColumn = (From col
                                 In Columns
                                 Where col.colMaps.Index <= 0
                                 Select col  ' 未建立索引的对象按照名称排序
                                 Order By col.colMaps.Name Ascending).ToArray ' 由于在后面会涉及到修改list对象，所以在这里使用ToArray来隔绝域list的关系，避免出现冲突

            For Each col In UnIndexColumn
                Call Columns.Remove(col)
                Call Columns.Add(col) '将未建立索引的对象放置到列表的最末尾
            Next

            Return Columns.ToDictionary(Function(x) x.colMaps, Function(x) x.property)
        End Function
    End Module
End Namespace