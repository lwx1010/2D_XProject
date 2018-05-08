--- 
-- 动态添加/删除屏蔽字
-- @module DealSensitiveWord
-- 
local SensitiveWord = SensitiveWord

local DealSensitiveWord = {}

local addWords = {}

local removeWords = {"鸡"}

function DealSensitiveWord.AddAndDeleteSensitiveWord()
	if #removeWords > 0 then
		SensitiveWord:RemoveSensitiveWord(removeWords)
	end

	if #addWords > 0 then
		SensitiveWord:AddSensitiveWord(addWords)
	end
end

return DealSensitiveWord