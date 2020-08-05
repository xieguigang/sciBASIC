#!/bin/bash

# Get the source directory of a Bash script from within the script itself
#
# https://stackoverflow.com/questions/59895/get-the-source-directory-of-a-bash-script-from-within-the-script-itself
#
# #!/bin/bash
#
# DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
#
# is a useful one-liner which will give you the full directory name of the script 
# no matter where it is being called from.
#
# It will work as long as the last component of the path used to find the script 
# is not a symlink (directory links are OK). If you also want to resolve any links 
# to the script itself, you need a multi-line solution:
#

# target script may be a symlink
# so the vb application may not exists in the 
# same directory with current script's symlink
SOURCE="${BASH_SOURCE[0]}"

# resolve $SOURCE until the file is no longer a symlink
while [ -h "$SOURCE" ]; do 
  TARGET="$(readlink "$SOURCE")"

  if [[ $TARGET == /* ]]; then
    # echo "SOURCE '$SOURCE' is an absolute symlink to '$TARGET'"
    SOURCE="$TARGET"
  else
    DIR="$( dirname "$SOURCE" )"
    
    # echo "SOURCE '$SOURCE' is a relative symlink to '$TARGET' (relative to '$DIR')"
    
    # if $SOURCE was a relative symlink, we need to resolve it 
    # relative to the path where the symlink file was located
    SOURCE="$DIR/$TARGET" 
  fi
done

# echo "SOURCE is '$SOURCE'"

# This last one will work with any combination of 
# aliases, source, bash -c, symlinks, etc.

# Watch out for $CDPATH gotchas, and stderr output side effects if the user has 
# smartly overridden cd to redirect output to stderr instead (including escape 
# sequences, such as when calling  update_terminal_cwd >&2 on Mac). 
# Adding >/dev/null 2>&1 at the end of your cd command will take care of both 
# possibilities.
RDIR="$( dirname "$SOURCE" )"
DIR="$( cd -P "$( dirname "$SOURCE" )" >/dev/null 2>&1 && pwd )"

# if [ "$DIR" != "$RDIR" ]; then
#  echo "DIR '$RDIR' resolves to '$DIR'"
# fi

# echo "DIR is '$DIR'"