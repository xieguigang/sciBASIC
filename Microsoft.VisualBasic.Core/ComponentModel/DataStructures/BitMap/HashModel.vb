#Region "Microsoft.VisualBasic::6454ef3a138ca347bcd3778898de75ea, Microsoft.VisualBasic.Core\ComponentModel\DataStructures\BitMap\HashModel.vb"

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

    '     Interface IAddressOf
    ' 
    ' 
    ' 
    '     Interface IAddress
    ' 
    '         Properties: Address
    ' 
    '         Sub: Assign
    ' 
    '     Interface IHashHandle
    ' 
    ' 
    ' 
    '     Class IHashValue
    ' 
    '         Properties: Address, ID, obj
    ' 
    '         Sub: Assign
    ' 
    '     Module AddressedValueExtensions
    ' 
    '         Function: Vector
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Language

Namespace ComponentModel

    ''' <summary>
    ''' This object gets a object handle value to indicated that the position this object exists 
    ''' in the list collection structure. 
    ''' (这个对象具有一个用于指明该对象在列表对象中的位置的对象句柄值)
    ''' </summary>
    ''' <remarks></remarks>
    Public Interface IAddressOf : Inherits IAddress(Of Integer)
    End Interface

    ''' <summary>
    ''' This object gets a object handle value to indicated that the position this object exists 
    ''' in the list collection structure. 
    ''' (这个对象具有一个用于指明该对象在列表对象中的位置的对象句柄值)
    ''' </summary>
    ''' <remarks></remarks>
    Public Interface IAddress(Of T As IComparable)

        ''' <summary>
        ''' The ID that this object in a list instance.
        ''' (本对象在一个列表对象中的位置索引号) 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>因为索引号在赋值之后是不可以被修改了的，所以这个属性使用ReadOnly</remarks>
        ReadOnly Property Address As T

        Sub Assign(address As T)
    End Interface

    Public Interface IHashHandle : Inherits IAddressOf, INamedValue
    End Interface

    Public Class IHashValue(Of T As INamedValue)
        Implements IHashHandle

        Public Property obj As T
        Public Property Address As Integer Implements IAddressOf.Address
        Public Property ID As String Implements INamedValue.Key
            Get
                Return obj.Key
            End Get
            Set(value As String)
                obj.Key = value
            End Set
        End Property

        Public Sub Assign(address As Integer) Implements IAddress(Of Integer).Assign
            Me.Address = address
        End Sub
    End Class

    Public Module AddressedValueExtensions

        <Extension>
        Public Function Vector(Of T As IAddress(Of Integer), TOut)(source As IEnumerable(Of T), length As Integer, getValue As Func(Of T, TOut)) As TOut()
            Dim chunk As TOut() = New TOut(length - 1) {}

            For Each x As T In source
                chunk(x.Address) = getValue(x)
            Next

            Return chunk
        End Function
    End Module
End Namespace
