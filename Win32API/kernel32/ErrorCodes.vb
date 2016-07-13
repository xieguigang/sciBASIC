
''' <summary>
''' #### Win32 API error code definitions
''' 
''' This section contains the error code definitions for the Win32 API functions.
''' </summary>
''' <remarks>这些错误代码都已经很老旧了，新的NT系统增添了很多错误代码</remarks>
Public Module ErrorCodes
    ' -----------------------------------------
    ' Win32 API error code definitions
    ' -----------------------------------------
    ' This section contains the error code definitions for the Win32 API functions.

    ' NO_ERROR
    Public Const NO_ERROR As Short = 0 '  dderror

    ' The configuration registry database operation completed successfully.
    Public Const ERROR_SUCCESS As Short = 0

    '   Incorrect function.
    Public Const ERROR_INVALID_FUNCTION As Short = 1 '  dderror

    '   The system cannot find the file specified.
    Public Const ERROR_FILE_NOT_FOUND As Short = 2

    '   The system cannot find the path specified.
    Public Const ERROR_PATH_NOT_FOUND As Short = 3

    '   The system cannot open the file.
    Public Const ERROR_TOO_MANY_OPEN_FILES As Short = 4

    '   Access is denied.
    Public Const ERROR_ACCESS_DENIED As Short = 5

    '   The handle is invalid.
    Public Const ERROR_INVALID_HANDLE As Short = 6

    '   The storage control blocks were destroyed.
    Public Const ERROR_ARENA_TRASHED As Short = 7

    '   Not enough storage is available to process this command.
    Public Const ERROR_NOT_ENOUGH_MEMORY As Short = 8 '  dderror

    '   The storage control block address is invalid.
    Public Const ERROR_INVALID_BLOCK As Short = 9

    '   The environment is incorrect.
    Public Const ERROR_BAD_ENVIRONMENT As Short = 10

    '   An attempt was made to load a program with an
    '   incorrect format.
    Public Const ERROR_BAD_FORMAT As Short = 11

    '   The access code is invalid.
    Public Const ERROR_INVALID_ACCESS As Short = 12

    '   The data is invalid.
    Public Const ERROR_INVALID_DATA As Short = 13

    '   Not enough storage is available to complete this operation.
    Public Const ERROR_OUTOFMEMORY As Short = 14

    '   The system cannot find the drive specified.
    Public Const ERROR_INVALID_DRIVE As Short = 15

    '   The directory cannot be removed.
    Public Const ERROR_CURRENT_DIRECTORY As Short = 16

    '   The system cannot move the file
    '   to a different disk drive.
    Public Const ERROR_NOT_SAME_DEVICE As Short = 17

    '   There are no more files.
    Public Const ERROR_NO_MORE_FILES As Short = 18

    '   The media is write protected.
    Public Const ERROR_WRITE_PROTECT As Short = 19

    '   The system cannot find the device specified.
    Public Const ERROR_BAD_UNIT As Short = 20

    '   The device is not ready.
    Public Const ERROR_NOT_READY As Short = 21

    '   The device does not recognize the command.
    Public Const ERROR_BAD_COMMAND As Short = 22

    '   Data error (cyclic redundancy check)
    Public Const ERROR_CRC As Short = 23

    '   The program issued a command but the
    '   command length is incorrect.
    Public Const ERROR_BAD_LENGTH As Short = 24

    '   The drive cannot locate a specific
    '   area or track on the disk.
    Public Const ERROR_SEEK As Short = 25

    '   The specified disk or diskette cannot be accessed.
    Public Const ERROR_NOT_DOS_DISK As Short = 26

    '   The drive cannot find the sector requested.
    Public Const ERROR_SECTOR_NOT_FOUND As Short = 27

    '   The printer is out of paper.
    Public Const ERROR_OUT_OF_PAPER As Short = 28

    '   The system cannot write to the specified device.
    Public Const ERROR_WRITE_FAULT As Short = 29

    '   The system cannot read from the specified device.
    Public Const ERROR_READ_FAULT As Short = 30

    '   A device attached to the system is not functioning.
    Public Const ERROR_GEN_FAILURE As Short = 31

    '   The process cannot access the file because
    '   it is being used by another process.
    Public Const ERROR_SHARING_VIOLATION As Short = 32

    '   The process cannot access the file because
    '   another process has locked a portion of the file.
    Public Const ERROR_LOCK_VIOLATION As Short = 33

    '   The wrong diskette is in the drive.
    '   Insert %2 (Volume Serial Number: %3)
    '   into drive %1.
    Public Const ERROR_WRONG_DISK As Short = 34

    '   Too many files opened for sharing.
    Public Const ERROR_SHARING_BUFFER_EXCEEDED As Short = 36

    '   Reached end of file.
    Public Const ERROR_HANDLE_EOF As Short = 38

    '   The disk is full.
    Public Const ERROR_HANDLE_DISK_FULL As Short = 39

    '   The network request is not supported.
    Public Const ERROR_NOT_SUPPORTED As Short = 50

    '   The remote computer is not available.
    Public Const ERROR_REM_NOT_LIST As Short = 51

    '   A duplicate name exists on the network.
    Public Const ERROR_DUP_NAME As Short = 52

    '   The network path was not found.
    Public Const ERROR_BAD_NETPATH As Short = 53

    '   The network is busy.
    Public Const ERROR_NETWORK_BUSY As Short = 54

    '   The specified network resource or device is no longer
    '   available.
    Public Const ERROR_DEV_NOT_EXIST As Short = 55 '  dderror

    '   The network BIOS command limit has been reached.
    Public Const ERROR_TOO_MANY_CMDS As Short = 56

    '   A network adapter hardware error occurred.
    Public Const ERROR_ADAP_HDW_ERR As Short = 57

    '   The specified server cannot perform the requested
    '   operation.
    Public Const ERROR_BAD_NET_RESP As Short = 58

    '   An unexpected network error occurred.
    Public Const ERROR_UNEXP_NET_ERR As Short = 59

    '   The remote adapter is not compatible.
    Public Const ERROR_BAD_REM_ADAP As Short = 60

    '   The printer queue is full.
    Public Const ERROR_PRINTQ_FULL As Short = 61

    '   Space to store the file waiting to be printed is
    '   not available on the server.
    Public Const ERROR_NO_SPOOL_SPACE As Short = 62

    '   Your file waiting to be printed was deleted.
    Public Const ERROR_PRINT_CANCELLED As Short = 63

    '   The specified network name is no longer available.
    Public Const ERROR_NETNAME_DELETED As Short = 64

    '   Network access is denied.
    Public Const ERROR_NETWORK_ACCESS_DENIED As Short = 65

    '   The network resource type is not correct.
    Public Const ERROR_BAD_DEV_TYPE As Short = 66

    '   The network name cannot be found.
    Public Const ERROR_BAD_NET_NAME As Short = 67

    '   The name limit for the local computer network
    '   adapter card was exceeded.
    Public Const ERROR_TOO_MANY_NAMES As Short = 68

    '   The network BIOS session limit was exceeded.
    Public Const ERROR_TOO_MANY_SESS As Short = 69

    '   The remote server has been paused or is in the
    '   process of being started.
    Public Const ERROR_SHARING_PAUSED As Short = 70

    '   The network request was not accepted.
    Public Const ERROR_REQ_NOT_ACCEP As Short = 71

    '   The specified printer or disk device has been paused.
    Public Const ERROR_REDIR_PAUSED As Short = 72

    '   The file exists.
    Public Const ERROR_FILE_EXISTS As Short = 80

    '   The directory or file cannot be created.
    Public Const ERROR_CANNOT_MAKE As Short = 82

    '   Fail on INT 24
    Public Const ERROR_FAIL_I24 As Short = 83

    '   Storage to process this request is not available.
    Public Const ERROR_OUT_OF_STRUCTURES As Short = 84

    '   The local device name is already in use.
    Public Const ERROR_ALREADY_ASSIGNED As Short = 85

    '   The specified network password is not correct.
    Public Const ERROR_INVALID_PASSWORD As Short = 86

    '   The parameter is incorrect.
    Public Const ERROR_INVALID_PARAMETER As Short = 87 '  dderror

    '   A write fault occurred on the network.
    Public Const ERROR_NET_WRITE_FAULT As Short = 88

    '   The system cannot start another process at
    '   this time.
    Public Const ERROR_NO_PROC_SLOTS As Short = 89

    '   Cannot create another system semaphore.
    Public Const ERROR_TOO_MANY_SEMAPHORES As Short = 100

    '   The exclusive semaphore is owned by another process.
    Public Const ERROR_EXCL_SEM_ALREADY_OWNED As Short = 101

    '   The semaphore is set and cannot be closed.
    Public Const ERROR_SEM_IS_SET As Short = 102

    '   The semaphore cannot be set again.
    Public Const ERROR_TOO_MANY_SEM_REQUESTS As Short = 103

    '   Cannot request exclusive semaphores at interrupt time.
    Public Const ERROR_INVALID_AT_INTERRUPT_TIME As Short = 104

    '   The previous ownership of this semaphore has ended.
    Public Const ERROR_SEM_OWNER_DIED As Short = 105

    '   Insert the diskette for drive %1.
    Public Const ERROR_SEM_USER_LIMIT As Short = 106

    '   Program stopped because alternate diskette was not inserted.
    Public Const ERROR_DISK_CHANGE As Short = 107

    '   The disk is in use or locked by
    '   another process.
    Public Const ERROR_DRIVE_LOCKED As Short = 108

    '   The pipe has been ended.
    Public Const ERROR_BROKEN_PIPE As Short = 109

    '   The system cannot open the
    '   device or file specified.
    Public Const ERROR_OPEN_FAILED As Short = 110

    '   The file name is too long.
    Public Const ERROR_BUFFER_OVERFLOW As Short = 111

    '   There is not enough space on the disk.
    Public Const ERROR_DISK_FULL As Short = 112

    '   No more internal file identifiers available.
    Public Const ERROR_NO_MORE_SEARCH_HANDLES As Short = 113

    '   The target internal file identifier is incorrect.
    Public Const ERROR_INVALID_TARGET_HANDLE As Short = 114

    '   The IOCTL call made by the application program is
    '   not correct.
    Public Const ERROR_INVALID_CATEGORY As Short = 117

    '   The verify-on-write switch parameter value is not
    '   correct.
    Public Const ERROR_INVALID_VERIFY_SWITCH As Short = 118

    '   The system does not support the command requested.
    Public Const ERROR_BAD_DRIVER_LEVEL As Short = 119

    '   This function is only valid in Windows NT mode.
    Public Const ERROR_CALL_NOT_IMPLEMENTED As Short = 120

    '   The semaphore timeout period has expired.
    Public Const ERROR_SEM_TIMEOUT As Short = 121

    '   The data area passed to a system call is too
    '   small.
    Public Const ERROR_INSUFFICIENT_BUFFER As Short = 122 '  dderror

    '   The filename, directory name, or volume label syntax is incorrect.
    Public Const ERROR_INVALID_NAME As Short = 123

    '   The system call level is not correct.
    Public Const ERROR_INVALID_LEVEL As Short = 124

    '   The disk has no volume label.
    Public Const ERROR_NO_VOLUME_LABEL As Short = 125

    '   The specified module could not be found.
    Public Const ERROR_MOD_NOT_FOUND As Short = 126

    '   The specified procedure could not be found.
    Public Const ERROR_PROC_NOT_FOUND As Short = 127

    '   There are no child processes to wait for.
    Public Const ERROR_WAIT_NO_CHILDREN As Short = 128

    '   The %1 application cannot be run in Windows NT mode.
    Public Const ERROR_CHILD_NOT_COMPLETE As Short = 129

    '   Attempt to use a file handle to an open disk partition for an
    '   operation other than raw disk I/O.
    Public Const ERROR_DIRECT_ACCESS_HANDLE As Short = 130

    '   An attempt was made to move the file pointer before the beginning of the file.
    Public Const ERROR_NEGATIVE_SEEK As Short = 131

    '   The file pointer cannot be set on the specified device or file.
    Public Const ERROR_SEEK_ON_DEVICE As Short = 132

    '   A JOIN or SUBST command
    '   cannot be used for a drive that
    '   contains previously joined drives.
    Public Const ERROR_IS_JOIN_TARGET As Short = 133

    '   An attempt was made to use a
    '   JOIN or SUBST command on a drive that has
    '   already been joined.
    Public Const ERROR_IS_JOINED As Short = 134

    '   An attempt was made to use a
    '   JOIN or SUBST command on a drive that has
    '   already been substituted.
    Public Const ERROR_IS_SUBSTED As Short = 135

    '   The system tried to delete
    '   the JOIN of a drive that is not joined.
    Public Const ERROR_NOT_JOINED As Short = 136

    '   The system tried to delete the
    '   substitution of a drive that is not substituted.
    Public Const ERROR_NOT_SUBSTED As Short = 137

    '   The system tried to join a drive
    '   to a directory on a joined drive.
    Public Const ERROR_JOIN_TO_JOIN As Short = 138

    '   The system tried to substitute a
    '   drive to a directory on a substituted drive.
    Public Const ERROR_SUBST_TO_SUBST As Short = 139

    '   The system tried to join a drive to
    '   a directory on a substituted drive.
    Public Const ERROR_JOIN_TO_SUBST As Short = 140

    '   The system tried to SUBST a drive
    '   to a directory on a joined drive.
    Public Const ERROR_SUBST_TO_JOIN As Short = 141

    '   The system cannot perform a JOIN or SUBST at this time.
    Public Const ERROR_BUSY_DRIVE As Short = 142

    '   The system cannot join or substitute a
    '   drive to or for a directory on the same drive.
    Public Const ERROR_SAME_DRIVE As Short = 143

    '   The directory is not a subdirectory of the root directory.
    Public Const ERROR_DIR_NOT_ROOT As Short = 144

    '   The directory is not empty.
    Public Const ERROR_DIR_NOT_EMPTY As Short = 145

    '   The path specified is being used in
    '   a substitute.
    Public Const ERROR_IS_SUBST_PATH As Short = 146

    '   Not enough resources are available to
    '   process this command.
    Public Const ERROR_IS_JOIN_PATH As Short = 147

    '   The path specified cannot be used at this time.
    Public Const ERROR_PATH_BUSY As Short = 148

    '   An attempt was made to join
    '   or substitute a drive for which a directory
    '   on the drive is the target of a previous
    '   substitute.
    Public Const ERROR_IS_SUBST_TARGET As Short = 149

    '   System trace information was not specified in your
    '   CONFIG.SYS file, or tracing is disallowed.
    Public Const ERROR_SYSTEM_TRACE As Short = 150

    '   The number of specified semaphore events for
    '   DosMuxSemWait is not correct.
    Public Const ERROR_INVALID_EVENT_COUNT As Short = 151

    '   DosMuxSemWait did not execute; too many semaphores
    '   are already set.
    Public Const ERROR_TOO_MANY_MUXWAITERS As Short = 152

    '   The DosMuxSemWait list is not correct.
    Public Const ERROR_INVALID_LIST_FORMAT As Short = 153

    '   The volume label you entered exceeds the
    '   11 character limit.  The first 11 characters were written
    '   to disk.  Any characters that exceeded the 11 character limit
    '   were automatically deleted.
    Public Const ERROR_LABEL_TOO_LONG As Short = 154

    '   Cannot create another thread.
    Public Const ERROR_TOO_MANY_TCBS As Short = 155

    '   The recipient process has refused the signal.
    Public Const ERROR_SIGNAL_REFUSED As Short = 156

    '   The segment is already discarded and cannot be locked.
    Public Const ERROR_DISCARDED As Short = 157

    '   The segment is already unlocked.
    Public Const ERROR_NOT_LOCKED As Short = 158

    '   The address for the thread ID is not correct.
    Public Const ERROR_BAD_THREADID_ADDR As Short = 159

    '   The argument string passed to DosExecPgm is not correct.
    Public Const ERROR_BAD_ARGUMENTS As Short = 160

    '   The specified path is invalid.
    Public Const ERROR_BAD_PATHNAME As Short = 161

    '   A signal is already pending.
    Public Const ERROR_SIGNAL_PENDING As Short = 162

    '   No more threads can be created in the system.
    Public Const ERROR_MAX_THRDS_REACHED As Short = 164

    '   Unable to lock a region of a file.
    Public Const ERROR_LOCK_FAILED As Short = 167

    '   The requested resource is in use.
    Public Const ERROR_BUSY As Short = 170

    '   A lock request was not outstanding for the supplied cancel region.
    Public Const ERROR_CANCEL_VIOLATION As Short = 173

    '   The file system does not support atomic changes to the lock type.
    Public Const ERROR_ATOMIC_LOCKS_NOT_SUPPORTED As Short = 174

    '   The system detected a segment number that was not correct.
    Public Const ERROR_INVALID_SEGMENT_NUMBER As Short = 180

    '   The operating system cannot run %1.
    Public Const ERROR_INVALID_ORDINAL As Short = 182

    '   Cannot create a file when that file already exists.
    Public Const ERROR_ALREADY_EXISTS As Short = 183

    '   The flag passed is not correct.
    Public Const ERROR_INVALID_FLAG_NUMBER As Short = 186

    '   The specified system semaphore name was not found.
    Public Const ERROR_SEM_NOT_FOUND As Short = 187

    '   The operating system cannot run %1.
    Public Const ERROR_INVALID_STARTING_CODESEG As Short = 188

    '   The operating system cannot run %1.
    Public Const ERROR_INVALID_STACKSEG As Short = 189

    '   The operating system cannot run %1.
    Public Const ERROR_INVALID_MODULETYPE As Short = 190

    '   Cannot run %1 in Windows NT mode.
    Public Const ERROR_INVALID_EXE_SIGNATURE As Short = 191

    '   The operating system cannot run %1.
    Public Const ERROR_EXE_MARKED_INVALID As Short = 192

    '   %1 is not a valid Windows NT application.
    Public Const ERROR_BAD_EXE_FORMAT As Short = 193

    '   The operating system cannot run %1.
    Public Const ERROR_ITERATED_DATA_EXCEEDS_64k As Short = 194

    '   The operating system cannot run %1.
    Public Const ERROR_INVALID_MINALLOCSIZE As Short = 195

    '   The operating system cannot run this
    '   application program.
    Public Const ERROR_DYNLINK_FROM_INVALID_RING As Short = 196

    '   The operating system is not presently
    '   configured to run this application.
    Public Const ERROR_IOPL_NOT_ENABLED As Short = 197

    '   The operating system cannot run %1.
    Public Const ERROR_INVALID_SEGDPL As Short = 198

    '   The operating system cannot run this
    '   application program.
    Public Const ERROR_AUTODATASEG_EXCEEDS_64k As Short = 199

    '   The code segment cannot be greater than or equal to 64KB.
    Public Const ERROR_RING2SEG_MUST_BE_MOVABLE As Short = 200

    '   The operating system cannot run %1.
    Public Const ERROR_RELOC_CHAIN_XEEDS_SEGLIM As Short = 201

    '   The operating system cannot run %1.
    Public Const ERROR_INFLOOP_IN_RELOC_CHAIN As Short = 202

    '   The system could not find the environment
    '   option that was entered.
    Public Const ERROR_ENVVAR_NOT_FOUND As Short = 203

    '   No process in the command subtree has a
    '   signal handler.
    Public Const ERROR_NO_SIGNAL_SENT As Short = 205

    '   The filename or extension is too long.
    Public Const ERROR_FILENAME_EXCED_RANGE As Short = 206

    '   The ring 2 stack is in use.
    Public Const ERROR_RING2_STACK_IN_USE As Short = 207

    '   The Global filename characters,  or ?, are entered
    '   incorrectly or too many Global filename characters are specified.
    Public Const ERROR_META_EXPANSION_TOO_LONG As Short = 208

    '   The signal being posted is not correct.
    Public Const ERROR_INVALID_SIGNAL_NUMBER As Short = 209

    '   The signal handler cannot be set.
    Public Const ERROR_THREAD_1_INACTIVE As Short = 210

    '   The segment is locked and cannot be reallocated.
    Public Const ERROR_LOCKED As Short = 212

    '   Too many dynamic link modules are attached to this
    '   program or dynamic link module.
    Public Const ERROR_TOO_MANY_MODULES As Short = 214

    '   Can't nest calls to LoadModule.
    Public Const ERROR_NESTING_NOT_ALLOWED As Short = 215

    '   The pipe state is invalid.
    Public Const ERROR_BAD_PIPE As Short = 230

    '   All pipe instances are busy.
    Public Const ERROR_PIPE_BUSY As Short = 231

    '   The pipe is being closed.
    Public Const ERROR_NO_DATA As Short = 232

    '   No process is on the other end of the pipe.
    Public Const ERROR_PIPE_NOT_CONNECTED As Short = 233

    '   More data is available.
    Public Const ERROR_MORE_DATA As Short = 234 '  dderror

    '   The session was cancelled.
    Public Const ERROR_VC_DISCONNECTED As Short = 240

    '   The specified extended attribute name was invalid.
    Public Const ERROR_INVALID_EA_NAME As Short = 254

    '   The extended attributes are inconsistent.
    Public Const ERROR_EA_LIST_INCONSISTENT As Short = 255

    '   No more data is available.
    Public Const ERROR_NO_MORE_ITEMS As Short = 259

    '   The Copy API cannot be used.
    Public Const ERROR_CANNOT_COPY As Short = 266

    '   The directory name is invalid.
    Public Const ERROR_DIRECTORY As Short = 267

    '   The extended attributes did not fit in the buffer.
    Public Const ERROR_EAS_DIDNT_FIT As Short = 275

    '   The extended attribute file on the mounted file system is corrupt.
    Public Const ERROR_EA_FILE_CORRUPT As Short = 276

    '   The extended attribute table file is full.
    Public Const ERROR_EA_TABLE_FULL As Short = 277

    '   The specified extended attribute handle is invalid.
    Public Const ERROR_INVALID_EA_HANDLE As Short = 278

    '   The mounted file system does not support extended attributes.
    Public Const ERROR_EAS_NOT_SUPPORTED As Short = 282

    '   Attempt to release mutex not owned by caller.
    Public Const ERROR_NOT_OWNER As Short = 288

    '   Too many posts were made to a semaphore.
    Public Const ERROR_TOO_MANY_POSTS As Short = 298

    '   The system cannot find message for message number 0x%1
    '   in message file for %2.
    Public Const ERROR_MR_MID_NOT_FOUND As Short = 317

    '   Attempt to access invalid address.
    Public Const ERROR_INVALID_ADDRESS As Short = 487

    '   Arithmetic result exceeded 32 bits.
    Public Const ERROR_ARITHMETIC_OVERFLOW As Short = 534

    '   There is a process on other end of the pipe.
    Public Const ERROR_PIPE_CONNECTED As Short = 535

    '   Waiting for a process to open the other end of the pipe.
    Public Const ERROR_PIPE_LISTENING As Short = 536

    '   Access to the extended attribute was denied.
    Public Const ERROR_EA_ACCESS_DENIED As Short = 994

    '   The I/O operation has been aborted because of either a thread exit
    '   or an application request.
    Public Const ERROR_OPERATION_ABORTED As Short = 995

    '   Overlapped I/O event is not in a signalled state.
    Public Const ERROR_IO_INCOMPLETE As Short = 996

    '   Overlapped I/O operation is in progress.
    Public Const ERROR_IO_PENDING As Short = 997 '  dderror

    '   Invalid access to memory location.
    Public Const ERROR_NOACCESS As Short = 998

    '   Error performing inpage operation.
    Public Const ERROR_SWAPERROR As Short = 999

    '   Recursion too deep, stack overflowed.
    Public Const ERROR_STACK_OVERFLOW As Short = 1001

    '   The window cannot act on the sent message.
    Public Const ERROR_INVALID_MESSAGE As Short = 1002

    '   Cannot complete this function.
    Public Const ERROR_CAN_NOT_COMPLETE As Short = 1003

    '   Invalid flags.
    Public Const ERROR_INVALID_FLAGS As Short = 1004

    '   The volume does not contain a recognized file system.
    '   Please make sure that all required file system drivers are loaded and that the
    '   volume is not corrupt.
    Public Const ERROR_UNRECOGNIZED_VOLUME As Short = 1005

    '   The volume for a file has been externally altered such that the
    '   opened file is no longer valid.
    Public Const ERROR_FILE_INVALID As Short = 1006

    '   The requested operation cannot be performed in full-screen mode.
    Public Const ERROR_FULLSCREEN_MODE As Short = 1007

    '   An attempt was made to reference a token that does not exist.
    Public Const ERROR_NO_TOKEN As Short = 1008

    '   The configuration registry database is corrupt.
    Public Const ERROR_BADDB As Short = 1009

    '   The configuration registry key is invalid.
    Public Const ERROR_BADKEY As Short = 1010

    '   The configuration registry key could not be opened.
    Public Const ERROR_CANTOPEN As Short = 1011

    '   The configuration registry key could not be read.
    Public Const ERROR_CANTREAD As Short = 1012

    '   The configuration registry key could not be written.
    Public Const ERROR_CANTWRITE As Short = 1013

    '   One of the files in the Registry database had to be recovered
    '   by use of a log or alternate copy.  The recovery was successful.
    Public Const ERROR_REGISTRY_RECOVERED As Short = 1014

    '   The Registry is corrupt. The Public Structure of one of the files that contains
    '   Registry data is corrupt, or the system's image of the file in memory
    '   is corrupt, or the file could not be recovered because the alternate
    '   copy or log was absent or corrupt.
    Public Const ERROR_REGISTRY_CORRUPT As Short = 1015

    '   An I/O operation initiated by the Registry failed unrecoverably.
    '   The Registry could not read in, or write out, or flush, one of the files
    '   that contain the system's image of the Registry.
    Public Const ERROR_REGISTRY_IO_FAILED As Short = 1016

    '   The system has attempted to load or restore a file into the Registry, but the
    '   specified file is not in a Registry file format.
    Public Const ERROR_NOT_REGISTRY_FILE As Short = 1017

    '   Illegal operation attempted on a Registry key which has been marked for deletion.
    Public Const ERROR_KEY_DELETED As Short = 1018

    '   System could not allocate the required space in a Registry log.
    Public Const ERROR_NO_LOG_SPACE As Short = 1019

    '   Cannot create a symbolic link in a Registry key that already
    '   has subkeys or values.
    Public Const ERROR_KEY_HAS_CHILDREN As Short = 1020

    '   Cannot create a stable subkey under a volatile parent key.
    Public Const ERROR_CHILD_MUST_BE_VOLATILE As Short = 1021

    '   A notify change request is being completed and the information
    '   is not being returned in the caller's buffer. The caller now
    '   needs to enumerate the files to find the changes.
    Public Const ERROR_NOTIFY_ENUM_DIR As Short = 1022

    '   A stop control has been sent to a service which other running services
    '   are dependent on.
    Public Const ERROR_DEPENDENT_SERVICES_RUNNING As Short = 1051

    '   The requested control is not valid for this service
    Public Const ERROR_INVALID_SERVICE_CONTROL As Short = 1052

    '   The service did not respond to the start or control request in a timely
    '   fashion.
    Public Const ERROR_SERVICE_REQUEST_TIMEOUT As Short = 1053

    '   A thread could not be created for the service.
    Public Const ERROR_SERVICE_NO_THREAD As Short = 1054

    '   The service database is locked.
    Public Const ERROR_SERVICE_DATABASE_LOCKED As Short = 1055

    '   An instance of the service is already running.
    Public Const ERROR_SERVICE_ALREADY_RUNNING As Short = 1056

    '   The account name is invalid or does not exist.
    Public Const ERROR_INVALID_SERVICE_ACCOUNT As Short = 1057

    '   The specified service is disabled and cannot be started.
    Public Const ERROR_SERVICE_DISABLED As Short = 1058

    '   Circular service dependency was specified.
    Public Const ERROR_CIRCULAR_DEPENDENCY As Short = 1059

    '   The specified service does not exist as an installed service.
    Public Const ERROR_SERVICE_DOES_NOT_EXIST As Short = 1060

    '   The service cannot accept control messages at this time.
    Public Const ERROR_SERVICE_CANNOT_ACCEPT_CTRL As Short = 1061

    '   The service has not been started.
    Public Const ERROR_SERVICE_NOT_ACTIVE As Short = 1062

    '   The service process could not connect to the service controller.
    Public Const ERROR_FAILED_SERVICE_CONTROLLER_CONNECT As Short = 1063

    '   An exception occurred in the service when handling the control request.
    Public Const ERROR_EXCEPTION_IN_SERVICE As Short = 1064

    '   The database specified does not exist.
    Public Const ERROR_DATABASE_DOES_NOT_EXIST As Short = 1065

    '   The service has returned a service-specific error code.
    Public Const ERROR_SERVICE_SPECIFIC_ERROR As Short = 1066

    '   The process terminated unexpectedly.
    Public Const ERROR_PROCESS_ABORTED As Short = 1067

    '   The dependency service or group failed to start.
    Public Const ERROR_SERVICE_DEPENDENCY_FAIL As Short = 1068

    '   The service did not start due to a logon failure.
    Public Const ERROR_SERVICE_LOGON_FAILED As Short = 1069

    '   After starting, the service hung in a start-pending state.
    Public Const ERROR_SERVICE_START_HANG As Short = 1070

    '   The specified service database lock is invalid.
    Public Const ERROR_INVALID_SERVICE_LOCK As Short = 1071

    '   The specified service has been marked for deletion.
    Public Const ERROR_SERVICE_MARKED_FOR_DELETE As Short = 1072

    '   The specified service already exists.
    Public Const ERROR_SERVICE_EXISTS As Short = 1073

    '   The system is currently running with the last-known-good configuration.
    Public Const ERROR_ALREADY_RUNNING_LKG As Short = 1074

    '   The dependency service does not exist or has been marked for
    '   deletion.
    Public Const ERROR_SERVICE_DEPENDENCY_DELETED As Short = 1075

    '   The current boot has already been accepted for use as the
    '   last-known-good control set.
    Public Const ERROR_BOOT_ALREADY_ACCEPTED As Short = 1076

    '   No attempts to start the service have been made since the last boot.
    Public Const ERROR_SERVICE_NEVER_STARTED As Short = 1077

    '   The name is already in use as either a service name or a service display
    '   name.
    Public Const ERROR_DUPLICATE_SERVICE_NAME As Short = 1078

    '   The physical end of the tape has been reached.
    Public Const ERROR_END_OF_MEDIA As Short = 1100

    '   A tape access reached a filemark.
    Public Const ERROR_FILEMARK_DETECTED As Short = 1101

    '   Beginning of tape or partition was encountered.
    Public Const ERROR_BEGINNING_OF_MEDIA As Short = 1102

    '   A tape access reached the end of a set of files.
    Public Const ERROR_SETMARK_DETECTED As Short = 1103

    '   No more data is on the tape.
    Public Const ERROR_NO_DATA_DETECTED As Short = 1104

    '   Tape could not be partitioned.
    Public Const ERROR_PARTITION_FAILURE As Short = 1105

    '   When accessing a new tape of a multivolume partition, the current
    '   blocksize is incorrect.
    Public Const ERROR_INVALID_BLOCK_LENGTH As Short = 1106

    '   Tape partition information could not be found when loading a tape.
    Public Const ERROR_DEVICE_NOT_PARTITIONED As Short = 1107

    '   Unable to lock the media eject mechanism.
    Public Const ERROR_UNABLE_TO_LOCK_MEDIA As Short = 1108

    '   Unable to unload the media.
    Public Const ERROR_UNABLE_TO_UNLOAD_MEDIA As Short = 1109

    '   Media in drive may have changed.
    Public Const ERROR_MEDIA_CHANGED As Short = 1110

    '   The I/O bus was reset.
    Public Const ERROR_BUS_RESET As Short = 1111

    '   No media in drive.
    Public Const ERROR_NO_MEDIA_IN_DRIVE As Short = 1112

    '   No mapping for the Unicode character exists in the target multi-byte code page.
    Public Const ERROR_NO_UNICODE_TRANSLATION As Short = 1113

    '   A dynamic link library (DLL) initialization routine failed.
    Public Const ERROR_DLL_INIT_FAILED As Short = 1114

    '   A system shutdown is in progress.
    Public Const ERROR_SHUTDOWN_IN_PROGRESS As Short = 1115

    '   Unable to abort the system shutdown because no shutdown was in progress.
    Public Const ERROR_NO_SHUTDOWN_IN_PROGRESS As Short = 1116

    '   The request could not be performed because of an I/O device error.
    Public Const ERROR_IO_DEVICE As Short = 1117

    '   No serial device was successfully initialized.  The serial driver will unload.
    Public Const ERROR_SERIAL_NO_DEVICE As Short = 1118

    '   Unable to open a device that was sharing an interrupt request (IRQ)
    '   with other devices. At least one other device that uses that IRQ
    '   was already opened.
    Public Const ERROR_IRQ_BUSY As Short = 1119

    '   A serial I/O operation was completed by another write to the serial port.
    '   (The IOCTL_SERIAL_XOFF_COUNTER reached zero.)
    Public Const ERROR_MORE_WRITES As Short = 1120

    '   A serial I/O operation completed because the time-out period expired.
    '   (The IOCTL_SERIAL_XOFF_COUNTER did not reach zero.)
    Public Const ERROR_COUNTER_TIMEOUT As Short = 1121

    '   No ID address mark was found on the floppy disk.
    Public Const ERROR_FLOPPY_ID_MARK_NOT_FOUND As Short = 1122

    '   Mismatch between the floppy disk sector ID field and the floppy disk
    '   controller track address.
    Public Const ERROR_FLOPPY_WRONG_CYLINDER As Short = 1123

    '   The floppy disk controller reported an error that is not recognized
    '   by the floppy disk driver.
    Public Const ERROR_FLOPPY_UNKNOWN_ERROR As Short = 1124

    '   The floppy disk controller returned inconsistent results in its registers.
    Public Const ERROR_FLOPPY_BAD_REGISTERS As Short = 1125

    '   While accessing the hard disk, a recalibrate operation failed, even after retries.
    Public Const ERROR_DISK_RECALIBRATE_FAILED As Short = 1126

    '   While accessing the hard disk, a disk operation failed even after retries.
    Public Const ERROR_DISK_OPERATION_FAILED As Short = 1127

    '   While accessing the hard disk, a disk controller reset was needed, but
    '   even that failed.
    Public Const ERROR_DISK_RESET_FAILED As Short = 1128

    '   Physical end of tape encountered.
    Public Const ERROR_EOM_OVERFLOW As Short = 1129

    '   Not enough server storage is available to process this command.
    Public Const ERROR_NOT_ENOUGH_SERVER_MEMORY As Short = 1130

    '   A potential deadlock condition has been detected.
    Public Const ERROR_POSSIBLE_DEADLOCK As Short = 1131

    '   The base address or the file offset specified does not have the proper
    '   alignment.
    Public Const ERROR_MAPPED_ALIGNMENT As Short = 1132

    ' NEW for Win32
    Public Const ERROR_INVALID_PIXEL_FORMAT As Short = 2000
    Public Const ERROR_BAD_DRIVER As Short = 2001
    Public Const ERROR_INVALID_WINDOW_STYLE As Short = 2002
    Public Const ERROR_METAFILE_NOT_SUPPORTED As Short = 2003
    Public Const ERROR_TRANSFORM_NOT_SUPPORTED As Short = 2004
    Public Const ERROR_CLIPPING_NOT_SUPPORTED As Short = 2005
    Public Const ERROR_UNKNOWN_PRINT_MONITOR As Short = 3000
    Public Const ERROR_PRINTER_DRIVER_IN_USE As Short = 3001
    Public Const ERROR_SPOOL_FILE_NOT_FOUND As Short = 3002
    Public Const ERROR_SPL_NO_STARTDOC As Short = 3003
    Public Const ERROR_SPL_NO_ADDJOB As Short = 3004
    Public Const ERROR_PRINT_PROCESSOR_ALREADY_INSTALLED As Short = 3005
    Public Const ERROR_PRINT_MONITOR_ALREADY_INSTALLED As Short = 3006
    Public Const ERROR_WINS_INTERNAL As Short = 4000
    Public Const ERROR_CAN_NOT_DEL_LOCAL_WINS As Short = 4001
    Public Const ERROR_STATIC_INIT As Short = 4002
    Public Const ERROR_INC_BACKUP As Short = 4003
    Public Const ERROR_FULL_BACKUP As Short = 4004
    Public Const ERROR_REC_NON_EXISTENT As Short = 4005
    Public Const ERROR_RPL_NOT_ALLOWED As Short = 4006
    Public Const SEVERITY_SUCCESS As Short = 0
    Public Const SEVERITY_ERROR As Short = 1
    Public Const FACILITY_NT_BIT As Integer = &H10000000
    Public Const NOERROR As Short = 0
    Public Const E_UNEXPECTED As Integer = &H8000FFFF
    Public Const E_NOTIMPL As Integer = &H80004001
    Public Const E_OUTOFMEMORY As Integer = &H8007000E
    Public Const E_INVALIDARG As Integer = &H80070057
    Public Const E_NOINTERFACE As Integer = &H80004002
    Public Const E_POINTER As Integer = &H80004003
    Public Const E_HANDLE As Integer = &H80070006
    Public Const E_ABORT As Integer = &H80004004
    Public Const E_FAIL As Integer = &H80004005
    Public Const E_ACCESSDENIED As Integer = &H80070005
    Public Const CO_E_INIT_TLS As Integer = &H80004006
    Public Const CO_E_INIT_SHARED_ALLOCATOR As Integer = &H80004007
    Public Const CO_E_INIT_MEMORY_ALLOCATOR As Integer = &H80004008
    Public Const CO_E_INIT_CLASS_CACHE As Integer = &H80004009
    Public Const CO_E_INIT_RPC_CHANNEL As Integer = &H8000400A
    Public Const CO_E_INIT_TLS_SET_CHANNEL_CONTROL As Integer = &H8000400B
    Public Const CO_E_INIT_TLS_CHANNEL_CONTROL As Integer = &H8000400C
    Public Const CO_E_INIT_UNACCEPTED_USER_ALLOCATOR As Integer = &H8000400D
    Public Const CO_E_INIT_SCM_MUTEX_EXISTS As Integer = &H8000400E
    Public Const CO_E_INIT_SCM_FILE_MAPPING_EXISTS As Integer = &H8000400F
    Public Const CO_E_INIT_SCM_MAP_VIEW_OF_FILE As Integer = &H80004010
    Public Const CO_E_INIT_SCM_EXEC_FAILURE As Integer = &H80004011
    Public Const CO_E_INIT_ONLY_SINGLE_THREADED As Integer = &H80004012
    Public Const S_OK As Short = &H0S
    Public Const S_FALSE As Short = &H1S
    Public Const OLE_E_FIRST As Integer = &H80040000
    Public Const OLE_E_LAST As Integer = &H800400FF
    Public Const OLE_S_FIRST As Integer = &H40000
    Public Const OLE_S_LAST As Integer = &H400FF
    Public Const OLE_E_OLEVERB As Integer = &H80040000
    Public Const OLE_E_ADVF As Integer = &H80040001
    Public Const OLE_E_ENUM_NOMORE As Integer = &H80040002
    Public Const OLE_E_ADVISENOTSUPPORTED As Integer = &H80040003
    Public Const OLE_E_NOCONNECTION As Integer = &H80040004
    Public Const OLE_E_NOTRUNNING As Integer = &H80040005
    Public Const OLE_E_NOCACHE As Integer = &H80040006
    Public Const OLE_E_BLANK As Integer = &H80040007
    Public Const OLE_E_CLASSDIFF As Integer = &H80040008
    Public Const OLE_E_CANT_GETMONIKER As Integer = &H80040009
    Public Const OLE_E_CANT_BINDTOSOURCE As Integer = &H8004000A
    Public Const OLE_E_STATIC As Integer = &H8004000B
    Public Const OLE_E_PROMPTSAVECANCELLED As Integer = &H8004000C
    Public Const OLE_E_INVALIDRECT As Integer = &H8004000D
    Public Const OLE_E_WRONGCOMPOBJ As Integer = &H8004000E
    Public Const OLE_E_INVALIDHWND As Integer = &H8004000F
    Public Const OLE_E_NOT_INPLACEACTIVE As Integer = &H80040010
    Public Const OLE_E_CANTCONVERT As Integer = &H80040011
    Public Const OLE_E_NOSTORAGE As Integer = &H80040012
    Public Const DV_E_FORMATETC As Integer = &H80040064
    Public Const DV_E_DVTARGETDEVICE As Integer = &H80040065
    Public Const DV_E_STGMEDIUM As Integer = &H80040066
    Public Const DV_E_STATDATA As Integer = &H80040067
    Public Const DV_E_LINDEX As Integer = &H80040068
    Public Const DV_E_TYMED As Integer = &H80040069
    Public Const DV_E_CLIPFORMAT As Integer = &H8004006A
    Public Const DV_E_DVASPECT As Integer = &H8004006B
    Public Const DV_E_DVTARGETDEVICE_SIZE As Integer = &H8004006C
    Public Const DV_E_NOIVIEWOBJECT As Integer = &H8004006D
    Public Const DRAGDROP_E_FIRST As Integer = &H80040100
    Public Const DRAGDROP_E_LAST As Integer = &H8004010F
    Public Const DRAGDROP_S_FIRST As Integer = &H40100
    Public Const DRAGDROP_S_LAST As Integer = &H4010F
    Public Const DRAGDROP_E_NOTREGISTERED As Integer = &H80040100
    Public Const DRAGDROP_E_ALREADYREGISTERED As Integer = &H80040101
    Public Const DRAGDROP_E_INVALIDHWND As Integer = &H80040102
    Public Const CLASSFACTORY_E_FIRST As Integer = &H80040110
    Public Const CLASSFACTORY_E_LAST As Integer = &H8004011F
    Public Const CLASSFACTORY_S_FIRST As Integer = &H40110
    Public Const CLASSFACTORY_S_LAST As Integer = &H4011F
    Public Const CLASS_E_NOAGGREGATION As Integer = &H80040110
    Public Const CLASS_E_CLASSNOTAVAILABLE As Integer = &H80040111
    Public Const MARSHAL_E_FIRST As Integer = &H80040120
    Public Const MARSHAL_E_LAST As Integer = &H8004012F
    Public Const MARSHAL_S_FIRST As Integer = &H40120
    Public Const MARSHAL_S_LAST As Integer = &H4012F
    Public Const DATA_E_FIRST As Integer = &H80040130
    Public Const DATA_E_LAST As Integer = &H8004013F
    Public Const DATA_S_FIRST As Integer = &H40130
    Public Const DATA_S_LAST As Integer = &H4013F
    Public Const VIEW_E_FIRST As Integer = &H80040140
    Public Const VIEW_E_LAST As Integer = &H8004014F
    Public Const VIEW_S_FIRST As Integer = &H40140
    Public Const VIEW_S_LAST As Integer = &H4014F
    Public Const VIEW_E_DRAW As Integer = &H80040140
    Public Const REGDB_E_FIRST As Integer = &H80040150
    Public Const REGDB_E_LAST As Integer = &H8004015F
    Public Const REGDB_S_FIRST As Integer = &H40150
    Public Const REGDB_S_LAST As Integer = &H4015F
    Public Const REGDB_E_READREGDB As Integer = &H80040150
    Public Const REGDB_E_WRITEREGDB As Integer = &H80040151
    Public Const REGDB_E_KEYMISSING As Integer = &H80040152
    Public Const REGDB_E_INVALIDVALUE As Integer = &H80040153
    Public Const REGDB_E_CLASSNOTREG As Integer = &H80040154
    Public Const REGDB_E_IIDNOTREG As Integer = &H80040155
    Public Const CACHE_E_FIRST As Integer = &H80040170
    Public Const CACHE_E_LAST As Integer = &H8004017F
    Public Const CACHE_S_FIRST As Integer = &H40170
    Public Const CACHE_S_LAST As Integer = &H4017F
    Public Const CACHE_E_NOCACHE_UPDATED As Integer = &H80040170
    Public Const OLEOBJ_E_FIRST As Integer = &H80040180
    Public Const OLEOBJ_E_LAST As Integer = &H8004018F
    Public Const OLEOBJ_S_FIRST As Integer = &H40180
    Public Const OLEOBJ_S_LAST As Integer = &H4018F
    Public Const OLEOBJ_E_NOVERBS As Integer = &H80040180
    Public Const OLEOBJ_E_INVALIDVERB As Integer = &H80040181
    Public Const CLIENTSITE_E_FIRST As Integer = &H80040190
    Public Const CLIENTSITE_E_LAST As Integer = &H8004019F
    Public Const CLIENTSITE_S_FIRST As Integer = &H40190
    Public Const CLIENTSITE_S_LAST As Integer = &H4019F
    Public Const INPLACE_E_NOTUNDOABLE As Integer = &H800401A0
    Public Const INPLACE_E_NOTOOLSPACE As Integer = &H800401A1
    Public Const INPLACE_E_FIRST As Integer = &H800401A0
    Public Const INPLACE_E_LAST As Integer = &H800401AF
    Public Const INPLACE_S_FIRST As Integer = &H401A0
    Public Const INPLACE_S_LAST As Integer = &H401AF
    Public Const ENUM_E_FIRST As Integer = &H800401B0
    Public Const ENUM_E_LAST As Integer = &H800401BF
    Public Const ENUM_S_FIRST As Integer = &H401B0
    Public Const ENUM_S_LAST As Integer = &H401BF
    Public Const CONVERT10_E_FIRST As Integer = &H800401C0
    Public Const CONVERT10_E_LAST As Integer = &H800401CF
    Public Const CONVERT10_S_FIRST As Integer = &H401C0
    Public Const CONVERT10_S_LAST As Integer = &H401CF
    Public Const CONVERT10_E_OLESTREAM_GET As Integer = &H800401C0
    Public Const CONVERT10_E_OLESTREAM_PUT As Integer = &H800401C1
    Public Const CONVERT10_E_OLESTREAM_FMT As Integer = &H800401C2
    Public Const CONVERT10_E_OLESTREAM_BITMAP_TO_DIB As Integer = &H800401C3
    Public Const CONVERT10_E_STG_FMT As Integer = &H800401C4
    Public Const CONVERT10_E_STG_NO_STD_STREAM As Integer = &H800401C5
    Public Const CONVERT10_E_STG_DIB_TO_BITMAP As Integer = &H800401C6
    Public Const CLIPBRD_E_FIRST As Integer = &H800401D0
    Public Const CLIPBRD_E_LAST As Integer = &H800401DF
    Public Const CLIPBRD_S_FIRST As Integer = &H401D0
    Public Const CLIPBRD_S_LAST As Integer = &H401DF
    Public Const CLIPBRD_E_CANT_OPEN As Integer = &H800401D0
    Public Const CLIPBRD_E_CANT_EMPTY As Integer = &H800401D1
    Public Const CLIPBRD_E_CANT_SET As Integer = &H800401D2
    Public Const CLIPBRD_E_BAD_DATA As Integer = &H800401D3
    Public Const CLIPBRD_E_CANT_CLOSE As Integer = &H800401D4
    Public Const MK_E_FIRST As Integer = &H800401E0
    Public Const MK_E_LAST As Integer = &H800401EF
    Public Const MK_S_FIRST As Integer = &H401E0
    Public Const MK_S_LAST As Integer = &H401EF
    Public Const MK_E_CONNECTMANUALLY As Integer = &H800401E0
    Public Const MK_E_EXCEEDEDDEADLINE As Integer = &H800401E1
    Public Const MK_E_NEEDGENERIC As Integer = &H800401E2
    Public Const MK_E_UNAVAILABLE As Integer = &H800401E3
    Public Const MK_E_SYNTAX As Integer = &H800401E4
    Public Const MK_E_NOOBJECT As Integer = &H800401E5
    Public Const MK_E_INVALIDEXTENSION As Integer = &H800401E6
    Public Const MK_E_INTERMEDIATEINTERFACENOTSUPPORTED As Integer = &H800401E7
    Public Const MK_E_NOTBINDABLE As Integer = &H800401E8
    Public Const MK_E_NOTBOUND As Integer = &H800401E9
    Public Const MK_E_CANTOPENFILE As Integer = &H800401EA
    Public Const MK_E_MUSTBOTHERUSER As Integer = &H800401EB
    Public Const MK_E_NOINVERSE As Integer = &H800401EC
    Public Const MK_E_NOSTORAGE As Integer = &H800401ED
    Public Const MK_E_NOPREFIX As Integer = &H800401EE
    Public Const MK_E_ENUMERATION_FAILED As Integer = &H800401EF
    Public Const CO_E_FIRST As Integer = &H800401F0
    Public Const CO_E_LAST As Integer = &H800401FF
    Public Const CO_S_FIRST As Integer = &H401F0
    Public Const CO_S_LAST As Integer = &H401FF
    Public Const CO_E_NOTINITIALIZED As Integer = &H800401F0
    Public Const CO_E_ALREADYINITIALIZED As Integer = &H800401F1
    Public Const CO_E_CANTDETERMINECLASS As Integer = &H800401F2
    Public Const CO_E_CLASSSTRING As Integer = &H800401F3
    Public Const CO_E_IIDSTRING As Integer = &H800401F4
    Public Const CO_E_APPNOTFOUND As Integer = &H800401F5
    Public Const CO_E_APPSINGLEUSE As Integer = &H800401F6
    Public Const CO_E_ERRORINAPP As Integer = &H800401F7
    Public Const CO_E_DLLNOTFOUND As Integer = &H800401F8
    Public Const CO_E_ERRORINDLL As Integer = &H800401F9
    Public Const CO_E_WRONGOSFORAPP As Integer = &H800401FA
    Public Const CO_E_OBJNOTREG As Integer = &H800401FB
    Public Const CO_E_OBJISREG As Integer = &H800401FC
    Public Const CO_E_OBJNOTCONNECTED As Integer = &H800401FD
    Public Const CO_E_APPDIDNTREG As Integer = &H800401FE
    Public Const CO_E_RELEASED As Integer = &H800401FF
    Public Const OLE_S_USEREG As Integer = &H40000
    Public Const OLE_S_STATIC As Integer = &H40001
    Public Const OLE_S_MAC_CLIPFORMAT As Integer = &H40002
    Public Const DRAGDROP_S_DROP As Integer = &H40100
    Public Const DRAGDROP_S_CANCEL As Integer = &H40101
    Public Const DRAGDROP_S_USEDEFAULTCURSORS As Integer = &H40102
    Public Const DATA_S_SAMEFORMATETC As Integer = &H40130
    Public Const VIEW_S_ALREADY_FROZEN As Integer = &H40140
    Public Const CACHE_S_FORMATETC_NOTSUPPORTED As Integer = &H40170
    Public Const CACHE_S_SAMECACHE As Integer = &H40171
    Public Const CACHE_S_SOMECACHES_NOTUPDATED As Integer = &H40172
    Public Const OLEOBJ_S_INVALIDVERB As Integer = &H40180
    Public Const OLEOBJ_S_CANNOT_DOVERB_NOW As Integer = &H40181
    Public Const OLEOBJ_S_INVALIDHWND As Integer = &H40182
    Public Const INPLACE_S_TRUNCATED As Integer = &H401A0
    Public Const CONVERT10_S_NO_PRESENTATION As Integer = &H401C0
    Public Const MK_S_REDUCED_TO_SELF As Integer = &H401E2
    Public Const MK_S_ME As Integer = &H401E4
    Public Const MK_S_HIM As Integer = &H401E5
    Public Const MK_S_US As Integer = &H401E6
    Public Const MK_S_MONIKERALREADYREGISTERED As Integer = &H401E7
    Public Const CO_E_CLASS_CREATE_FAILED As Integer = &H80080001
    Public Const CO_E_SCM_ERROR As Integer = &H80080002
    Public Const CO_E_SCM_RPC_FAILURE As Integer = &H80080003
    Public Const CO_E_BAD_PATH As Integer = &H80080004
    Public Const CO_E_SERVER_EXEC_FAILURE As Integer = &H80080005
    Public Const CO_E_OBJSRV_RPC_FAILURE As Integer = &H80080006
    Public Const MK_E_NO_NORMALIZED As Integer = &H80080007
    Public Const CO_E_SERVER_STOPPING As Integer = &H80080008
    Public Const MEM_E_INVALID_ROOT As Integer = &H80080009
    Public Const MEM_E_INVALID_LINK As Integer = &H80080010
    Public Const MEM_E_INVALID_SIZE As Integer = &H80080011
    Public Const DISP_E_UNKNOWNINTERFACE As Integer = &H80020001
    Public Const DISP_E_MEMBERNOTFOUND As Integer = &H80020003
    Public Const DISP_E_PARAMNOTFOUND As Integer = &H80020004
    Public Const DISP_E_TYPEMISMATCH As Integer = &H80020005
    Public Const DISP_E_UNKNOWNNAME As Integer = &H80020006
    Public Const DISP_E_NONAMEDARGS As Integer = &H80020007
    Public Const DISP_E_BADVARTYPE As Integer = &H80020008
    Public Const DISP_E_EXCEPTION As Integer = &H80020009
    Public Const DISP_E_OVERFLOW As Integer = &H8002000A
    Public Const DISP_E_BADINDEX As Integer = &H8002000B
    Public Const DISP_E_UNKNOWNLCID As Integer = &H8002000C
    Public Const DISP_E_ARRAYISLOCKED As Integer = &H8002000D
    Public Const DISP_E_BADPARAMCOUNT As Integer = &H8002000E
    Public Const DISP_E_PARAMNOTOPTIONAL As Integer = &H8002000F
    Public Const DISP_E_BADCALLEE As Integer = &H80020010
    Public Const DISP_E_NOTACOLLECTION As Integer = &H80020011
    Public Const TYPE_E_BUFFERTOOSMALL As Integer = &H80028016
    Public Const TYPE_E_INVDATAREAD As Integer = &H80028018
    Public Const TYPE_E_UNSUPFORMAT As Integer = &H80028019
    Public Const TYPE_E_REGISTRYACCESS As Integer = &H8002801C
    Public Const TYPE_E_LIBNOTREGISTERED As Integer = &H8002801D
    Public Const TYPE_E_UNDEFINEDTYPE As Integer = &H80028027
    Public Const TYPE_E_QUALIFIEDNAMEDISALLOWED As Integer = &H80028028
    Public Const TYPE_E_INVALIDSTATE As Integer = &H80028029
    Public Const TYPE_E_WRONGTYPEKIND As Integer = &H8002802A
    Public Const TYPE_E_ELEMENTNOTFOUND As Integer = &H8002802B
    Public Const TYPE_E_AMBIGUOUSNAME As Integer = &H8002802C
    Public Const TYPE_E_NAMECONFLICT As Integer = &H8002802D
    Public Const TYPE_E_UNKNOWNLCID As Integer = &H8002802E
    Public Const TYPE_E_DLLFUNCTIONNOTFOUND As Integer = &H8002802F
    Public Const TYPE_E_BADMODULEKIND As Integer = &H800288BD
    Public Const TYPE_E_SIZETOOBIG As Integer = &H800288C5
    Public Const TYPE_E_DUPLICATEID As Integer = &H800288C6
    Public Const TYPE_E_INVALIDID As Integer = &H800288CF
    Public Const TYPE_E_TYPEMISMATCH As Integer = &H80028CA0
    Public Const TYPE_E_OUTOFBOUNDS As Integer = &H80028CA1
    Public Const TYPE_E_IOERROR As Integer = &H80028CA2
    Public Const TYPE_E_CANTCREATETMPFILE As Integer = &H80028CA3
    Public Const TYPE_E_CANTLOADLIBRARY As Integer = &H80029C4A
    Public Const TYPE_E_INCONSISTENTPROPFUNCS As Integer = &H80029C83
    Public Const TYPE_E_CIRCULARTYPE As Integer = &H80029C84
    Public Const STG_E_INVALIDFUNCTION As Integer = &H80030001
    Public Const STG_E_FILENOTFOUND As Integer = &H80030002
    Public Const STG_E_PATHNOTFOUND As Integer = &H80030003
    Public Const STG_E_TOOMANYOPENFILES As Integer = &H80030004
    Public Const STG_E_ACCESSDENIED As Integer = &H80030005
    Public Const STG_E_INVALIDHANDLE As Integer = &H80030006
    Public Const STG_E_INSUFFICIENTMEMORY As Integer = &H80030008
    Public Const STG_E_INVALIDPOINTER As Integer = &H80030009
    Public Const STG_E_NOMOREFILES As Integer = &H80030012
    Public Const STG_E_DISKISWRITEPROTECTED As Integer = &H80030013
    Public Const STG_E_SEEKERROR As Integer = &H80030019
    Public Const STG_E_WRITEFAULT As Integer = &H8003001D
    Public Const STG_E_READFAULT As Integer = &H8003001E
    Public Const STG_E_SHAREVIOLATION As Integer = &H80030020
    Public Const STG_E_LOCKVIOLATION As Integer = &H80030021
    Public Const STG_E_FILEALREADYEXISTS As Integer = &H80030050
    Public Const STG_E_INVALIDPARAMETER As Integer = &H80030057
    Public Const STG_E_MEDIUMFULL As Integer = &H80030070
    Public Const STG_E_ABNORMALAPIEXIT As Integer = &H800300FA
    Public Const STG_E_INVALIDHEADER As Integer = &H800300FB
    Public Const STG_E_INVALIDNAME As Integer = &H800300FC
    Public Const STG_E_UNKNOWN As Integer = &H800300FD
    Public Const STG_E_UNIMPLEMENTEDFUNCTION As Integer = &H800300FE
    Public Const STG_E_INVALIDFLAG As Integer = &H800300FF
    Public Const STG_E_INUSE As Integer = &H80030100
    Public Const STG_E_NOTCURRENT As Integer = &H80030101
    Public Const STG_E_REVERTED As Integer = &H80030102
    Public Const STG_E_CANTSAVE As Integer = &H80030103
    Public Const STG_E_OLDFORMAT As Integer = &H80030104
    Public Const STG_E_OLDDLL As Integer = &H80030105
    Public Const STG_E_SHAREREQUIRED As Integer = &H80030106
    Public Const STG_E_NOTFILEBASEDSTORAGE As Integer = &H80030107
    Public Const STG_E_EXTANTMARSHALLINGS As Integer = &H80030108
    Public Const STG_S_CONVERTED As Integer = &H30200
    Public Const RPC_E_CALL_REJECTED As Integer = &H80010001
    Public Const RPC_E_CALL_CANCELED As Integer = &H80010002
    Public Const RPC_E_CANTPOST_INSENDCALL As Integer = &H80010003
    Public Const RPC_E_CANTCALLOUT_INASYNCCALL As Integer = &H80010004
    Public Const RPC_E_CANTCALLOUT_INEXTERNALCALL As Integer = &H80010005
    Public Const RPC_E_CONNECTION_TERMINATED As Integer = &H80010006
    Public Const RPC_E_SERVER_DIED As Integer = &H80010007
    Public Const RPC_E_CLIENT_DIED As Integer = &H80010008
    Public Const RPC_E_INVALID_DATAPACKET As Integer = &H80010009
    Public Const RPC_E_CANTTRANSMIT_CALL As Integer = &H8001000A
    Public Const RPC_E_CLIENT_CANTMARSHAL_DATA As Integer = &H8001000B
    Public Const RPC_E_CLIENT_CANTUNMARSHAL_DATA As Integer = &H8001000C
    Public Const RPC_E_SERVER_CANTMARSHAL_DATA As Integer = &H8001000D
    Public Const RPC_E_SERVER_CANTUNMARSHAL_DATA As Integer = &H8001000E
    Public Const RPC_E_INVALID_DATA As Integer = &H8001000F
    Public Const RPC_E_INVALID_PARAMETER As Integer = &H80010010
    Public Const RPC_E_CANTCALLOUT_AGAIN As Integer = &H80010011
    Public Const RPC_E_SERVER_DIED_DNE As Integer = &H80010012
    Public Const RPC_E_SYS_CALL_FAILED As Integer = &H80010100
    Public Const RPC_E_OUT_OF_RESOURCES As Integer = &H80010101
    Public Const RPC_E_ATTEMPTED_MULTITHREAD As Integer = &H80010102
    Public Const RPC_E_NOT_REGISTERED As Integer = &H80010103
    Public Const RPC_E_FAULT As Integer = &H80010104
    Public Const RPC_E_SERVERFAULT As Integer = &H80010105
    Public Const RPC_E_CHANGED_MODE As Integer = &H80010106
    Public Const RPC_E_INVALIDMETHOD As Integer = &H80010107
    Public Const RPC_E_DISCONNECTED As Integer = &H80010108
    Public Const RPC_E_RETRY As Integer = &H80010109
    Public Const RPC_E_SERVERCALL_RETRYLATER As Integer = &H8001010A
    Public Const RPC_E_SERVERCALL_REJECTED As Integer = &H8001010B
    Public Const RPC_E_INVALID_CALLDATA As Integer = &H8001010C
    Public Const RPC_E_CANTCALLOUT_ININPUTSYNCCALL As Integer = &H8001010D
    Public Const RPC_E_WRONG_THREAD As Integer = &H8001010E
    Public Const RPC_E_THREAD_NOT_INIT As Integer = &H8001010F
    Public Const RPC_E_UNEXPECTED As Integer = &H8001FFFF


    ' /////////////////////////
    '                        //
    '  Winnet32 Status Codes //
    '                        //
    ' /////////////////////////

    '   The specified username is invalid.
    Public Const ERROR_BAD_USERNAME As Short = 2202

    '   This network connection does not exist.
    Public Const ERROR_NOT_CONNECTED As Short = 2250

    '   This network connection has files open or requests pending.
    Public Const ERROR_OPEN_FILES As Short = 2401

    '   The device is in use by an active process and cannot be disconnected.
    Public Const ERROR_DEVICE_IN_USE As Short = 2404

    '   The specified device name is invalid.
    Public Const ERROR_BAD_DEVICE As Short = 1200

    '   The device is not currently connected but it is a remembered connection.
    Public Const ERROR_CONNECTION_UNAVAIL As Short = 1201

    '   An attempt was made to remember a device that had previously been remembered.
    Public Const ERROR_DEVICE_ALREADY_REMEMBERED As Short = 1202

    '   No network provider accepted the given network path.
    Public Const ERROR_NO_NET_OR_BAD_PATH As Short = 1203

    '   The specified network provider name is invalid.
    Public Const ERROR_BAD_PROVIDER As Short = 1204

    '   Unable to open the network connection profile.
    Public Const ERROR_CANNOT_OPEN_PROFILE As Short = 1205

    '   The network connection profile is corrupt.
    Public Const ERROR_BAD_PROFILE As Short = 1206

    '   Cannot enumerate a non-container.
    Public Const ERROR_NOT_CONTAINER As Short = 1207

    '   An extended error has occurred.
    Public Const ERROR_EXTENDED_ERROR As Short = 1208

    '   The format of the specified group name is invalid.
    Public Const ERROR_INVALID_GROUPNAME As Short = 1209

    '   The format of the specified computer name is invalid.
    Public Const ERROR_INVALID_COMPUTERNAME As Short = 1210

    '   The format of the specified event name is invalid.
    Public Const ERROR_INVALID_EVENTNAME As Short = 1211

    '   The format of the specified domain name is invalid.
    Public Const ERROR_INVALID_DOMAINNAME As Short = 1212

    '   The format of the specified service name is invalid.
    Public Const ERROR_INVALID_SERVICENAME As Short = 1213

    '   The format of the specified network name is invalid.
    Public Const ERROR_INVALID_NETNAME As Short = 1214

    '   The format of the specified share name is invalid.
    Public Const ERROR_INVALID_SHARENAME As Short = 1215

    '   The format of the specified password is invalid.
    Public Const ERROR_INVALID_PASSWORDNAME As Short = 1216

    '   The format of the specified message name is invalid.
    Public Const ERROR_INVALID_MESSAGENAME As Short = 1217

    '   The format of the specified message destination is invalid.
    Public Const ERROR_INVALID_MESSAGEDEST As Short = 1218

    '   The credentials supplied conflict with an existing set of credentials.
    Public Const ERROR_SESSION_CREDENTIAL_CONFLICT As Short = 1219

    '   An attempt was made to establish a session to a Lan Manager server, but there
    '   are already too many sessions established to that server.
    Public Const ERROR_REMOTE_SESSION_LIMIT_EXCEEDED As Short = 1220

    '   The workgroup or domain name is already in use by another computer on the
    '   network.
    Public Const ERROR_DUP_DOMAINNAME As Short = 1221

    '   The network is not present or not started.
    Public Const ERROR_NO_NETWORK As Short = 1222


    ' /////////////////////////
    '                        //
    '  Security Status Codes //
    '                        //
    ' /////////////////////////

    '   Not all privileges referenced are assigned to the caller.
    Public Const ERROR_NOT_ALL_ASSIGNED As Short = 1300

    '   Some mapping between account names and security IDs was not done.
    Public Const ERROR_SOME_NOT_MAPPED As Short = 1301

    '   No system quota limits are specifically set for this account.
    Public Const ERROR_NO_QUOTAS_FOR_ACCOUNT As Short = 1302

    '   No encryption key is available.  A well-known encryption key was returned.
    Public Const ERROR_LOCAL_USER_SESSION_KEY As Short = 1303

    '   The NT password is too complex to be converted to a LAN Manager
    '   password.  The LAN Manager password returned is a NULL string.
    Public Const ERROR_NULL_LM_PASSWORD As Short = 1304

    '   The revision level is unknown.
    Public Const ERROR_UNKNOWN_REVISION As Short = 1305

    '   Indicates two revision levels are incompatible.
    Public Const ERROR_REVISION_MISMATCH As Short = 1306

    '   This security ID may not be assigned as the owner of this object.
    Public Const ERROR_INVALID_OWNER As Short = 1307

    '   This security ID may not be assigned as the primary group of an object.
    Public Const ERROR_INVALID_PRIMARY_GROUP As Short = 1308

    '   An attempt has been made to operate on an impersonation token
    '   by a thread that is not currently impersonating a client.
    Public Const ERROR_NO_IMPERSONATION_TOKEN As Short = 1309

    '   The group may not be disabled.
    Public Const ERROR_CANT_DISABLE_MANDATORY As Short = 1310

    '   There are currently no logon servers available to service the logon
    '   request.
    Public Const ERROR_NO_LOGON_SERVERS As Short = 1311

    '    A specified logon session does not exist.  It may already have
    '    been terminated.
    Public Const ERROR_NO_SUCH_LOGON_SESSION As Short = 1312

    '    A specified privilege does not exist.
    Public Const ERROR_NO_SUCH_PRIVILEGE As Short = 1313

    '    A required privilege is not held by the client.
    Public Const ERROR_PRIVILEGE_NOT_HELD As Short = 1314

    '   The name provided is not a properly formed account name.
    Public Const ERROR_INVALID_ACCOUNT_NAME As Short = 1315

    '   The specified user already exists.
    Public Const ERROR_USER_EXISTS As Short = 1316

    '   The specified user does not exist.
    Public Const ERROR_NO_SUCH_USER As Short = 1317

    '   The specified group already exists.
    Public Const ERROR_GROUP_EXISTS As Short = 1318

    '   The specified group does not exist.
    Public Const ERROR_NO_SUCH_GROUP As Short = 1319

    '   Either the specified user account is already a member of the specified
    '   group, or the specified group cannot be deleted because it contains
    '   a member.
    Public Const ERROR_MEMBER_IN_GROUP As Short = 1320

    '   The specified user account is not a member of the specified group account.
    Public Const ERROR_MEMBER_NOT_IN_GROUP As Short = 1321

    '   The last remaining administration account cannot be disabled
    '   or deleted.
    Public Const ERROR_LAST_ADMIN As Short = 1322

    '   Unable to update the password.  The value provided as the current
    '   password is incorrect.
    Public Const ERROR_WRONG_PASSWORD As Short = 1323

    '   Unable to update the password.  The value provided for the new password
    '   contains values that are not allowed in passwords.
    Public Const ERROR_ILL_FORMED_PASSWORD As Short = 1324

    '   Unable to update the password because a password update rule has been
    '   violated.
    Public Const ERROR_PASSWORD_RESTRICTION As Short = 1325

    '   Logon failure: unknown user name or bad password.
    Public Const ERROR_LOGON_FAILURE As Short = 1326

    '   Logon failure: user account restriction.
    Public Const ERROR_ACCOUNT_RESTRICTION As Short = 1327

    '   Logon failure: account logon time restriction violation.
    Public Const ERROR_INVALID_LOGON_HOURS As Short = 1328

    '   Logon failure: user not allowed to log on to this computer.
    Public Const ERROR_INVALID_WORKSTATION As Short = 1329

    '   Logon failure: the specified account password has expired.
    Public Const ERROR_PASSWORD_EXPIRED As Short = 1330

    '   Logon failure: account currently disabled.
    Public Const ERROR_ACCOUNT_DISABLED As Short = 1331

    '   No mapping between account names and security IDs was done.
    Public Const ERROR_NONE_MAPPED As Short = 1332

    '   Too many local user identifiers (LUIDs) were requested at one time.
    Public Const ERROR_TOO_MANY_LUIDS_REQUESTED As Short = 1333

    '   No more local user identifiers (LUIDs) are available.
    Public Const ERROR_LUIDS_EXHAUSTED As Short = 1334

    '   The subauthority part of a security ID is invalid for this particular use.
    Public Const ERROR_INVALID_SUB_AUTHORITY As Short = 1335

    '   The access control list (ACL) Public Structure is invalid.
    Public Const ERROR_INVALID_ACL As Short = 1336

    '   The security ID Public Structure is invalid.
    Public Const ERROR_INVALID_SID As Short = 1337

    '   The security descriptor Public Structure is invalid.
    Public Const ERROR_INVALID_SECURITY_DESCR As Short = 1338

    '   The inherited access control list (ACL) or access control entry (ACE)
    '   could not be built.
    Public Const ERROR_BAD_INHERITANCE_ACL As Short = 1340

    '   The server is currently disabled.
    Public Const ERROR_SERVER_DISABLED As Short = 1341

    '   The server is currently enabled.
    Public Const ERROR_SERVER_NOT_DISABLED As Short = 1342

    '   The value provided was an invalid value for an identifier authority.
    Public Const ERROR_INVALID_ID_AUTHORITY As Short = 1343

    '   No more memory is available for security information updates.
    Public Const ERROR_ALLOTTED_SPACE_EXCEEDED As Short = 1344

    '   The specified attributes are invalid, or incompatible with the
    '   attributes for the group as a whole.
    Public Const ERROR_INVALID_GROUP_ATTRIBUTES As Short = 1345

    '   Either a required impersonation level was not provided, or the
    '   provided impersonation level is invalid.
    Public Const ERROR_BAD_IMPERSONATION_LEVEL As Short = 1346

    '   Cannot open an anonymous level security token.
    Public Const ERROR_CANT_OPEN_ANONYMOUS As Short = 1347

    '   The validation information class requested was invalid.
    Public Const ERROR_BAD_VALIDATION_CLASS As Short = 1348

    '   The type of the token is inappropriate for its attempted use.
    Public Const ERROR_BAD_TOKEN_TYPE As Short = 1349

    '   Unable to perform a security operation on an object
    '   which has no associated security.
    Public Const ERROR_NO_SECURITY_ON_OBJECT As Short = 1350

    '   Indicates a Windows NT Advanced Server could not be contacted or that
    '   objects within the domain are protected such that necessary
    '   information could not be retrieved.
    Public Const ERROR_CANT_ACCESS_DOMAIN_INFO As Short = 1351

    '   The security account manager (SAM) or local security
    '   authority (LSA) server was in the wrong state to perform
    '   the security operation.
    Public Const ERROR_INVALID_SERVER_STATE As Short = 1352

    '   The domain was in the wrong state to perform the security operation.
    Public Const ERROR_INVALID_DOMAIN_STATE As Short = 1353

    '   This operation is only allowed for the Primary Domain Controller of the domain.
    Public Const ERROR_INVALID_DOMAIN_ROLE As Short = 1354

    '   The specified domain did not exist.
    Public Const ERROR_NO_SUCH_DOMAIN As Short = 1355

    '   The specified domain already exists.
    Public Const ERROR_DOMAIN_EXISTS As Short = 1356

    '   An attempt was made to exceed the limit on the number of domains per server.
    Public Const ERROR_DOMAIN_LIMIT_EXCEEDED As Short = 1357

    '   Unable to complete the requested operation because of either a
    '   catastrophic media failure or a data Public Structure corruption on the disk.
    Public Const ERROR_INTERNAL_DB_CORRUPTION As Short = 1358

    '   The security account database contains an internal inconsistency.
    Public Const ERROR_INTERNAL_ERROR As Short = 1359

    '   Generic access types were contained in an access mask which should
    '   already be mapped to non-generic types.
    Public Const ERROR_GENERIC_NOT_MAPPED As Short = 1360

    '   A security descriptor is not in the right format (absolute or self-relative).
    Public Const ERROR_BAD_DESCRIPTOR_FORMAT As Short = 1361

    '   The requested action is restricted for use by logon processes
    '   only.  The calling process has not registered as a logon process.
    Public Const ERROR_NOT_LOGON_PROCESS As Short = 1362

    '   Cannot start a new logon session with an ID that is already in use.
    Public Const ERROR_LOGON_SESSION_EXISTS As Short = 1363

    '   A specified authentication package is unknown.
    Public Const ERROR_NO_SUCH_PACKAGE As Short = 1364

    '   The logon session is not in a state that is consistent with the
    '   requested operation.
    Public Const ERROR_BAD_LOGON_SESSION_STATE As Short = 1365

    '   The logon session ID is already in use.
    Public Const ERROR_LOGON_SESSION_COLLISION As Short = 1366

    '   A logon request contained an invalid logon type value.
    Public Const ERROR_INVALID_LOGON_TYPE As Short = 1367

    '   Unable to impersonate via a named pipe until data has been read
    '   from that pipe.
    Public Const ERROR_CANNOT_IMPERSONATE As Short = 1368

    '   The transaction state of a Registry subtree is incompatible with the
    '   requested operation.
    Public Const ERROR_RXACT_INVALID_STATE As Short = 1369

    '   An internal security database corruption has been encountered.
    Public Const ERROR_RXACT_COMMIT_FAILURE As Short = 1370

    '   Cannot perform this operation on built-in accounts.
    Public Const ERROR_SPECIAL_ACCOUNT As Short = 1371

    '   Cannot perform this operation on this built-in special group.
    Public Const ERROR_SPECIAL_GROUP As Short = 1372

    '   Cannot perform this operation on this built-in special user.
    Public Const ERROR_SPECIAL_USER As Short = 1373

    '   The user cannot be removed from a group because the group
    '   is currently the user's primary group.
    Public Const ERROR_MEMBERS_PRIMARY_GROUP As Short = 1374

    '   The token is already in use as a primary token.
    Public Const ERROR_TOKEN_ALREADY_IN_USE As Short = 1375

    '   The specified local group does not exist.
    Public Const ERROR_NO_SUCH_ALIAS As Short = 1376

    '   The specified account name is not a member of the local group.
    Public Const ERROR_MEMBER_NOT_IN_ALIAS As Short = 1377

    '   The specified account name is already a member of the local group.
    Public Const ERROR_MEMBER_IN_ALIAS As Short = 1378

    '   The specified local group already exists.
    Public Const ERROR_ALIAS_EXISTS As Short = 1379

    '   Logon failure: the user has not been granted the requested
    '   logon type at this computer.
    Public Const ERROR_LOGON_NOT_GRANTED As Short = 1380

    '   The maximum number of secrets that may be stored in a single system has been
    '   exceeded.
    Public Const ERROR_TOO_MANY_SECRETS As Short = 1381

    '   The length of a secret exceeds the maximum length allowed.
    Public Const ERROR_SECRET_TOO_LONG As Short = 1382

    '   The local security authority database contains an internal inconsistency.
    Public Const ERROR_INTERNAL_DB_ERROR As Short = 1383

    '   During a logon attempt, the user's security context accumulated too many
    '   security IDs.
    Public Const ERROR_TOO_MANY_CONTEXT_IDS As Short = 1384

    '   Logon failure: the user has not been granted the requested logon type
    '   at this computer.
    Public Const ERROR_LOGON_TYPE_NOT_GRANTED As Short = 1385

    '   A cross-encrypted password is necessary to change a user password.
    Public Const ERROR_NT_CROSS_ENCRYPTION_REQUIRED As Short = 1386

    '   A new member could not be added to a local group because the member does
    '   not exist.
    Public Const ERROR_NO_SUCH_MEMBER As Short = 1387

    '   A new member could not be added to a local group because the member has the
    '   wrong account type.
    Public Const ERROR_INVALID_MEMBER As Short = 1388

    '   Too many security IDs have been specified.
    Public Const ERROR_TOO_MANY_SIDS As Short = 1389

    '   A cross-encrypted password is necessary to change this user password.
    Public Const ERROR_LM_CROSS_ENCRYPTION_REQUIRED As Short = 1390

    '   Indicates an ACL contains no inheritable components
    Public Const ERROR_NO_INHERITANCE As Short = 1391

    '   The file or directory is corrupt and non-readable.
    Public Const ERROR_FILE_CORRUPT As Short = 1392

    '   The disk Public Structure is corrupt and non-readable.
    Public Const ERROR_DISK_CORRUPT As Short = 1393

    '   There is no user session key for the specified logon session.
    Public Const ERROR_NO_USER_SESSION_KEY As Short = 1394

    '  End of security error codes


    ' /////////////////////////
    '                        //
    '  WinUser Error Codes   //
    '                        //
    ' /////////////////////////

    '   Invalid window handle.
    Public Const ERROR_INVALID_WINDOW_HANDLE As Short = 1400

    '   Invalid menu handle.
    Public Const ERROR_INVALID_MENU_HANDLE As Short = 1401

    '   Invalid cursor handle.
    Public Const ERROR_INVALID_CURSOR_HANDLE As Short = 1402

    '   Invalid accelerator table handle.
    Public Const ERROR_INVALID_ACCEL_HANDLE As Short = 1403

    '   Invalid hook handle.
    Public Const ERROR_INVALID_HOOK_HANDLE As Short = 1404

    '   Invalid handle to a multiple-window position structure.
    Public Const ERROR_INVALID_DWP_HANDLE As Short = 1405

    '   Cannot create a top-level child window.
    Public Const ERROR_TLW_WITH_WSCHILD As Short = 1406

    '   Cannot find window class.
    Public Const ERROR_CANNOT_FIND_WND_CLASS As Short = 1407

    '   Invalid window, belongs to other thread.
    Public Const ERROR_WINDOW_OF_OTHER_THREAD As Short = 1408

    '   Hot key is already registered.
    Public Const ERROR_HOTKEY_ALREADY_REGISTERED As Short = 1409

    '   Class already exists.
    Public Const ERROR_CLASS_ALREADY_EXISTS As Short = 1410

    '   Class does not exist.
    Public Const ERROR_CLASS_DOES_NOT_EXIST As Short = 1411

    '   Class still has open windows.
    Public Const ERROR_CLASS_HAS_WINDOWS As Short = 1412

    '   Invalid index.
    Public Const ERROR_INVALID_INDEX As Short = 1413

    '   Invalid icon handle.
    Public Const ERROR_INVALID_ICON_HANDLE As Short = 1414

    '   Using private DIALOG window words.
    Public Const ERROR_PRIVATE_DIALOG_INDEX As Short = 1415

    '   The listbox identifier was not found.
    Public Const ERROR_LISTBOX_ID_NOT_FOUND As Short = 1416

    '   No wildcards were found.
    Public Const ERROR_NO_WILDCARD_CHARACTERS As Short = 1417

    '   Thread does not have a clipboard open.
    Public Const ERROR_CLIPBOARD_NOT_OPEN As Short = 1418

    '   Hot key is not registered.
    Public Const ERROR_HOTKEY_NOT_REGISTERED As Short = 1419

    '   The window is not a valid dialog window.
    Public Const ERROR_WINDOW_NOT_DIALOG As Short = 1420

    '   Control ID not found.
    Public Const ERROR_CONTROL_ID_NOT_FOUND As Short = 1421

    '   Invalid message for a combo box because it does not have an edit control.
    Public Const ERROR_INVALID_COMBOBOX_MESSAGE As Short = 1422

    '   The window is not a combo box.
    Public Const ERROR_WINDOW_NOT_COMBOBOX As Short = 1423

    '   Height must be less than 256.
    Public Const ERROR_INVALID_EDIT_HEIGHT As Short = 1424

    '   Invalid device context (DC) handle.
    Public Const ERROR_DC_NOT_FOUND As Short = 1425

    '   Invalid hook procedure type.
    Public Const ERROR_INVALID_HOOK_FILTER As Short = 1426

    '   Invalid hook procedure.
    Public Const ERROR_INVALID_FILTER_PROC As Short = 1427

    '   Cannot set non-local hook without a module handle.
    Public Const ERROR_HOOK_NEEDS_HMOD As Short = 1428

    '   This hook procedure can only be set Globally.
    '
    Public Const ERROR_PUBLIC_ONLY_HOOK As Short = 1429

    '   The journal hook procedure is already installed.
    Public Const ERROR_JOURNAL_HOOK_SET As Short = 1430

    '   The hook procedure is not installed.
    Public Const ERROR_HOOK_NOT_INSTALLED As Short = 1431

    '   Invalid message for single-selection listbox.
    Public Const ERROR_INVALID_LB_MESSAGE As Short = 1432

    '   LB_SETCOUNT sent to non-lazy listbox.
    Public Const ERROR_SETCOUNT_ON_BAD_LB As Short = 1433

    '   This list box does not support tab stops.
    Public Const ERROR_LB_WITHOUT_TABSTOPS As Short = 1434

    '   Cannot destroy object created by another thread.
    Public Const ERROR_DESTROY_OBJECT_OF_OTHER_THREAD As Short = 1435

    '   Child windows cannot have menus.
    Public Const ERROR_CHILD_WINDOW_MENU As Short = 1436

    '   The window does not have a system menu.
    Public Const ERROR_NO_SYSTEM_MENU As Short = 1437

    '   Invalid message box style.
    Public Const ERROR_INVALID_MSGBOX_STYLE As Short = 1438

    '   Invalid system-wide (SPI_) parameter.
    Public Const ERROR_INVALID_SPI_VALUE As Short = 1439

    '   Screen already locked.
    Public Const ERROR_SCREEN_ALREADY_LOCKED As Short = 1440

    '   All handles to windows in a multiple-window position Public Structure must
    '   have the same parent.
    Public Const ERROR_HWNDS_HAVE_DIFF_PARENT As Short = 1441

    '   The window is not a child window.
    Public Const ERROR_NOT_CHILD_WINDOW As Short = 1442

    '   Invalid GW_ command.
    Public Const ERROR_INVALID_GW_COMMAND As Short = 1443

    '   Invalid thread identifier.
    Public Const ERROR_INVALID_THREAD_ID As Short = 1444

    '   Cannot process a message from a window that is not a multiple document
    '   interface (MDI) window.
    Public Const ERROR_NON_MDICHILD_WINDOW As Short = 1445

    '   Popup menu already active.
    Public Const ERROR_POPUP_ALREADY_ACTIVE As Short = 1446

    '   The window does not have scroll bars.
    Public Const ERROR_NO_SCROLLBARS As Short = 1447

    '   Scroll bar range cannot be greater than 0x7FFF.
    Public Const ERROR_INVALID_SCROLLBAR_RANGE As Short = 1448

    '   Cannot show or remove the window in the way specified.
    Public Const ERROR_INVALID_SHOWWIN_COMMAND As Short = 1449

    '  End of WinUser error codes


    ' /////////////////////////
    '                        //
    '  Eventlog Status Codes //
    '                        //
    ' /////////////////////////

    '   The event log file is corrupt.
    Public Const ERROR_EVENTLOG_FILE_CORRUPT As Short = 1500

    '   No event log file could be opened, so the event logging service did not start.
    Public Const ERROR_EVENTLOG_CANT_START As Short = 1501

    '   The event log file is full.
    Public Const ERROR_LOG_FILE_FULL As Short = 1502

    '   The event log file has changed between reads.
    Public Const ERROR_EVENTLOG_FILE_CHANGED As Short = 1503

    '  End of eventlog error codes


    ' /////////////////////////
    '                        //
    '    RPC Status Codes    //
    '                        //
    ' /////////////////////////

    '   The string binding is invalid.
    Public Const RPC_S_INVALID_STRING_BINDING As Short = 1700

    '   The binding handle is not the correct type.
    Public Const RPC_S_WRONG_KIND_OF_BINDING As Short = 1701

    '   The binding handle is invalid.
    Public Const RPC_S_INVALID_BINDING As Short = 1702

    '   The RPC protocol sequence is not supported.
    Public Const RPC_S_PROTSEQ_NOT_SUPPORTED As Short = 1703

    '   The RPC protocol sequence is invalid.
    Public Const RPC_S_INVALID_RPC_PROTSEQ As Short = 1704

    '   The string universal unique identifier (UUID) is invalid.
    Public Const RPC_S_INVALID_STRING_UUID As Short = 1705

    '   The endpoint format is invalid.
    Public Const RPC_S_INVALID_ENDPOINT_FORMAT As Short = 1706

    '   The network address is invalid.
    Public Const RPC_S_INVALID_NET_ADDR As Short = 1707

    '   No endpoint was found.
    Public Const RPC_S_NO_ENDPOINT_FOUND As Short = 1708

    '   The timeout value is invalid.
    Public Const RPC_S_INVALID_TIMEOUT As Short = 1709

    '   The object universal unique identifier (UUID) was not found.
    Public Const RPC_S_OBJECT_NOT_FOUND As Short = 1710

    '   The object universal unique identifier (UUID) has already been registered.
    Public Const RPC_S_ALREADY_REGISTERED As Short = 1711

    '   The type universal unique identifier (UUID) has already been registered.
    Public Const RPC_S_TYPE_ALREADY_REGISTERED As Short = 1712

    '   The RPC server is already listening.
    Public Const RPC_S_ALREADY_LISTENING As Short = 1713

    '   No protocol sequences have been registered.
    Public Const RPC_S_NO_PROTSEQS_REGISTERED As Short = 1714

    '   The RPC server is not listening.
    Public Const RPC_S_NOT_LISTENING As Short = 1715

    '   The manager type is unknown.
    Public Const RPC_S_UNKNOWN_MGR_TYPE As Short = 1716

    '   The interface is unknown.
    Public Const RPC_S_UNKNOWN_IF As Short = 1717

    '   There are no bindings.
    Public Const RPC_S_NO_BINDINGS As Short = 1718

    '   There are no protocol sequences.
    Public Const RPC_S_NO_PROTSEQS As Short = 1719

    '   The endpoint cannot be created.
    Public Const RPC_S_CANT_CREATE_ENDPOINT As Short = 1720

    '   Not enough resources are available to complete this operation.
    Public Const RPC_S_OUT_OF_RESOURCES As Short = 1721

    '   The RPC server is unavailable.
    Public Const RPC_S_SERVER_UNAVAILABLE As Short = 1722

    '   The RPC server is too busy to complete this operation.
    Public Const RPC_S_SERVER_TOO_BUSY As Short = 1723

    '   The network options are invalid.
    Public Const RPC_S_INVALID_NETWORK_OPTIONS As Short = 1724

    '   There is not a remote procedure call active in this thread.
    Public Const RPC_S_NO_CALL_ACTIVE As Short = 1725

    '   The remote procedure call failed.
    Public Const RPC_S_CALL_FAILED As Short = 1726

    '   The remote procedure call failed and did not execute.
    Public Const RPC_S_CALL_FAILED_DNE As Short = 1727

    '   A remote procedure call (RPC) protocol error occurred.
    Public Const RPC_S_PROTOCOL_ERROR As Short = 1728

    '   The transfer syntax is not supported by the RPC server.
    Public Const RPC_S_UNSUPPORTED_TRANS_SYN As Short = 1730

    '   The universal unique identifier (UUID) type is not supported.
    Public Const RPC_S_UNSUPPORTED_TYPE As Short = 1732

    '   The tag is invalid.
    Public Const RPC_S_INVALID_TAG As Short = 1733

    '   The array bounds are invalid.
    Public Const RPC_S_INVALID_BOUND As Short = 1734

    '   The binding does not contain an entry name.
    Public Const RPC_S_NO_ENTRY_NAME As Short = 1735

    '   The name syntax is invalid.
    Public Const RPC_S_INVALID_NAME_SYNTAX As Short = 1736

    '   The name syntax is not supported.
    Public Const RPC_S_UNSUPPORTED_NAME_SYNTAX As Short = 1737

    '   No network address is available to use to construct a universal
    '   unique identifier (UUID).
    Public Const RPC_S_UUID_NO_ADDRESS As Short = 1739

    '   The endpoint is a duplicate.
    Public Const RPC_S_DUPLICATE_ENDPOINT As Short = 1740

    '   The authentication type is unknown.
    Public Const RPC_S_UNKNOWN_AUTHN_TYPE As Short = 1741

    '   The maximum number of calls is too small.
    Public Const RPC_S_MAX_CALLS_TOO_SMALL As Short = 1742

    '   The string is too long.
    Public Const RPC_S_STRING_TOO_LONG As Short = 1743

    '   The RPC protocol sequence was not found.
    Public Const RPC_S_PROTSEQ_NOT_FOUND As Short = 1744

    '   The procedure number is out of range.
    Public Const RPC_S_PROCNUM_OUT_OF_RANGE As Short = 1745

    '   The binding does not contain any authentication information.
    Public Const RPC_S_BINDING_HAS_NO_AUTH As Short = 1746

    '   The authentication service is unknown.
    Public Const RPC_S_UNKNOWN_AUTHN_SERVICE As Short = 1747

    '   The authentication level is unknown.
    Public Const RPC_S_UNKNOWN_AUTHN_LEVEL As Short = 1748

    '   The security context is invalid.
    Public Const RPC_S_INVALID_AUTH_IDENTITY As Short = 1749

    '   The authorization service is unknown.
    Public Const RPC_S_UNKNOWN_AUTHZ_SERVICE As Short = 1750

    '   The entry is invalid.
    Public Const EPT_S_INVALID_ENTRY As Short = 1751

    '   The server endpoint cannot perform the operation.
    Public Const EPT_S_CANT_PERFORM_OP As Short = 1752

    '   There are no more endpoints available from the endpoint mapper.
    Public Const EPT_S_NOT_REGISTERED As Short = 1753

    '   No interfaces have been exported.
    Public Const RPC_S_NOTHING_TO_EXPORT As Short = 1754

    '   The entry name is incomplete.
    Public Const RPC_S_INCOMPLETE_NAME As Short = 1755

    '   The version option is invalid.
    Public Const RPC_S_INVALID_VERS_OPTION As Short = 1756

    '   There are no more members.
    Public Const RPC_S_NO_MORE_MEMBERS As Short = 1757

    '   There is nothing to unexport.
    Public Const RPC_S_NOT_ALL_OBJS_UNEXPORTED As Short = 1758

    '   The interface was not found.
    Public Const RPC_S_INTERFACE_NOT_FOUND As Short = 1759

    '   The entry already exists.
    Public Const RPC_S_ENTRY_ALREADY_EXISTS As Short = 1760

    '   The entry is not found.
    Public Const RPC_S_ENTRY_NOT_FOUND As Short = 1761

    '   The name service is unavailable.
    Public Const RPC_S_NAME_SERVICE_UNAVAILABLE As Short = 1762

    '   The network address family is invalid.
    Public Const RPC_S_INVALID_NAF_ID As Short = 1763

    '   The requested operation is not supported.
    Public Const RPC_S_CANNOT_SUPPORT As Short = 1764

    '   No security context is available to allow impersonation.
    Public Const RPC_S_NO_CONTEXT_AVAILABLE As Short = 1765

    '   An internal error occurred in a remote procedure call (RPC).
    Public Const RPC_S_INTERNAL_ERROR As Short = 1766

    '   The RPC server attempted an integer division by zero.'
    Public Const RPC_S_ZERO_DIVIDE As Short = 1767

    '   An addressing error occurred in the RPC server.
    Public Const RPC_S_ADDRESS_ERROR As Short = 1768

    '   A floating-point operation at the RPC server caused a division by zero.
    Public Const RPC_S_FP_DIV_ZERO As Short = 1769

    '   A floating-point underflow occurred at the RPC server.
    Public Const RPC_S_FP_UNDERFLOW As Short = 1770

    '   A floating-point overflow occurred at the RPC server.
    Public Const RPC_S_FP_OVERFLOW As Short = 1771

    '   The list of RPC servers available for the binding of auto handles
    '   has been exhausted.
    Public Const RPC_X_NO_MORE_ENTRIES As Short = 1772

    '   Unable to open the character translation table file.
    Public Const RPC_X_SS_CHAR_TRANS_OPEN_FAIL As Short = 1773

    '   The file containing the character translation table has fewer than
    '   512 bytes.
    Public Const RPC_X_SS_CHAR_TRANS_SHORT_FILE As Short = 1774

    '   A null context handle was passed from the client to the host during
    '   a remote procedure call.
    Public Const RPC_X_SS_IN_NULL_CONTEXT As Short = 1775

    '   The context handle changed during a remote procedure call.
    Public Const RPC_X_SS_CONTEXT_DAMAGED As Short = 1777

    '   The binding handles passed to a remote procedure call do not match.
    Public Const RPC_X_SS_HANDLES_MISMATCH As Short = 1778

    '   The stub is unable to get the remote procedure call handle.
    Public Const RPC_X_SS_CANNOT_GET_CALL_HANDLE As Short = 1779

    '   A null reference pointer was passed to the stub.
    Public Const RPC_X_NULL_REF_POINTER As Short = 1780

    '   The enumeration value is out of range.
    Public Const RPC_X_ENUM_VALUE_OUT_OF_RANGE As Short = 1781

    '   The byte count is too small.
    Public Const RPC_X_BYTE_COUNT_TOO_SMALL As Short = 1782

    '   The stub received bad data.
    Public Const RPC_X_BAD_STUB_DATA As Short = 1783

    '   The supplied user buffer is not valid for the requested operation.
    Public Const ERROR_INVALID_USER_BUFFER As Short = 1784

    '   The disk media is not recognized.  It may not be formatted.
    Public Const ERROR_UNRECOGNIZED_MEDIA As Short = 1785

    '   The workstation does not have a trust secret.
    Public Const ERROR_NO_TRUST_LSA_SECRET As Short = 1786

    '   The SAM database on the Windows NT Advanced Server does not have a computer
    '   account for this workstation trust relationship.
    Public Const ERROR_NO_TRUST_SAM_ACCOUNT As Short = 1787

    '   The trust relationship between the primary domain and the trusted
    '   domain failed.
    Public Const ERROR_TRUSTED_DOMAIN_FAILURE As Short = 1788

    '   The trust relationship between this workstation and the primary
    '   domain failed.
    Public Const ERROR_TRUSTED_RELATIONSHIP_FAILURE As Short = 1789

    '   The network logon failed.
    Public Const ERROR_TRUST_FAILURE As Short = 1790

    '   A remote procedure call is already in progress for this thread.
    Public Const RPC_S_CALL_IN_PROGRESS As Short = 1791

    '   An attempt was made to logon, but the network logon service was not started.
    Public Const ERROR_NETLOGON_NOT_STARTED As Short = 1792

    '   The user's account has expired.
    Public Const ERROR_ACCOUNT_EXPIRED As Short = 1793

    '   The redirector is in use and cannot be unloaded.
    Public Const ERROR_REDIRECTOR_HAS_OPEN_HANDLES As Short = 1794

    '   The specified printer driver is already installed.
    Public Const ERROR_PRINTER_DRIVER_ALREADY_INSTALLED As Short = 1795

    '   The specified port is unknown.
    Public Const ERROR_UNKNOWN_PORT As Short = 1796

    '   The printer driver is unknown.
    Public Const ERROR_UNKNOWN_PRINTER_DRIVER As Short = 1797

    '   The print processor is unknown.
    '
    Public Const ERROR_UNKNOWN_PRINTPROCESSOR As Short = 1798

    '   The specified separator file is invalid.
    Public Const ERROR_INVALID_SEPARATOR_FILE As Short = 1799

    '   The specified priority is invalid.
    Public Const ERROR_INVALID_PRIORITY As Short = 1800

    '   The printer name is invalid.
    Public Const ERROR_INVALID_PRINTER_NAME As Short = 1801

    '   The printer already exists.
    Public Const ERROR_PRINTER_ALREADY_EXISTS As Short = 1802

    '   The printer command is invalid.
    Public Const ERROR_INVALID_PRINTER_COMMAND As Short = 1803

    '   The specified datatype is invalid.
    Public Const ERROR_INVALID_DATATYPE As Short = 1804

    '   The Environment specified is invalid.
    Public Const ERROR_INVALID_ENVIRONMENT As Short = 1805

    '   There are no more bindings.
    Public Const RPC_S_NO_MORE_BINDINGS As Short = 1806

    '   The account used is an interdomain trust account.  Use your Global user account or local user account to access this server.
    Public Const ERROR_NOLOGON_INTERDOMAIN_TRUST_ACCOUNT As Short = 1807

    '   The account used is a Computer Account.  Use your Global user account or local user account to access this server.
    Public Const ERROR_NOLOGON_WORKSTATION_TRUST_ACCOUNT As Short = 1808

    '   The account used is an server trust account.  Use your Global user account or local user account to access this server.
    Public Const ERROR_NOLOGON_SERVER_TRUST_ACCOUNT As Short = 1809

    '   The name or security ID (SID) of the domain specified is inconsistent
    '   with the trust information for that domain.
    Public Const ERROR_DOMAIN_TRUST_INCONSISTENT As Short = 1810

    '   The server is in use and cannot be unloaded.
    Public Const ERROR_SERVER_HAS_OPEN_HANDLES As Short = 1811

    '   The specified image file did not contain a resource section.
    Public Const ERROR_RESOURCE_DATA_NOT_FOUND As Short = 1812

    '   The specified resource type can not be found in the image file.
    Public Const ERROR_RESOURCE_TYPE_NOT_FOUND As Short = 1813

    '   The specified resource name can not be found in the image file.
    Public Const ERROR_RESOURCE_NAME_NOT_FOUND As Short = 1814

    '   The specified resource language ID cannot be found in the image file.
    Public Const ERROR_RESOURCE_LANG_NOT_FOUND As Short = 1815

    '   Not enough quota is available to process this command.
    Public Const ERROR_NOT_ENOUGH_QUOTA As Short = 1816

    '   The group member was not found.
    Public Const RPC_S_GROUP_MEMBER_NOT_FOUND As Short = 1898

    '   The endpoint mapper database could not be created.
    Public Const EPT_S_CANT_CREATE As Short = 1899

    '   The object universal unique identifier (UUID) is the nil UUID.
    Public Const RPC_S_INVALID_OBJECT As Short = 1900

    '   The specified time is invalid.
    Public Const ERROR_INVALID_TIME As Short = 1901

    '   The specified Form name is invalid.
    Public Const ERROR_INVALID_FORM_NAME As Short = 1902

    '   The specified Form size is invalid
    Public Const ERROR_INVALID_FORM_SIZE As Short = 1903

    '   The specified Printer handle is already being waited on
    Public Const ERROR_ALREADY_WAITING As Short = 1904

    '   The specified Printer has been deleted
    Public Const ERROR_PRINTER_DELETED As Short = 1905

    '   The state of the Printer is invalid
    Public Const ERROR_INVALID_PRINTER_STATE As Short = 1906

    '   The list of servers for this workgroup is not currently available
    Public Const ERROR_NO_BROWSER_SERVERS_FOUND As Short = 6118

End Module
