#Region "Microsoft.VisualBasic::40a7258cd312a7a1d0d62667ac0e6412, Microsoft.VisualBasic.Core\src\ComponentModel\DataSource\Property\Extensions.vb"

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
    '    Code Lines: 37 (68.52%)
    ' Comment Lines: 8 (14.81%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 9 (16.67%)
    '     File Size: 2.18 KB


    '     Module Extensions
    ' 
    '         Function: ColRenames, PropertyNames
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

        ''' <summary>
        ''' Gets the union collection of the keys from <see cref="DynamicPropertyBase(Of T).Properties"/>.
        ''' </summary>
        ''' <param name="list"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' (包含所有的已经去除重复了的属性名称)
        ''' </remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function PropertyNames(Of T)(list As IEnumerable(Of DynamicPropertyBase(Of T))) As String()
            Return list _
                .Where(Function(a) Not a Is Nothing) _
                .Select(Function(o) o.EnumerateKeys(joinProperties:=False)) _
                .IteratesALL _
                .Distinct _
                .ToArray
        End Function
    End Module
End Namespace
