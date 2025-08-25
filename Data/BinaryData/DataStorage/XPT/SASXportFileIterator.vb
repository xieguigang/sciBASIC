Imports System.IO
Imports Microsoft.VisualBasic.Data.IO.Xpt.Types

Namespace Xpt

    ''' <summary>
    ''' SAS XPT file reader
    ''' </summary>
    ''' <remarks>
    ''' SAS XPT (SAS Transport File) is a standardized file format primarily used for transferring data 
    ''' between different SAS software versions and systems, particularly in regulated environments 
    ''' like clinical trials. Here are the key aspects based on the search results:
    ''' 
    ''' ### 1. **Core Purpose and Functionality**  
    ''' 
    ''' XPT files enable cross-version compatibility for SAS datasets, resolving issues when data is 
    ''' shared between different SAS installations (e.g., SAS 9 and SAS Viya). This ensures seamless 
    ''' data exchange without version conflicts .  
    ''' 
    ''' ### 2. **Structure and Naming Conventions**  
    ''' 
    ''' - Each XPT file corresponds to a single dataset.  
    ''' - The dataset name must match the XPT filename exactly.  
    ''' - Files use a standardized extension (e.g., `.xpt`), as specified in regulatory guidelines like 
    '''   the *Drug Clinical Trial Data Submission Guideline* .  
    '''   
    ''' ### 3. **Regulatory and Industry Applications**  
    ''' 
    ''' XPT is the mandated format for submitting clinical trial data to regulatory agencies (e.g., 
    ''' FDA, NMPA). Its structured format ensures data integrity and consistency during reviews .  
    ''' 
    ''' ### 4. **Interoperability with Other Tools**  
    ''' 
    ''' Beyond SAS, XPT files can be processed in other programming environments. For example, in R, 
    ''' the `Hmisc` package's `sasxport.get` function imports XPT data for analysis, demonstrating 
    ''' cross-platform utility .  
    ''' 
    ''' ### 5. **Key Advantages**  
    ''' 
    ''' - **Standardization**: Uniform structure simplifies data validation and regulatory submissions.  
    ''' - **Compatibility**: Eliminates barriers between SAS versions and external systems.  
    ''' - **Efficiency**: Streamlines data transfer in large-scale studies (e.g., pharmaceutical trials) .  
    ''' 
    ''' </remarks>
    Public Class SASXportFileIterator : Inherits SASXportConverter

        Private crecord As IEnumerable(Of Object)
        Private cPrimitiveRecord As IList(Of ReadstatValue)
        Private crow As Byte() = Nothing

        Public Sub New(fileName As String)
            MyBase.New(fileName)
        End Sub

        Public Sub New([is] As Stream)
            MyBase.New([is])
        End Sub

        Public Sub New(fileName As String, offset As Integer)
            Me.New(fileName)
            MyBase.seek(offset)
        End Sub

        Public Overridable Function hasNext() As Boolean
            Return Not MyBase.Done
        End Function

        ''' <summary>
        ''' read the data frame line by line
        ''' </summary>
        ''' <returns></returns>
        Public Overridable Function [next]() As IEnumerable(Of Object)
            crecord = MyBase.Record
            cPrimitiveRecord = MyBase.PrimitiveRecord
            crow = MyBase.Row

            MyBase.readNextRecord()

            Return crecord
        End Function

        Public Overridable Function nextPrimitive() As IList(Of ReadstatValue)
            [next]()
            Return cPrimitiveRecord
        End Function

        Public Overridable Function nextRaw() As Byte()
            [next]()
            Return crow
        End Function
    End Class

End Namespace
