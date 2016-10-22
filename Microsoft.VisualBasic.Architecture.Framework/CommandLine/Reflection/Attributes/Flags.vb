Imports System.ComponentModel

Namespace CommandLine.Reflection

    Public Enum PipelineTypes

        undefined
        ''' <summary>
        ''' This argument can accept the std_out from upstream app as input
        ''' </summary>
        <Description("(This argument can accept the std_out from upstream app as input)")>
        std_in
        ''' <summary>
        ''' This argument can output data to std_out
        ''' </summary>
        <Description("(This argument can output data to std_out)")>
        std_out
    End Enum
End Namespace