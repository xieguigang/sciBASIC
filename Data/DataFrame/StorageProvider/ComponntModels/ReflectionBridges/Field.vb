#Region "Microsoft.VisualBasic::eb39a5883d6e1da5e7923c666c41d69e, Data\DataFrame\StorageProvider\ComponntModels\ReflectionBridges\Field.vb"

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

    '     Class Column
    ' 
    '         Properties: Define, Name, ProviderId
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: CreateObject, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace StorageProvider.ComponentModels

    Public Class Column : Inherits StorageProvider

        ''' <summary>
        ''' The column attribute definition.
        ''' </summary>
        ''' <returns></returns>
        Public Property Define As ColumnAttribute

        Public Overrides ReadOnly Property Name As String
            Get
                Return Define.Name
            End Get
        End Property

        Public Overrides ReadOnly Property ProviderId As ProviderIds
            Get
                Return ProviderIds.Column
            End Get
        End Property

        Private Sub New(attr As ColumnAttribute, BindProperty As PropertyInfo)
            Call MyBase.New(BindProperty)
            Define = attr
            __toString = AddressOf Scripting.ToString
        End Sub

        ReadOnly __toString As Func(Of Object, String)

        ''' <summary>
        ''' With custom parser from the user code.
        ''' </summary>
        ''' <param name="attr"></param>
        ''' <param name="bindProperty"></param>
        ''' <param name="parser"></param>
        Private Sub New(attr As ColumnAttribute, bindProperty As PropertyInfo, parser As IParser)
            Call MyBase.New(bindProperty, AddressOf parser.TryParse)
            Define = attr
            __toString = AddressOf parser.ToString
        End Sub

        Public Shared Function CreateObject(attr As ColumnAttribute, BindProperty As PropertyInfo) As Column
            Dim parser As IParser = attr.GetParser

            If parser Is Nothing Then
                Return New Column(attr, BindProperty)
            Else
                Return New Column(attr, BindProperty, parser)
            End If
        End Function

        Public Overrides Function ToString([object] As Object) As String
            Return __toString([object])
        End Function
    End Class
End Namespace
