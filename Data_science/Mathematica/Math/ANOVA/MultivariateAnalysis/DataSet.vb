#Region "Microsoft.VisualBasic::cbe78d34170ada4380dee1a3c1736afe, Data_science\Mathematica\Math\ANOVA\MultivariateAnalysis\DataSet.vb"

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

    '   Total Lines: 89
    '    Code Lines: 58 (65.17%)
    ' Comment Lines: 21 (23.60%)
    '    - Xml Docs: 85.71%
    ' 
    '   Blank Lines: 10 (11.24%)
    '     File Size: 3.73 KB


    ' Module DataSetHelper
    ' 
    '     Function: (+3 Overloads) CommonDataSet
    ' 
    ' /********************************************************************************/

#End Region

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

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="df"></param>
    ''' <param name="labels">
    ''' should be the sample name of each sample data, should be unique, and should not be the class label of each sample.
    ''' </param>
    ''' <returns></returns>
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
