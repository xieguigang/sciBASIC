#Region "Microsoft.VisualBasic::4ddf331566947d935b363d058d7018ac, Data\DataFrame\DataSet\Union.vb"

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

    '   Total Lines: 51
    '    Code Lines: 36 (70.59%)
    ' Comment Lines: 4 (7.84%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 11 (21.57%)
    '     File Size: 1.74 KB


    '     Class CbindProvider
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: (+2 Overloads) Union
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.Framework.IO

Namespace DATA

    ''' <summary>
    ''' An abstract union operation data model
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public NotInheritable Class CbindProvider(Of T)

        Shared ReadOnly schema As Dictionary(Of String, PropertyInfo) = DataFramework.Schema(Of T)(PropertyAccess.Readable, True, True)

        Private Sub New()
        End Sub

        Public Shared Function Union(dataset As DataSet, obj As T) As DataSet
            Dim d As New DataSet With {
                .ID = dataset.ID,
                .Properties = New Dictionary(Of String, Double)(dataset.Properties)
            }

            Static numericFields As KeyValuePair(Of String, PropertyInfo)() =
                CbindProvider(Of T).schema _
                .Where(Function(f)
                           Return f.Value.PropertyType.IsNumericType
                       End Function) _
                .ToArray

            For Each field In numericFields
                d(field.Key) = CDbl(field.Value.GetValue(obj))
            Next

            Return d
        End Function

        Public Shared Function Union(entity As EntityObject, obj As T) As EntityObject
            Dim e As New EntityObject With {
                .ID = entity.ID,
                .Properties = New Dictionary(Of String, String)(entity.Properties)
            }

            For Each field In CbindProvider(Of T).schema
                e(field.Key) = CStr(field.Value.GetValue(obj))
            Next

            Return e
        End Function
    End Class
End Namespace
