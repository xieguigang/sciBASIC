#Region "Microsoft.VisualBasic::d68023fd62338db0f12e0a895c6091c6, Microsoft.VisualBasic.Core\ComponentModel\DataSource\Repository\IMatched.vb"

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

    '     Interface IMatched
    ' 
    '         Properties: IsMatched
    ' 
    '     Interface IKeyIndex
    ' 
    '         Properties: Entity, Index
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection

Namespace ComponentModel.DataSourceModel.Repository

    ''' <summary>
    ''' The object implements on this interface can be matched with some rules.
    ''' </summary>
    Public Interface IMatched

        ''' <summary>
        ''' Is this object matched the condition?
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property IsMatched As Boolean
    End Interface

    Public Interface IKeyIndex(Of T)

        Property Entity As T
        ''' <summary>
        ''' 一般是一些短的字符串所构成的能够唯一标记该<see cref="Entity"/>对象的术语列表
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property Index As Index(Of String)

    End Interface
End Namespace
