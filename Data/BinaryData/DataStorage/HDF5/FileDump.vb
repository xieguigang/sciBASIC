#Region "Microsoft.VisualBasic::f1b87093e3dae4b826410dffe724fc66, Data\BinaryData\DataStorage\HDF5\FileDump.vb"

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

    '     Module FileDump
    ' 
    '         Sub: CreateFileDump
    ' 
    '     Interface IFileDump
    ' 
    '         Sub: printValues
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text

Namespace HDF5

    <HideModuleName> Public Module FileDump

        <Extension>
        Public Sub CreateFileDump(obj As IFileDump, out As TextWriter)
            Call obj.printValues(out)
        End Sub
    End Module

    Public Interface IFileDump

        ''' <summary>
        ''' 可以通过下面的两种方法构建出所需要的<paramref name="console"/>参数
        ''' 
        ''' + <see cref="StringBuilder"/> => new <see cref="TextWriter"/>
        ''' + <see cref="StreamWriter"/>
        ''' </summary>
        ''' <param name="console"></param>
        Sub printValues(console As TextWriter)
    End Interface
End Namespace
