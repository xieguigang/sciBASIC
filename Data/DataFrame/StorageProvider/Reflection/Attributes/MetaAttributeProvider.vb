#Region "Microsoft.VisualBasic::3b6e151081b172a80977acff7a66a25f, Data\DataFrame\StorageProvider\Reflection\Attributes\MetaAttributeProvider.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 44
    '    Code Lines: 23
    ' Comment Lines: 14
    '   Blank Lines: 7
    '     File Size: 1.92 KB


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
