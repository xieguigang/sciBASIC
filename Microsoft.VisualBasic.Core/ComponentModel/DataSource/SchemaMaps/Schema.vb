#Region "Microsoft.VisualBasic::0723d0ed2e2c569bb018511a68c9a99d, Microsoft.VisualBasic.Core\ComponentModel\DataSource\SchemaMaps\Schema.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class Schema
    ' 
    '         Properties: Fields, SchemaName
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ComponentModel.DataSourceModel.SchemaMaps

    ''' <summary>
    ''' Schema for two dimension table.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class Schema(Of T As Attribute)

        Public Property SchemaName As String
        Public Property Fields As BindProperty(Of T)()

        Sub New()
        End Sub

        Sub New(type As Type, Optional explict As Boolean = False)
            Fields = type.GetFields(Of T)(Function(o) o.ToString, explict)
            SchemaName = type.Name
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{SchemaName}: {Fields.Keys.GetJson}]"
        End Function
    End Class
End Namespace
