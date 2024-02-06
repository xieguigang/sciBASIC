
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.DataFrame
Imports Microsoft.VisualBasic.Scripting.Expressions
Imports any = Microsoft.VisualBasic.Scripting
Imports df = Microsoft.VisualBasic.Math.DataFrame.DataFrame

''' <summary>
''' helper module for create the input dataset for run analysis
''' </summary>
Public Module DataSetHelper

    <Extension>
    Public Function CommonDataSet(df As df, Optional labels As Array = Nothing) As StatisticsObject
        Return df.NumericMatrix().CommonDataSet(colnames:=df.featureNames, labels)
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="Row"></typeparam>
    ''' <param name="mat"></param>
    ''' <param name="labels">the sample class labels for each row of the <paramref name="mat"/>, 
    ''' should be a string array(classify) or a numeric array(regression).
    ''' </param>
    ''' <returns></returns>
    <Extension>
    Public Function CommonDataSet(Of Row As {INamedValue, IVector})(mat As IEnumerable(Of Row),
                                                                    colnames As String(),
                                                                    Optional labels As Array = Nothing) As StatisticsObject
        Return mat _
            .Select(Function(r) New NamedCollection(Of Double)(r.Key, r.Data)) _
            .CommonDataSet(colnames, labels)
    End Function

    <Extension>
    Public Function CommonDataSet(mat As IEnumerable(Of NamedCollection(Of Double)), colnames As String(), Optional labels As Array = Nothing) As StatisticsObject
        Dim pool As NamedCollection(Of Double)() = mat.SafeQuery.ToArray
        Dim xm As Double()() = pool.Select(Function(v) v.value).ToArray
        Dim ylabels As String() = Nothing
        Dim yval As Double() = Nothing
        Dim yfactors As Index(Of String) = Nothing
        Dim ncols As Integer = pool(0).Length
        Dim nrows As Integer = pool.Length

        If Not labels.IsNullOrEmpty Then
            If DataFramework.IsNumericCollection(labels.GetType) Then
                ' regression
                yval = (From yi As Object In labels Select CDbl(yi)).ToArray
                ylabels = pool.Select(Function(a) a.name).ToArray
            Else
                ylabels = (From yi As Object In labels Select any.ToString(yi)).ToArray
                yfactors = New Index(Of String)(ylabels, base:=1)
                yval = ylabels _
                    .Select(Function(xi) CDbl(ylabels.IndexOf(xi))) _
                    .ToArray
            End If
        End If

        If yfactors Is Nothing Then
            yfactors = New Index(Of String)
        End If

        Dim ds As New StatisticsObject(xm, yval) With {
            .decoder = yfactors
        }

        Call Enumerable.Range(0, ncols).DoEach(AddressOf ds.XIndexes.Add)
        Call Enumerable.Range(0, nrows).DoEach(AddressOf ds.YIndexes.Add)
        Call colnames.DoEach(AddressOf ds.XLabels.Add)

        If Not ylabels.IsNullOrEmpty Then
            Call ylabels.DoEach(AddressOf ds.YLabels.Add)
        End If

        Return ds
    End Function
End Module
