# -*- coding: utf8 -*-

import sys
import os

def readPbFile():
    pbFileList = {}
    endLine = 0
    pb_path = os.getcwd() + "\\Assets\\LuaFramework\\Lua\\protocol\\pbc"
    for root,dirs,files in os.walk(pb_path):
        for f in files:
            if f.endswith(".pb"):
                full_path = os.path.join(root,f)
                pbFileList[endLine] = full_path[full_path.find("\\protocol\\pbc") + 14:].replace('\\','/')
                print "get pb file: " + pbFileList[endLine]
                endLine += 1
    return pbFileList, endLine

def readHandlers():
    handlerFileList = {}
    endLine = 0
    handler_path = os.getcwd() + "\\Assets\\LuaFramework\\Lua\\protocol\\handler"
    for f in os.listdir(handler_path):
        if f.endswith(".lua"):
            handlerFileList[endLine] = f
            print "get handler file: " + handlerFileList[endLine]
            endLine += 1
    return handlerFileList, endLine

def genrateInit(path):
    print "Generate " + path + " begin.."
    content = "---\n-- 由外部脚本工具gen_pbc_handler.py 生成,不要手动修改\n--\n\n"
    return content

def save(path, content):
    luaFile_object = open(path, "wb").write(content)
    print "generate " + path + " end..."

def generatePbcs():
    fileName = os.getcwd() + "\\Assets\\LuaFramework\\Lua\\protocol\\pbcs.lua"
    content = genrateInit(fileName)
    content += "local protobuf = require \"protobuf\"\nlocal function GetFullPath(path)\n\treturn Util.LuaPath..path\nend\n\nmodule(\'protocol.pbcs\')\n\n"
    _pbFileList, endLine = readPbFile()
    if not _pbFileList or not endLine:
        print "generate " + fileName + " failed"
        return
    for i in _pbFileList:
        content += "protobuf.register_file(GetFullPath(\'protocol/pbc/%s\'))"%(_pbFileList[i])
        content += "\n"
    save(fileName, content)

def generateHandlers():
    fileName = os.getcwd() + "\\Assets\\LuaFramework\\Lua\\protocol\\handlers.txt"
    content = ""
    _handlerFileList, endLine = readHandlers()
    if not _handlerFileList or not endLine:
        print "no handler files!!"
    for i in _handlerFileList:
        content += _handlerFileList[i].split('.')[0]
        if i != endLine - 1:
            content += ";"
    save(fileName, content)

def main():
    generatePbcs()
    generateHandlers()

main()
