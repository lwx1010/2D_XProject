# -*- coding: utf-8 -*-

import xlrd
import sys

if __name__ == "__main__":
	global tmp_file
	tmp_file = open("middle_lua_convert.tmp", "w")
	n_date = float(sys.argv[1])
	time_tuple = xlrd.xldate_as_tuple(n_date, 0)
	time_str = "__PY_DATE_RESULT = "
	time_str += "\"%d-%d-%d\"" % \
	(time_tuple[0], time_tuple[1], time_tuple[2])
	print >> tmp_file, time_str
	#print >> sys.stdout, time_str
	tmp_file.close()