# -*- coding: utf-8 -*-
import xlrd
import sys

#执行结果
EXEC_SUCCESS    = 0		#执行成功
EXEC_FILE_FAILE = 1		#文件错误

READ_XLS_RESULT_STR = "READ_XLS_RESULT = %d"


def output(_str):
	_str = _str.encode("gbk")
	print >> tmp_file, _str


#
def read_xls(filename, date_as_string=True):
	#文件不存在
	import os
	if not os.path.isfile(filename):
		output(READ_XLS_RESULT_STR % EXEC_FILE_FAILE)
		return
	book = xlrd.open_workbook(filename,formatting_info=True)
	#sheet name
	output("sheet_names = {")
	for sheet_name in book.sheet_names():
		output("\"%s\", " % sheet_name)
	output("}")
	#sheet size
	output("sheet_sizes = {")
	for i in xrange(book.nsheets):
		sh = book.sheet_by_index(i)
		output("{%d, %d}," % (sh.nrows, sh.ncols))
	output("}")
	
	
	data_table = {}
	for i in xrange(book.nsheets):
		sh = book.sheet_by_index(i)
		data_table[i] ={}
		for r in xrange(0, sh.nrows):
			data_table[i][r] = {}
			for c in xrange(0, sh.ncols):
				cell_value = sh.cell_value(r, c)
				data_table[i][r][c] = cell_value
		for crange in sh.merged_cells:
			rlo, rhi, clo, chi = crange
			for rowx in xrange(rlo, rhi):
				for colx in xrange(clo, chi):
					data_table[i][rowx][colx] = sh.cell_value (rlo, clo)
					
	#数据
	output("data_table = {")
	for i in xrange(book.nsheets):
		sh = book.sheet_by_index(i)
		sheet_data = "{"
		for r in xrange(1, sh.nrows+1):
			sheet_data += "	[%d] = {" % r
			for c in xrange(1, sh.ncols+1):
				cell_value = data_table[i][r-1][c-1]
				sheet_data += "[%d] = [[%s]], " % (c, cell_value)
			sheet_data += "},\n"
		sheet_data += "},"
		output(sheet_data)
		
	output("}")
	#写执行成功标记
	output(READ_XLS_RESULT_STR % EXEC_SUCCESS)


if __name__ == "__main__":
	global tmp_file
	tmp_file = open("middle_lua.tmp", "w")
	filename = sys.argv[1]
	read_xls(filename)
	tmp_file.close()


