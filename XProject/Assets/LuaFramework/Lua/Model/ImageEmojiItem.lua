ImageEmojiItem = {}

function ImageEmojiItem:New(o)
	o = o or {}
	setmetatable(o, ImageEmojiItem)
	self.__index = ImageEmojiItem
	return o
end

function ImageEmojiItem:OnCreate(go)
	self.gameOject = go
	self.transform = go.transform

	self.emotion = go.transform:Find('emotion'):GetComponent('UISprite')
	self.sprAni = go.transform:Find('emotion'):GetComponent('UISpriteAnimation')
	self.behaviour = go.transform:GetComponent('LuaComponent')
	self.behaviour:AddClick(self.gameOject, function() CHATLOGIC.AddImageEmoji(self.emojiName) end)
end

function ImageEmojiItem:SetEmoji(info)
	self.emotion.spriteName = info.Name .. "_01"
	self.emojiName = info.Name
	self.sprAni.namePrefix = info.Name
	self.sprAni.framesPerSecond = 0
end