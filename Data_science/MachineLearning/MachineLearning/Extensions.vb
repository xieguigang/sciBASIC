#Region "Microsoft.VisualBasic::25fa859aeddc4c3185df2c815c5cf5d4, Data_science\MachineLearning\MachineLearning\Extensions.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Module Extensions
    ' 
    '     Function: Delta, ToDataMatrix
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.StoreProcedure

<HideModuleName> Public Module Extensions

    ''' <summary>
    ''' Generate small delta for GA mutations
    ''' </summary>
    ''' <param name="x#"></param>
    ''' <param name="d#"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' 1 = 10 ^ 0  ~  0.1 = 10 ^ 1 * 0.1
    ''' 10 = 10 ^ 1  ~ 1 = 10 ^ 2 * 0.1
    ''' </remarks>
    <Extension>
    Public Function Delta(x#, Optional d# = 1 / 10) As Double
        Dim p10 = Fix(Math.Log10(x))
        Dim small = (10 ^ (p10 + 1)) * d
        Return small
    End Function

    <Extension>
    Public Function ToDataMatrix(Of T As {New, DynamicPropertyBase(Of Double), INamedValue})(samples As IEnumerable(Of Sample), names$(), outputNames$()) As IEnumerable(Of T)
        Dim nameIndex = names.SeqIterator.ToArray
        Dim outsIndex = outputNames.SeqIterator.ToArray

        Return samples _
            .Select(Function(sample)
                        Dim row As New T

                        row.Key = sample.ID
                        row.Properties = New Dictionary(Of String, Double)

                        Call nameIndex.DoEach(Sub(i) Call row.Add(i.value, sample.status(i)))
                        Call outsIndex.DoEach(Sub(i) Call row.Add(i.value, sample.target(i)))

                        Return row
                    End Function)
    End Function
End Module
