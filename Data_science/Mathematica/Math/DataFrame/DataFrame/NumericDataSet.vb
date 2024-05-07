#Region "Microsoft.VisualBasic::f9377cddc2267fce265e1866458942c1, G:/GCModeller/src/runtime/sciBASIC#/Data_science/Mathematica/Math/DataFrame//DataFrame/NumericDataSet.vb"

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

    '   Total Lines: 131
    '    Code Lines: 105
    ' Comment Lines: 6
    '   Blank Lines: 20
    '     File Size: 4.53 KB


    ' Module NumericDataSet
    ' 
    '     Function: IndexGetter, NumericGetter, NumericMatrix, ZScale, ZScaleByCol
    '               ZScaleByRow
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Distributions
Imports Microsoft.VisualBasic.Scripting
Imports any = Microsoft.VisualBasic.Scripting

Public Module NumericDataSet

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
    Public Iterator Function NumericMatrix(df As DataFrame) As IEnumerable(Of NamedCollection(Of Double))
        Dim colnames As String() = df.featureNames
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

            Yield New NamedCollection(Of Double)(rownames(i), row)
        Next
    End Function

    <Extension>
    Public Function Log(df As DataFrame, Optional base As Double = 2) As DataFrame
        Dim df_z As New DataFrame With {
            .features = New Dictionary(Of String, FeatureVector),
            .rownames = df.rownames.ToArray
        }

        For Each name As String In df.featureNames
            Dim v As Double() = df(name).TryCast(Of Double)
            v = v.AsVector.Log(base)
            df_z(name) = New FeatureVector(name, v)
        Next

        Return df_z
    End Function

    <Extension>
    Public Function Center(df As DataFrame, Optional byrow As Boolean = False) As DataFrame
        If byrow Then
            Return df.CenterByRow
        Else
            Return df.CenterByCol
        End If
    End Function

    <Extension>
    Private Function CenterByRow(df As DataFrame) As DataFrame
        Dim df_z As New List(Of Double())
        Dim v As Double()

        For Each row As NamedCollection(Of Object) In df.foreachRow
            v = row.value.CTypeDynamic(type:=GetType(Double))
            v = v.AsVector - v.Average
            df_z.Add(v)
        Next

        Dim cols As String() = df.featureNames
        Dim z As New DataFrame With {
            .features = New Dictionary(Of String, FeatureVector),
            .rownames = df.rownames.ToArray
        }
        Dim offset As Integer

        For i As Integer = 0 To cols.Length - 1
            offset = i
            v = df_z.Select(Function(row) row(offset)).ToArray
            z.add(cols(i), v)
        Next

        Return z
    End Function

    <Extension>
    Private Function CenterByCol(df As DataFrame) As DataFrame
        Dim df_z As New DataFrame With {
           .features = New Dictionary(Of String, FeatureVector),
           .rownames = df.rownames.ToArray
       }

        For Each name As String In df.featureNames
            Dim v As Double() = df(name).TryCast(Of Double)

            If v.Length > 1 Then
                v = v.AsVector - v.Average
            End If

            df_z(name) = New FeatureVector(name, v)
        Next

        Return df_z
    End Function

    <Extension>
    Public Function Scale01(df As DataFrame, Optional byrow As Boolean = False) As DataFrame
        If byrow Then
            Return df.Scale01ByRow
        Else
            Return df.Scale01ByCol
        End If
    End Function

    <Extension>
    Private Function Scale01ByRow(df As DataFrame) As DataFrame
        Dim df_z As New List(Of Double())
        Dim v As Double()

        For Each row As NamedCollection(Of Object) In df.foreachRow
            v = row.value.CTypeDynamic(type:=GetType(Double))
            v = v.AsVector / v.Max
            df_z.Add(v)
        Next

        Dim cols As String() = df.featureNames
        Dim z As New DataFrame With {
            .features = New Dictionary(Of String, FeatureVector),
            .rownames = df.rownames.ToArray
        }
        Dim offset As Integer

        For i As Integer = 0 To cols.Length - 1
            offset = i
            v = df_z.Select(Function(row) row(offset)).ToArray
            z.add(cols(i), v)
        Next

        Return z
    End Function

    <Extension>
    Private Function Scale01ByCol(df As DataFrame) As DataFrame
        Dim df_z As New DataFrame With {
           .features = New Dictionary(Of String, FeatureVector),
           .rownames = df.rownames.ToArray
       }

        For Each name As String In df.featureNames
            Dim v As Double() = df(name).TryCast(Of Double)

            If v.Length > 1 Then
                v = v.AsVector / v.Max
            End If

            df_z(name) = New FeatureVector(name, v)
        Next

        Return df_z
    End Function


    <Extension>
    Public Function Standard(df As DataFrame, Optional byrow As Boolean = False) As DataFrame
        If byrow Then
            Return df.StandardByRow
        Else
            Return df.StandardByCol
        End If
    End Function

    <Extension>
    Private Function StandardByRow(df As DataFrame) As DataFrame
        Dim df_z As New List(Of Double())
        Dim v As Double()

        For Each row As NamedCollection(Of Object) In df.foreachRow
            v = row.value.CTypeDynamic(type:=GetType(Double))
            v = (v.AsVector - v.Min) / v.Max
            df_z.Add(v)
        Next

        Dim cols As String() = df.featureNames
        Dim z As New DataFrame With {
            .features = New Dictionary(Of String, FeatureVector),
            .rownames = df.rownames.ToArray
        }
        Dim offset As Integer

        For i As Integer = 0 To cols.Length - 1
            offset = i
            v = df_z.Select(Function(row) row(offset)).ToArray
            z.add(cols(i), v)
        Next

        Return z
    End Function

    <Extension>
    Private Function StandardByCol(df As DataFrame) As DataFrame
        Dim df_z As New DataFrame With {
            .features = New Dictionary(Of String, FeatureVector),
            .rownames = df.rownames.ToArray
        }

        For Each name As String In df.featureNames
            Dim v As Double() = df(name).TryCast(Of Double)

            If v.Length > 1 Then
                v = (v.AsVector - v.Min) / v.Max
            End If

            df_z(name) = New FeatureVector(name, v)
        Next

        Return df_z
    End Function

    ''' <summary>
    ''' z-score scale of the dataframe data, usually used for the heatmap drawing
    ''' </summary>
    ''' <param name="df"></param>
    ''' <param name="byrow"></param>
    ''' <returns></returns>
    <Extension>
    Public Function ZScale(df As DataFrame, Optional byrow As Boolean = False) As DataFrame
        If byrow Then
            Return df.ZScaleByRow
        Else
            Return df.ZScaleByCol
        End If
    End Function

    <Extension>
    Private Function ZScaleByCol(df As DataFrame) As DataFrame
        Dim df_z As New DataFrame With {
            .features = New Dictionary(Of String, FeatureVector),
            .rownames = df.rownames.ToArray
        }

        For Each name As String In df.featureNames
            Dim v As Double() = df(name).TryCast(Of Double)

            If v.Length > 1 Then
                v = v.AsVector.Z
            End If

            df_z(name) = New FeatureVector(name, v)
        Next

        Return df_z
    End Function

    <Extension>
    Private Function ZScaleByRow(df As DataFrame) As DataFrame
        Dim df_z As New List(Of Double())
        Dim v As Double()

        For Each row As NamedCollection(Of Object) In df.foreachRow
            v = row.value.CTypeDynamic(type:=GetType(Double))
            v = v.AsVector.Z
            df_z.Add(v)
        Next

        Dim cols As String() = df.featureNames
        Dim z As New DataFrame With {
            .features = New Dictionary(Of String, FeatureVector),
            .rownames = df.rownames.ToArray
        }
        Dim offset As Integer

        For i As Integer = 0 To cols.Length - 1
            offset = i
            v = df_z.Select(Function(row) row(offset)).ToArray
            z.add(cols(i), v)
        Next

        Return z
    End Function
End Module
