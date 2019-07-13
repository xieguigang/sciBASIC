#Region "Microsoft.VisualBasic::9323d6cc7d0a0aa572c751ef79fd2eab, Microsoft.VisualBasic.Core\ComponentModel\DataSource\Property\IProperty.vb"

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

    '     Interface IProperty
    ' 
    '         Function: GetValue
    ' 
    '         Sub: SetValue
    ' 
    '     Interface IDynamicMeta
    ' 
    '         Properties: Properties
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

Namespace ComponentModel.DataSourceModel

    Public Interface IProperty : Inherits IReadOnlyId

        ''' <summary>
        ''' Gets property value from <paramref name="target"/> object.
        ''' </summary>
        ''' <param name="target"></param>
        ''' <returns></returns>
        Function GetValue(target As Object) As Object

        ''' <summary>
        ''' Set <paramref name="value"/> to the property of <paramref name="target"/> object.
        ''' </summary>
        ''' <param name="target"></param>
        ''' <param name="value"></param>
        Sub SetValue(target As Object, value As Object)
    End Interface

    ''' <summary>
    ''' Abstracts for the dynamics property.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Interface IDynamicMeta(Of T)

        ''' <summary>
        ''' Properties
        ''' </summary>
        ''' <returns></returns>
        Property Properties As Dictionary(Of String, T)
    End Interface
End Namespace
