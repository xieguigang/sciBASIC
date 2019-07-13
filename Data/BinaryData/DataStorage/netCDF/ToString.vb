#Region "Microsoft.VisualBasic::5f2aeac9da2411d56c7f64ebc0bbaace, Data\BinaryData\DataStorage\netCDF\ToString.vb"

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

    '     Module ToStringHelper
    ' 
    '         Sub: toString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.IO.netCDF.Components
Imports Microsoft.VisualBasic.Linq

Namespace netCDF

    ''' <summary>
    ''' CDF file summary
    ''' </summary>
    Module ToStringHelper

        ''' <summary>
        ''' Summary netCDF data
        ''' </summary>
        ''' <param name="file"></param>
        ''' <param name="dev"></param>
        <Extension> Public Sub toString(file As netCDFReader, dev As TextWriter)
            Dim [dim] As Dimension
            Dim summary$
            Dim record = file.recordDimension

            Call dev.WriteLine("DIMENSIONS")

            For Each dimension As SeqValue(Of Dimension) In file.dimensions.SeqIterator
                [dim] = dimension
                summary = $"  [{dimension.i.ToString.PadLeft(2)}] {[dim].name.PadEnd(25)} = size: {[dim].size}"

                If [dim].name = record.name Then
                    summary &= $" [recordDimension, size={record.length}x{record.recordStep}]"
                End If

                Call dev.WriteLine(summary)
            Next

            Call dev.WriteLine()
            Call dev.WriteLine("GLOBAL ATTRIBUTES")
            For Each attribute As attribute In file.globalAttributes
                Call dev.WriteLine($"  {attribute.name.PadEnd(30)} = {attribute.value}")
            Next

            Call dev.WriteLine()
            Call dev.WriteLine("VARIABLES:")
            For Each variable As variable In file.variables
                Dim value As CDFData = file.getDataVariable(variable)
                Dim stringify = value.ToString

                Call dev.WriteLine($"  {variable.name.PadEnd(30)} = {stringify}")
            Next

            Call dev.Flush()
        End Sub
    End Module
End Namespace
