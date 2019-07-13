#Region "Microsoft.VisualBasic::a20517397c69a40615c8be491f50fc38, Data\SearchEngine\SearchEngine\Adapter\DictionaryKey.vb"

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

    ' Structure DictionaryKey
    ' 
    '     Properties: Key
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: GetValue, ToString
    ' 
    '     Sub: SetValue
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Public Structure DictionaryKey
    Implements IProperty

    Public ReadOnly Property Key As String Implements IReadOnlyId.Identity

    Sub New(name$)
        Key = name
    End Sub

    Public Sub SetValue(target As Object, value As Object) Implements IProperty.SetValue
        DirectCast(target, IDictionary)(Key) = value
    End Sub

    Public Function GetValue(target As Object) As Object Implements IProperty.GetValue
        Return DirectCast(target, IDictionary)(Key)
    End Function

    Public Overrides Function ToString() As String
        Return Key
    End Function
End Structure
