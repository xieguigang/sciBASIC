@echo off

git pull local HEAD
git pull gitee HEAD

git push local HEAD
git push gitee HEAD

echo synchronization of this code repository job done!