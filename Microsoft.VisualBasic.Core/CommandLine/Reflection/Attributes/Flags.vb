#Region "Microsoft.VisualBasic::e6ec7bef4c88e217265d0a358da1fd20, Microsoft.VisualBasic.Core\CommandLine\Reflection\Attributes\Flags.vb"

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

    '     Enum PipelineTypes
    ' 
    '         std_in, std_out, undefined
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel

Namespace CommandLine.Reflection

    Public Enum PipelineTypes As Byte

        undefined
        ''' <summary>
        ''' This argument can accept the std_out from upstream app as input
        ''' </summary>
        <Description("(This argument can accept the ``std_out`` from upstream app as input)")>
        std_in
        ''' <summary>
        ''' This argument can output data to std_out
        ''' </summary>
        <Description("(This argument can output data to ``std_out``)")>
        std_out
    End Enum
End Namespace
