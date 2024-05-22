#Region "Microsoft.VisualBasic::4e94875a10296ed8905d9a85878934fd, Data_science\Mathematica\Math\ANOVA\ANOVA\DistributionTable.vb"

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

    '   Total Lines: 152
    '    Code Lines: 62 (40.79%)
    ' Comment Lines: 71 (46.71%)
    '    - Xml Docs: 14.08%
    ' 
    '   Blank Lines: 19 (12.50%)
    '     File Size: 8.31 KB


    ' Class DistributionTable
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: getColIndex, getCriticalNumber, getRowIndex, ToString
    ' 
    ' /********************************************************************************/

#End Region

Friend MustInherit Class DistributionTable

    Shared ReadOnly numerators As Integer() = New Integer() {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12, 15, 20, 24, 30, 40, 60, 120, 121}
    Shared ReadOnly denominators As Integer() = New Integer() {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 40, 60, 120, 121}

    ReadOnly dist As Double()()
    ReadOnly type As String

    Protected Sub New(type As String)
        Me.type = type
        Me.dist = loadMatrix.ToArray
    End Sub

    Protected MustOverride Function loadMatrix() As IEnumerable(Of Double())

    ' 
    '  The numerator = the number of groups that are being compared ( i.e., group degrees of freedom )
    '  The denomiator = the number of all the observations of all the groups ( i.e., group degrees of freedom )
    '  type = 0.05% or 0.01% test 
    '  
    '  Returns a double... ...if the F score is higher than the returned critial number
    '  then _reject the null hypothesis_
    ' 
    Public Shared Function getCriticalNumber(numerator As Integer, denominator As Integer, type As String) As Double
        Dim n As Integer = getRowIndex(numerator)
        Dim d As Integer = getColIndex(denominator)
        Dim criticalNumber As Double
        Dim row As Double()
        Dim matrix As Double()()

        Static five As New TableFivepercent
        Static one As New TableOnepercent

        ' NOTE: The table is 1 based but array are 0 based so -1 from each of the n and d
        Select Case type
            Case AnovaTest.P_FIVE_PERCENT : matrix = five.dist
            Case AnovaTest.P_ONE_PERCENT : matrix = one.dist
            Case Else
                Throw New NotImplementedException(type)
        End Select

        '  Error in <globalEnvironment> -> InitializeEnvironment -> "RunAnalysis" -> "RunAnalysis" -> RunAnalysis -> ".workflow" -> ".workflow" -> .workflow -> "autoreport" -> "autoreport" -> "RunMSImaging" -> "RunMSImaging" -> "RunDataVisualization" -> "RunDataVisualization" -> "RunBioDeep" -> "RunBioDeep" -> RunBioDeep -> else_false -> if_closure -> if_closure -> "checkRegionSigIons" -> "checkRegionSigIons" -> checkRegionSigIons -> else_false -> "MSI_single_stat" -> "MSI_single_stat" -> MSI_single_stat -> for_loop_[1] -> else_false -> "MSI_ion_stat" -> "MSI_ion_stat" -> MSI_ion_stat -> plot
        '   1. IndexOutOfRangeException: Index was outside the bounds of the array.
        '   2. stackFrames: 
        '    at Microsoft.VisualBasic.Math.Statistics.Hypothesis.ANOVA.DistributionTable.getCriticalNumber(Int32 numerator, Int32 denominator, String type)
        '    at Microsoft.VisualBasic.Math.Statistics.Hypothesis.ANOVA.AnovaTest.get_criticalNumber()
        '    at Microsoft.VisualBasic.Math.Statistics.Hypothesis.ANOVA.AnovaTest.ToString()
        '    at ggplot.layers.ggplotStatPvalue.plotAnova(ggplotPipeline stream, OrdinalScale x)
        '    at ggplot.layers.ggplotStatPvalue.PlotOrdinal(ggplotPipeline stream, OrdinalScale x)
        '    at ggplot.layers.ggplotGroup.Plot(ggplotPipeline stream)
        '    at ggplot.ggplot.plot2D(ggplotData baseData, IGraphics& g, GraphicsRegion canvas)
        '    at ggplot.ggplot.PlotInternal(IGraphics& g, GraphicsRegion canvas)
        '    at ggplot.zzz.plotGGplot(ggplot ggplot, list args, Environment env)
        '    at ggplot.zzz._Closure$__._Lambda$__R1-1(Object a0, list a1, Environment a2)
        '    at SMRUCC.Rsharp.Runtime.Internal.generic.invokeGeneric(list args, Object x, Environment env, String funcName, Type type)
        '    at SMRUCC.Rsharp.Runtime.Internal.generic.invokeGeneric(list args, Object x, Environment env, String funcName)
        '    at SMRUCC.Rsharp.Runtime.Internal.Invokes.graphics.plot(Object graphics, Object args, Environment env)

        '  R# source: Call "plot"(&bar)

        ' graphics.R#_interop::.plot at REnv.dll:line <unknown>
        ' MSI.declare_function.MSI_ion_stat at MSI_single_stat.R:line 91
        ' MSI.call_function."MSI_ion_stat" at MSI_single_stat.R:line 48
        ' MSI.call_function."MSI_ion_stat" at MSI_single_stat.R:line 48
        ' MSI.n/a.else_false at MSI_single_stat.R:line 47
        ' MSI.forloop.for_loop_[1] at MSI_single_stat.R:line 38
        ' MSI.declare_function.MSI_single_stat at MSI_single_stat.R:line 8
        ' MSI.call_function."MSI_single_stat" at checkRegionSigIons.R:line 71
        ' MSI.call_function."MSI_single_stat" at checkRegionSigIons.R:line 71
        ' MSI.n/a.else_false at checkRegionSigIons.R:line 65
        ' MSI.declare_function.checkRegionSigIons at checkRegionSigIons.R:line 1
        ' MSI.call_function."checkRegionSigIons" at biodeep.R:line 23
        ' MSI.call_function."checkRegionSigIons" at biodeep.R:line 23
        ' MSI.n/a.if_closure at biodeep.R:line 21
        ' MSI.n/a.if_closure at biodeep.R:line 21
        ' MSI.n/a.else_false at biodeep.R:line 11
        ' MSI.declare_function.RunBioDeep at biodeep.R:line 7
        ' MSI.call_function."RunBioDeep" at workflow.R:line 36
        ' MSI.call_function."RunBioDeep" at workflow.R:line 36
        ' MSI.call_function."RunDataVisualization" at workflow.R:line 39
        ' MSI.call_function."RunDataVisualization" at workflow.R:line 39
        ' MSI.call_function."RunMSImaging" at workflow.R:line 43
        ' MSI.call_function."RunMSImaging" at workflow.R:line 43
        ' MSI.call_function."autoreport" at workflow.R:line 45
        ' MSI.call_function."autoreport" at workflow.R:line 45
        ' MSI.declare_function..workflow at workflow.R:line 8
        ' MSI.call_function.".workflow" at MSI_analysis.R:line 269
        ' MSI.call_function.".workflow" at MSI_analysis.R:line 269
        ' MSI.declare_function.RunAnalysis at MSI_analysis.R:line 49
        ' SMRUCC/R#.call_function."RunAnalysis" at MSI_analysis.R:line 663
        ' SMRUCC/R#.call_function."RunAnalysis" at MSI_analysis.R:line 663
        ' SMRUCC/R#.n/a.InitializeEnvironment at MSI_analysis.R:line 0
        ' SMRUCC/R#.global.<globalEnvironment> at <globalEnvironment>:line n/a

        row = If(n - 1 >= matrix.Length, matrix.Last, matrix(n - 1))
        criticalNumber = If(d - 1 >= row.Length, row.Last, row(d - 1))

        Return criticalNumber
    End Function

    Public Overrides Function ToString() As String
        Return $"These F({type}) distribution tables are adapted from: http://www.socr.ucla.edu/applets.dir/f_table.html"
    End Function

    ''' <summary>
    ''' Row = numerator lookup
    ''' </summary>
    ''' <param name="actualNumerator"></param>
    ''' <returns></returns>
    Protected Friend Shared Function getRowIndex(actualNumerator As Integer) As Integer
        Dim chosen = -1

        For i As Integer = 0 To numerators.Length - 1
            If actualNumerator = numerators(i) Then
                chosen = numerators(i)
            ElseIf actualNumerator > numerators(i) Then
                Try
                    chosen = numerators(i + 1)
                Catch __unusedIndexOutOfRangeException1__ As IndexOutOfRangeException
                    ' set to infinity. I.e., the last possible option.
                    chosen = numerators(numerators.Length - 1)
                End Try
            End If
        Next

        Return chosen
    End Function

    ''' <summary>
    ''' Col = denominator lookup
    ''' </summary>
    ''' <param name="actualNumerator"></param>
    ''' <returns></returns>
    Protected Friend Shared Function getColIndex(actualNumerator As Integer) As Integer
        Dim choosen = -1

        For i As Integer = 0 To denominators.Length - 1
            If actualNumerator = denominators(i) Then
                choosen = denominators(i)
            ElseIf actualNumerator > denominators(i) Then
                Try
                    choosen = denominators(i + 1)
                Catch __unusedIndexOutOfRangeException1__ As IndexOutOfRangeException
                    ' set to infinity. I.e., the last possible option.
                    choosen = denominators(denominators.Length - 1)
                End Try
            End If
        Next

        Return choosen
    End Function
End Class
