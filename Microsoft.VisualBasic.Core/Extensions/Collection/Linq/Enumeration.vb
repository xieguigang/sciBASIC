#Region "Microsoft.VisualBasic::c3aab7de20209777ad612db67e2ee059, Microsoft.VisualBasic.Core\Extensions\Collection\Linq\Enumeration.vb"

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

    '     Interface Enumeration
    ' 
    '         Function: GenericEnumerator, GetEnumerator
    ' 
    '     Module EnumerationExtensions
    ' 
    '         Function: AsEnumerable
    '         Class Enumerator
    ' 
    '             Function: GetEnumerator, IEnumerable_GetEnumerator
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Linq

    ''' <summary>
    ''' Exposes the enumerator, which supports a simple iteration over a collection of
    ''' a specified type.To browse the .NET Framework source code for this type, see
    ''' the Reference Source.
    ''' (使用这个的原因是系统自带的<see cref="IEnumerable(Of T)"/>在Xml序列化之中的支持不太友好，
    ''' 实现这个接口之后可以通过<see cref="EnumerationExtensions.AsEnumerable(Of T)(Enumeration(Of T))"/>
    ''' 拓展来转换为查询操作的数据源)
    ''' </summary>
    ''' <typeparam name="T">The type of objects to enumerate.This type parameter is covariant. That is, you
    ''' can use either the type you specified or any type that is more derived. For more
    ''' information about covariance and contravariance, see Covariance and Contravariance
    ''' in Generics.</typeparam>
    Public Interface Enumeration(Of T)

        ''' <summary>
        ''' Returns an enumerator that iterates through the collection.
        ''' </summary>
        ''' <returns>An enumerator that can be used to iterate through the collection.</returns>
        Function GenericEnumerator() As IEnumerator(Of T)

        ''' <summary>
        ''' Returns an enumerator that iterates through a collection.
        ''' </summary>
        ''' <returns>An System.Collections.IEnumerator object that can be used to iterate through
        ''' the collection.</returns>
        Function GetEnumerator() As IEnumerator
    End Interface

    Public Module EnumerationExtensions

        Private Class Enumerator(Of T) : Implements IEnumerable(Of T)

            Public Enumeration As Enumeration(Of T)

            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Public Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
                Return Enumeration.GenericEnumerator
            End Function

            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
                Return Enumeration.GetEnumerator
            End Function
        End Class

        ''' <summary>
        ''' Returns the input typed as <see cref="IEnumerable(Of T)"/>.
        ''' </summary>
        ''' <typeparam name="T">The type of the elements of source.</typeparam>
        ''' <param name="enums">The sequence to type as <see cref="IEnumerable(Of T)"/></param>
        ''' <returns>The input sequence typed as <see cref="IEnumerable(Of T)"/>.</returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function AsEnumerable(Of T)(enums As Enumeration(Of T)) As IEnumerable(Of T)
            Return New Enumerator(Of T) With {.Enumeration = enums}
        End Function
    End Module
End Namespace
