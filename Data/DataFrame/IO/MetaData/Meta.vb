#Region "Microsoft.VisualBasic::31985d9ddfcbaf16b15d46198913056c, Data\DataFrame\IO\MetaData\Meta.vb"

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

    '   Total Lines: 97
    '    Code Lines: 74 (76.29%)
    ' Comment Lines: 6 (6.19%)
    '    - Xml Docs: 83.33%
    ' 
    '   Blank Lines: 17 (17.53%)
    '     File Size: 3.57 KB


    '     Module Meta
    ' 
    '         Function: DataFrameWithMeta, IsMetaRow, (+4 Overloads) ToCsvMeta, (+2 Overloads) TryGetMetaData
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Emit.Marshal

Namespace IO

    Module Meta

        Public Function TryGetMetaData(Of T)(reader As File, ByRef i%) As T
            Dim [in] As Dictionary(Of String, String) = TryGetMetaData(reader, i)
            Dim schema = DataFrameColumnAttribute.LoadMapping(Of T)(mapsAll:=True)
            Dim x As Object = Activator.CreateInstance(Of T)
            Dim value As String = Nothing

            For Each prop In schema
                If [in].TryGetValue(prop.Key, value) Then
                    Dim o As Object =
                        Scripting.CTypeDynamic(value, prop.Value.Type)
                    Call prop.Value.SetValue(x, o)
                End If
            Next

            Return DirectCast(x, T)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="reader"></param>
        ''' <param name="i">下一行是标题行</param>
        ''' <returns></returns>
        Public Function TryGetMetaData(reader As File, ByRef i%) As Dictionary(Of String, String)
            Dim p As New Pointer(Of RowObject)(reader)
            Dim out As New Dictionary(Of String, String)
            Dim name As String

            Do While (++p).IsMetaRow
                Dim row = p.Current.First.GetTagValue("=")
                name = row.Name
                name = Regex.Replace(name, "^#+", "", RegexOptions.Multiline)

                Call out.Add(name, row.Value)
            Loop

            i = p.Position

            Return out
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function DataFrameWithMeta(Of T)(x As T) As File
            Return New File(x.ToCsvMeta)
        End Function

        <Extension>
        Public Function IsMetaRow(row As RowObject) As Boolean
            If row.First <> "#"c Then
                Return False
            Else
                Return row.Count <= 2
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ToCsvMeta(Of T)(x As T) As RowObject()
            Return ToCsvMeta(x, GetType(T))
        End Function

        Public Function ToCsvMeta(o As Object, type As Type) As RowObject()
            Dim schema = DataFrameColumnAttribute.LoadMapping(type, mapsAll:=True)
            Dim source = schema.Select(Function(x) New NamedValue(Of Object)(x.Key, x.Value.GetValue(o)))
            Dim out As RowObject() = ToCsvMeta(source).ToArray
            Return out
        End Function

        <Extension>
        Public Iterator Function ToCsvMeta(Of T)(source As IEnumerable(Of NamedValue(Of T))) As IEnumerable(Of RowObject)
            Dim s As String

            For Each x As NamedValue(Of T) In source
                s = Scripting.ToString(x.Value)
                Yield New RowObject({$"##{x.Name}={s}"})
            Next
        End Function

        <Extension>
        Public Iterator Function ToCsvMeta(source As IEnumerable(Of KeyValuePair(Of String, String))) As IEnumerable(Of RowObject)
            For Each x In source
                Yield New RowObject({$"##{x.Key}={x.Value}"})
            Next
        End Function
    End Module
End Namespace
