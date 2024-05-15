#Region "Microsoft.VisualBasic::fda1c38ae53e0b9f9133d16483a1adca, Microsoft.VisualBasic.Core\src\ComponentModel\DataSource\Property\Extensions.vb"

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

    '   Total Lines: 35
    '    Code Lines: 27
    ' Comment Lines: 0
    '   Blank Lines: 8
    '     File Size: 1.38 KB


    '     Module Extensions
    ' 
    '         Function: ColRenames
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

Namespace ComponentModel.DataSourceModel

    <HideModuleName> Public Module Extensions

        <Extension>
        Public Iterator Function ColRenames(Of T)(data As IEnumerable(Of DynamicPropertyBase(Of T)), newColNames$()) As IEnumerable(Of DynamicPropertyBase(Of T))
            Dim dataframe As DynamicPropertyBase(Of T)() = data.ToArray
            Dim oldColNames As String() = dataframe _
                .Select(Function(r) r.Properties.Keys) _
                .IteratesALL _
                .Distinct _
                .ToArray

            If oldColNames.Length <> newColNames.Length Then
                Throw New InvalidConstraintException($"the length of old column name array is not equals to the new column name array!")
            End If

            For Each item As DynamicPropertyBase(Of T) In dataframe
                Dim renames As New Dictionary(Of String, T)
                Dim rowData As Dictionary(Of String, T) = item.Properties

                For i As Integer = 0 To oldColNames.Length - 1
                    renames.Add(newColNames(i), rowData.TryGetValue(oldColNames(i)))
                Next

                item.Properties = renames

                Yield item
            Next
        End Function
    End Module
End Namespace
