
Imports System.Reflection

Namespace ComponentModel.Settings

    ''' <summary>
    ''' 这个并不是指宿主属性的数据类型，而是指代这一数据类型所代表的具体的实际对象
    ''' </summary>
    Public Enum ValueTypes
        ''' <summary>
        ''' 这个字符串的值是一个文件夹
        ''' </summary>
        Directory
        ''' <summary>
        ''' 这个字符串的值是一个文件的路径
        ''' </summary>
        File
        ''' <summary>
        ''' 普通的文本字符串
        ''' </summary>
        Text
        ''' <summary>
        ''' 带有小数的数值
        ''' </summary>
        [Double]
        ''' <summary>
        ''' 整数
        ''' </summary>
        [Integer]
    End Enum

    ''' <summary>
    ''' The simple configuration mapping node in the current profile data, the data type of this node 
    ''' object should be just the simplest data type such as String, Integer, Long, Double, Boolean.
    ''' (当前的配置节点为一个简单节点，即目标属性的属性值类型的字符串，数字或者逻辑值等最基本的数据类型)
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Property, AllowMultiple:=False, Inherited:=True)>
    Public Class ProfileItem : Inherits Attribute
        Implements Microsoft.VisualBasic.ComponentModel.Collection.Generic.IKeyValuePairObject(Of String, String)
        Implements Microsoft.VisualBasic.ComponentModel.Collection.Generic.sIdEnumerable

        <Xml.Serialization.XmlAttribute> Public Property Name As String Implements Collection.Generic.IKeyValuePairObject(Of String, String).Identifier, Collection.Generic.sIdEnumerable.Identifier
        <Xml.Serialization.XmlAttribute> Public Property Description As String Implements Collection.Generic.IKeyValuePairObject(Of String, String).Value

        Friend _PropertyInfo As System.Reflection.PropertyInfo
        Friend _TargetData As Object

        ''' <summary>
        ''' 默认的数据类型是字符串类型
        ''' </summary>
        ''' <returns></returns>
        Public Property Type As ValueTypes = ValueTypes.Text

        Sub New()
        End Sub

        ''' <summary>
        ''' Initialize a node in the settings xml document.
        ''' </summary>
        ''' <param name="NodeName">The name of the node in the document xml file</param>
        ''' <param name="NodeDescription">The brief introduction information about this profile node.</param>
        Sub New(NodeName As String, Optional NodeDescription As String = "")
            Name = NodeName
            Description = NodeDescription
        End Sub

        Friend Function Initialize(PropertyInfo As PropertyInfo, TargetData As Object) As ProfileItem ',SettingMethod As System.Action(Of String), GetMethod As System.Func(Of Object)
            Me._TargetData = TargetData
            Me._PropertyInfo = PropertyInfo
            Return Me
        End Function

        Public ReadOnly Property Value As String
            Get
                Dim result = _PropertyInfo.GetValue(_TargetData) '_Value()
                If result Is Nothing OrElse String.IsNullOrEmpty(CStr(result)) Then
                    Return ""
                Else
                    Return result.ToString
                End If
            End Get
        End Property

        Public Sub [Set](value As String)
            Dim obj As Object =
                Scripting.CTypeDynamic(value, _PropertyInfo.PropertyType)
            Call _PropertyInfo.SetValue(_TargetData, obj)
        End Sub

        ''' <summary>
        ''' 打印在终端窗口上面的字符串
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property AsOutString As String
            Get
                Return String.Format("{0} = ""{1}""", Name, Value)
            End Get
        End Property

        Public Overrides Function ToString() As String
            If Not String.IsNullOrEmpty(Description) Then
                Return String.Format("{0}: {1}", Name, Description)
            Else
                Return Name
            End If
        End Function

        ''' <summary>
        ''' 这个方法只是针对<see cref="ValueTypes.File"/>和<see cref="ValueTypes.Directory"/>这两种类型而言才有效的
        ''' 对于其他的类型数据，都是返回False
        ''' </summary>
        ''' <returns></returns>
        Public Function IsFsysValid() As Boolean
            If Type = ValueTypes.Directory Then
                Return Value.DirectoryExists
            ElseIf Type = ValueTypes.File
                Return Value.FileExists
            Else
                Return False
            End If
        End Function
    End Class

    ''' <summary>
    ''' 当前的配置节点为一个复杂数据类型的配置节点，即目标属性的属性类型为一个Class对象
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Property, AllowMultiple:=False, Inherited:=True)>
    Public Class ProfileNodeItem : Inherits Attribute
    End Class
End Namespace