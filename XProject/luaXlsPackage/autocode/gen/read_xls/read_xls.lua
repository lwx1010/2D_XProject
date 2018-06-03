dofile("global/util.lua")

local PYTHON = "python"
local READ_XLS_PY = "autocode/gen/read_xls/read_xls.py"
local CONVERT_XLS_DATE_PY = "autocode/read_xls/convert_xls_date.py"

local JAVA = "java -jar"
local READ_XLS_JAVA = "autocode/gen/read_xls/read_xls.jar"


----------------返回的对象类型------------------
CXlsObj = {_sheet_names = nil, _data_table = nil}


function CXlsObj:new(sheet_names, data_table, sheet_sizes)
	o = {}
	setmetatable(o, self)
	self.__index = self
	o._sheet_names = sheet_names
	o._data_table = data_table
	o._sheet_sizes = sheet_sizes
	--print(self)
	return o
end


--私有接口
function CXlsObj:__get_cell(sheet_index, row, col)
	assert(type(sheet_index) == "number")
	--获取一个元素
	if (type(row) == "number" and type(col) == "number") then
		return self._data_table[sheet_index][row][col]
	--获取一行
	elseif (type(row) == "number" and col == "*") then
		--应该做深拷贝!!!
		return DeepCopy(self._data_table[sheet_index][row])
	--获取一列
	elseif (row == "*" and type(col) == "number") then
		col_table = {}
		for r in pairs(self._data_table[sheet_index]) do
			for c in pairs(self._data_table[sheet_index][r]) do
				if c == col then
				col_table[r] = self._data_table[sheet_index][r][c]
				end
			end
		end
		return col_table
	--获取所有
	elseif (row == "*" and col == "*") then
		return DeepCopy(self._data_table[sheet_index])
	--非法输入
	else
		return nil
	end
end


--通过sheet的index获取元素
--sheet_index可以为"*"
--row, col均可以为"*",
--"*"代表所有的元素,如row为"*",代表所有行
function CXlsObj:get_cell(sheet_index, row, col)
	if sheet_index == "*" then
		result = {}
		for i in pairs(self._sheet_names) do
			tmp = self:__get_cell(i, row, col)
			result[i] = tmp
		end
		return result
	end
	return self:__get_cell(sheet_index, row, col)
end


--通过sheet的名字获取元素
--sheet_name为"*"代表该sheet的名字为"*",而不是代表所有sheet
--row, col均可以为"*",
--"*"代表所有的元素,如row为"*",代表所有行
function CXlsObj:get_cell_by_sheet_name(sheet_name, row, col)
	local sheet_index = self:get_index_by_sheet_name(sheet_name)
	if not sheet_index then
		return nil
	else
		return self:get_cell(sheet_index, row, col)
	end
end


function CXlsObj:get_sheet_cnt()
	return table.getn(self._sheet_names)
end


function CXlsObj:get_sheet_size(sheet_index)
	local size = self._sheet_sizes[sheet_index]
	return size[1], size[2]
end


function CXlsObj:convert_to_xls_date(float_num)
	local python_run_str = string.format(
		"%s %s %f", PYTHON, CONVERT_XLS_DATE_PY, float_num)
	local piple_str = io.popen(python_run_str)
	print(piple_str:read("*all"))
	tmp_file = io.open("middle_lua_convert.tmp", "r")
	loadstring(tmp_file:read("*all"))()
	return __PY_DATE_RESULT
end

function CXlsObj:get_index_by_sheet_name(sheet_name)
	local sheet_index = nil
	for i, name in ipairs(self._sheet_names) do
		if name == sheet_name then
			sheet_index = i
			break
		end
	end
	return sheet_index
	
end

function CXlsObj:get_sheet_name_by_index(sheet_index)
	local sheet_name = nil
	for i, name in ipairs(self._sheet_names) do
		if i == sheet_index then
			sheet_name = name
			break
		end
	end
	return sheet_name
	
end

-------------------------------------------------



--------------------外部接口---------------------
function read_xls(filename)
	--local java_run_str = string.format("%s %s %s", JAVA, READ_XLS_JAVA, filename)
	--print("java_run_str = "..java_run_str)
	--local piple_str = io.popen(java_run_str)
	local python_run_str = string.format("%s %s %s", PYTHON, READ_XLS_PY, filename)
	local piple_str = io.popen(python_run_str)
	print(piple_str:read("*all"))
	tmp_file = io.open("middle_lua.tmp", "r")
	--如果这里报错。说明tmp_file的lua格式有问题，需要对xls表作语法检查
	loadstring(tmp_file:read("*all"))()
	tmp_file:close()
	if READ_XLS_RESULT ~= 0 then
		return nil
	else
		return CXlsObj:new(sheet_names, data_table, sheet_sizes)
	end
end
-------------------------------------------------



--------------------测试代码---------------------
--通过sheet索引取一个单元
function test1()
	xls_inst = read_xls("111.xls")
	print(xls_inst:get_cell(1, 1, 1))
end


--通过sheet名字取一个单元
function test2()
	xls_inst = read_xls("111.xls")
	print(xls_inst:get_cell_by_sheet_name("Sheet1", 1, 1))
end


--通过sheet索引取一行
function test3()
	xls_inst = read_xls("111.xls")
	t = xls_inst:get_cell(2, 2, "*")
	for i in pairs(t) do
		print(i, t[i])
	end
end


--通过sheet名字取一列
function test4()
	xls_inst = read_xls("111.xls")
	t = xls_inst:get_cell_by_sheet_name("Sheet2", "*", 3)
	for i in pairs(t) do
		print(i, t[i])
	end
end


--通过sheet索引取所有行列
function test5()
	xls_inst = read_xls("111.xls")
	t = xls_inst:get_cell(3, "*", "*")
	for i in pairs(t) do
		for j in pairs(t[i]) do
			print(i, j, t[i][j])
		end
	end
end


--取所有sheet所有行列
function test6()
	xls_inst = read_xls("111.xls")
	t = xls_inst:get_cell("*", "*", "*")
	for i in pairs(t) do
		for j in pairs(t[i]) do
			for k in pairs(t[i][j]) do
				print(i, j, k, t[i][j][k])
			end
		end
	end
end
