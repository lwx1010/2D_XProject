dofile("autocode/gen/read_xls/read_xls.lua")
dofile("base/class.lua")
dofile("base/extend.lua")
dofile("global/util.lua")


clsXlsBase = clsObject:Inherit{__ClassType = "XLS_BASE"}


function clsXlsBase:__init__(InFile, VarRow, TypeRow, NullRow, VarDesc)
	Super(clsXlsBase).__init__(self, InFile, VarRow, TypeRow)
	self._VarRow     = VarRow
	self._TypeRow    = TypeRow
	self._NullRow    = NullRow
	self._VarDesc 	 = VarDesc
	self._OutFile    = nil
	self._Vars       = {}
	self._Types      = {}
	self._NotNullCols= {}
	self._VarDescs 	 = {}
	self._sheetInfoTip = {}
	self._Data       = {}
	self._SaveData = {}
	self._XlsInst    = read_xls(InFile)
	self._InFile = InFile
	self._VarFunctions = {}
end


function clsXlsBase:Destroy()
	os.execute("rm middle_lua.tmp")
	os.execute("rm middle_lua_convert.tmp")
end


function clsXlsBase:XlsToLua(Sheet, OutFile, DataStart, TableName)
	--_RUNTIME("XlsToLua...",Sheet, OutFile, DataStart, TableName)
	self._OutFile = OutFile
	self:_ParseVars(Sheet)
	self:_ParseTypes(Sheet)
	self:_NotNullCells(Sheet)
	self:_GenData(Sheet, DataStart)
	self:_WriteFile(TableName)
	return self._Data
end


function clsXlsBase:_ReadRow(Sheet, Row, Table)
	local RowData = self._XlsInst:get_cell(Sheet, Row, "*")
	local MaxRow, MaxCol = self._XlsInst:get_sheet_size(Sheet)
	for i = 1, MaxCol do
		if RowData[i] then
			Table[i] = RowData[i]
		end
	end
end


function clsXlsBase:_ParseVars(Sheet,RowNo,Vars)
	--self:_ReadRow(Sheet, self._VarRow, self._Vars)
	local RowNo = RowNo or self._VarRow
	local Vars = Vars or self._Vars
	
	self:_ReadRow(Sheet, RowNo, Vars)
	local MaxRow, MaxCol = self._XlsInst:get_sheet_size(Sheet)
	for i = 1, MaxCol do
		if Vars[i] then
			Vars[i] = string.strip(Vars[i])
		end
	end
	
	--_DEBUG("vars:", Serialize(Vars))
end


function clsXlsBase:_ParseTypes(Sheet,RowNo,Vars)
	--self:_ReadRow(Sheet, self._TypeRow, self._Types)
	
	local RowNo = RowNo or self._TypeRow
	local Vars = Vars or self._Types
	
	self:_ReadRow(Sheet, RowNo, Vars)
	
	local MaxRow, MaxCol = self._XlsInst:get_sheet_size(Sheet)
	for i = 1, MaxCol do
		if Vars[i] then
			Vars[i] = string.strip(Vars[i])
		end
	end
	
	--_DEBUG("types: ", Serialize(Vars))
end


function clsXlsBase:_NotNullCells(Sheet,RowNo,Vars)
	
	--local RowData = self._XlsInst:get_cell(Sheet, self._NullRow, "*")
	
	local RowNo = RowNo or self._NullRow
	local Vars = Vars or self._NotNullCols
	
	local RowData = self._XlsInst:get_cell(Sheet, RowNo, "*")
	
	local MaxRow, MaxCol = self._XlsInst:get_sheet_size(Sheet)
	for Col = 1, MaxCol do
		if type(RowData[Col]) == "string" and
			RowData[Col] == "notnull" then
			Vars[Col] = true
		end
	end
	--_DEBUG("not null cols", Serialize(Vars))
end

function clsXlsBase:_ParseVarDescs(Sheet, RowNo, Vars)
	local RowNo = RowNo or self._VarDesc
	local Vars = Vars or Self._VarDescs
	
	local RowData = self._XlsInst:get_cell(Sheet, RowNo, "*")
	local MaxRow, MaxCol = self._XlsInst:get_sheet_size(Sheet)
	for Col = 1, MaxCol do
		Vars[Col] = RowData[Col]
	end
end


function clsXlsBase:_GenData(Sheet, DataStart)
	local MaxRow, MaxCol = self._XlsInst:get_sheet_size(Sheet)
	assert(DataStart <= MaxRow, "DataStart > MaxRow")
	for Row = DataStart, MaxRow do
		local RowTable = {}
		for Col = 1, MaxCol do
			local Value = self._XlsInst:get_cell(Sheet, Row, Col)
			local Var   = self._Vars[Col]
			local Type  = self._Types[Col]
			local ParsedValue = self:_ParseCell(Var, Type, Value)
			if ParsedValue then
				RowTable[Var] = ParsedValue
			elseif self._NotNullCols[Col] then
				local sheet_name  = self._XlsInst:get_sheet_name_by_index(Sheet)
				LogStr = string.format("[WARNING]%s %s cell value (sheet_index=%d, rowno=%d, colno=%d) must not be null",
					self._InFile,sheet_name,Sheet, Row, Col)
				--_DEBUG(LogStr)
				assert(false, LogStr)
				break
			end
		end
		table.insert(self._Data, RowTable)
	end
end

local function IsFuncStr(s)
	if string.find(s, "^[ ]*function[ ]+(.*)[ ]+.*end[ ]*$") then
		return true
	end
	return false
end
local function IsTableStr(s)
	-- 没有严格判断
	if string.find(s, "^[ ]*\{.*\}[ ]*$") then
		return true
	end
	return false
end

--设置变量函数 注意每个sheet必须唯一
function clsXlsBase:SetVarFunction(Var, FunctionStr)
	self._VarFunctions[Var] = FunctionStr
end

function clsXlsBase:_ParseCell(Var, Type, Value)
	if not Var or not Type then
		return nil
	end
	if not Value then
		return nil
	end
	
	if Type == "string" then
		if string.find(Value,"^%d+%.%d+$") then
			Value = tonumber(Value)
		end
		local str = tostring(Value)
		local s, e = string.find(str, "^&&&%[")
		if s and e then
			str = string.sub(str, e)
		end
		s, e = string.find(str, "%]&&&$")
		if s and e then
			str = string.sub(str, 1, s)
		end		
		return str
	end
	if Type == "table" then
		if IsTableStr(Value) then 
				fn,err = loadstring("return "..Value)
				Value = fn()
				return Value
		else 
			return nil
		end
	end
	if Type == "float" then
		return tonumber(Value)
	end
	if Type == "number" then
		return tonumber(Value)
	end
	if Type == "numbers" then
		local StrTable = string.split(Value, ",")
		local ValueTable = {}
		for _, Str in pairs(StrTable) do
			table.insert(ValueTable, tonumber(Str))
		end
		return ValueTable
	end
	if Type == "function" and Value~="" then
		if not self._VarFunctions[Var] then 
			if string.find(Value,"^%d+%.%d+$") then
				Value = tonumber(Value)
			end
			local str = tostring(Value)
			local s, e = string.find(str, "^&&&%[")
			if s and e then
				str = string.sub(str, e)
			end
			s, e = string.find(str, "%]&&&$")
			if s and e then
				str = string.sub(str, 1, s)
			end		
			return str			
		end
		
		assert(self._VarFunctions[Var], "Please Set "..Var.." FunctionStr...")
		assert(not string.find(Value,"<func>"), "Cell Value can not Contain <func>")
		local func = string.format(self._VarFunctions[Var], Value)
		--print(func)
		func = string.gsub(func, "\n", "")
		func = string.gsub(func, "，", ",")
		func = string.gsub(func, "\"", "'")
		
		if string.find(func,"^%d+%.%d+$") then
			func = tonumber(func)
		end
		
--		local _,func_err=loadstring("return"..func)
--		if func_err then
--			print(func)
--			print(func_err)
--			print(string.format("[WARNING]%s %s cell value (sheet_index=%d, rowno=%d, colno=%d) function error!",
--					self._InFile,sheet_name,Sheet, Row, Col))
--		end
		
		return "<func>"..func.."<func>"
	end
end

function clsXlsBase:_ParseTipCell(Var, Type, IsNull, VarDesc)
	if not Var or not Type or not VarDesc then
		return nil
	end
	
	local	ValueTable = {
		["var"] = Var, 
		["type"] = Type, 
		["isNull"] = IsNull, 
		["varDesc"] = VarDesc,
	}
	
	return ValueTable
end

local function Get_DEBUGTable(n)
	local Str = ""
	if n < 3 then 
		for i = 1, n do	
			Str = Str .. "\t"
		end
	else 
		Str = " "
	end
	return Str
end


function DumpTbl(Tbl, Deep)
	if not Deep then
		Deep = 0
	end
	assert(Deep <= 9, "DumpTbl. recursion is too deep")
	
	if Deep == 0 then
		if type(Tbl) ~= "table" then 
			local ResultStr = Serialize(Tbl) .."\n"
			return ResultStr
		end
	end
	local ResultStr = "{\n"
	if Deep >1 then 
		ResultStr = "{"
	end 
	for i, v in pairs(Tbl) do
		ResultStr = ResultStr .. Get_DEBUGTable(Deep +
			1).. "[" .. Serialize(i) .. "] = "
		if type(v) == "table" then
			ResultStr = ResultStr .. DumpTbl(v, Deep + 1)
		else
			ResultStr = ResultStr .. Serialize(v)
		end
		if Deep < 2 then 
			ResultStr = ResultStr .. ",\n"
		else 
			ResultStr = ResultStr .. ","			
		end
	end
	ResultStr = ResultStr .. Get_DEBUGTable(Deep) .. "}"
	return ResultStr
end


function clsXlsBase:_WriteFile(TableName)
	File = io.open(self._OutFile, "w")
	--_DEBUG("out file", self._OutFile)
	File:write("--($Id)\n")
	File:write("--该文件为代码自动生成，请不要手动修改\n")
	File:write("local " .. TableName .. " = \n")
	File:write(DumpTbl(self._Data))
	File:write("\n")
	File:write("\nfunction Get" .. TableName .. "() return " .. TableName .. " end\n")
	File:close()
	self:ToUtf8File(self._OutFile)
end


function clsXlsBase:_WriteRow(File, RowTable)
	File:write(Serialize(RowTable))
end

--[[ 这个是有BUG的函数，对于data/record/xxxx这样的目录无法正确创建
function Mkdirs(path)
	local paths = string.split(path,"/")
	local mypath = ""
	for _,name in pairs(paths) do 
		if mypath ~= "" then 
			if string.sub(mypath, 1, 1) == "/" then 
				mypath = string.sub(mypath, 2, -1)
			end
			posix.mkdir(mypath)
		end
		mypath = mypath.."/"..name
	end
end

--]]

function clsXlsBase:WriteOneFile(FileName,Info)
	local path = string.match(FileName,".+%.lua")
	if not path then return end 
		function Mkdirs(path)
		local paths = string.split(path,"/")
		local mypath = ""
		for _,name in pairs(paths) do 
			if mypath ~= "" then 
				if string.sub(mypath, 1, 1) == "/" then 
					mypath = string.sub(mypath, 2, -1)
				end
				posix.mkdir(mypath)
			end
			mypath = mypath.."/"..name
		end
		end
	Mkdirs(path)
	local FileInfo = io.open(FileName,"w") 
	FileInfo:write(Info)
	FileInfo:close()
	
	self:ToUtf8File(FileName)
	
end

function clsXlsBase:CallFile()
	--对严重错误日志记录文件及line信息
	local dinfo = debug.getinfo(1, 'Sl')
	local filename = dinfo.short_src
	local fileline = dinfo.currentline
	self.filename = filename
	return self.filename
end

--koria add for 自动转换文件后保存为utf8格式
function clsXlsBase:ToUtf8File(FileName)
	ToUtf8File(FileName)
end

--koria 扩展基类
function clsXlsBase:GenSheetData(SheetInfo,DataStart,VarNameRowNo, VarTypeRowNo, NullRowNo)
	
	local Data = {}
	local Sheet 
	if type(SheetInfo) == "number" then
			Sheet = SheetInfo --SheetInfo is sheet index
	elseif type(SheetInfo) == "string"  then 
		if Sheet ~= "*" then 
			Sheet = self._XlsInst:get_index_by_sheet_name(SheetInfo)
		else
			return Data
		end
	else
		return Data
	end
	
	
	local MaxRow, MaxCol = self._XlsInst:get_sheet_size(Sheet)
	
	if DataStart > MaxRow then
		return Data
	end

	assert(DataStart <= MaxRow, "DataStart > MaxRow")
	
	local RowNo = VarNameRowNo or self._VarRow
	local TypeNo = VarTypeRowNo or self._TypeRow
	local NullNo = NullRowNo or self._NullRow
	local VarDescNo = self._VarDesc
	local _Vars = {}
	local _Types = {}
	local _NotNullCols = {}
	local _VarDescs = {}
	
	self:_ParseVars(Sheet,RowNo,_Vars)
	self:_ParseTypes(Sheet,TypeNo,_Types)
	self:_NotNullCells(Sheet,NullNo,_NotNullCols)
	self:_ParseVarDescs(Sheet,VarDescNo,_VarDescs)
	
	for Row = 1, (DataStart-1) do
		local RowTable0 = {}
		for Col = 1, MaxCol do
			local Var   = _Vars[Col]
			local Type  = _Types[Col]
			local IsNull = _NotNullCols[Col]
			local VarDesc = _VarDescs[Col]
			local ParsedValue = self:_ParseTipCell(Var, Type, IsNull, VarDesc)
			if ParsedValue then
				self._sheetInfoTip[Var] = ParsedValue
			end
		end
	end
	
	for Row = DataStart, MaxRow do
		
		local RowTable = {}
		for Col = 1, MaxCol do
			local Value = self._XlsInst:get_cell(Sheet, Row, Col)
			local Var   = _Vars[Col]
			local Type  = _Types[Col]
			local ParsedValue = self:_ParseCell(Var, Type, Value)
			if ParsedValue then
				RowTable[Var] = ParsedValue
			elseif _NotNullCols[Col] then
				local sheet_name  = self._XlsInst:get_sheet_name_by_index(Sheet)
				LogStr = string.format("[WARNING]%s %s cell value (sheet_index=%d, rowno=%d, colno=%d) must not be null",
					self._InFile,sheet_name,Sheet, Row, Col)
				--_DEBUG(LogStr)
				assert(false, LogStr)
				break
			end
		end
		table.insert(Data, RowTable)
		--Data[RowTable[_Vars[1]]]=RowTable
	end
	return Data
end


--获取表格数据 
--Sheet参数可以为 sheet_name，也可以为sheet_index(从1开始算起）
--Sheet参数也可以为 lua的table格式的变量，table格式变量时，表里内容可为sheet_index,或者sheet_name
--返回的数据格式为：
--{[sheet_index1]={[row1]={},[row2]={}},[sheet_index2]=....}
--sheet_name方式直接返回为
--{[row1]={xxx,xxx},[row2]={xx,xx,}}的某个表的数据

function clsXlsBase:GenData(Sheet,DataStart,VarNameRowNo, VarTypeRowNo, NullRowNo)
	local Data = {}
	if type(Sheet) == "number" then
			local sheet_index = Sheet
			--[[ --改成只用sheet_index做key
			local sheet_name  = self._XlsInst:get_sheet_name_by_index(sheet_index)
			if sheet_name then 
				Data[sheet_name] = self:GenSheetData(sheet_index,DataStart,VarNameRowNo, VarTypeRowNo, NullRowNo)
			end
			]]
			
			Data[sheet_index] = self:GenSheetData(sheet_index,DataStart,VarNameRowNo, VarTypeRowNo, NullRowNo)
			return Data
	end
	if type(Sheet) == "string"  then 
		if Sheet == "*" then 
			local sheetCount = self._XlsInst:get_sheet_cnt()
			for sheet_index = 1,sheetCount do
					--[[--改成只用sheet_index做key
					local sheet_name  = self._XlsInst:get_sheet_name_by_index(sheet_index)
					if sheet_name then 
						Data[sheet_name] =self:GenSheetData(sheet_index,DataStart,VarNameRowNo, VarTypeRowNo, NullRowNo) or {}
						
					end
					]]
					
					Data[sheet_index] =self:GenSheetData(sheet_index,DataStart,VarNameRowNo, VarTypeRowNo, NullRowNo) or {}
			end
			return Data
		else
			local sheet_index = self._XlsInst:get_index_by_sheet_name(Sheet)
			local sheet_name  = Sheet
			if not sheet_index then 
				return Data
			end
			--[[--改成只用sheet_index做key
			Data[sheet_name] = self:GenSheetData(sheet_index,DataStart,VarNameRowNo, VarTypeRowNo, NullRowNo)
			]]
			
			Data[sheet_index] = self:GenSheetData(sheet_index,DataStart,VarNameRowNo, VarTypeRowNo, NullRowNo)
			return Data
		end
	elseif type(Sheet) == "table" then 
			for _,sheetinfo in ipairs(Sheet) do
				if type(sheetinfo) == "number" then
						local sheet_index = sheetinfo 
						--[[--改成只用sheet_index做key
						local sheet_name  = self._XlsInst:get_sheet_name_by_index(sheet_index)
						if sheet_name then 
							Data[sheet_name] = self:GenSheetData(sheet_index,DataStart,VarNameRowNo, VarTypeRowNo, NullRowNo) or {}
						end
						]]
						
						
						Data[sheet_index] = self:GenSheetData(sheet_index,DataStart,VarNameRowNo, VarTypeRowNo, NullRowNo) or {}
					
				elseif type(sheetinfo) == "string" then 
					local sheet_index = self._XlsInst:get_index_by_sheet_name(sheetinfo)
					--[[--改成只用sheet_index做key
					local sheet_name = sheetinfo
					Data[sheet_name] = self:GenSheetData(sheet_index,DataStart,VarNameRowNo, VarTypeRowNo, NullRowNo) or {}
					]]
					
					
					Data[sheet_index] = self:GenSheetData(sheet_index,DataStart,VarNameRowNo, VarTypeRowNo, NullRowNo) or {}
					
				end
			end
			return Data
	end
	return Data
end


function clsXlsBase:GetXlsSheetCount()
	return self._XlsInst:get_sheet_cnt()
end

function clsXlsBase:GenDataByName(sheet_name,DataStart,VarNameRowNo, VarTypeRowNo, NullRowNo )
	local sheet_index = self._XlsInst:get_index_by_sheet_name(sheet_name)
	local ret = self:GenSheetData(sheet_index,DataStart,VarNameRowNo, VarTypeRowNo, NullRowNo)
	return (ret or {}), self._sheetInfoTip
end

function clsXlsBase:GenDataByIndex(sheet_index,DataStart,VarNameRowNo, VarTypeRowNo, NullRowNo)
	return self:GenSheetData(sheet_index,DataStart,VarNameRowNo, VarTypeRowNo, NullRowNo) or {}
end

function clsXlsBase:GenDataByIndex2(sheet_index,DataStart,VarNameRowNo, VarTypeRowNo, NullRowNo)
	local ret = self:GenSheetData(sheet_index,DataStart,VarNameRowNo, VarTypeRowNo, NullRowNo)
	return (ret or {}), self._sheetInfoTip
end

local function file_exists(path)
	local file = io.open(path, "rb")
	if file then file:close() end
	return file ~= nil
end

function clsXlsBase:SaveData(outfile,Data,TableName)
	print("-==============" .. outfile)
	local path = string.match(outfile,".+%.lua")
	if not path then return end 
	local PathElements = string.split(path, "/")
	PathElements[#PathElements] = nil
	local FullPath = table.concat(PathElements, "/")
	--posix.makedirs(FullPath)
	
	if string.len(FullPath) > 0 and not file_exists(FullPath) then
		os.execute("mkdir "..FullPath)
	end

	--local File = io.open(outfile, "w")

	local File = (outfile == "-") and io.output() or io.output(outfile)
	
	--_DEBUG("out file", outfile)
	File:write("--($Id)\n")
	File:write("--该文件为代码自动生成，请不要手动修改\n")
	File:write("\n")
	
	--File:write("local SheetNames = \n")
	--File:write(DumpTbl(self._XlsInst._sheet_names))
	--File:write("\n")
	--File:write("local SheetSizes = \n")
	--File:write(DumpTbl(self._XlsInst._sheet_sizes))
	--File:write("\n")
	
	File:write("local " .. TableName .. " = \n")
	
	local TableStr = DumpTbl(Data)
	TableStr = string.gsub(TableStr, "\"<func>", "")
	TableStr = string.gsub(TableStr, "<func>\"", "")

	--File:write(DumpTbl(Data))
	File:write(TableStr)
	File:write("\n")
	File:write("\nreturn " .. TableName .. "\n")
	File:close()
	self:ToUtf8File(outfile)
end


function clsXlsBase:SheetToLua(Sheet, OutFile, DataStart, TableName,VarNameRowNo, VarTypeRowNo, NullRowNo)
	--_RUNTIME("XlsToLua...",Sheet, OutFile, DataStart, TableName)
	local Data = self:GenData(Sheet,DataStart,VarNameRowNo, VarTypeRowNo, NullRowNo)
	self:SaveData(OutFile,Data,TableName)
end


function clsXlsBase:GetSheetNames()
	return self._XlsInst._sheet_names
end

function clsXlsBase:GetSheetSizes()
	return self._XlsInst._sheet_sizes
end


function clsXlsBase:ClearSave()
	self._SaveData = {}
end

function clsXlsBase:SetSave(VarName,Data,Comment)
	if string.find(VarName, "[^A-Za-z0-9_-]") then 
		--_DEBUG("变量名设置错误", VarName,Comment)
		--_RUNTIME("变量名设置错误...",VarName,Comment)
		return 
	end
	self._SaveData[VarName] = {Data,Comment}
end

-- check result lua file by run in lua machine
function LuaFileCheck(luafile)
	local Cmd = string.format("lua %s", luafile)
	local ret = os.execute(Cmd)
	--local ret, err = pcall(loadfile(luafile))
	if ret ~= 0 then
		--_RUNTIME("[ERROR] luafile %s run error", luafile)
		return false
	end
	return true
end

--这个是服务端的写文件程序
function clsXlsBase:SaveToFile(outfile, data)
	local path = string.match(outfile,".+%.lua")
	if not path then return end 
	local PathElements = string.split(path, "/")
	PathElements[#PathElements] = nil
	local FullPath = table.concat(PathElements, "/")
	posix.makedirs(FullPath)
	
	--local File = io.open(outfile, "w")
	
	local File = (outfile == "-") and io.output() or io.output(outfile)
	
	--_DEBUG("out file", outfile)
	
	File:write("\n--autogen-begin\n")
	File:write("--($Id)\n")
	File:write("--该文件为代码自动生成，请不要手动修改\n")
	File:write("\n")
	
	for VarName ,DataTable in pairs(data) do
			File:write("---- " .. DataTable[2] .. " --- \n")
			if type(DataTable[1]) ~= "table" then
				File:write("local " .. VarName .. " = ")
			else
				File:write("local " .. VarName .. " = \n")
			end
			File:write(DumpTbl(DataTable[1]))
			
			File:write("\n")	
	end
	for VarName ,DataTable in pairs(data) do
			File:write("\n\n---- " .. DataTable[2] .. " ---\n")
			File:write("function Get" .. VarName .. "() return " .. VarName .. " end\n\n\n")
	end
	
	File:write("\n--autogen-end\n\n")
	
	File:close()
	
	if LuaFileCheck(outfile) then 
		self:ToUtf8File(outfile)
	else
		os.execute("rm "..outfile)
	end
end

function tableJson(t)
	local deep = 1
	local function serialize(tbl, deep)
		local tmp = {}
		for k, v in pairs(tbl) do
			local k_type = type(k)
			local v_type = type(v)
			local key = (k_type == "string" and "\"" .. k .. "\":")
					or (k_type == "number" and "")
					
			if k_type == "number" and table.maxn(tbl) ~= #tbl then
					key = "" .. k .. ":"
			end
			
			local value = (v_type == "table" and serialize(v, deep + 1))
					or (v_type == "boolean" and tostring(v))
					or (v_type == "string" and "\"" .. v .. "\"")
					or (v_type == "number" and v)
					
			if deep == 1 then
				tmp[#tmp + 1] = key and value and tostring(value) or nil
			else
				tmp[#tmp + 1] = key and value and tostring(key) .. tostring(value) or nil
			end
		end

		if deep == 1 then
			return  "" .. table.concat(tmp, ",\n") .. ""
		elseif table.maxn(tbl) == #tbl and table.maxn(tbl) ~= 0 then
			return "[" .. table.concat(tmp, ",") .. "]"
		else
			return "{\n" .. table.concat(tmp, ",\n") .. "\n}"
		end
	end 
	
	assert(type(t) == "table")
	return serialize(t, deep)
end

--新加写文件程序，只限于客户端（每次只写，一个表格的一个sheet）
function clsXlsBase:SaveToFileClient(outfile, outputName, data)
	local path = string.match(outfile,".+%.json")
	if not path then return end 
	local PathElements = string.split(path, "/")
	PathElements[#PathElements] = nil
	local FullPath = table.concat(PathElements, "/")
	posix.makedirs(FullPath)
	
	
	local File = (outfile == "-") and io.output() or io.output(outfile)
	File:write( "{\n\"" ..outputName .. "\":[\n" .. tableJson(data) .. "\n]\n}")
	
	File:close()
	
	ToUtf8File(outfile)
end
