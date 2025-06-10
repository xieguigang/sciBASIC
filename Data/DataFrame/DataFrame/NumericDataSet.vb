#Region "Microsoft.VisualBasic::4b75b3396d7dafc64a3124ee3c95f7dc, Data\DataFrame\DataFrame\NumericDataSet.vb"

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

    '   Total Lines: 167
    '    Code Lines: 130 (77.84%)
    ' Comment Lines: 11 (6.59%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 26 (15.57%)
    '     File Size: 5.89 KB


    ' Module NumericDataSet
    ' 
    '     Function: FromDataSet, IndexGetter, NumericGetter, NumericMatrix, PullDataSet
    '               Transpose
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports any = Microsoft.VisualBasic.Scripting

Public Module NumericDataSet

    <Extension>
    Public Function Transpose(mat As DataFrame) As DataFrame
        Dim table As New Dictionary(Of String, FeatureVector)
        Dim cols As String() = mat.featureNames

        If mat.rownames.IsNullOrEmpty Then
            mat.rownames = mat.dims.Height _
                .Sequence _
                .Select(Function(i) CStr(i + 1)) _
                .ToArray
        End If

        Dim nrows = mat.dims.Height
        Dim index As Integer

        For i As Integer = 0 To nrows - 1
            index = i
            table(mat.rownames(i)) = New FeatureVector(
                name:=mat.rownames(i),
                doubles:=cols _
                    .Select(Function(k) CDbl(mat(k)(index))) _
                    .ToArray
            )
        Next

        Return New DataFrame With {
            .features = table,
            .rownames = cols
        }
    End Function

    <Extension>
    Private Function IndexGetter(v As Double()) As Func(Of Integer, Double)
        If v.IsNullOrEmpty Then
            Return Function(any) 0.0
        ElseIf v.Length = 1 Then
            Dim scalar As Double = v(0)
            Return Function(any) scalar
        Else
            Return Function(i) v(i)
        End If
    End Function

    ''' <summary>
    ''' a getter delegate helper for number type
    ''' </summary>
    ''' <param name="v"></param>
    ''' <returns></returns>
    <Extension>
    Public Function NumericGetter(v As FeatureVector) As Func(Of Integer, Double)
        Select Case v.type
            Case GetType(Double), GetType(Single), GetType(Integer),
                 GetType(Long), GetType(UInteger), GetType(ULong),
                 GetType(Short), GetType(UShort),
                 GetType(Boolean),
                 GetType(DateTime),
                 GetType(Byte), GetType(SByte)

                Return v.TryCast(Of Double)().IndexGetter
            Case GetType(String), GetType(Char)
                Dim factors As Index(Of String) = v.vector _
                    .AsObjectEnumerator _
                    .Select(Function(s) any.ToString(s)) _
                    .Indexing
                Dim vals As Double() = DirectCast(v.vector, String()) _
                    .Select(Function(f) CDbl(factors(f))) _
                    .ToArray

                Call $"cast the feature {v.name} in character type to numeric vector.".Warning

                Return vals.IndexGetter
            Case Else
                Throw New NotImplementedException($"could not cast object of type '{v.type.Name}' to numeric value!")
        End Select
    End Function

    ''' <summary>
    ''' convert the row data inside the given dataframe as given type of the row data objects
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="df"></param>
    ''' <returns></returns>
    <Extension>
    Public Iterator Function PullDataSet(Of T As {New, INamedValue, DynamicPropertyBase(Of Double)})(df As DataFrame) As IEnumerable(Of T)
        Dim colnames As String() = df.featureNames
        Dim fieldGetters As Func(Of Integer, Double)() = colnames _
            .Select(Function(s) df(s).NumericGetter) _
            .ToArray
        Dim nrow As Integer = df.nsamples
        Dim rownames As String() = df.rownames
        Dim offset As Integer
        Dim row As Dictionary(Of String, Double)
        Dim datasetRow As T

        For i As Integer = 0 To nrow - 1
            offset = i
            row = New Dictionary(Of String, Double)

            For j As Integer = 0 To colnames.Length - 1
                row.Add(colnames(j), fieldGetters(j)(offset))
            Next

            datasetRow = New T With {.Key = rownames(i)}
            datasetRow.Properties = row

            Yield datasetRow
        Next
    End Function

    <Extension>
    Public Function FromDataSet(Of T As {INamedValue, DynamicPropertyBase(Of Double)})(dataset As IEnumerable(Of T)) As DataFrame
        If dataset Is Nothing Then
            Return Nothing
        End If

        Dim pool As New List(Of T)
        Dim cols As New List(Of String)
        Dim labels As New List(Of String)

        For Each row As T In dataset
            Call pool.Add(row)
            Call cols.AddRange(row.Properties.Keys)
            Call labels.Add(row.Key)
        Next

        Dim fields As New List(Of FeatureVector)

        For Each colname As String In cols.Distinct
            Dim v As Double() = pool.Select(Function(x) x(colname)).ToArray
            Dim vec As New FeatureVector(colname, v)

            Call fields.Add(vec)
        Next

        Return New DataFrame(fields, labels)
    End Function

    ''' <summary>
    ''' returns a data matrix in row collections
    ''' </summary>
    ''' <param name="df"></param>
    ''' <returns></returns>
    <Extension>
    Public Iterator Function NumericMatrix(df As DataFrame) As IEnumerable(Of NamedCollection(Of Double))
        Dim colnames As String() = df.featureNames
        ' field columns
        Dim fieldGetters As Func(Of Integer, Double)() = colnames _
            .Select(Function(s) df(s).NumericGetter) _
            .ToArray
        Dim nrow As Integer = df.nsamples
        Dim rownames As String() = df.rownames
        Dim offset As Integer
        Dim row As Double()

        For i As Integer = 0 To nrow - 1
            offset = i
            row = fieldGetters _
                .Select(Function(v) v(offset)) _
                .ToArray

            ' populate each row data
            Yield New NamedCollection(Of Double)(rownames(i), row)
        Next
    End Function
End Module
