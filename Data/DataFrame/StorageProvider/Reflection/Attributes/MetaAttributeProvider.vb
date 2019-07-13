#Region "Microsoft.VisualBasic::3b6e151081b172a80977acff7a66a25f, Data\DataFrame\StorageProvider\Reflection\Attributes\MetaAttributeProvider.vb"

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

    '     Class MetaAttribute
    ' 
    '         Properties: ProviderId, TypeId
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language.Default

Namespace StorageProvider.Reflection

    ''' <summary>
    ''' 在执行解析操作的时候，所有的没有被序列化的属性都将会被看作为字典元素，该字典元素的数据都存储在这个属性值之中
    ''' </summary>
    <AttributeUsage(AttributeTargets.Property, AllowMultiple:=False, Inherited:=False)>
    Public Class MetaAttribute : Inherits Attribute
        Implements IAttributeComponent

        ''' <summary>
        ''' The value type of the value slot in the meta attribute dictionary.(被序列化的对象之中的元数据的字典的值的类型)
        ''' </summary>
        ''' <returns></returns>
        Public Overloads ReadOnly Property TypeId As Type

        Public ReadOnly Property ProviderId As ProviderIds Implements IAttributeComponent.ProviderId
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return ProviderIds.MetaAttribute
            End Get
        End Property

        Shared ReadOnly stringType As [Default](Of  Type) = GetType(String)

        ''' <summary>
        ''' 在执行解析操作的时候，所有的没有被序列化的属性都将会被看作为字典元素，该字典元素的数据都存储在这个属性值之中
        ''' </summary>
        ''' <param name="type">
        ''' The value type of the value slot in the meta attribute dictionary.
        ''' (被序列化的对象之中的元数据的字典的值的类型)
        ''' </param>
        Sub New(Optional type As Type = Nothing)
            TypeId = type Or stringType
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return TypeId.FullName
        End Function
    End Class
End Namespace
