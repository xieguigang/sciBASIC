#Region "Microsoft.VisualBasic::d543155fe16ecea3b2d889a417a9c03b, Data_science\MachineLearning\MachineLearning\Darwinism\GeneticAlgorithm\Helper\MemoryDump.vb"

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

    '   Total Lines: 65
    '    Code Lines: 38 (58.46%)
    ' Comment Lines: 14 (21.54%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 13 (20.00%)
    '     File Size: 2.44 KB


    '     Class Memory
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: (+2 Overloads) Dispose, Flush, WriteLine
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Text

Namespace Darwinism.GAF.Helper

    ''' <summary>
    ''' Dump the genetic algorithm evolution process into a text table file, 
    ''' so that the optimization procedure can be reviewed afterwards.
    ''' </summary>
    ''' <remarks>
    ''' The output file is a tabular delimited text file which contains the 
    ''' columns ``Time``, ``iteration``, ``fitness`` and ``chromosome``.
    ''' </remarks>
    Public Class Memory : Implements IDisposable

        Dim writer As StreamWriter

        ''' <summary>
        ''' Open the dump file for appending the evolution records.
        ''' </summary>
        ''' <param name="file">The path of the dump file.</param>
        ''' <remarks>
        ''' The column header line will only be written when the target file 
        ''' does not exists yet.
        ''' </remarks>
        Sub New(file As String)
            Dim writeHeader As Boolean = Not file.FileExists

            writer = New StreamWriter(file.Open(doClear:=False))

            If writeHeader Then
                writer.WriteLine({"Time", "iteration", "fitness", "chromosome"}.JoinBy(vbTab))
            End If
        End Sub

        ''' <summary>
        ''' Append one evolution record into the dump file.
        ''' </summary>
        ''' <param name="iter">The current iteration number.</param>
        ''' <param name="fit">The best fitness value of the current iteration.</param>
        ''' <param name="chromosome">The gene values of the best individual of the current iteration.</param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub WriteLine(iter%, fit#, chromosome As IEnumerable(Of Double))
            Call writer.WriteLine(New String() {Now.ToString, iter, fit, chromosome.JoinBy(", ")}.JoinBy(ASCII.TAB))
        End Sub

        ''' <summary>
        ''' Flush the buffered content into the dump file.
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Flush()
            Call writer.Flush()
        End Sub

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    Call Flush()
                    Call writer.Dispose()
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        ''' <summary>
        ''' Flush the buffered content and then close the dump file.
        ''' </summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            ' TODO: uncomment the following line if Finalize() is overridden above.
            ' GC.SuppressFinalize(Me)
        End Sub
#End Region
    End Class
End Namespace
