
Œ
common/toproto/aoi/aoi.proto":
S2c_aoi_sync_int

id (	
key (	
value ("<
S2c_aoi_sync_float

id (	
key (	
value ("=
S2c_aoi_sync_string

id (	
key (	
value (	"-
S2c_aoi_addbuff

id (	
buffid ("-
S2c_aoi_delbuff

id (	
buffid ("¥
Aoi_syncplayer
shape (
name (	
speed (
dir (
camp (
buffs (
hpmax (
clubid (	
teamid	 (	
extend
 (	"ª
Aoi_syncpartner
	partnerno (
ownerid (
shape (
name (	
speed (
dir (
camp (
buffs (
hpmax	 (
extend
 (	"£
Aoi_syncnpc
npcno (
shape (
name (	
speed (
dir (
camp (
buffs (
hpmax (
isstatic	 (
extend
 (	"r
S2c_aoi_addplayer

id (		
x (	
z (
move_dir (

hp (
sync (2.Aoi_syncplayer"~
S2c_aoi_addself
map_no (
map_id (

id (		
x (	
z (

hp (
sync (2.Aoi_syncplayer"„
S2c_aoi_addpartner

id (		
x (	
z (
move_dir (

hp (
follow (
sync (2.Aoi_syncpartner"l
S2c_aoi_addnpc

id (		
x (	
z (
move_dir (

hp (
sync (2.Aoi_syncnpc"3
S2c_aoi_createmap
map_no (
map_id ("6
C2s_aoi_createmap_ok
map_no (
map_id ("7
C2s_aoi_move_start	
x (	
z (
dir ("C
S2c_aoi_move_start

id (		
x (	
z (
dir ("6
C2s_aoi_move_stop	
x (	
z (
dir ("B
S2c_aoi_move_stop

id (		
x (	
z (
dir ("@
S2c_aoi_move_to

id (		
x (	
z (
dir ("+
S2c_aoi_move_update
place_holder ("
S2c_aoi_leave

id (	",
S2c_aoi_move_error

ox (

oz ("=
S2c_aoi_jump

id (		
x (	
z (
dir (";
S2c_aoi_sync_hp

id (	
nhp (
hpstamp ("8
S2c_aoi_partner_follow
type (
mill_sec ("2
C2s_aoi_dodge
dir (	
x (	
z (">
S2c_aoi_dodge

id (	
dir (	
x (	
z (";
S2c_aoi_dodge_error
type (

ox (

oz ("#
C2s_aoi_skilllock
tar_id (	"’
Aoi_hitchar
tar_id (	
type (
show_hp (
nhp (
hpstamp (
back_x (
back_z (
floatdown_buff ("×
S2c_aoi_skillhit_bycharpos
skill_id (
att_id (	
dir (
mtar_id (	

tx (

tz (
	tar_chars (2.Aoi_hitchar
move_dir (
move_x	 (
move_z
 (
calcnt ("–
C2s_aoi_skillact_bycharpos
skill_id (
dir (
mtar_id (	

tx (

tz (
move_dir (
move_x (
move_z ("¦
S2c_aoi_skillact_bycharpos
skill_id (
att_id (	
dir (
mtar_id (	

tx (

tz (
move_dir (
move_x (
move_z	 ("5
S2c_aoi_skillcd
skill_id (
cooltime ("5
S2c_aoi_skillstop
skill_id (
att_id (	"%
S2c_aoi_skillerror
erro_no (