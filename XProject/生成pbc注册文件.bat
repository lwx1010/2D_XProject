@echo off

python gen_pbc_handler.py

xcopy "%~dp0\protocol\pbc" "%~dp0\Assets\LuaFramework\Lua\protocol\pbc" /c/q/e/y
xcopy "%~dp0\protocol\proto.conf" "%~dp0\Assets\LuaFramework\Lua\protocol\proto.conf" /c/q/e/y

pause 
