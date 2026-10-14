#Region "Microsoft.VisualBasic::1f2cedaf32eb0b630ad559b02d7f0e47, vs_solutions\JavaScript\Runtime\ClrAttributes.vb"

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

    '   Total Lines: 53
    '    Code Lines: 24 (45.28%)
    ' Comment Lines: 22 (41.51%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 7 (13.21%)
    '     File Size: 2.26 KB


    '     Class JsNameAttribute
    ' 
    '         Properties: Name
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class JsIgnoreAttribute
    ' 
    ' 
    ' 
    '     Class JsExposeAttribute
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Runtime

    ''' <summary>
    ''' Renames a CLR member for script access: <c>&lt;JsName("count")&gt;</c> makes the
    ''' member visible as <c>obj.count</c>.
    ''' <para>
    ''' The bridge matches member names exactly (there is no implicit
    ''' camelCase↔PascalCase mapping), so this attribute is the way to expose
    ''' <c>TotalCount</c> as <c>obj.totalCount</c>.
    ''' </para>
    ''' </summary>
    <AttributeUsage(AttributeTargets.Property Or AttributeTargets.Field Or AttributeTargets.Method Or
                    AttributeTargets.Constructor Or AttributeTargets.Class Or AttributeTargets.Struct,
                    AllowMultiple:=False, Inherited:=True)>
    Public NotInheritable Class JsNameAttribute
        Inherits Attribute

        ''' <summary>The script-visible name.</summary>
        Public ReadOnly Property Name As String

        Public Sub New(name As String)
            Me.Name = name
        End Sub

    End Class

    ''' <summary>
    ''' Hides a CLR member from script (it stays reachable from .NET code only).
    ''' </summary>
    <AttributeUsage(AttributeTargets.Property Or AttributeTargets.Field Or AttributeTargets.Method Or
                    AttributeTargets.Constructor Or AttributeTargets.Class Or AttributeTargets.Struct,
                    AllowMultiple:=False, Inherited:=True)>
    Public NotInheritable Class JsIgnoreAttribute
        Inherits Attribute
    End Class

    ''' <summary>
    ''' Opts a single member into script access and — as soon as one member of a type
    ''' carries it — switches that type to WHITELIST mode: only members marked with
    ''' this attribute stay visible, so a type can be exposed selectively without
    ''' hand-maintaining an ignore list.
    ''' <para>
    ''' <see cref="ClrInterop.ExplicitOnly"/> forces whitelist mode for every type.
    ''' </para>
    ''' </summary>
    <AttributeUsage(AttributeTargets.Property Or AttributeTargets.Field Or AttributeTargets.Method Or
                    AttributeTargets.Constructor,
                    AllowMultiple:=False, Inherited:=True)>
    Public NotInheritable Class JsExposeAttribute
        Inherits Attribute
    End Class

End Namespace
