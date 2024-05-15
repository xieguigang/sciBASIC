#Region "Microsoft.VisualBasic::d0b77250564abb52ce7fc40e1741e987, Data_science\DataMining\DataMining\Evaluation\Validate.vb"

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
    '    Code Lines: 61
    ' Comment Lines: 16
    '   Blank Lines: 12
    '     File Size: 3.41 KB


    '     Structure Validate
    ' 
    '         Properties: actuals, err, predicts, width
    ' 
    '         Function: AUC, ROC, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text.Xml.Models
Imports stdNum = System.Math

Namespace Evaluation

    ''' <summary>
    ''' 一个包含有多维度验证结果输出的样本验证结果
    ''' </summary>
    Public Structure Validate

        <XmlAttribute> Public Property actuals As Double()
        <XmlAttribute> Public Property predicts As Double()

        Public ReadOnly Property err As Double
            Get
                Dim predicts = Me.predicts

                Return actuals _
                    .Select(Function(x, i) stdNum.Abs(x - predicts(i))) _
                    .Average
            End Get
        End Property

        ''' <summary>
        ''' 验证的结果的维度的数量, <see cref="actuals"/>和<see cref="predicts"/>这两个向量应该都是等长的
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property width As Integer
            Get
                If actuals.Length <> predicts.Length Then
                    Throw New DataMisalignedException
                Else
                    Return actuals.Length
                End If
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"|{actuals.JoinBy(", ")} - {predicts.JoinBy(", ")}| = {err}"
        End Function

        ''' <summary>
        ''' populate ROC validation for each output labels
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="threshold"></param>
        ''' <param name="outputLabels">
        ''' tag the output names
        ''' </param>
        ''' <returns></returns>
        Public Shared Iterator Function ROC(data As IEnumerable(Of Validate),
                                            Optional threshold As Sequence = Nothing,
                                            Optional outputLabels$() = Nothing) As IEnumerable(Of NamedCollection(Of Validation))

            Dim dataArray = data.ToArray
            Dim width = dataArray(Scan0).width

            If outputLabels.IsNullOrEmpty Then
                outputLabels = width _
                    .SeqIterator _
                    .Select(Function(i) $"output_i") _
                    .ToArray
            End If

#Disable Warning
            For i As Integer = 0 To width - 1
                Yield New NamedCollection(Of Validation) With {
                    .name = outputLabels(i),
                    .value = Validation.ROC(Of Validate)(
                        entity:=dataArray,
                        getValidate:=Function(x, cutoff) x.actuals(i) >= cutoff,
                        getPredict:=Function(x, cutoff) x.predicts(i) >= cutoff,
                        threshold:=threshold Or Validation.normalRange
                    ).ToArray
                }
            Next
#Enable Warning
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function AUC(data As IEnumerable(Of Validate), Optional outputLabels$() = Nothing) As IEnumerable(Of NamedValue(Of Double))
            Return data.AUC(names:=outputLabels)
        End Function
    End Structure
End Namespace
