#Region "Microsoft.VisualBasic::c0cc5a70a998e2f1a3f31e309de01fa5, Data_science\DataMining\DataMining\ComponentModel\TraceBackAlgorithm.vb"

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
    '    Code Lines: 31 (57.41%)
    ' Comment Lines: 14 (25.93%)
    '    - Xml Docs: 85.71%
    ' 
    '   Blank Lines: 9 (16.67%)
    '     File Size: 2.13 KB


    '     Class TraceBackAlgorithm
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetTraceBack, (+2 Overloads) MeasureCurve
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.DataMining.KMeans

Namespace ComponentModel

    Public MustInherit Class TraceBackAlgorithm

        Protected traceback As TraceBackIterator

        Sub New()
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns>
        ''' this function is a safe function, will never returns the null value
        ''' </returns>
        Public Function GetTraceBack() As IEnumerable(Of NamedCollection(Of String))
            If traceback Is Nothing Then
                Return New NamedCollection(Of String)() {}
            Else
                Return traceback.GetTraceback
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function MeasureCurve(ds As EntityClusterModel(), traceback As TraceBackIterator) As IEnumerable(Of EvaluationScore)
            Return MeasureCurve(New DataSetConvertor(ds).GetVectors(ds).ToArray, traceback)
        End Function

        ''' <summary>
        ''' this function set entity <see cref="ClusterEntity.cluster"/> for each iteration 
        ''' traceback and evaluate the silhouette score for the traceback.
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="traceback"></param>
        ''' <returns></returns>
        Public Shared Iterator Function MeasureCurve(data As ClusterEntity(), traceback As TraceBackIterator) As IEnumerable(Of EvaluationScore)
            Dim score As EvaluationScore

            ' zero no clusters
            For Each i As Integer In Tqdm.Wrap(Enumerable.Range(1, traceback.size - 1).ToArray, useColor:=True, wrap_console:=App.EnableTqdm)
                traceback.SetTraceback(data, itr:=i)
                score = EvaluationScore.Evaluate(data)

                Yield score
            Next
        End Function
    End Class
End Namespace
