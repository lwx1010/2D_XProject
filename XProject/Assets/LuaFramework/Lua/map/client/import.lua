_G._ImportModule = _G._ImportModule or {}

function Import(pathFile)
	if IsClient() then
	    if string.endswith(pathFile, ".lua") then
	    	pathFile = string.sub(pathFile, 1, -5)
	    end	
	end

	if _G._ImportModule[pathFile] then
		return _G._ImportModule[pathFile]
	end

	local func, err = loadfile(pathFile)
	local New = {}
	_G._ImportModule[pathFile] = New
	setmetatable(New, {__index = _G})
	setfenv(func, New)()
	if New.__init__ then
		New.__init__()
	end
	return New
end