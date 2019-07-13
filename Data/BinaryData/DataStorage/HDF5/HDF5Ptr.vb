#Region "Microsoft.VisualBasic::fcec78a1669fac9f6156214513ecacbf, Data\BinaryData\DataStorage\HDF5\HDF5Ptr.vb"

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

    '     Class HDF5Ptr
    ' 
    '         Properties: address
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO

Namespace HDF5

    ''' <summary>
    ''' A internal pointer in a hdf5 binary data file.
    ''' </summary>
    Public MustInherit Class HDF5Ptr : Implements IFileDump

        ''' <summary>
        ''' 当前的这个对象在文件之中的起始位置
        ''' </summary>
        Protected m_address&

        ''' <summary>
        ''' 获取当前的这个对象在文件之中的起始位置
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property address As Long
            Get
                Return m_address
            End Get
        End Property

        Sub New(address&)
            Me.m_address = address
        End Sub

        Public Overrides Function ToString() As String
            Return $"&{address} {Me.GetType.Name}"
        End Function

        Protected Friend MustOverride Sub printValues(console As TextWriter) Implements IFileDump.printValues

    End Class
End Namespace
