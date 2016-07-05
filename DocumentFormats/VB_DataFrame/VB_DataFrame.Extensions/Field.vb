Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider
Imports Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider.ComponentModels
Imports Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider.Reflection

''' <summary>
''' + ``#`` uid;
''' + ``[FiledName]`` This field links to a external file, and id is point to the ``#`` uid field in the external file.
''' </summary>
Public Class Field

    ''' <summary>
    ''' Field Name
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Name As String
        Get
            Return Binding.Name
        End Get
    End Property

    ''' <summary>
    ''' 首先DirectCast为<see cref="IAttributeComponent"/>类型
    ''' </summary>
    ''' <returns></returns>
    Public Property Binding As ComponentModels.StorageProvider
    ''' <summary>
    ''' 假若这个为Nothing，则说明当前的域是简单类型
    ''' </summary>
    ''' <returns></returns>
    Public Property InnerClass As [Class]

    Public Overrides Function ToString() As String
        Return Binding.ToString
    End Function
End Class

Public Class [Class]

    Public Property Fields As Field()
    Public Property Type As Type

    Public Overrides Function ToString() As String
        Return "Public Class " & Type.FullName
    End Function

    Public Shared Function GetSchema(Of T)() As [Class]
        Return GetSchema(GetType(T))
    End Function

    Public Shared Function GetSchema(type As Type) As [Class]
        Dim props As PropertyInfo() =
            type.GetProperties(BindingFlags.Public + BindingFlags.Instance)
        Dim fields As New List(Of Field)

        For Each prop As PropertyInfo In props
            Dim sp = TypeSchemaProvider.GetInterfaces(prop, False, False)
            Dim cls As [Class] = Nothing

            If sp Is Nothing Then  ' 复杂类型，需要建立外部文件的连接
                Dim pType As Type = prop.PropertyType
                cls = GetSchema(pType)
                sp = New Column(New ColumnAttribute(prop.Name), prop)
            Else
                ' 简单类型，不需要再做额外域的处理
            End If

            fields += New Field With {
                .Binding = sp,
                .InnerClass = cls
            }
        Next

        Return New [Class] With {
            .Fields = fields,
            .Type = type
        }
    End Function
End Class
