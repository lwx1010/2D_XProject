require "Common/protocal"
require "Common/functions"
Event = require 'events'

require "3rd/pbc/protobuf"

Network = {}
local this = Network

local transform
local gameObject

local _proIdNameTb

this.lastSendTime = 0
this.isNewRole = false
this.printPb = false

--开发阶段缓存 收发协议
local isCache = true
local CACHE_MAX = 20
local sendCache = {}
local recieveCache = {}

local function CacheSend(pbName,pb)
	if not isCache then
		return
	end
	local tmp = {
		name = pbName,
		pb = pb,
	}
	if #sendCache >= CACHE_MAX then
		table.remove(sendCache,1)
	end
	table.insert(sendCache,tmp)
end

local function CacheRecieve(pbName,pb)
	if not isCache then
		return
	end
	local tmp = {
		name = pbName,
		pb = pb
	}
	if #recieveCache >= CACHE_MAX then
		table.remove(recieveCache,1)
	end
	table.insert(recieveCache,tmp)
end

function Network.PrintCache(type)
	if type == 1 then
		print(TableToString(sendCache))
	elseif type == 2 then
		print(TableToString(recieveCache))
	end
end

--开发阶段缓存 收发协议


function Network.Start() 
    log("Network.Start!!")
    Event.AddListener(Protocal.Connect, this.OnConnect)
    Event.AddListener(Protocal.Message, this.OnMessage)
    Event.AddListener(Protocal.Exception, this.OnException)
    Event.AddListener(Protocal.Disconnect, this.OnDisconnect)
    Event.AddListener(Protocal.Reconnected, this.OnReconnected)
    Event.AddListener(Protocal.BeginReconnect, this.OnBeginReconnect)
    Event.AddListener(Protocal.KickOut, this.OnKickOut)
    Event.AddListener(Protocal.PingUpdate, this.OnPingUpdate)
    Event.AddListener(Protocal.AccConflict, this.OnAccConflict)
    Event.AddListener(Protocal.HeartBeat, this.OnHeartBeat2Server)
    Event.AddListener(Protocal.WeakMessage, this.OnMessage)
end

--Socket消息--
function Network.OnSocket(key, data)
	if key == 0 then
		logError("recieve 0 key to network")
		--networkMgr:KickOut()
		return
	end
	Event.Brocast(tostring(key), data)
end

--当连接建立时--
function Network.OnConnect()
	print("=============Network.OnConnect===========", SceneHelper.GetCurrentSceneName())
	if SceneHelper.GetCurrentSceneName() == "updatescene" then
    	local LoginCtrl = CtrlManager.GetCtrl(CtrlNames.Login)
		LoginCtrl.OnConnectCallback()
	end
end

--异常断线--
function Network.OnException()
	print("OnException")
    if SceneHelper.GetCurrentSceneName() ~= "updatescene" then
    	if networkMgr.lostConnect then
    		COMMONCTRL.CreateQuanQuan(nil)
    	end
    else
    	local login = CtrlManager.GetCtrl(CtrlNames.Login)
		if login ~= nil then
			login.StopCheckTimeout()
			login.SetEnterMsg("")
		end
		MessageBox.DisplayMessageBox(LANGUAGE_TIP.ConnectServerError, 0, null, null)
    end
end

--连接中断，或者被踢掉--
function Network.OnDisconnect()
	print("OnDisconnect")
    if SceneHelper.GetCurrentSceneName() ~= "updatescene" then
    	if networkMgr.lostConnect then
    		COMMONCTRL.CreateQuanQuan(nil)
    	end
    else
    	local login = CtrlManager.GetCtrl(CtrlNames.Login)
		if login ~= nil then
			login.StopCheckTimeout()
			login.SetEnterMsg("")
		end
		MessageBox.DisplayMessageBox(LANGUAGE_TIP.ConnectServerError, 0, null, null)
    end
end

--登录返回--
function Network.OnMessage(buffer)
	if TestProtoType == ProtocalType.PBC then
		this.processRecieveBuffer(buffer)
	end
end

function Network.OnBeginReconnect()
	print("--------------begin reconnect")
	COMMONCTRL.RemoveQuanQuan()
	COMMONCTRL.CreateQuanQuan(nil)
	CtrlManager.PopUpNotifyText(LANGUAGE_TIP.Reconnecting)
end

function Network.OnReconnected()
	print("Reconnect to server")
	local cmd = {}
	cmd.id = HERO.Uid
	cmd.time = os.time()
	cmd.sign = Util.md5("uid="..cmd.id.."time="..cmd.time.."key=65e89E1264e5C4dc6f9D1e827cd13F57")
	cmd.scene_name = {}
	local list = gameMgr:GetSceneFileList()
	if list.Length == 0 then
		cmd.isall = 1
	else
		cmd.isall = 0
		for i = 0, list.Length - 1 do
			table.insert(cmd.scene_name, list[i])
		end
	end
	print("send C2s_login_player_recon:"..TableToString(cmd))
	this.send("C2s_login_player_recon", cmd)
end

function Network.OnKickOut()
	logWarn("kicked out from server")
	COMMONCTRL.RemoveQuanQuan()
	networkMgr.isKickOut = true
	MessageBox.DisplayMessageBox(LANGUAGE_TIP.Disconnected, 0, function()
		Game.TurnToLoginScene()
		if User_Config.internal_sdk == 1 then
			--登出SDK
			CenterServerManager.Instance.UserID = ""
			SDKInterface.Instance:Logout()
		end
	end, null)
end

function Network.OnPingUpdate()
	local ping = 0
	if this.lastRecieveTime ~= nil then
		ping = math.abs(TimeManager.GetRealTimeSinceStartUp() - this.lastRecieveTime - 5)
	end
	this.lastRecieveTime = TimeManager.GetRealTimeSinceStartUp()
	local mainCtrl = CtrlManager.GetCtrl('MainCtrl')
	if mainCtrl ~= nil then
		mainCtrl.PingUpdate(ping)
	end
end

function Network.OnAccConflict()
	COMMONCTRL.RemoveQuanQuan()
	MessageBox.DisplayMessageBox(LANGUAGE_TIP.AccountConfict, 0, function()
		Game.TurnToLoginScene()	
		if User_Config.internal_sdk == 1 then
			--登出SDK
			CenterServerManager.Instance.UserID = ""
			SDKInterface.Instance:Logout()
		end
	end, null)
end

--卸载网络监听--
function Network.Unload()
    Event.RemoveListener(Protocal.Connect)
    Event.RemoveListener(Protocal.Message)
    Event.RemoveListener(Protocal.Exception)
    Event.RemoveListener(Protocal.Disconnect)
    Event.RemoveListener(Protocal.Reconnected)
    Event.RemoveListener(Protocal.BeginReconnect)
    Event.RemoveListener(Protocal.KickOut)
    Event.RemoveListener(Protocal.PingUpdate)
    Event.RemoveListener(Protocal.AccConflict)
    Event.RemoveListener(Protocal.HeartBeat)
    log('Unload Network...')
end

---------------------------------------------------------------------------------------------------------
-- 添加自定义函数
---------------------------------------------------------------------------------------------------------

---
-- 协议ID，类名对应表
-- @field [parent=#utils.GameNet] #table _proIdNameTb
-- 


---
-- 加载pb和P2p
-- @function [parent=#utils.GameNet] loadPbAndP2p
-- @param #string pbFolder	*.pb文件所在目录，如 "Lua/protocol/pbc"，
--					非debug模式下，内部将转变为读取"Lua/protocol/pbcs.lua"文件
-- @param #string p2pFile	p2p文件，如 "Lua/protocol/proto.conf"
-- @return #boolean, #string 是否成功, 错误信息
-- 
function Network.loadPbAndP2p( pbFolder, p2pFile )
	if( _proIdNameTb ) then return true end

	if( not pbFolder or not p2pFile ) then
		return false, "params error"
	end

	local subBegin = 1
	if( string.sub(pbFolder, 1, string.len("scripts"))=="scripts" ) then 
		subBegin = string.len("scripts")+2
	end

	local moduleName = string.sub(string.gsub(pbFolder, "/", "%."), subBegin).."s"
	log("register pb files -> "..moduleName)
	require(moduleName)

	log("load p2pFile -> "..p2pFile)
	_proIdNameTb = {}

	local path = Util.LuaPath..p2pFile
	log("p2pFile path"..path)
	local p2pF = io.open(path, 'r')
	local p2pStr = p2pF:read("*all")
	p2pF:close()
	for k, v in string.gmatch(p2pStr, "(%d+),[%w_/]+%.([%w_]+)") do
		k = tonumber(k)
		_proIdNameTb[k] = v
		_proIdNameTb[v] = k
	end

	--log("~~~~~~~"..TableToString(_proIdNameTb))
	return true
end

---
-- 注册处理器
-- @function [parent=#utils.GameNet] registerHandlers
-- @param #string handlerFolder	协议处理类所在的目录，如 "Lua/protocol/handler"
--					非debug模式下，内部将转变为读取"Lua/protocol/handlers.lua"文件
-- @param #boolean inDebug	是否在debug模式
-- @return #boolean, #string 是否成功, 错误信息
-- 
function Network.registerHandlers( handlerFolder )
	if( not handlerFolder ) then
		return false, "params error"
	end

	local subBegin = 1
	if( string.sub(handlerFolder, 1, string.len("Lua"))=="Lua" ) then 
		subBegin = string.len("Lua")+2
	end

	local moduleName = string.sub(string.gsub(handlerFolder, "/", "%."), subBegin).."s"
	log("register pb handlers -> "..moduleName)
	require(moduleName)

	return true
end

--- 
-- 错误处理
-- @function [parent=#utils.GameNet] _errorHandler
-- @param #string err 错误信息
-- 
local function _errorHandler( err )
	local stack = debug.traceback("", 2)
	--sendCloudError(tostring(err).."\n"..stack)
	local errStr = "----------------------------------------\n"
	errStr = errStr.."协议处理错误: "..tostring(err).."\n"
	errStr = errStr..stack.."\n"
	errStr = errStr.."----------------------------------------\n"
	--_errs = _errs..errStr
	
	logError(errStr)
end

function Network.processRecieveBuffer(buffer)
	local pbId = buffer:ReadShort()
	local pbName = _proIdNameTb[pbId]
	-- log("~~~~~~收到协议 ["..pbId.."]"..pbName)

	local data = buffer:ReadPbBuffer()
	if not _proIdNameTb[pbId] then
		logError("协议不存在! "..pbId)
		return
	end
	local pb = protobuf.decode(pbName, data)
	if not pb then
		logError("pb解析失败："..pbName.."_"..protobuf.lasterror())
		local pbLen = buffer.Length + 2 + 4
		logError("协议长度: "..pbLen)
		logError(TableToString(data))
	else
		if this.printPb then
			log(TableToString(pb,"id: "..pbId.."  "..pbName))
		end

		--缓存接受协议
		CacheRecieve(pbName,pb)

		local fun = Network[pbName] 
		if( type(fun)=="function" ) then
			xpcall(function() fun(pb) end, _errorHandler)
		else
			logError("找不到协议处理函数："..pbName)
		end
	end
end

function Network.send( pbName, pbObj )
	-- print("-=-----------------", pbName)
	if( not _proIdNameTb or not pbName or not pbObj ) then return end
	local proId = _proIdNameTb[pbName]
	if not proId then
		logError("找不到proId："..pbName)
		return
	end
	--缓存发送协议
	CacheSend(pbName, pbObj)

	-- 检测是否是跨场景协议
	if this.CheckCrossSceneMsg(pbName) == false then return end

	local pb = protobuf.encode(pbName, pbObj)
	local buffer = ByteBuffer.New()
    buffer:WriteInt(string.len(pb))
    buffer:WriteShort(proId)
    buffer:WriteBuffer(pb)
    networkMgr:SendMessage(buffer)
end

function Network.CheckCrossSceneMsg( pbName )
	local crossScenePb = pbName == "C2s_hero_canflyshoe" or pbName == "C2s_aoi_convey_door" or pbName == "C2s_fuben_fight"
		or pbName == "C2s_yunbiao_continue"
	if crossScenePb then
		--当前正在跳转场景 返回false
		if roleMgr.crossScene then
			CtrlManager.PopUpNotifyText(LANGUAGE_TIP.InCrossScene)
			return false
		end
		--当前没有跳转场景 则设置为3秒跳转状态
		roleMgr:SetToCrossScene()
	end
	return true
end

local function Get7Number(num)
	local cnt = string.len(math.floor(num))
	return (math.floor(10^(7-cnt)*num))*10^cnt
end

function Network.sendMove(args)
	local aoi_move = {}
	-- aoi_move.x = Get7Number(args[0]) --math.floor(args[0]*10000000)
	-- aoi_move.z = Get7Number(args[1]) --math.floor(args[1]*10000000)
	aoi_move.x = args[0]
	aoi_move.z = args[1]
	aoi_move.dir = args[2]

	-- print("=======C2s_aoi_move_start=======", args[2], args[0], aoi_move.x, args[1], aoi_move.z)
	this.send("C2s_aoi_move_start", aoi_move)

	-- aoi_move.id = HERO.Id .. "_1"
	-- this["S2c_aoi_move_start"](aoi_move)
end

function Network.sendMoveStop(args)
	local aoi_move = {}
	-- aoi_move.x = Get7Number(args[0]) --math.floor(args[0]*10000000)
	-- aoi_move.z = Get7Number(args[1]) --math.floor(args[1]*10000000)
	aoi_move.x = args[0]
	aoi_move.z = args[1]
	aoi_move.dir = args[2]

	-- print("=========C2s_aoi_move_stop=====", args[2], args[0], aoi_move.x, args[1], aoi_move.z)
	this.send("C2s_aoi_move_stop", aoi_move)

	-- aoi_move.id = HERO.Id .. "_1"
	-- this["S2c_aoi_move_stop"](aoi_move)
end

-----------------------------------------------------------
function Network.SendRideHorse(ride)
	local C2s_mount_upmount = {}
	C2s_mount_upmount.ison = ride
	Network.send("C2s_mount_upmount", C2s_mount_upmount)
	CtrlManager.GetCtrl(CtrlNames.Main).UpdateHorseBtnState()
end

function Network.SendLoginSuccess()
	if this.isNewRole == true then
		local C2s_login_newok = {}
		C2s_login_newok.placeholder = 1
		Network.send("C2s_login_newok", C2s_login_newok)
	end
	this.isNewRole = false
end

function Network.OnHeartBeat2Server()
	print("send heart beat 2 server")
	local C2s_hero_beat = { place_holder = 1}
	Network.send("C2s_hero_beat", C2s_hero_beat)
end

function Network.OnPackageDownloadComplete(percent)
	if this.downloadResult == nil then
		this.downloadResult = 
		{
			[30] 	= false,
			[60] 	= false, 
			[100] 	= false,
		}
	end
	if this.downloadResult[percent] == false then
		local C2s_hero_downfinish = { place_holder = percent }
		print("download finished: "..percent.."%")
		Network.send("C2s_hero_downfinish", C2s_hero_downfinish)
		this.downloadResult[percent] = true
	end
end