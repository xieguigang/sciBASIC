#Region "Microsoft.VisualBasic::f4ca75404a6cc162a8db3e3ad9659766, ..\sciBASIC#\CLI_tools\ErrorCodes\errors1.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Public Enum ErrorCodes As Long
'''<summary>
''' Compression algorithm not
''' recognized.
'''</summary>
LZERROR_UNKNOWNALG = -8

'''<summary>
''' Input parameter out of
''' acceptable range.
'''</summary>
LZERROR_BADVALUE = -7

'''<summary>
''' Bad global handle.
'''</summary>
LZERROR_GLOBLOCK = -6

'''<summary>
''' Insufficient memory for LZFile
''' structure.
'''</summary>
LZERROR_GLOBALLOC = -5

'''<summary>
''' Out of space for output file.
'''</summary>
LZERROR_WRITE = -4

'''<summary>
''' Corrupt compressed file
''' format.
'''</summary>
LZERROR_READ = -3

'''<summary>
''' Invalid output handle.
'''</summary>
LZERROR_BADOUTHANDLE = -2

'''<summary>
''' Invalid input handle.
'''</summary>
LZERROR_BADINHANDLE = -1

'''<summary>
''' No error.
'''</summary>
NO_ERROR = 0L

'''<summary>
''' The operation was successfully
''' completed.
'''</summary>
ERROR_SUCCESS = 0L

'''<summary>
''' The function is incorrect.
'''</summary>
ERROR_INVALID_FUNCTION = 1L

'''<summary>
''' The system cannot find the
''' file specified.
'''</summary>
ERROR_FILE_NOT_FOUND = 2L

'''<summary>
''' The system cannot find the
''' specified path.
'''</summary>
ERROR_PATH_NOT_FOUND = 3L

'''<summary>
''' The system cannot open the
''' file.
'''</summary>
ERROR_TOO_MANY_OPEN_FILES = 4L

'''<summary>
''' Access is denied.
'''</summary>
ERROR_ACCESS_DENIED = 5L

'''<summary>
''' The internal file identifier
''' is incorrect.
'''</summary>
ERROR_INVALID_HANDLE = 6L

'''<summary>
''' The storage control blocks
''' were destroyed.
'''</summary>
ERROR_ARENA_TRASHED = 7L

'''<summary>
''' Not enough storage is
''' available to process this
''' command.
'''</summary>
ERROR_NOT_ENOUGH_MEMORY = 8L

'''<summary>
''' The storage control block
''' address is invalid.
'''</summary>
ERROR_INVALID_BLOCK = 9L

'''<summary>
''' The environment is incorrect.
'''</summary>
ERROR_BAD_ENVIRONMENT = 10L

'''<summary>
''' An attempt was made to load a
''' program with an incorrect
''' format.
'''</summary>
ERROR_BAD_FORMAT = 11L

'''<summary>
''' The access code is invalid.
'''</summary>
ERROR_INVALID_ACCESS = 12L

'''<summary>
''' The data is invalid.
'''</summary>
ERROR_INVALID_DATA = 13L

'''<summary>
''' Not enough storage is
''' available to complete this
''' operation.
'''</summary>
ERROR_OUTOFMEMORY = 14L

'''<summary>
''' The system cannot find the
''' specified drive.
'''</summary>
ERROR_INVALID_DRIVE = 15L

'''<summary>
''' The directory cannot be
''' removed.
'''</summary>
ERROR_CURRENT_DIRECTORY = 16L

'''<summary>
''' The system cannot move the
''' file to a different disk
''' drive.
'''</summary>
ERROR_NOT_SAME_DEVICE = 17L

'''<summary>
''' There are no more files.
'''</summary>
ERROR_NO_MORE_FILES = 18L

'''<summary>
''' The media is write protected.
'''</summary>
ERROR_WRITE_PROTECT = 19L

'''<summary>
''' The system cannot find the
''' specified device.
'''</summary>
ERROR_BAD_UNIT = 20L

'''<summary>
''' The drive is not ready.
'''</summary>
ERROR_NOT_READY = 21L

'''<summary>
''' The device does not recognize
''' the command.
'''</summary>
ERROR_BAD_COMMAND = 22L

'''<summary>
''' Data error (cyclic redundancy
''' check).
'''</summary>
ERROR_CRC = 23L

'''<summary>
''' The program issued a command
''' but the command length is
''' incorrect.
'''</summary>
ERROR_BAD_LENGTH = 24L

'''<summary>
''' The drive cannot locate a
''' specific area or track on the
''' disk.
'''</summary>
ERROR_SEEK = 25L

'''<summary>
''' The specified disk cannot be
''' accessed.
'''</summary>
ERROR_NOT_DOS_DISK = 26L

'''<summary>
''' The drive cannot find the
''' requested sector.
'''</summary>
ERROR_SECTOR_NOT_FOUND = 27L

'''<summary>
''' The printer is out of paper.
'''</summary>
ERROR_OUT_OF_PAPER = 28L

'''<summary>
''' The system cannot write to the
''' specified device.
'''</summary>
ERROR_WRITE_FAULT = 29L

'''<summary>
''' The system cannot read from
''' the specified device.
'''</summary>
ERROR_READ_FAULT = 30L

'''<summary>
''' A device attached to the
''' system is not functioning.
'''</summary>
ERROR_GEN_FAILURE = 31L

'''<summary>
''' The process cannot access the
''' file because it is being used
''' by another process.
'''</summary>
ERROR_SHARING_VIOLATION = 32L

'''<summary>
''' The process cannot access the
''' file because another process
''' has locked a portion of the
''' file.
'''</summary>
ERROR_LOCK_VIOLATION = 33L

'''<summary>
''' The wrong disk is in the
''' drive. Insert %2 (Volume
''' Serial Number: %3) into drive
''' %1.
'''</summary>
ERROR_WRONG_DISK = 34L

'''<summary>
''' Too many files opened for
''' sharing.
'''</summary>
ERROR_SHARING_BUFFER_EXCEEDED = 36L

'''<summary>
''' Reached End Of File.
'''</summary>
ERROR_HANDLE_EOF = 38L

'''<summary>
''' The disk is full.
'''</summary>
ERROR_HANDLE_DISK_FULL = 39L

'''<summary>
''' The network request is not
''' supported.
'''</summary>
ERROR_NOT_SUPPORTED = 50L

'''<summary>
''' The remote computer is not
''' available.
'''</summary>
ERROR_REM_NOT_LIST = 51L

'''<summary>
''' A duplicate name exists on the
''' network.
''' 
'''</summary>
ERROR_DUP_NAME = 52L

'''<summary>
''' The network path was not
''' found.
'''</summary>
ERROR_BAD_NETPATH = 53L

'''<summary>
''' The network is busy.
'''</summary>
ERROR_NETWORK_BUSY = 54L

'''<summary>
''' The specified network resource
''' is no longer available.
'''</summary>
ERROR_DEV_NOT_EXIST = 55L

'''<summary>
''' The network BIOS command limit
''' has been reached.
'''</summary>
ERROR_TOO_MANY_CMDS = 56L

'''<summary>
''' A network adapter hardware
''' error occurred.
'''</summary>
ERROR_ADAP_HDW_ERR = 57L

'''<summary>
''' The specified server cannot
''' perform the requested
''' operation.
'''</summary>
ERROR_BAD_NET_RESP = 58L

'''<summary>
''' An unexpected network error
''' occurred.
'''</summary>
ERROR_UNEXP_NET_ERR = 59L

'''<summary>
''' The remote adapter is not
''' compatible.
'''</summary>
ERROR_BAD_REM_ADAP = 60L

'''<summary>
''' The printer queue is full.
'''</summary>
ERROR_PRINTQ_FULL = 61L

'''<summary>
''' Space to store the file
''' waiting to be printed is not
''' available on the server.
'''</summary>
ERROR_NO_SPOOL_SPACE = 62L

'''<summary>
''' File waiting to be printed was
''' deleted.
'''</summary>
ERROR_PRINT_CANCELLED = 63L

'''<summary>
''' The specified network name is
''' no longer available.
'''</summary>
ERROR_NETNAME_DELETED = 64L

'''<summary>
''' Network access is denied.
'''</summary>
ERROR_NETWORK_ACCESS_DENIED = 65L

'''<summary>
''' The network resource type is
''' incorrect.
'''</summary>
ERROR_BAD_DEV_TYPE = 66L

'''<summary>
''' The network name cannot be
''' found.
'''</summary>
ERROR_BAD_NET_NAME = 67L

'''<summary>
''' The name limit for the local
''' computer network adapter card
''' exceeded.
'''</summary>
ERROR_TOO_MANY_NAMES = 68L

'''<summary>
''' The network BIOS session limit
''' exceeded.
'''</summary>
ERROR_TOO_MANY_SESS = 69L

'''<summary>
''' The remote server is paused or
''' is in the process of being
''' started.
'''</summary>
ERROR_SHARING_PAUSED = 70L

'''<summary>
''' The network request was not
''' accepted.
'''</summary>
ERROR_REQ_NOT_ACCEP = 71L

'''<summary>
''' The specified printer or disk
''' device has been paused.
'''</summary>
ERROR_REDIR_PAUSED = 72L

'''<summary>
''' The file exists.
'''</summary>
ERROR_FILE_EXISTS = 80L

'''<summary>
''' The directory or file cannot
''' be created.
'''</summary>
ERROR_CANNOT_MAKE = 82L

'''<summary>
''' Fail on INT 24.
'''</summary>
ERROR_FAIL_I24 = 83L

'''<summary>
''' Storage to process this
''' request is not available.
'''</summary>
ERROR_OUT_OF_STRUCTURES = 84L

'''<summary>
''' The local device name is
''' already in use.
'''</summary>
ERROR_ALREADY_ASSIGNED = 85L

'''<summary>
''' The specified network password
''' is incorrect.
'''</summary>
ERROR_INVALID_PASSWORD = 86L

'''<summary>
''' The parameter is incorrect.
'''</summary>
ERROR_INVALID_PARAMETER = 87L

'''<summary>
''' A write fault occurred on the
''' network.
'''</summary>
ERROR_NET_WRITE_FAULT = 88L

'''<summary>
''' The system cannot start
''' another process at this time.
'''</summary>
ERROR_NO_PROC_SLOTS = 89L

'''<summary>
''' Cannot create another system
''' semaphore.
'''</summary>
ERROR_TOO_MANY_SEMAPHORES = 100L

'''<summary>
''' The exclusive semaphore is
''' owned by another process.
'''</summary>
ERROR_EXCL_SEM_ALREADY_OWNED = 101L

'''<summary>
''' The semaphore is set and
''' cannot be closed.
'''</summary>
ERROR_SEM_IS_SET = 102L

'''<summary>
''' The semaphore cannot be set
''' again.
'''</summary>
ERROR_TOO_MANY_SEM_REQUESTS = 103L

'''<summary>
''' Cannot request exclusive
''' semaphores at interrupt time.
'''</summary>
ERROR_INVALID_AT_INTERRUPT_TIME = 104L

'''<summary>
''' The previous ownership of this
''' semaphore has ended.
'''</summary>
ERROR_SEM_OWNER_DIED = 105L

'''<summary>
''' Insert the disk for drive 1.
'''</summary>
ERROR_SEM_USER_LIMIT = 106L

'''<summary>
''' Program stopped because
''' alternate disk was not
''' inserted.
'''</summary>
ERROR_DISK_CHANGE = 107L

'''<summary>
''' The disk is in use or locked
''' by another process.
'''</summary>
ERROR_DRIVE_LOCKED = 108L

'''<summary>
''' The pipe was ended.
'''</summary>
ERROR_BROKEN_PIPE = 109L

'''<summary>
''' The system cannot open the
''' specified device or file.
'''</summary>
ERROR_OPEN_FAILED = 110L

'''<summary>
''' The file name is too long.
'''</summary>
ERROR_BUFFER_OVERFLOW = 111L

'''<summary>
''' There is not enough space on
''' the disk.
'''</summary>
ERROR_DISK_FULL = 112L

'''<summary>
''' No more internal file
''' identifiers available.
'''</summary>
ERROR_NO_MORE_SEARCH_HANDLES = 113L

'''<summary>
''' The target internal file
''' identifier is incorrect.
'''</summary>
ERROR_INVALID_TARGET_HANDLE = 114L

'''<summary>
''' The IOCTL call made by the
''' application program is
''' incorrect.
'''</summary>
ERROR_INVALID_CATEGORY = 117L

'''<summary>
''' The verify-on-write switch
''' parameter value is incorrect.
'''</summary>
ERROR_INVALID_VERIFY_SWITCH = 118L

'''<summary>
''' The system does not support
''' the requested command.
'''</summary>
ERROR_BAD_DRIVER_LEVEL = 119L

'''<summary>
''' The Application Program
''' Interface (API) entered will
''' only work in Windows/NT mode.
'''</summary>
ERROR_CALL_NOT_IMPLEMENTED = 120L

'''<summary>
''' The semaphore timeout period
''' has expired.
'''</summary>
ERROR_SEM_TIMEOUT = 121L

'''<summary>
''' The data area passed to a
''' system call is too small.
'''</summary>
ERROR_INSUFFICIENT_BUFFER = 122L

'''<summary>
''' The file name, directory name,
''' or volume label is
''' syntactically incorrect.
'''</summary>
ERROR_INVALID_NAME = 123L

'''<summary>
''' The system call level is
''' incorrect.
'''</summary>
ERROR_INVALID_LEVEL = 124L

'''<summary>
''' The disk has no volume label.
'''</summary>
ERROR_NO_VOLUME_LABEL = 125L

'''<summary>
''' The specified module cannot be
''' found.
'''</summary>
ERROR_MOD_NOT_FOUND = 126L

'''<summary>
''' The specified procedure could
''' not be found.
'''</summary>
ERROR_PROC_NOT_FOUND = 127L

'''<summary>
''' There are no child processes
''' to wait for.
'''</summary>
ERROR_WAIT_NO_CHILDREN = 128L

'''<summary>
''' The %1 application cannot be
''' run in Windows mode.
'''</summary>
ERROR_CHILD_NOT_COMPLETE = 129L

'''<summary>
''' Attempt to use a file handle
''' to an open disk partition for
''' an operation other than raw
''' disk I/O.
'''</summary>
ERROR_DIRECT_ACCESS_HANDLE = 130L

'''<summary>
''' An attempt was made to move
''' the file pointer before the
''' beginning of the file.
'''</summary>
ERROR_NEGATIVE_SEEK = 131L

'''<summary>
''' The file pointer cannot be set
''' on the specified device or
''' file.
'''</summary>
ERROR_SEEK_ON_DEVICE = 132L

'''<summary>
''' A JOIN or SUBST command cannot
''' be used for a drive that
''' contains previously joined
''' drives.
'''</summary>
ERROR_IS_JOIN_TARGET = 133L

'''<summary>
''' An attempt was made to use a
''' JOIN or SUBST command on a
''' drive that is already joined.
'''</summary>
ERROR_IS_JOINED = 134L

'''<summary>
''' An attempt was made to use a
''' JOIN or SUBST command on a
''' drive already substituted.
'''</summary>
ERROR_IS_SUBSTED = 135L

'''<summary>
''' The system attempted to delete
''' the JOIN of a drive not
''' previously joined.
'''</summary>
ERROR_NOT_JOINED = 136L

'''<summary>
''' The system attempted to delete
''' the substitution of a drive
''' not previously substituted.
'''</summary>
ERROR_NOT_SUBSTED = 137L

'''<summary>
''' The system tried to join a
''' drive to a directory on a
''' joined drive.
'''</summary>
ERROR_JOIN_TO_JOIN = 138L

'''<summary>
''' The system attempted to
''' substitute a drive to a
''' directory on a substituted
''' drive.
'''</summary>
ERROR_SUBST_TO_SUBST = 139L

'''<summary>
''' The system tried to join a
''' drive to a directory on a
''' substituted drive.
'''</summary>
ERROR_JOIN_TO_SUBST = 140L

'''<summary>
''' The system attempted to SUBST
''' a drive to a directory on a
''' joined drive.
'''</summary>
ERROR_SUBST_TO_JOIN = 141L

'''<summary>
''' The system cannot perform a
''' JOIN or SUBST at this time.
'''</summary>
ERROR_BUSY_DRIVE = 142L

'''<summary>
''' The system cannot join or
''' substitute a drive to or for a
''' directory on the same drive.
'''</summary>
ERROR_SAME_DRIVE = 143L

'''<summary>
''' The directory is not a
''' subdirectory of the root
''' directory.
'''</summary>
ERROR_DIR_NOT_ROOT = 144L

'''<summary>
''' The directory is not empty.
'''</summary>
ERROR_DIR_NOT_EMPTY = 145L

'''<summary>
''' The path specified is being
''' used in a substitute.
'''</summary>
ERROR_IS_SUBST_PATH = 146L

'''<summary>
''' Not enough resources are
''' available to process this
''' command.
'''</summary>
ERROR_IS_JOIN_PATH = 147L

'''<summary>
''' The specified path cannot be
''' used at this time.
'''</summary>
ERROR_PATH_BUSY = 148L

'''<summary>
''' An attempt was made to join or
''' substitute a drive for which a
''' directory on the drive is the
''' target of a previous
''' substitute.
'''</summary>
ERROR_IS_SUBST_TARGET = 149L

'''<summary>
''' System trace information not
''' specified in your CONFIG.SYS
''' file, or tracing is not
''' allowed.
'''</summary>
ERROR_SYSTEM_TRACE = 150L

'''<summary>
''' The number of specified
''' semaphore events is incorrect.
'''</summary>
ERROR_INVALID_EVENT_COUNT = 151L

'''<summary>
''' Too many semaphores are
''' already set.
'''</summary>
ERROR_TOO_MANY_MUXWAITERS = 152L

'''<summary>
''' The list is not correct.
'''</summary>
ERROR_INVALID_LIST_FORMAT = 153L

'''<summary>
''' The volume label entered
''' exceeds the 11 character
''' limit. The first 11 characters
''' were written to disk. Any
''' characters that exceeded the
''' 11 character limit were
''' automatically deleted.
'''</summary>
ERROR_LABEL_TOO_LONG = 154L

'''<summary>
''' Cannot create another thread.
'''</summary>
ERROR_TOO_MANY_TCBS = 155L

'''<summary>
''' The recipient process has
''' refused the signal.
'''</summary>
ERROR_SIGNAL_REFUSED = 156L

'''<summary>
''' The segment is already
''' discarded and cannot be
''' locked.
'''</summary>
ERROR_DISCARDED = 157L

'''<summary>
''' The segment is already
''' unlocked.
'''</summary>
ERROR_NOT_LOCKED = 158L

'''<summary>
''' The address for the thread ID
''' is incorrect.
'''</summary>
ERROR_BAD_THREADID_ADDR = 159L

'''<summary>
''' The argument string passed to
''' DosExecPgm is incorrect.
'''</summary>
ERROR_BAD_ARGUMENTS = 160L

'''<summary>
''' The specified path name is
''' invalid.
'''</summary>
ERROR_BAD_PATHNAME = 161L

'''<summary>
''' A signal is already pending.
'''</summary>
ERROR_SIGNAL_PENDING = 162L

'''<summary>
''' No more threads can be created
''' in the system.
'''</summary>
ERROR_MAX_THRDS_REACHED = 164L

'''<summary>
''' Attempt to lock a region of a
''' file failed.
'''</summary>
ERROR_LOCK_FAILED = 167L

'''<summary>
''' The requested resource is in
''' use.
'''</summary>
ERROR_BUSY = 170L

'''<summary>
''' A lock request was not
''' outstanding for the supplied
''' cancel region.
'''</summary>
ERROR_CANCEL_VIOLATION = 173L

'''<summary>
''' The file system does not
''' support atomic changing of the
''' lock type.
'''</summary>
ERROR_ATOMIC_LOCKS_NOT_SUPPORTED = 174L

'''<summary>
''' The system detected a segment
''' number that is incorrect.
'''</summary>
ERROR_INVALID_SEGMENT_NUMBER = 180L

'''<summary>
''' The operating system cannot
''' run %1.
'''</summary>
ERROR_INVALID_ORDINAL = 182L

'''<summary>
''' Attempt to create file that
''' already exists.
'''</summary>
ERROR_ALREADY_EXISTS = 183L

'''<summary>
''' The flag passed is incorrect.
'''</summary>
ERROR_INVALID_FLAG_NUMBER = 186L

'''<summary>
''' The specified system semaphore
''' name was not found.
'''</summary>
ERROR_SEM_NOT_FOUND = 187L

'''<summary>
''' The operating system cannot
''' run %1.
'''</summary>
ERROR_INVALID_STARTING_CODESEG = 188L

'''<summary>
''' The operating system cannot
''' run %1.
'''</summary>
ERROR_INVALID_STACKSEG = 189L

'''<summary>
''' The operating system cannot
''' run %1.
'''</summary>
ERROR_INVALID_MODULETYPE = 190L

'''<summary>
''' %1 cannot be run in Windows/NT
''' mode.
'''</summary>
ERROR_INVALID_EXE_SIGNATURE = 191L

'''<summary>
''' The operating system cannot
''' run %1.
'''</summary>
ERROR_EXE_MARKED_INVALID = 192L

'''<summary>
''' %1 is not a valid Windows-
''' based application.
'''</summary>
ERROR_BAD_EXE_FORMAT = 193L

'''<summary>
''' The operating system cannot
''' run %1.
'''</summary>
ERROR_ITERATED_DATA_EXCEEDS_64k = 194L

'''<summary>
''' The operating system cannot
''' run %1.
'''</summary>
ERROR_INVALID_MINALLOCSIZE = 195L

'''<summary>
''' The operating system cannot
''' run this application program.
'''</summary>
ERROR_DYNLINK_FROM_INVALID_RING = 196L

'''<summary>
''' The operating system is not
''' presently configured to run
''' this application.
'''</summary>
ERROR_IOPL_NOT_ENABLED = 197L

'''<summary>
''' The operating system cannot
''' run %1.
'''</summary>
ERROR_INVALID_SEGDPL = 198L

'''<summary>
''' The operating system cannot
''' run this application program.
'''</summary>
ERROR_AUTODATASEG_EXCEEDS_64k = 199L

'''<summary>
''' The code segment cannot be
''' greater than or equal to 64KB.
'''</summary>
ERROR_RING2SEG_MUST_BE_MOVABLE = 200L

'''<summary>
''' The operating system cannot
''' run %1.
'''</summary>
ERROR_RELOC_CHAIN_XEEDS_SEGLIM = 201L

'''<summary>
''' The operating system cannot
''' run %1.
'''</summary>
ERROR_INFLOOP_IN_RELOC_CHAIN = 202L

'''<summary>
''' The system could not find the
''' environment option entered.
'''</summary>
ERROR_ENVVAR_NOT_FOUND = 203L

'''<summary>
''' No process in the command
''' subtree has a signal handler.
'''</summary>
ERROR_NO_SIGNAL_SENT = 205L

'''<summary>
''' The file name or extension is
''' too long.
'''</summary>
ERROR_FILENAME_EXCED_RANGE = 206L

'''<summary>
''' The ring 2 stack is in use.
'''</summary>
ERROR_RING2_STACK_IN_USE = 207L

'''<summary>
''' The global filename characters
''' * or ? are entered
''' incorrectly, or too many
''' global filename characters are
''' specified.
'''</summary>
ERROR_META_EXPANSION_TOO_LONG = 208L

'''<summary>
''' The signal being posted is
''' incorrect.
'''</summary>
ERROR_INVALID_SIGNAL_NUMBER = 209L

'''<summary>
''' The signal handler cannot be
''' set.
'''</summary>
ERROR_THREAD_1_INACTIVE = 210L

'''<summary>
''' The segment is locked and
''' cannot be reallocated.
'''</summary>
ERROR_LOCKED = 212L

'''<summary>
''' Too many dynamic link modules
''' are attached to this program
''' or dynamic link module.
'''</summary>
ERROR_TOO_MANY_MODULES = 214L

'''<summary>
''' Can't nest calls to
''' LoadModule.
'''</summary>
ERROR_NESTING_NOT_ALLOWED = 215L

'''<summary>
''' The pipe state is invalid.
'''</summary>
ERROR_BAD_PIPE = 230L

'''<summary>
''' All pipe instances busy.
'''</summary>
ERROR_PIPE_BUSY = 231L

'''<summary>
''' Pipe close in progress.
'''</summary>
ERROR_NO_DATA = 232L

'''<summary>
''' No process on other end of
''' pipe.
'''</summary>
ERROR_PIPE_NOT_CONNECTED = 233L

'''<summary>
''' More data is available.
'''</summary>
ERROR_MORE_DATA = 234L

'''<summary>
''' The session was canceled.
'''</summary>
ERROR_VC_DISCONNECTED = 240L

'''<summary>
''' The specified EA name is
''' invalid.
'''</summary>
ERROR_INVALID_EA_NAME = 254L

'''<summary>
''' The EAs are inconsistent.
'''</summary>
ERROR_EA_LIST_INCONSISTENT = 255L

'''<summary>
''' No more data is available.
'''</summary>
ERROR_NO_MORE_ITEMS = 259L

'''<summary>
''' The Copy API cannot be used.
'''</summary>
ERROR_CANNOT_COPY = 266L

'''<summary>
''' The directory name is invalid.
'''</summary>
ERROR_DIRECTORY = 267L

'''<summary>
''' The EAs did not fit in the
''' buffer.
'''</summary>
ERROR_EAS_DIDNT_FIT = 275L

'''<summary>
''' The EA file on the mounted
''' file system is damaged.
'''</summary>
ERROR_EA_FILE_CORRUPT = 276L

'''<summary>
''' The EA table in the EA file on
''' the mounted file system is
''' full.
'''</summary>
ERROR_EA_TABLE_FULL = 277L

'''<summary>
''' The specified EA handle is
''' invalid.
'''</summary>
ERROR_INVALID_EA_HANDLE = 278L

'''<summary>
''' The mounted file system does
''' not support extended
''' attributes.
'''</summary>
ERROR_EAS_NOT_SUPPORTED = 282L

'''<summary>
''' Attempt to release mutex not
''' owned by caller.
'''</summary>
ERROR_NOT_OWNER = 288L

'''<summary>
''' Too many posts made to a
''' semaphore.
'''</summary>
ERROR_TOO_MANY_POSTS = 298L

'''<summary>
''' Only part of a
''' Read/WriteProcessMemory
''' request was completed.
'''</summary>
ERROR_PARTIAL_COPY = 299L

'''<summary>
''' The system cannot find message
''' for message number 0x%1 in
''' message file for %2.
'''</summary>
ERROR_MR_MID_NOT_FOUND = 317L

'''<summary>
''' Attempt to access invalid
''' address.
'''</summary>
ERROR_INVALID_ADDRESS = 487L

'''<summary>
''' Arithmetic result exceeded 32-
''' bits.
'''</summary>
ERROR_ARITHMETIC_OVERFLOW = 534L

'''<summary>
''' There is a process on other
''' end of the pipe.
'''</summary>
ERROR_PIPE_CONNECTED = 535L

'''<summary>
''' Waiting for a process to open
''' the other end of the pipe.
'''</summary>
ERROR_PIPE_LISTENING = 536L

'''<summary>
''' Access to the EA is denied.
'''</summary>
ERROR_EA_ACCESS_DENIED = 994L

'''<summary>
''' The I/O operation was aborted
''' due to either thread exit or
''' application request.
'''</summary>
ERROR_OPERATION_ABORTED = 995L

'''<summary>
''' Overlapped IO event not in
''' signaled state.
'''</summary>
ERROR_IO_INCOMPLETE = 996L

'''<summary>
''' Overlapped IO operation in
''' progress.
'''</summary>
ERROR_IO_PENDING = 997L

'''<summary>
''' Invalid access to memory
''' location.
'''</summary>
ERROR_NOACCESS = 998L

'''<summary>
''' Error accessing paging file.
'''</summary>
ERROR_SWAPERROR = 999L

'''<summary>
''' Recursion too deep, stack
''' overflowed.
'''</summary>
ERROR_STACK_OVERFLOW = 1001L

'''<summary>
''' Window can't handle sent
''' message.
'''</summary>
ERROR_INVALID_MESSAGE = 1002L

'''<summary>
''' Cannot complete function for
''' some reason.
'''</summary>
ERROR_CAN_NOT_COMPLETE = 1003L

'''<summary>
''' The flags are invalid.
'''</summary>
ERROR_INVALID_FLAGS = 1004L

'''<summary>
''' The volume does not contain a
''' recognized file system. Make
''' sure that all required file
''' system drivers are loaded and
''' the volume is not damaged.
'''</summary>
ERROR_UNRECOGNIZED_VOLUME = 1005L

'''<summary>
''' The volume for a file was
''' externally altered and the
''' opened file is no longer
''' valid.
'''</summary>
ERROR_FILE_INVALID = 1006L

'''<summary>
''' The requested operation cannot
''' be performed in full-screen
''' mode.
'''</summary>
ERROR_FULLSCREEN_MODE = 1007L

'''<summary>
''' An attempt was made to
''' reference a token that does
''' not exist.
'''</summary>
ERROR_NO_TOKEN = 1008L

'''<summary>
''' The configuration registry
''' database is damaged.
'''</summary>
ERROR_BADDB = 1009L

'''<summary>
''' The configuration registry key
''' is invalid.
'''</summary>
ERROR_BADKEY = 1010L

'''<summary>
''' The configuration registry key
''' cannot be opened.
'''</summary>
ERROR_CANTOPEN = 1011L

'''<summary>
''' The configuration registry key
''' cannot be read.
'''</summary>
ERROR_CANTREAD = 1012L

'''<summary>
''' The configuration registry key
''' cannot be written.
'''</summary>
ERROR_CANTWRITE = 1013L

'''<summary>
''' One of the files containing
''' the system's registry data had
''' to be recovered by use of a
''' log or alternate copy. The
''' recovery succeeded.
'''</summary>
ERROR_REGISTRY_RECOVERED = 1014L

'''<summary>
''' The registry is damaged. The
''' structure of one of the files
''' that contains registry data is
''' damaged, or the system's in
''' memory image of the file is
''' damaged, or the file could not
''' be recovered because its
''' alternate copy or log was
''' absent or damaged.
'''</summary>
ERROR_REGISTRY_CORRUPT = 1015L

'''<summary>
''' The registry initiated an I/O
''' operation that had an
''' unrecoverable failure. The
''' registry could not read in, or
''' write out, or flush, one of
''' the files that contain the
''' system's image of the
''' registry.
'''</summary>
ERROR_REGISTRY_IO_FAILED = 1016L

'''<summary>
''' The system attempted to load
''' or restore a file into the
''' registry, and the specified
''' file is not in the format of a
''' registry file.
'''</summary>
ERROR_NOT_REGISTRY_FILE = 1017L

'''<summary>
''' Illegal operation attempted on
''' a registry key that has been
''' marked for deletion.
'''</summary>
ERROR_KEY_DELETED = 1018L

'''<summary>
''' System could not allocate
''' required space in a registry
''' log.
'''</summary>
ERROR_NO_LOG_SPACE = 1019L

'''<summary>
''' An attempt was made to create
''' a symbolic link in a registry
''' key that already has subkeys
''' or values.
'''</summary>
ERROR_KEY_HAS_CHILDREN = 1020L

'''<summary>
''' An attempt was made to create
''' a stable subkey under a
''' volatile parent key.
'''</summary>
ERROR_CHILD_MUST_BE_VOLATILE = 1021L

'''<summary>
''' This indicates that a notify
''' change request is being
''' completed and the information
''' is not being returned in the
''' caller's buffer. The caller
''' now needs to enumerate the
''' files to find the changes.
'''</summary>
ERROR_NOTIFY_ENUM_DIR = 1022L

'''<summary>
''' A stop control has been sent
''' to a service which other
''' running services are dependent
''' on.
'''</summary>
ERROR_DEPENDENT_SERVICES_RUNNING = 1051L

'''<summary>
''' The requested control is not
''' valid for this service.
'''</summary>
ERROR_INVALID_SERVICE_CONTROL = 1052L

'''<summary>
''' The service did not respond to
''' the start or control request
''' in a timely fashion.
'''</summary>
ERROR_SERVICE_REQUEST_TIMEOUT = 1053L

'''<summary>
''' A thread could not be created
''' for the service.
'''</summary>
ERROR_SERVICE_NO_THREAD = 1054L

'''<summary>
''' The service database is
''' locked.
'''</summary>
ERROR_SERVICE_DATABASE_LOCKED = 1055L

'''<summary>
''' An instance of the service is
''' already running.
'''</summary>
ERROR_SERVICE_ALREADY_RUNNING = 1056L

'''<summary>
''' The account name is invalid or
''' does not exist.
'''</summary>
ERROR_INVALID_SERVICE_ACCOUNT = 1057L

'''<summary>
''' The specified service is
''' disabled and cannot be
''' started.
'''</summary>
ERROR_SERVICE_DISABLED = 1058L

'''<summary>
''' Circular service dependency
''' was specified.
'''</summary>
ERROR_CIRCULAR_DEPENDENCY = 1059L

'''<summary>
''' The specified service does not
''' exist as an installed service.
'''</summary>
ERROR_SERVICE_DOES_NOT_EXIST = 1060L

'''<summary>
''' The service cannot accept
''' control messages at this time.
'''</summary>
ERROR_SERVICE_CANNOT_ACCEPT_CTRL = 1061L

'''<summary>
''' The service has not been
''' started.
'''</summary>
ERROR_SERVICE_NOT_ACTIVE = 1062L

'''<summary>
''' The service process could
''' not connect to the service
''' controller.
'''</summary>
ERROR_FAILED_SERVICE_CONTROLLER_CONNECT = 1063L

'''<summary>
''' An exception occurred in the
''' service when handling the
''' control request.
'''</summary>
ERROR_EXCEPTION_IN_SERVICE = 1064L

'''<summary>
''' The database specified does
''' not exist.
'''</summary>
ERROR_DATABASE_DOES_NOT_EXIST = 1065L

'''<summary>
''' The service has returned a
''' service-specific error code.
'''</summary>
ERROR_SERVICE_SPECIFIC_ERROR = 1066L

'''<summary>
''' The process terminated
''' unexpectedly.
'''</summary>
ERROR_PROCESS_ABORTED = 1067L

'''<summary>
''' The dependency service or
''' group failed to start.
'''</summary>
ERROR_SERVICE_DEPENDENCY_FAIL = 1068L

'''<summary>
''' The service did not start due
''' to a logon failure.
'''</summary>
ERROR_SERVICE_LOGON_FAILED = 1069L

'''<summary>
''' After starting, the service
''' hung in a start-pending state.
'''</summary>
ERROR_SERVICE_START_HANG = 1070L

'''<summary>
''' The specified service database
''' lock is invalid.
'''</summary>
ERROR_INVALID_SERVICE_LOCK = 1071L

'''<summary>
''' The specified service has been
''' marked for deletion.
'''</summary>
ERROR_SERVICE_MARKED_FOR_DELETE = 1072L

'''<summary>
''' The specified service already
''' exists.
'''</summary>
ERROR_SERVICE_EXISTS = 1073L

'''<summary>
''' The system is currently
''' running with the last-known-
''' good configuration.
'''</summary>
ERROR_ALREADY_RUNNING_LKG = 1074L

'''<summary>
''' The dependency service does
''' not exist or has been marked
''' for deletion.
'''</summary>
ERROR_SERVICE_DEPENDENCY_DELETED = 1075L

'''<summary>
''' The current boot has already
''' been accepted for use as the
''' last-known-good control set.
'''</summary>
ERROR_BOOT_ALREADY_ACCEPTED = 1076L

'''<summary>
''' No attempts to start the
''' service have been made since
''' the last boot.
'''</summary>
ERROR_SERVICE_NEVER_STARTED = 1077L

'''<summary>
''' The name is already in use as
''' either a service name or a
''' service display name.
'''</summary>
ERROR_DUPLICATE_SERVICE_NAME = 1078L

'''<summary>
''' The account specified for this
''' service is different from the
''' account specified for other
''' services running in the same
''' process.
'''</summary>
ERROR_DIFFERENT_SERVICE_ACCOUNT = 1079L

'''<summary>
''' The physical end of the tape
''' has been reached.
'''</summary>
ERROR_END_OF_MEDIA = 1100L

'''<summary>
''' A tape access reached a
''' filemark.
'''</summary>
ERROR_FILEMARK_DETECTED = 1101L

'''<summary>
''' The beginning of the tape or
''' partition was encountered.
'''</summary>
ERROR_BEGINNING_OF_MEDIA = 1102L

'''<summary>
''' A tape access reached a
''' setmark.
'''</summary>
ERROR_SETMARK_DETECTED = 1103L

'''<summary>
''' During a tape access, the end
''' of the data marker was
''' reached.
'''</summary>
ERROR_NO_DATA_DETECTED = 1104L

'''<summary>
''' Tape could not be partitioned.
'''</summary>
ERROR_PARTITION_FAILURE = 1105L

'''<summary>
''' When accessing a new tape of a
''' multivolume partition, the
''' current block size is
''' incorrect.
'''</summary>
ERROR_INVALID_BLOCK_LENGTH = 1106L

'''<summary>
''' Tape partition information
''' could not be found when
''' loading a tape.
'''</summary>
ERROR_DEVICE_NOT_PARTITIONED = 1107L

'''<summary>
''' Attempt to lock the eject
''' media mechanism failed.
'''</summary>
ERROR_UNABLE_TO_LOCK_MEDIA = 1108L

'''<summary>
''' Unload media failed.
'''</summary>
ERROR_UNABLE_TO_UNLOAD_MEDIA = 1109L

'''<summary>
''' Media in drive may have
''' changed.
'''</summary>
ERROR_MEDIA_CHANGED = 1110L

'''<summary>
''' The I/O bus was reset.
'''</summary>
ERROR_BUS_RESET = 1111L

'''<summary>
''' Tape query failed because of
''' no media in drive.
'''</summary>
ERROR_NO_MEDIA_IN_DRIVE = 1112L

'''<summary>
''' No mapping for the Unicode
''' character exists in the target
''' multi-byte code page.
'''</summary>
ERROR_NO_UNICODE_TRANSLATION = 1113L

'''<summary>
''' A DLL initialization routine
''' failed.
'''</summary>
ERROR_DLL_INIT_FAILED = 1114L

'''<summary>
''' A system shutdown is in
''' progress.
'''</summary>
ERROR_SHUTDOWN_IN_PROGRESS = 1115L

'''<summary>
''' An attempt to abort the
''' shutdown of the system failed
''' because no shutdown was in
''' progress.
'''</summary>
ERROR_NO_SHUTDOWN_IN_PROGRESS = 1116L

'''<summary>
''' The request could not be
''' performed because of an I/O
''' device error.
'''</summary>
ERROR_IO_DEVICE = 1117L

'''<summary>
''' No serial device was
''' successfully initialized. The
''' serial driver will unload.
'''</summary>
ERROR_SERIAL_NO_DEVICE = 1118L

'''<summary>
''' Unable to open a device that
''' was sharing an interrupt
''' request (IRQ) with other
''' devices. At least one other
''' device that uses that IRQ was
''' already opened.
'''</summary>
ERROR_IRQ_BUSY = 1119L

'''<summary>
''' A serial I/O operation was
''' completed by another write to
''' the serial port. (The
''' IOCTL_SERIAL_XOFF_COUNTER
''' reached zero.)
'''</summary>
ERROR_MORE_WRITES = 1120L

'''<summary>
''' A serial I/O operation
''' completed because the time-out
''' period expired. (The
''' IOCTL_SERIAL_XOFF_COUNTER did
''' not reach zero.)
'''</summary>
ERROR_COUNTER_TIMEOUT = 1121L

'''<summary>
''' No ID address mark was found
''' on the floppy disk.
'''</summary>
ERROR_FLOPPY_ID_MARK_NOT_FOUND = 1122L

'''<summary>
''' Mismatch between the floppy
''' disk sector ID field and the
''' floppy disk controller track
''' address.
'''</summary>
ERROR_FLOPPY_WRONG_CYLINDER = 1123L

'''<summary>
''' The floppy disk controller
''' reported an error that is not
''' recognized by the floppy disk
''' driver.
'''</summary>
ERROR_FLOPPY_UNKNOWN_ERROR = 1124L

'''<summary>
''' The floppy disk controller
''' returned inconsistent results
''' in its registers.
'''</summary>
ERROR_FLOPPY_BAD_REGISTERS = 1125L

'''<summary>
''' While accessing the hard disk,
''' a recalibrate operation
''' failed, even after retries.
'''</summary>
ERROR_DISK_RECALIBRATE_FAILED = 1126L

'''<summary>
''' While accessing the hard disk,
''' a disk operation failed even
''' after retries.
'''</summary>
ERROR_DISK_OPERATION_FAILED = 1127L

'''<summary>
''' While accessing the hard disk,
''' a disk controller reset was
''' needed, but even that failed.
'''</summary>
ERROR_DISK_RESET_FAILED = 1128L

'''<summary>
''' Physical end of tape
''' encountered.
'''</summary>
ERROR_EOM_OVERFLOW = 1129L

'''<summary>
''' Not enough server storage is
''' available to process this
''' command.
'''</summary>
ERROR_NOT_ENOUGH_SERVER_MEMORY = 1130L

'''<summary>
''' A potential deadlock condition
''' has been detected.
'''</summary>
ERROR_POSSIBLE_DEADLOCK = 1131L

'''<summary>
''' The base address or the file
''' offset specified does not have
''' the proper alignment.
'''</summary>
ERROR_MAPPED_ALIGNMENT = 1132L

'''<summary>
''' An attempt to change the
''' system power state was vetoed
''' by another application or
''' driver.
'''</summary>
ERROR_SET_POWER_STATE_VETOED = 1140L

'''<summary>
''' The system BIOS failed an
''' attempt to change the system
''' power state.
'''</summary>
ERROR_SET_POWER_STATE_FAILED = 1141L

'''<summary>
''' An attempt was made to create
''' more links on a file than the
''' file system supports.
'''</summary>
ERROR_TOO_MANY_LINKS = 1142L

'''<summary>
''' The specified program requires
''' a newer version of Windows.
'''</summary>
ERROR_OLD_WIN_VERSION = 1150L

'''<summary>
''' The specified program is not a
''' Windows or MS-DOS program.
'''</summary>
ERROR_APP_WRONG_OS = 1151L

'''<summary>
''' Cannot start more than one
''' instance of the specified
''' program.
'''</summary>
ERROR_SINGLE_INSTANCE_APP = 1152L

'''<summary>
''' The specified program was
''' written for an older version
''' of Windows.
'''</summary>
ERROR_RMODE_APP = 1153L

'''<summary>
''' One of the library files
''' needed to run this application
''' is damaged.
'''</summary>
ERROR_INVALID_DLL = 1154L

'''<summary>
''' No application is associated
''' with the specified file for
''' this operation.
'''</summary>
ERROR_NO_ASSOCIATION = 1155L

'''<summary>
''' An error occurred in sending
''' the command to the
''' application.
'''</summary>
ERROR_DDE_FAIL = 1156L

'''<summary>
''' One of the library files
''' needed to run this application
''' cannot be found.
'''</summary>
ERROR_DLL_NOT_FOUND = 1157L

'''<summary>
''' The specified device name is
''' invalid.
'''</summary>
ERROR_BAD_DEVICE = 1200L

'''<summary>
''' The device is not currently
''' connected but is a remembered
''' connection.
'''</summary>
ERROR_CONNECTION_UNAVAIL = 1201L

'''<summary>
''' An attempt was made to
''' remember a device that was
''' previously remembered.
'''</summary>
ERROR_DEVICE_ALREADY_REMEMBERED = 1202L

'''<summary>
''' No network provider accepted
''' the given network path.
'''</summary>
ERROR_NO_NET_OR_BAD_PATH = 1203L

'''<summary>
''' The specified network provider
''' name is invalid.
'''</summary>
ERROR_BAD_PROVIDER = 1204L

'''<summary>
''' Unable to open the network
''' connection profile.
'''</summary>
ERROR_CANNOT_OPEN_PROFILE = 1205L

'''<summary>
''' The network connection profile
''' is damaged.
'''</summary>
ERROR_BAD_PROFILE = 1206L

'''<summary>
''' Cannot enumerate a non-
''' container.
'''</summary>
ERROR_NOT_CONTAINER = 1207L

'''<summary>
''' An extended error has
''' occurred.
'''</summary>
ERROR_EXTENDED_ERROR = 1208L

'''<summary>
''' The format of the specified
''' group name is invalid.
'''</summary>
ERROR_INVALID_GROUPNAME = 1209L

'''<summary>
''' The format of the specified
''' computer name is invalid.
'''</summary>
ERROR_INVALID_COMPUTERNAME = 1210L

'''<summary>
''' The format of the specified
''' event name is invalid.
'''</summary>
ERROR_INVALID_EVENTNAME = 1211L

'''<summary>
''' The format of the specified
''' domain name is invalid.
'''</summary>
ERROR_INVALID_DOMAINNAME = 1212L

'''<summary>
''' The format of the specified
''' service name is invalid.
'''</summary>
ERROR_INVALID_SERVICENAME = 1213L

'''<summary>
''' The format of the specified
''' network name is invalid.
'''</summary>
ERROR_INVALID_NETNAME = 1214L

'''<summary>
''' The format of the specified
''' share name is invalid.
'''</summary>
ERROR_INVALID_SHARENAME = 1215L

'''<summary>
''' The format of the specified
''' password is invalid.
'''</summary>
ERROR_INVALID_PASSWORDNAME = 1216L

'''<summary>
''' The format of the specified
''' message name is invalid.
'''</summary>
ERROR_INVALID_MESSAGENAME = 1217L

'''<summary>
''' The format of the specified
''' message destination is
''' invalid.
'''</summary>
ERROR_INVALID_MESSAGEDEST = 1218L

'''<summary>
''' The credentials supplied
''' conflict with an existing set
''' of credentials.
'''</summary>
ERROR_SESSION_CREDENTIAL_CONFLICT = 1219L

'''<summary>
''' An attempt was made to
''' establish a session to a LAN
''' Manager server, but there are
''' already too many sessions
''' established to that server.
'''</summary>
ERROR_REMOTE_SESSION_LIMIT_EXCEEDED = 1220L

'''<summary>
''' The workgroup or domain name
''' is already in use by another
''' computer on the network.
'''</summary>
ERROR_DUP_DOMAINNAME = 1221L

'''<summary>
''' The network is not present or
''' not started.
'''</summary>
ERROR_NO_NETWORK = 1222L

'''<summary>
''' The operation was cancelled by
''' the user.
'''</summary>
ERROR_CANCELLED = 1223L

'''<summary>
''' The requested operation cannot
''' be performed on a file with a
''' user mapped section open.
'''</summary>
ERROR_USER_MAPPED_FILE = 1224L

'''<summary>
''' The remote system refused the
''' network connection.
'''</summary>
ERROR_CONNECTION_REFUSED = 1225L

'''<summary>
''' The network connection was
''' gracefully closed.
'''</summary>
ERROR_GRACEFUL_DISCONNECT = 1226L

'''<summary>
''' The network transport endpoint
''' already has an address
''' associated with it.
'''</summary>
ERROR_ADDRESS_ALREADY_ASSOCIATED = 1227L

'''<summary>
''' An address has not yet been
''' associated with the network
''' endpoint.
'''</summary>
ERROR_ADDRESS_NOT_ASSOCIATED = 1228L

'''<summary>
''' An operation was attempted on
''' a non-existent network
''' connection.
'''</summary>
ERROR_CONNECTION_INVALID = 1229L

'''<summary>
''' An invalid operation was
''' attempted on an active network
''' connection.
'''</summary>
ERROR_CONNECTION_ACTIVE = 1230L

'''<summary>
''' The remote network is not
''' reachable by the transport.
'''</summary>
ERROR_NETWORK_UNREACHABLE = 1231L

'''<summary>
''' The remote system is not
''' reachable by the transport.
'''</summary>
ERROR_HOST_UNREACHABLE = 1232L

'''<summary>
''' The remote system does not
''' support the transport
''' protocol.
'''</summary>
ERROR_PROTOCOL_UNREACHABLE = 1233L

'''<summary>
''' No service is operating at the
''' destination network endpoint
''' on the remote system.
'''</summary>
ERROR_PORT_UNREACHABLE = 1234L

'''<summary>
''' The request was aborted.
'''</summary>
ERROR_REQUEST_ABORTED = 1235L

'''<summary>
''' The network connection was
''' aborted by the local system.
'''</summary>
ERROR_CONNECTION_ABORTED = 1236L

'''<summary>
''' The operation could not be
''' completed. A retry should be
''' performed.
'''</summary>
ERROR_RETRY = 1237L

'''<summary>
''' A connection to the server
''' could not be made because the
''' limit on the number of
''' concurrent connections for
''' this account has been reached.
'''</summary>
ERROR_CONNECTION_COUNT_LIMIT = 1238L

'''<summary>
''' Attempting to login during an
''' unauthorized time of day for
''' this account.
'''</summary>
ERROR_LOGIN_TIME_RESTRICTION = 1239L

'''<summary>
''' The account is not authorized
''' to login from this station.
'''</summary>
ERROR_LOGIN_WKSTA_RESTRICTION = 1240L

'''<summary>
''' The network address could not
''' be used for the operation
''' requested.
'''</summary>
ERROR_INCORRECT_ADDRESS = 1241L

'''<summary>
''' The service is already
''' registered.
'''</summary>
ERROR_ALREADY_REGISTERED = 1242L

'''<summary>
''' The specified service does not
''' exist.
'''</summary>
ERROR_SERVICE_NOT_FOUND = 1243L

'''<summary>
''' The operation being requested
''' was not performed because the
''' user has not been
''' authenticated.
'''</summary>
ERROR_NOT_AUTHENTICATED = 1244L

'''<summary>
''' The operation being requested
''' was not performed because the
''' user has not logged on to the
''' network.
'''</summary>
ERROR_NOT_LOGGED_ON = 1245L

'''<summary>
''' Return that wants caller to
''' continue with work in
''' progress.
'''</summary>
ERROR_CONTINUE = 1246L

'''<summary>
''' An attempt was made to perform
''' an initialization operation
''' when initialization has
''' already been completed.
'''</summary>
ERROR_ALREADY_INITIALIZED = 1247L

'''<summary>
''' No more local devices.
'''</summary>
ERROR_NO_MORE_DEVICES = 1248L

End Enum
