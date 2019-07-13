#Region "Microsoft.VisualBasic::4efc758f3f1c45982f3bc04e05a3137a, mime\text%yaml\1.1\Base\YAMLNodeType.vb"

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

    '     Enum YAMLNodeType
    ' 
    '         Mapping, Scalar, Sequence
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Grammar11

    Public Enum YAMLNodeType
        ''' <summary>
        ''' The node is a <see cref="YamlMappingNode"/>.
        ''' </summary>
        Mapping

        ''' <summary>
        ''' The node is a <see cref="YamlScalarNode"/>.
        ''' </summary>
        Scalar

        ''' <summary>
        ''' The node is a <see cref="YamlSequenceNode"/>.
        ''' </summary>
        Sequence
    End Enum
End Namespace
