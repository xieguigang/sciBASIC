#Region "Microsoft.VisualBasic::f55348a849147486e2689e6ff877cb26, Data_science\DataMining\DataMining\ComponentModel\Extensions.vb"

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

    '     Module Extensions
    ' 
    '         Function: ToEnumsTable
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace ComponentModel

    Public Module Extensions

        <Extension>
        Public Function ToEnumsTable(Of T)(classes As IEnumerable(Of ColorClass)) As Dictionary(Of T, ColorClass)
            Return classes.ToDictionary(Function(c) DirectCast(CObj(c.enumInt), T))
        End Function
    End Module
End Namespace
