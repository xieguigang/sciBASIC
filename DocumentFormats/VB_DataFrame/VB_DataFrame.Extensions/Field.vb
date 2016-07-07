Imports System.IO
Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.DocumentFormat.Csv.DocumentStream
Imports Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider
Imports Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider.ComponentModels
Imports Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider.Reflection
Imports Microsoft.VisualBasic.Language

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

    Public Function GetValue(x As Object) As Object
        Return Binding.BindProperty.GetValue(x, Nothing)
    End Function

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

    Public Property Stack As String

    Friend __writer As Writer

    Public Shared Function GetSchema(type As Type, Optional stack As String = "#") As [Class]
        Dim props As PropertyInfo() =
            type.GetProperties(BindingFlags.Public + BindingFlags.Instance)
        Dim fields As New List(Of Field)

        For Each prop As PropertyInfo In props
            Dim sp = TypeSchemaProvider.GetInterfaces(prop, False, False)
            Dim cls As [Class] = Nothing

            If sp Is Nothing Then  ' 复杂类型，需要建立外部文件的连接
                Dim pType As Type = prop.PropertyType
                cls = GetSchema(pType, stack & "::" & prop.Name)
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
            .Type = type,
            .Stack = stack
        }
    End Function
End Class


Public Class Writer
    Implements IDisposable

    ReadOnly __file As StreamWriter
    ReadOnly __class As [Class]

    Sub New(cls As [Class], DIR As String, encoding As Encodings)
        Dim path As String = DIR & $"/{cls.Stack.Replace("::", "/")}.Csv"
        Call path.ParentPath.MkDIR
        Dim fs As New FileStream(path, FileMode.OpenOrCreate)

        __class = cls
        __file = New StreamWriter(fs, encoding.GetEncodings)

        row += "#"

        For Each field As Field In cls.Fields
            If Not field.InnerClass Is Nothing Then
                field.InnerClass.__writer =
                    New Writer(field.InnerClass, DIR, encoding)
            End If

            Call row.Add(field.Name)
        Next

        Call __file.WriteLine(New RowObject(row).AsLine)
    End Sub

    ReadOnly row As New List(Of String)

    Public Sub WriteRow(obj As Object, i As String)
        Call row.Clear()
        Call row.Add(i)

        For Each field As Field In __class.Fields
            Dim x As Object = field.GetValue(obj)

            If field.InnerClass Is Nothing Then  ' 对于简单属性，直接生成字符串
                Call row.Add(field.Binding.ToString(x))
            Else
                Call field.InnerClass.__writer.WriteRow(x, i)
            End If
        Next

        Call __file.WriteLine(New RowObject(row).AsLine)
    End Sub

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
                Call __file.Flush()
                Call __file.Close()
                Call __file.Dispose()

                For Each field In __class.Fields
                    If Not field.InnerClass Is Nothing Then
                        Call field.InnerClass.__writer.Dispose()
                    End If
                Next
            End If

            ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
            ' TODO: set large fields to null.
        End If
        disposedValue = True
    End Sub

    ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
    'Protected Overrides Sub Finalize()
    '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        Dispose(True)
        ' TODO: uncomment the following line if Finalize() is overridden above.
        ' GC.SuppressFinalize(Me)
    End Sub
#End Region
End Class