#Region "Microsoft.VisualBasic::41574a2d9ca790083c7913f69845eb99, ..\sciBASIC#\Data\DataFrame\StorageProvider\ComponntModels\DynamicObjectLoader.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2016 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
' 
' This program is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' GNU General Public License for more details.
' 
' You should have received a copy of the GNU General Public License
' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region

Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Linq.Extensions

Namespace StorageProvider.ComponentModels

    ''' <summary>
    ''' Data structure for high perfermence data loading.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class DynamicObjectLoader : Inherits Dynamic.DynamicObject
        Implements IDataRecord
        Implements IEnumerable(Of KeyValuePair(Of String, String))

#If NET_40 = 0 Then
        Implements IReadOnlyDictionary(Of String, String)
#End If

        Public Property RowData As RowObject
        Public Property Schema As Dictionary(Of String, Integer)
        Public Property LineNumber As Long

        Protected Friend _innerDataFrame As DataFrame

        Public Sub New()
        End Sub

        Public Sub New(DataFrame As DataFrame)
            Schema = DataFrame.SchemaOridinal
        End Sub

        Sub New(row As RowObject, schema As Dictionary(Of String, Integer))
            Me.RowData = row
            Me.Schema = schema
        End Sub

        ''' <summary>
        ''' Get or set the string value in the specific attribute name of current line.
        ''' </summary>
        ''' <param name="columnName"></param>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Attribute(columnName As String) As String
            Get
                If Schema.ContainsKey(columnName) Then
                    Return RowData(Schema(columnName))
                Else
                    Return ""
                End If
            End Get
            Set(value As String)
                Call SetAttributeValue(columnName, value)
            End Set
        End Property

        Public Function Read(idx As IEnumerable(Of Integer)) As String()
            Return idx.ToArray(Function(x) RowData.Column(x))
        End Function

        Public Function SetAttributeValue(Name As String, Value As String) As Boolean
            If Schema.ContainsKey(Name) Then
                Dim Order As Integer = Schema(Name)
                RowData(Order) = Value
            Else
                Call Schema.Add(Name, Schema.Values.Max + 1)
                Call RowData.Add(Value)
                Call _innerDataFrame.AddAttribute(Name)
            End If

            Return True
        End Function

        Public Function GetOrdinal(Column As String) As Integer
            If Schema.ContainsKey(Column) Then
                Return Schema(Column)
            Else
                Return -1
            End If
        End Function

        Public Function GetOrdinal(Column As IEnumerable(Of String)) As Integer()
            Return Column.ToArray(Function(x) GetOrdinal(x))
        End Function

        Public Function GetValue(Ordinal As Integer) As String
            Return RowData.Column(Ordinal)
        End Function

        Public Function GetValues(ords As Integer()) As String()
            Return ords.ToArray(Function(n) RowData.Column(n))
        End Function

        Public Shared Function CreateSchema(columns As String()) As Dictionary(Of String, Integer)
            Dim LQuery = (From i As Integer In columns.Sequence Select i, Col = columns(i)).ToArray
            Return LQuery.ToDictionary(Function(item) item.Col, elementSelector:=Function(item) item.i)
        End Function

        Public Overrides Function ToString() As String
            Return RowData.ToString
        End Function

        ''' <summary>
        ''' 函数会尝试将目标对象的属性值按照名称进行赋值，前提是目标属性值的类型应该为基本的类型。假若类型转换不成功，则会返回空对象
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function [TryCast](Of T As Class)() As T
            Dim Properties As System.Reflection.PropertyInfo() =
                (From pInfo As System.Reflection.PropertyInfo
                 In GetType(T).GetProperties(System.Reflection.BindingFlags.Public)
                 Where Scripting.IsPrimitive(pInfo.PropertyType)
                 Select pInfo).ToArray
            Dim FilledObject As T = Activator.CreateInstance(Of T)()

            For Each [Property] As System.Reflection.PropertyInfo In Properties

                Dim value As String = ""
                Call __tryGetValue([Property].Name, value)

                Dim obj_Value As Object = Scripting.CTypeDynamic(value, [Property].PropertyType)
                Call [Property].SetValue(FilledObject, obj_Value, Nothing)
            Next

            Return FilledObject
        End Function

#Region ""
        Public Overrides Function GetDynamicMemberNames() As IEnumerable(Of String)
            Return Schema.Keys
        End Function

        ''' <summary>
        ''' Provides the implementation for operations that get member values. Classes derived
        ''' from the System.Dynamic.DynamicObject class can override this method to specify
        ''' dynamic behavior for operations such as getting a value for a property.
        ''' </summary>
        ''' <param name="binder">
        ''' Provides information about the object that called the dynamic operation. The
        ''' binder.Name property provides the name of the member on which the dynamic operation
        ''' is performed. For example, for the Console.WriteLine(sampleObject.SampleProperty)
        ''' statement, where sampleObject is an instance of the class derived from the System.Dynamic.DynamicObject
        ''' class, binder.Name returns "SampleProperty". The binder.IgnoreCase property specifies
        ''' whether the member name is case-sensitive.
        ''' </param>
        ''' <param name="result">The result of the get operation. For example, if the method is called for a property, you can assign the property value to result.</param>
        ''' <returns>
        ''' true if the operation is successful; otherwise, false. If this method returns
        ''' false, the run-time binder of the language determines the behavior. (In most
        ''' cases, a run-time exception is thrown.)
        ''' </returns>
        Public Overrides Function TryGetMember(binder As Dynamic.GetMemberBinder, ByRef result As Object) As Boolean
            Dim colName As String = binder.Name
            Dim value As String = ""
            Call __tryGetValue(colName, value)
            result = DirectCast(value, Object)
            Return True
        End Function

        ''' <summary>
        ''' Provides the implementation for operations that set member values. Classes derived
        ''' from the System.Dynamic.DynamicObject class can override this method to specify
        ''' dynamic behavior for operations such as setting a value for a property.
        ''' </summary>
        ''' <param name="binder">
        ''' Provides information about the object that called the dynamic operation. The
        ''' binder.Name property provides the name of the member to which the value is being
        ''' assigned. For example, for the statement sampleObject.SampleProperty = "Test",
        ''' where sampleObject is an instance of the class derived from the System.Dynamic.DynamicObject
        ''' class, binder.Name returns "SampleProperty". The binder.IgnoreCase property specifies
        ''' whether the member name is case-sensitive.
        ''' </param>
        ''' <param name="value">The value to set to the member. For example, for sampleObject.SampleProperty
        ''' = "Test", where sampleObject is an instance of the class derived from the System.Dynamic.DynamicObject
        ''' class, the value is "Test".</param>
        ''' <returns>
        ''' true if the operation is successful; otherwise, false. If this method returns
        ''' false, the run-time binder of the language determines the behavior. (In most
        ''' cases, a language-specific run-time exception is thrown.)
        ''' </returns>
        Public Overrides Function TrySetMember(binder As Dynamic.SetMemberBinder, value As Object) As Boolean
            Attribute(binder.Name) = If(value Is Nothing, "", value.ToString)
            Return True
        End Function
#End Region

        ''' <summary>
        ''' 将大小写敏感转换为大小写不敏感
        ''' </summary>
        ''' <param name="key"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function GetKey(key As String) As String
            Dim LQuery = (From item In Schema Where String.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase) Select value_key = item.Key).ToArray
            Return LQuery.FirstOrDefault
        End Function

        Private Function __tryGetValue(key As String, ByRef value As String) As Boolean
            key = GetKey(key)

            If String.IsNullOrEmpty(key) Then
                value = ""
                Return False      '不包含有这个键
            Else
                Dim p As Integer = Schema(key)
                value = RowData(p)
                Return True
            End If
        End Function

#Region "Implements IReadOnlyDictionary(Of String, String)"


        Public Iterator Function GetEnumerator() As IEnumerator(Of KeyValuePair(Of String, String)) Implements IEnumerable(Of KeyValuePair(Of String, String)).GetEnumerator
            For Each col In Me.Schema
                Yield New KeyValuePair(Of String, String)(col.Key, _RowData(Index:=col.Value))
            Next
        End Function

#If NET_40 = 0 Then

        Public ReadOnly Property Count As Integer Implements IReadOnlyCollection(Of KeyValuePair(Of String, String)).Count
            Get
                Return RowData.Count
            End Get
        End Property

        Public Function ContainsKey(key As String) As Boolean Implements IReadOnlyDictionary(Of String, String).ContainsKey
            Return Me.Schema.ContainsKey(key)
        End Function

        Default Public Overloads ReadOnly Property Value(key As String) As String Implements IReadOnlyDictionary(Of String, String).Item
            Get
                Return Me.Attribute(key)
            End Get
        End Property

        Public ReadOnly Property Keys As IEnumerable(Of String) Implements IReadOnlyDictionary(Of String, String).Keys
            Get
                Return Schema.Keys
            End Get
        End Property

        Public Function TryGetValue(key As String, ByRef value As String) As Boolean Implements IReadOnlyDictionary(Of String, String).TryGetValue
            Return __tryGetValue(key, value)
        End Function

        Public ReadOnly Property Values As IEnumerable(Of String) Implements IReadOnlyDictionary(Of String, String).Values
            Get
                Return RowData
            End Get
        End Property
#End If
#End Region

#Region "Implements System.Data.IDataRecord"

        ''' <summary>
        ''' Gets the number of columns in the current row.
        ''' </summary>
        ''' <returns>
        ''' When not positioned in a valid recordset, 0; otherwise, the number of columns in the current record. The default is -1.
        ''' </returns>
        Public ReadOnly Property FieldCount As Integer Implements IDataRecord.FieldCount
            Get
                Return Schema.Count
            End Get
        End Property

        Public ReadOnly Property DataRecordItem(i As Integer) As Object Implements IDataRecord.Item
            Get
                Return RowData.Column(i)
            End Get
        End Property

        Public Overloads ReadOnly Property Item(name As String) As Object Implements IDataRecord.Item
            Get
                Return DirectCast(Attribute(name), Object)
            End Get
        End Property

        Public Iterator Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function

        ''' <summary>
        ''' Gets the name for the field to find.
        ''' </summary>
        ''' <param name="i"></param>
        ''' <returns></returns>
        Public Function GetName(i As Integer) As String Implements IDataRecord.GetName
            Return Schema.ElementAt(i).Key
        End Function

        ''' <summary>
        ''' Gets the data type information for the specified field.
        ''' </summary>
        ''' <param name="i"></param>
        ''' <returns></returns>
        Public Function GetDataTypeName(i As Integer) As String Implements IDataRecord.GetDataTypeName
            Throw New NotImplementedException()
        End Function

        Public Function GetFieldType(i As Integer) As Type Implements IDataRecord.GetFieldType
            Throw New NotImplementedException()
        End Function

        Private Function IDataRecord_GetValue(i As Integer) As Object Implements IDataRecord.GetValue
            Throw New NotImplementedException()
        End Function

        Public Function GetValues(values() As Object) As Integer Implements IDataRecord.GetValues
            Throw New NotImplementedException()
        End Function

        Private Function IDataRecord_GetOrdinal(name As String) As Integer Implements IDataRecord.GetOrdinal
            Throw New NotImplementedException()
        End Function

        Public Function GetBoolean(i As Integer) As Boolean Implements IDataRecord.GetBoolean
            Throw New NotImplementedException()
        End Function

        Public Function GetByte(i As Integer) As Byte Implements IDataRecord.GetByte
            Throw New NotImplementedException()
        End Function

        Public Function GetBytes(i As Integer, fieldOffset As Long, buffer() As Byte, bufferoffset As Integer, length As Integer) As Long Implements IDataRecord.GetBytes
            Throw New NotImplementedException()
        End Function

        Public Function GetChar(i As Integer) As Char Implements IDataRecord.GetChar
            Throw New NotImplementedException()
        End Function

        Public Function GetChars(i As Integer, fieldoffset As Long, buffer() As Char, bufferoffset As Integer, length As Integer) As Long Implements IDataRecord.GetChars
            Throw New NotImplementedException()
        End Function

        Public Function GetGuid(i As Integer) As Guid Implements IDataRecord.GetGuid
            Throw New NotImplementedException()
        End Function

        Public Function GetInt16(i As Integer) As Short Implements IDataRecord.GetInt16
            Throw New NotImplementedException()
        End Function

        Public Function GetInt32(i As Integer) As Integer Implements IDataRecord.GetInt32
            Throw New NotImplementedException()
        End Function

        Public Function GetInt64(i As Integer) As Long Implements IDataRecord.GetInt64
            Throw New NotImplementedException()
        End Function

        Public Function GetFloat(i As Integer) As Single Implements IDataRecord.GetFloat
            Throw New NotImplementedException()
        End Function

        Public Function GetDouble(i As Integer) As Double Implements IDataRecord.GetDouble
            Throw New NotImplementedException()
        End Function

        Public Function GetString(i As Integer) As String Implements IDataRecord.GetString
            Throw New NotImplementedException()
        End Function

        Public Function GetDecimal(i As Integer) As Decimal Implements IDataRecord.GetDecimal
            Throw New NotImplementedException()
        End Function

        Public Function GetDateTime(i As Integer) As Date Implements IDataRecord.GetDateTime
            Throw New NotImplementedException()
        End Function

        Public Function GetData(i As Integer) As IDataReader Implements IDataRecord.GetData
            Return Nothing
        End Function

        Public Function IsDBNull(i As Integer) As Boolean Implements IDataRecord.IsDBNull
            Return String.IsNullOrEmpty(RowData.Column(i))
        End Function
#End Region
    End Class
End Namespace
