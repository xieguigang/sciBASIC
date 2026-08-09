#Region "Microsoft.VisualBasic::fbd7ab6d6fac1006d381b8deea4ff50b, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\Writer\Style\AppendAttribute.vb"

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

    '   Total Lines: 29
    '    Code Lines: 11 (37.93%)
    ' Comment Lines: 14 (48.28%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 4 (13.79%)
    '     File Size: 1.11 KB


    '     Class AppendAttribute
    ' 
    '         Properties: Ignore, NestedProperty
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace XLSX.Writer.Styling


    ''' <summary>
    ''' Attribute designated to control the copying of style properties
    ''' </summary>
    Public Class AppendAttribute
        Inherits Attribute
        ''' <summary>
        ''' Gets or sets a value indicating whether Ignore
        ''' Indicates whether the property annotated with the attribute is ignored during the copying of properties
        ''' </summary>
        Public Property Ignore As Boolean

        ''' <summary>
        ''' Gets or sets a value indicating whether NestedProperty
        ''' Indicates whether the property annotated with the attribute is a nested property. Nested properties are ignored during the copying of properties but can be broken down to its sub-properties
        ''' </summary>
        Public Property NestedProperty As Boolean

        ''' <summary>
        ''' Initializes a new instance of the <see cref="AppendAttribute"/> class
        ''' </summary>
        Public Sub New()
            Ignore = False
            NestedProperty = False
        End Sub
    End Class
End Namespace
