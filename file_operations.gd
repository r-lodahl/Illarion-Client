static func find_files(path, ends_with, exclude_dir):
	var files = []
	var dir = Directory.new()
	
	if dir.open(path) != OK:
		print("Cannot open " + path + "!")
		return
	
	dir.list_dir_begin(true)
	
	var file_name = dir.get_next()
	while (file_name != ""):
		if dir.current_is_dir():
			if file_name == exclude_dir:
				continue
				
			files += find_files(path+"/"+file_name, ends_with)
		elif file_name.ends_with(ends_with):
			files.push_back(path+"/"+file_name)
		file_name = dir.get_next()
	
	dir.list_dir_end()
	return files