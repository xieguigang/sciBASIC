Imports System.Reflection
Imports Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider.Reflection
Imports Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider.Reflection.Reflector

Namespace StorageProvider.ComponentModels

    ''' <summary>
    ''' 数据读写对象的基本类型
    ''' </summary>
    Public MustInherit Class StorageProvider

        ''' <summary>
        ''' 这个属性值在Csv文件的第几列？
        ''' </summary>
        ''' <returns></returns>
        Public Property Ordinal As Integer
        ''' <summary>
        ''' The bind property in the reflected class object.(在反射的类型定义之中所绑定的属性入口定义)
        ''' </summary>
        ''' <returns></returns>
        Public Property BindProperty As PropertyInfo

        ''' <summary>
        ''' 假若目标属性之中没有提供名称的话，则会使用属性名称来代替
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride ReadOnly Property Name As String

        ''' <summary>
        ''' 从目标类型对象之中可以读取这个属性的值将数据写入到文件之中
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property CanReadDataFromObject As Boolean
            Get
                Return BindProperty.CanRead
            End Get
        End Property

        ''' <summary>
        ''' 可以在读取Csv文件之中的数据之后将数据写入到这个属性之中从而将数据加载进入内存之中
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property CanWriteDataToObject As Boolean
            Get
                Return BindProperty.CanWrite
            End Get
        End Property

        Public MustOverride ReadOnly Property ProviderId As Reflection.ProviderIds

        Public ReadOnly Property LoadMethod As Func(Of String, Object)
        Public MustOverride Overloads Function ToString([object] As Object) As String

        Sub New(BindProperty As PropertyInfo)
            Call Me.New(BindProperty, BindProperty.PropertyType)
        End Sub

        Sub New(BindProperty As PropertyInfo, ElementType As Type)
            Me.BindProperty = BindProperty
            If Scripting.CasterString.ContainsKey(ElementType) Then
                Me.LoadMethod = Scripting.CasterString(ElementType)
            Else
                ' Meta 字典类型，则忽略掉
            End If
        End Sub

        Sub New(BindProperty As PropertyInfo, LoadMethod As Func(Of String, Object))
            Me.BindProperty = BindProperty
            Me.LoadMethod = LoadMethod
        End Sub

        Public Overrides Function ToString() As String
            Return $"[Dim {Name} As {BindProperty.PropertyType.FullName}] //{Me.GetType.Name}   --> {BindProperty.Name}"
        End Function
    End Class
End Namespace