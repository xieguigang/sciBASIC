#Region "Microsoft.VisualBasic::1dcfe98595b59fadc05eff88da28068f, core.test"

    ' Author:
    ' 
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 


    ' Source file summaries:

    ' Class ParallelLoadingTest
    ' 
    '     Properties: ddddd, fffd
    ' 
    '     Function: Load
    ' 
    ' 

#End Region

#Region "Microsoft.VisualBasic::b493f08649bb3120eb1d418929d10925, core.test"

    ' Author:
    ' 
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 


    ' Source file summaries:

    ' Class ParallelLoadingTest
    ' 
    '     Properties: ddddd, fffd
    ' 
    ' 
    '     Function: Load
    ' 
    ' 
    ' 

#End Region

<Serializable>
Public Class ParallelLoadingTest

    Public Property ddddd As String
    Public Property fffd As Date

    <Microsoft.VisualBasic.Parallel.ParallelLoading.LoadEntry>
    Public Shared Function Load(path As String) As ParallelLoadingTest()
        Call Threading.Thread.Sleep(10 * 1000)
        Return {New ParallelLoadingTest With {.ddddd = Rnd(), .fffd = Now}}
    End Function
End Class

