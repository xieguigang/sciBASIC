Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Public Module NumericDataSet
    ''' <summary>
    ''' log of the dataframe fields 
    ''' </summary>
    ''' <param name="df"></param>
    ''' <param name="base"></param>
    ''' <returns></returns>
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