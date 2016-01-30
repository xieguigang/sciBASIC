Namespace ComponentModel.DataSourceModel

    ''' <summary>
    ''' Represents a column of certain data frames. The mapping between to schema is also can be represent by this attribute. 
    ''' (也可以使用这个对象来完成在两个数据源之间的属性的映射，由于对于一些列名称的属性值缺失的映射而言，
    ''' 其是使用属性名来作为列映射名称的，故而在修改这些没有预设的列名称的映射属性的属性名的时候，请注意
    ''' 要小心维护这种映射关系)
    ''' </summary>
    <AttributeUsage(AttributeTargets.[Property] Or AttributeTargets.Field, Inherited:=True, AllowMultiple:=False)>
    Public Class DataFrameColumnAttribute : Inherits Attribute

        Protected Shared ReadOnly __emptyIndex As String() = New String(-1) {}

        ''' <summary>
        ''' Gets the index.
        ''' </summary>
        Public ReadOnly Property Index() As Integer

        ''' <summary>
        ''' Gets the name.
        ''' </summary>
        Public ReadOnly Property Name() As String

        Public Property Description As String

        ''' <summary>
        ''' Initializes a new instance by name.
        ''' </summary>
        ''' <param name="FieldName">The name.</param>
        Public Sub New(FieldName As String)
            If String.IsNullOrEmpty(FieldName) Then
                Throw New ArgumentNullException("name")
            End If
            Me._Name = FieldName
            Me._Index = -1
        End Sub

        ''' <summary>
        ''' Initializes a new instance by index.
        ''' </summary>
        ''' <param name="index">The index.</param>
        Public Sub New(index As Integer)
            If index < 0 Then
                Throw New ArgumentOutOfRangeException("index")
            End If
            Me._Name = Nothing
            Me._Index = index
        End Sub

        ''' <summary>
        ''' 会默认使用目标对象的反射的Name属性作为映射的名称
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            _Index = -1
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Name">列名称，假若本参数为空的话，则使用属性名称</param>
        ''' <param name="index">从1开始的下标，表示为第几列</param>
        ''' <remarks></remarks>
        Sub New(Optional Name As String = "", Optional index As Integer = -1)
            _Name = Name
            _Index = index
        End Sub

        Public Overrides Function ToString() As String
            Return _Name
        End Function

        Public Function SetNameValue(value As String) As DataFrameColumnAttribute
            If Me._Index < 0 AndAlso String.IsNullOrEmpty(value) Then
                Throw New ArgumentNullException("value", "Name must not be null when Index is not defined.")
            End If
            Me._Name = value
            Return Me
        End Function

        Public Function GetIndex(names As String()) As Integer
            Return If(Index >= 0, Index, Array.IndexOf(If(names, __emptyIndex), Name))
        End Function

        ''' <summary>
        ''' 没有名称属性的映射使用属性名来表述
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function LoadMapping(Of T As Class)() As Dictionary(Of DataFrameColumnAttribute, System.Reflection.PropertyInfo)
            Return LoadMapping(GetType(T))
        End Function

        ''' <summary>
        ''' Load the mapping property, if the custom attribute <see cref="DataFrameColumnAttribute"></see> 
        ''' have no name value, then the property name will be used as the mapping name.
        ''' </summary>
        ''' <param name="TypeInfo">The type should be a class type or its properties should have the 
        ''' mapping option which was created by the custom attribute <see cref="DataFrameColumnAttribute"></see>
        ''' </param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function LoadMapping(TypeInfo As System.Type) As Dictionary(Of DataFrameColumnAttribute, System.Reflection.PropertyInfo)
            Dim Properties = (From pInfo As System.Reflection.PropertyInfo
                              In TypeInfo.GetProperties()
                              Let attrs As Object() = pInfo.GetCustomAttributes(GetType(DataFrameColumnAttribute), True)
                              Where Not attrs.IsNullOrEmpty
                              Select pInfo, Mapping = DirectCast(attrs.First, DataFrameColumnAttribute)).ToArray
            Dim LQuery = (From pInfo In Properties
                          Let Mapping = If(String.IsNullOrEmpty(pInfo.Mapping.Name), pInfo.Mapping.SetNameValue(pInfo.pInfo.Name), pInfo.Mapping)
                          Select Mapping, pInfo.pInfo).ToArray  ' 补全名称属性
            Dim DictData = LQuery.ToDictionary(Function(obj) obj.Mapping, elementSelector:=Function(obj) obj.pInfo)
            Return DictData
        End Function
    End Class

    Public MustInherit Class DataFrameIO(Of TAttributeType As DataFrameColumnAttribute)

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected MustOverride Function InitializeSchema(Of TEntityType As Class)() As TAttributeType()
    End Class

    Public MustInherit Class DynamicPropertyBase(Of T)

        Public Overridable Property Properties As Dictionary(Of String, T)
            Get
                If _propHash Is Nothing Then
                    _propHash = New Dictionary(Of String, T)
                End If
                Return _propHash
            End Get
            Set(value As Dictionary(Of String, T))
                _propHash = value
            End Set
        End Property

        Dim _propHash As Dictionary(Of String, T)

        Public Overrides Function ToString() As String
            Return $"{Properties.Count} Property(s)."
        End Function
    End Class
End Namespace
