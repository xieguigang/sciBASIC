#Region "Microsoft.VisualBasic::789706b42533a1753c01bc35b32bdc75, Microsoft.VisualBasic.Core\src\Scripting\MetaData\Author.vb"

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

    '   Total Lines: 23
    '    Code Lines: 17 (73.91%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 6 (26.09%)
    '     File Size: 730 B


    '     Class Author
    ' 
    '         Properties: value
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ToString
    ' 
    '         Sub: EMail
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Scripting.MetaData

    <AttributeUsage(AttributeTargets.All, AllowMultiple:=True, Inherited:=True)>
    Public Class Author : Inherits Attribute

        Public ReadOnly Property value As NamedValue(Of String)

        Sub New(name As String, email As String)
            value = New NamedValue(Of String)(name, email)
        End Sub

        Public Sub EMail()
            Call Diagnostics.Process.Start($"mailto://{value.Value}")
        End Sub

        Public Overrides Function ToString() As String
            Return value.GetJson
        End Function
    End Class
End Namespace
