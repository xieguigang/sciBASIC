#Region "Microsoft.VisualBasic::9022d0d45d79d1be33b336d20cf0ef29, vs_solutions\JavaScript\Runtime\AccessorProperty.vb"

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

    '   Total Lines: 27
    '    Code Lines: 10 (37.04%)
    ' Comment Lines: 11 (40.74%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (22.22%)
    '     File Size: 974 B


    '     Class AccessorProperty
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region


Namespace Runtime

    ''' <summary>
    ''' Getter/setter pair created by object-literal accessors
    ''' (<c>{ get x() {...}, set x(v) {...} }</c>) and by class accessors.
    ''' <para>
    ''' Stored as the property VALUE in the object's dictionary;
    ''' <see cref="JsRuntime.JsGet"/> / <see cref="JsRuntime.JsSet"/> recognise it
    ''' and invoke the corresponding accessor instead of reading/writing a slot.
    ''' </para>
    ''' </summary>
    Public NotInheritable Class AccessorProperty

        ''' <summary>Getter body (a callable), or Nothing for a setter-only property.</summary>
        Public ReadOnly Getter As Object
        ''' <summary>Setter body (a callable), or Nothing for a getter-only property.</summary>
        Public ReadOnly Setter As Object

        Public Sub New(getter As Object, setter As Object)
            Me.Getter = getter
            Me.Setter = setter
        End Sub

    End Class

End Namespace

