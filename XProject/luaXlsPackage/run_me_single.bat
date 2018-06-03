
SET BIN_PATH=%~dp0
SET XLS_FILE=%~dp0\xls\
SET OUT_FILE=%~dp0\gen\data\
SET CFG_PATH=%~dp0\gen\cfg
SET OUT_CLASS_FILE=%~dp0\gen\dataclass\
Set /p a=请输入要导的表：
 
::md %OUT_FILE% %OUT_CLASS_FILE% 

cd %BIN_PATH% 

:: 生成json数据文件及c#类
lua genTest.lua %XLS_FILE% %OUT_FILE% %CFG_PATH% %OUT_CLASS_FILE% %a%
pause