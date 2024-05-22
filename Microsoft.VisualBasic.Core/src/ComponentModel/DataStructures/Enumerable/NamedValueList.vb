#Region "Microsoft.VisualBasic::b28acecbf305e4db101b0452f5ed115e, Microsoft.VisualBasic.Core\src\ComponentModel\DataStructures\Enumerable\NamedValueList.vb"

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

    '   Total Lines: 33
    '    Code Lines: 20 (60.61%)
    ' Comment Lines: 6 (18.18%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 7 (21.21%)
    '     File Size: 1.11 KB


    '     Class NamedValueList
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    '         Operators: +
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ComponentModel.Collection.Generic

    Public Class NamedValueList(Of T) : Inherits List(Of NamedValue(Of T))

        Sub New()
            Call MyBase.New()
        End Sub

        Public Overrides Function ToString() As String
            Return MyBase.Keys.GetJson
        End Function

        ''' <summary>
        ''' This operator will makes a copy of <paramref name="table"/>
        ''' </summary>
        ''' <param name="table"></param>
        ''' <param name="list"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator +(table As Dictionary(Of String, T), list As NamedValueList(Of T)) As Dictionary(Of String, T)
            table = New Dictionary(Of String, T)(table)

            For Each item As NamedValue(Of T) In list
                Call table.Add(item.Name, item.Value)
            Next

            Return table
        End Operator
    End Class
End Namespace
