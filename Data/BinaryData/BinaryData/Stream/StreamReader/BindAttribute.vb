#Region "Microsoft.VisualBasic::14fd378454dd18e4d7a05d38f8f00e58, Data\BinaryData\BinaryData\Stream\StreamReader\BindAttribute.vb"

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

    '   Total Lines: 18
    '    Code Lines: 12 (66.67%)
    ' Comment Lines: 3 (16.67%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 3 (16.67%)
    '     File Size: 529 B


    ' Class BindAttribute
    ' 
    '     Properties: Par, Type
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: ToString
    ' 
    ' /********************************************************************************/

#End Region

''' <summary>
''' bind data type to read/write
''' </summary>
<AttributeUsage(AttributeTargets.Method, AllowMultiple:=False, Inherited:=True)>
Public Class BindAttribute : Inherits Attribute

    Public ReadOnly Property Type As TypeCode
    Public ReadOnly Property Par As Object

    Sub New(type As TypeCode, Optional par As Object = Nothing)
        Me.Par = par
        Me.Type = type
    End Sub

    Public Overrides Function ToString() As String
        Return Type.Description
    End Function
End Class
