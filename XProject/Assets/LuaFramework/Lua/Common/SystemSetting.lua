-- @Author: LiangZG
-- @Date:   2017-03-17 14:22:00
-- @Last Modified time: 2017-06-08 12:06:39
-- @Desc  系统设置

local UserSetting = User_Config
local QualitySettings = QualitySettings

SystemSetting = {}

--极速渲染质量
function SystemSetting.rendererQualityFastest( )
    SystemSetting._setQualityLevel("Fastest")
    Application.targetFrameRate = AppConst.GameFrameRate
end

--流畅渲染质量
function SystemSetting.rendererQualityFast(  )

    SystemSetting._setQualityLevel("Simple")
    Application.targetFrameRate = AppConst.GameFrameRate
end

--完美渲染质量
function SystemSetting.rendererQualityGood(  )

    SystemSetting._setQualityLevel("Beautiful")

    Application.targetFrameRate = AppConst.GameFrameRate
end

--- 初始设置渲染质量
function SystemSetting.initRendererQuality( quality )
    if quality == 0 then
        SystemSetting.rendererQualityFastest()
    elseif quality == 1 then
        SystemSetting.rendererQualityFast()
    else
        SystemSetting.rendererQualityGood()
    end
    UserSetting.SetInt("quality" , quality)
    UserSetting.quality = quality
end

function SystemSetting._setQualityLevel( name )
    local names = QualitySettings.names
    for i=names.Length - 1,0 ,-1 do
        if names[i] == name then
            QualitySettings.SetQualityLevel(i , true)
            return
        end
    end
end