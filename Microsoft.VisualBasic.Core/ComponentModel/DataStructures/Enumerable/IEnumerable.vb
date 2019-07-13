#Region "Microsoft.VisualBasic::3eb69c5ec28321fbdb068dfd45528a54, Microsoft.VisualBasic.Core\ComponentModel\DataStructures\Enumerable\IEnumerable.vb"

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

    '     Interface INamedValue
    ' 
    ' 
    ' 
    '     Interface IReadOnlyId
    ' 
    '         Properties: Identity
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository

Namespace ComponentModel.Collection.Generic

    ''' <summary>
    ''' This type of object have a <see cref="INamedValue.Key"></see> property to unique identified itself in a collection.
    ''' This interface was inherits from type <see cref="IKeyedEntity(Of String)"/>.
    ''' (一个具有自己的名称的变量值的抽象)
    ''' </summary>
    ''' <remarks></remarks>
    Public Interface INamedValue : Inherits IKeyedEntity(Of String)
    End Interface

    ''' <summary>
    ''' 与<see cref="iNamedValue"/>所不同的是，这个对象的标识属性是只读的.
    ''' </summary>
    Public Interface IReadOnlyId

        ''' <summary>
        ''' The unique identifer in the object collection. Unique-Id of the target implements object
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property Identity As String
    End Interface
End Namespace
