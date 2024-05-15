#Region "Microsoft.VisualBasic::c1ba12dcf5aca3e001a103fa66263760, Microsoft.VisualBasic.Core\src\ComponentModel\DataSource\Property\GenericProperty.vb"

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

    '   Total Lines: 54
    '    Code Lines: 33
    ' Comment Lines: 13
    '   Blank Lines: 8
    '     File Size: 1.83 KB


    '     Class [Property]
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Operators: +
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace ComponentModel.DataSourceModel

    ''' <summary>
    ''' Dictionary for [<see cref="String"/>, <typeparamref name="T"/>]
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class [Property](Of T) : Inherits DynamicPropertyBase(Of T)

        Sub New()
        End Sub

        ''' <summary>
        ''' New with a init property value
        ''' </summary>
        ''' <param name="initKey"></param>
        ''' <param name="initValue"></param>
        Sub New(initKey$, initValue As T)
            Call Properties.Add(initKey, initValue)
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns></returns>
        Public Iterator Property src As IEnumerable(Of NamedValue(Of T))
            Get
                For Each x In Properties
                    Yield New NamedValue(Of T) With {
                        .Name = x.Key,
                        .Value = x.Value
                    }
                Next
            End Get
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Set(value As IEnumerable(Of NamedValue(Of T)))
                Properties = value.ToDictionary(Function(x) x.Name, Function(x) x.Value)
            End Set
        End Property

        Public Overloads Shared Narrowing Operator CType(properties As [Property](Of T)) As Dictionary(Of String, T)
            Return properties.Properties
        End Operator

        Public Shared Operator +(base As [Property](Of T), append As [Property](Of T)) As [Property](Of T)
            For Each item As NamedValue(Of T) In append.src
                base(item.Name) = item.Value
            Next

            Return base
        End Operator
    End Class
End Namespace
