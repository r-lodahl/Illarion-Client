# Modified from http://codetuto.com/2015/05/using-httpclient-in-godot/
# Added POST and Optional Header Perimeter
extends Node

var reqlist = []

class request:
	var t
	var params
	var finished = false
	var parent
	var async
	
	var response = {}
		
	signal loading(s,l)
	signal loaded(r)
	
	func _init(p, param, is_async):
		params = param
		parent = p
		async = is_async
		
		if (async):
			t = Thread.new()
			t.start(self,"_load",params)
		else:
			response.data = _load(params)
		
	func _load(params):
		response.error = null
		var err = 0
		var http = HTTPClient.new()
		err = http.connect_to_host(params.domain,params.port,params.ssl)
		if err:
			response.error = {"error": err}
			return
			
		while(http.get_status() == HTTPClient.STATUS_CONNECTING or http.get_status() == HTTPClient.STATUS_RESOLVING):
			http.poll()
			OS.delay_msec(100)
		
		var headers = PoolStringArray(["User-Agent: Pirulo/1.0 (Godot)", "Accept:*/*"])
		headers.append_array(PoolStringArray(params.header))
		headers = Array(headers)
		
		if params.method == "get":
			err = http.request(HTTPClient.METHOD_GET,params.url,headers)
		elif params.method =="post":
			err = http.request(HTTPClient.METHOD_POST,params.url,headers,params.data)
			
		if err:
			response.error = {"error": err}
			return
			
		while (http.get_status() == HTTPClient.STATUS_REQUESTING):
			http.poll()
			OS.delay_msec(500)
			
		if http.get_status() == http.STATUS_CONNECTION_ERROR:
			response.error = {"error": http.STATUS_CONNECTION_ERROR}
			return
			
		var rb = PoolByteArray()
		if(http.has_response()):
			headers = http.get_response_headers_as_dictionary()
			while(http.get_status()==HTTPClient.STATUS_BODY):
				http.poll()
				var chunk = http.read_response_body_chunk()
				if(chunk.size()==0):
					OS.delay_usec(100)
				else:
					rb = rb+chunk
					if (async): call_deferred("_send_loading_signal",rb.size(),http.get_response_body_length())
					
		if (async): call_deferred("_send_loaded_signal")
		parent.erase_req(self)
		http.close()
		
		response.is_json = headers["Content-Type"].begins_with("application/json")
		
		if response.is_json:
			var json = parse_json(rb.get_string_from_utf8())
			if (json.has("error")):
				response.error = json.error
				return
			return json
		else:
			return rb
	
	func _send_loading_signal(size,length):
		emit_signal("loading",size,length)
	 
	func _send_loaded_signal():
		response.data = t.wait_to_finish()
		emit_signal("loaded",response)
		
func sync_get(domain,url,port,ssl, header=[]):
	var req = request.new(self,{method="get",domain=domain,url=url,port=port,ssl=ssl,header=header}, false)
	reqlist.push_front(req)
	return req.response
	
func async_get(domain,url,port,ssl, header=[]):
	var req = request.new(self,{method="get",domain=domain,url=url,port=port,ssl=ssl,header=header}, true)
	reqlist.push_front(req)
	return req

func sync_post(domain,url,port,ssl,data, header=[]):
	var req = request.new(self,{method="post",domain=domain,url=url,port=port,ssl=ssl,data=data,header=header}, false)
	reqlist.push_front(req)
	return req.response

func async_post(domain,url,port,ssl,data, header=[]):
	var req = request.new(self,{method="post",domain=domain,url=url,port=port,ssl=ssl,data=data,header=header}, true)
	reqlist.push_front(req)
	return req
	
func check_req(req):
	if reqlist.find(req) != -1:
		return true
	return false
	
func erase_req(req):
	reqlist.erase(req)