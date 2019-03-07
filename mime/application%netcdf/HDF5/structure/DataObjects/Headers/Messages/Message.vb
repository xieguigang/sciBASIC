Namespace HDF5.[Structure]

    ''' <summary>
    ''' Data object header messages are small pieces of metadata that are stored in the 
    ''' data object header for each object in an HDF5 file. Data object header messages 
    ''' provide the metadata required to describe an object and its contents, as well as 
    ''' optional pieces of metadata that annotate the meaning or purpose of the object.
    '''
    ''' Data object header messages are either stored directly in the data object header 
    ''' for the object Or are shared between multiple objects in the file. When a message 
    ''' Is shared, a flag in the Message Flags indicates that the actual Message Data 
    ''' portion of that message Is stored in another location (such as another data object 
    ''' header, Or a heap in the file) And the Message Data field contains the information 
    ''' needed to locate the actual information for the message.
    ''' </summary>
    Public MustInherit Class Message

    End Class
End Namespace