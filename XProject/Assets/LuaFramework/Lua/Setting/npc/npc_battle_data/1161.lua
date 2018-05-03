--autogen-begin
local AllNpcData = 
{
 [10100101] = {
 ["Dp"] = 0, ["CanPush"] = 0, ["PunchAway"] = 1, ["Mr"] = 0, ["HpPos"] = "", ["NpcNo"] = 10100101, ["Spirit"] = 0, ["HitRate"] = 6, ["AIPatrol"] = 0, ["Head"] = 70000, ["PatrolRange"] = 2, ["SubShape"] = 70000, ["Tenacity"] = 0, ["ReParry"] = 0, ["NpcMapNos"] = {
 [1] = 1001, [2] = 1161, }
, ["Fire"] = 0, ["ExpReward"] = 15, ["IsActiveAttack"] = 0, ["AIRadius"] = 6, ["Wind"] = 0, ["Score"] = 1632, ["WalkGrid"] = 2, ["WalkIntervalTime"] = 0, ["AITrackRange"] = 10, ["Step"] = 1, ["Water"] = 0, ["HpMax"] = 1701, ["BossType"] = 0, ["Martials"] = {
 [1] = {
 [1] = 3000003, [2] = 1, }
, }
, ["Dodge"] = 0, ["Speed"] = 3.5, ["PlayRefreshAni"] = 1, ["KReward"] = {
 }
, ["Scale"] = 1, ["BloodType"] = 1, ["BloodCount"] = 1, ["Ma"] = 0, ["Double"] = 0, ["Soil"] = 0, ["Grade"] = 2, ["Parry"] = 0, ["TitleSpr"] = "", ["Ap"] = 72, ["AtkTime"] = 2000, ["Say"] = "", ["ShapeExtend"] = "", ["Name"] = "突厥士兵", ["Shape"] = 70000, ["Des"] = "河童", }
, [20100104] = {
 ["Dp"] = 0, ["CanPush"] = 0, ["PunchAway"] = 0, ["HpPos"] = "", ["NpcNo"] = 20100104, ["Spirit"] = 0, ["HitRate"] = 6, ["AIPatrol"] = 0, ["Head"] = 71001, ["PatrolRange"] = 2, ["SubShape"] = 71001, ["Tenacity"] = 0, ["ReParry"] = 0, ["Mr"] = 0, ["NpcMapNos"] = {
 [1] = 1001, [2] = 1161, }
, ["Water"] = 0, ["Fire"] = 0, ["ExpReward"] = 15, ["IsActiveAttack"] = 1, ["AIRadius"] = 4, ["Wind"] = 0, ["Score"] = 1632, ["AITrackRange"] = 4, ["WalkIntervalTime"] = 0, ["Martials"] = {
 [1] = {
 [1] = 3100001, [2] = 1, }
, [2] = {
 [1] = 3100002, [2] = 1, }
, }
, ["Step"] = 1, ["StopDistance"] = 3, ["HpMax"] = 8445, ["BloodCount"] = 10, ["Scale"] = 1, ["Dodge"] = 0, ["Speed"] = 5, ["BloodType"] = 2, ["KReward"] = {
 }
, ["SpNotHurt"] = 1, ["Ma"] = 0, ["Double"] = 0, ["WalkGrid"] = 2, ["Dir"] = 1, ["Soil"] = 0, ["BossType"] = 3, ["Grade"] = 4, ["Parry"] = 0, ["TitleSpr"] = "", ["Ap"] = 104, ["Name"] = "幽冥法王", ["Say"] = "", ["AtkTime"] = 2000, ["ShapeExtend"] = "", ["Shape"] = 71001, ["Des"] = "河童", }
, [20100108] = {
 ["Dp"] = 0, ["CanPush"] = 0, ["PunchAway"] = 0, ["HpPos"] = "", ["NpcNo"] = 20100108, ["Spirit"] = 0, ["HitRate"] = 12, ["AIPatrol"] = 0, ["Head"] = 71000, ["PatrolRange"] = 2, ["SubShape"] = 71000, ["Tenacity"] = 0, ["ReParry"] = 0, ["Mr"] = 0, ["NpcMapNos"] = {
 [1] = 1001, [2] = 1161, }
, ["Water"] = 0, ["Fire"] = 0, ["ExpReward"] = 17, ["IsActiveAttack"] = 1, ["AIRadius"] = 4, ["Wind"] = 0, ["Score"] = 1632, ["AITrackRange"] = 4, ["WalkIntervalTime"] = 0, ["Martials"] = {
 [1] = {
 [1] = 3100023, [2] = 1, }
, [2] = {
 [1] = 3100024, [2] = 1, }
, }
, ["Step"] = 1, ["StopDistance"] = 3, ["HpMax"] = 11109, ["BloodCount"] = 10, ["Scale"] = 1, ["Dodge"] = 0, ["Speed"] = 5, ["BloodType"] = 2, ["KReward"] = {
 }
, ["SpNotHurt"] = 1, ["Ma"] = 0, ["Double"] = 0, ["WalkGrid"] = 2, ["Dir"] = 8, ["Soil"] = 0, ["BossType"] = 3, ["Grade"] = 9, ["Parry"] = 0, ["TitleSpr"] = "", ["Ap"] = 184, ["Name"] = "完颜不败", ["Say"] = "", ["AtkTime"] = 2000, ["ShapeExtend"] = "", ["Shape"] = 71000, ["Des"] = "河童", }
, [10100107] = {
 ["Dp"] = 0, ["CanPush"] = 0, ["PunchAway"] = 1, ["Mr"] = 0, ["HpPos"] = "", ["NpcNo"] = 10100107, ["Spirit"] = 0, ["HitRate"] = 12, ["AIPatrol"] = 1, ["Head"] = 70001, ["PatrolRange"] = 2, ["SubShape"] = 70001, ["Tenacity"] = 0, ["ReParry"] = 0, ["NpcMapNos"] = {
 [1] = 1001, [2] = 1161, }
, ["Fire"] = 0, ["ExpReward"] = 17, ["IsActiveAttack"] = 0, ["AIRadius"] = 6, ["Wind"] = 0, ["Score"] = 1632, ["WalkGrid"] = 2, ["WalkIntervalTime"] = 0, ["AITrackRange"] = 10, ["Step"] = 1, ["Water"] = 0, ["HpMax"] = 2777, ["BossType"] = 0, ["Martials"] = {
 [1] = {
 [1] = 3000002, [2] = 1, }
, }
, ["Dodge"] = 0, ["Speed"] = 3.5, ["PlayRefreshAni"] = 1, ["KReward"] = {
 }
, ["Scale"] = 1, ["BloodType"] = 1, ["BloodCount"] = 1, ["Ma"] = 0, ["Double"] = 0, ["Soil"] = 0, ["Grade"] = 9, ["Parry"] = 0, ["TitleSpr"] = "", ["Ap"] = 184, ["AtkTime"] = 2000, ["Say"] = "", ["ShapeExtend"] = "", ["Name"] = "突厥秘卫", ["Shape"] = 70001, ["Des"] = "河童", }
, }


function GetAllNpcData() return AllNpcData end

--autogen-end