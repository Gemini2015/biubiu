# -*- coding: cp936 -*-
import os
import re

proto_path = "../../proto"
target_path = "../../../BiuBiu/Assets/Editor/BiuBiu/Proto/"
protogen_path = "../ProtoGen/protogen.exe"
msg_list_template_path = "MessageList.cs"
msg_list_path = "../../../BiuBiu/Assets/Editor/BiuBiu/Proto/MessageList.cs"

protoList = [
	'common',
	]

def convert_proto_to_cs():
	abs_proto_path = proto_path #os.path.abspath(proto_path)
	abs_target_path = os.path.abspath(target_path)
	abs_protogen_path = os.path.abspath(protogen_path)
	cwd = os.getcwd()
	os.chdir(abs_proto_path)
	for file_name in protoList:
		file = file_name + ".proto"
		file_path = file #os.path.join(abs_proto_path, file)
		target_file_path = os.path.join(abs_target_path, file_name + ".cs")
		print(file_path)
		cmd =  abs_protogen_path + " -i:" + file_path + " -o:" + target_file_path
		print(cmd)
		os.system(cmd)
	os.chdir(cwd)


def generate_msg_list():
	abs_proto_path = proto_path
	msg_list = []
	for file_name in protoList:
		file = file_name + ".proto"
		ret_list = process_msg_file(os.path.join(abs_proto_path, file))
		msg_list += ret_list

	write_msg_list(msg_list)


def process_msg_file(file_path):
	file = open(file_path, "r")
	content = file.read()
	package_name = 'common'
	package_mo = re.findall(r'package ([a-zA-Z]*);', content, re.I | re.M)
	if package_mo:
		package_name = package_mo[0]
		# print('package name:' + package_name)

	msg_list = []
	msg_mo = re.findall(r'^message ([a-zA-Z_0-9]+)', content, re.I | re.M)
	if msg_mo:
		for name in msg_mo:
			# print('message name: ' + name)
			# 过滤消息
			if name.startswith('req'):
				if package_name == '':
					msg_list.append(name)
				else:
					msg_list.append(package_name + "." + name)

	return msg_list


def write_msg_list(msg_list):
	template_file = open(msg_list_template_path, "r")
	template_content = template_file.read()
	template_file.close()

	msg_list_str = ''
	for msg in msg_list:
		# print('msg_name ' + msg)
		msg_list_str += '\t\ttypeof(' + msg + '),\n'

	msg_list_content = template_content % msg_list_str

	msg_list_file = open(msg_list_path, "w")
	msg_list_file.write(msg_list_content)
	msg_list_file.close()


if __name__ == '__main__':
    print "Generator Start"
    convert_proto_to_cs()
    generate_msg_list()