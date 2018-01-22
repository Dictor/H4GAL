<!DOCTYPE html> 
<html lang="ko">
	<head>
		<link rel="stylesheet" href="https://netdna.bootstrapcdn.com/bootstrap/3.1.1/css/bootstrap.min.css">
		<link rel="stylesheet" href="https://netdna.bootstrapcdn.com/bootstrap/3.1.1/css/bootstrap-theme.min.css">
		<script src="https://netdna.bootstrapcdn.com/bootstrap/3.1.1/js/bootstrap.min.js"></script>
		<meta charset="UTF-8">
		<meta name="viewport" content="width=device=width, initial-scale=1">
			
		<title>하늘고 4기 사진 저장소</title>
		<style>
			@import url(https://fonts.googleapis.com/earlyaccess/nanumgothic.css);
			.text {font-family: 'Nanum Gothic', sans-serif;}
			
			
			/*Center Align Class START*/
			.container-fluid{
			  height:100%;
			  display:table;
			  width: 100%;
			  padding: 0;
			}
			.row-fluid {height: 100%; display:table-cell; vertical-align: middle;}
			.centering {
			  float:none;
			  margin:0 auto;
			}
			html, body{height:100%; margin:0;padding:0}
			/*Center Align Class END*/
			
			
			body 
			{ 
				background-color : #ffffff;
			}	
		</style>
	</head>
	
	<body>
		<div class="container-fluid">
			<div class="row-fluid">
				<div class="centering text-center">
					<div class="container-fluid"  style="width:90%">
						
						<img src="./logo.png" style="width: 25%; max-width: 601px;">
						<br><br>
						
						<div class="row">
							<div class="col-md-12">
								<p style="text; font-size:5em; font-weight:bold;">하늘고 4기 남학생 사진창고</p>
							</div>
						</div>
		
						<p style="text;"><span class="glyphicon glyphicon-exclamation-sign" aria-hidden="true"></span>계정은 없지만, 하늘고 4기 학생이라면 <font style="font-weight:bold;">'일회용 자격증명'</font> 버튼을 사용하세요.</p>
		
						<div class="row" >
							<div class="col-md-3"></div>
							<div class="col-md-6">
								<div class="panel panel-default">
									<div class="panel-body">
										<div id = "loginnorreadyform">
											<p style="text; font-size:1em; font-weight:bold;">
											<span class="glyphicon glyphicon-refresh" aria-hidden="true"></span>
											서버에 연결중입니다...</p>
										</div>
										
										<div id = "loginerrorform">
											<p style="text; font-size:1em; font-weight:bold; color:red">
											<span class="glyphicon glyphicon-exclamation-sign" aria-hidden="true"></span>
											서버에 연결 중 오류가 발생했습니다! 페이지를 새로고쳐 연결을 재시도하세요.</p>
										</div>
										
										<div id = "loginform">
											<table style="width:100%">
												<tr style="vertical-align:top">
													<td style="width:50%; border-right:1px solid gray;">
														<button type="button" class="btn btn-primary btn-lg" style="width:90%; height:90%;" onclick="login();">
															<p style="text;">로그인</p>
														</button>
													</td>
													
													<td style="width:50%;">
														<button type="button" class="btn btn-warning btn-lg" style="width:90%; height:90%;" onclick="location.href='./disposableauth.html'">
															<p style="text;">일회용 자격증명</p>
														</button>
													</td>
													
												</tr>
											</table>
										</div>	
									</div>
								</div>
							</div>
							<div class="col-md-3"></div>
						</div>
						
						<br><p style="text; font-size:1em; color:gray;"> &copy; 김정현(kimdictor@gmail.com) 2017</p>
						
					</div>
				</div>
			</div>
		</div>
		
		<script>
			var sid = "";
			
			function getSID(){
				<?php
					session_start();
					echo "sid="."'".session_id()."';";
				?>
			}
			
			
			function login(){
				window.open("./login.php","로그인","width=462px; height=320px, scrollbars=no, resizeable=no");
			}
			
			document.getElementById("loginform").style.display = "none";
			document.getElementById("loginerrorform").style.display = "none";
			document.getElementById("loginnorreadyform").style.display = "block";
			
			try{
				var wsUrl = 'ws://localhost:81';
				var websocket = new WebSocket(wsUrl);
			}catch(e){}
			
			websocket.onopen = function (evt) { onOpen(evt) };
			websocket.onclose = function (evt) { onClose(evt) };
			websocket.onmessage = function (evt) { onMessage(evt) };
			websocket.onerror = function (evt) { onError(evt) };
					
			function onMessage(evt) {
				if (evt.data == "dummy_checkconn"){
				}else{
					var rawdata = evt.data
					var pdata = rawdata.split(",");
					
					if (pdata[0]=="loginres"){
					}
				}
			}
					
			function onError(evt) {
				document.getElementById("loginform").style.display = "none";
				document.getElementById("loginerrorform").style.display = "block";
				document.getElementById("loginnorreadyform").style.display = "none";
			}
			
			function onClose(evt) { 
				document.getElementById("loginform").style.display = "none";
				document.getElementById("loginerrorform").style.display = "block";
				document.getElementById("loginnorreadyform").style.display = "none";
			}
		</script>
	</body>
</html>

