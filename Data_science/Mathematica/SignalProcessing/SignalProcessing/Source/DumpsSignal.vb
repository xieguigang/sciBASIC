#Region "Microsoft.VisualBasic::ab264ba6af633ea183200078cf06f8ab, Data_science\Mathematica\SignalProcessing\SignalProcessing\Source\DumpsSignal.vb"

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

    '     Module DumpsSignal
    ' 
    '         Function: bumps
    ' 
    '         Sub: bump
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Source

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' Inspired by Lee Byron's test data generator.
    ''' </remarks>
    Public Module DumpsSignal

        ''' <summary>
        ''' Generate signal data for test
        ''' </summary>
        ''' <param name="length">信号的长度，点的数量</param>
        ''' <param name="m">信号的叠加次数</param>
        ''' <returns></returns>
        Public Function bumps(length%, Optional m% = 25) As Double()
            Dim a#() = New Double(length - 1) {}
            Dim seed As New Random

            For i As Integer = 0 To m - 1
                Call bump(a, length, seed)
            Next

            Return a
        End Function

        Private Sub bump(ByRef a#(), length%, seed As Random)
            Dim x = 1 / (0.1 + seed.NextDouble),
                y = 2 * seed.NextDouble - 0.5,
                Z = 10 / (0.1 + seed.NextDouble)

            For i As Integer = 0 To length - 1
                Dim w = (i / length - y) * Z
                a(i) += x * Math.Exp(-w * w)
            Next
        End Sub
    End Module
End Namespace
