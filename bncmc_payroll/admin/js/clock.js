/** USE:
<body onload="showTimeSec();"> or <body onload="showTimeMin();">
<p id="tm"></p> or <span id="tm"></span> or <div id="tm"></div> or ...
*/

function showTimeSec(){
  var tmN=new Date();
  var dH=''+tmN.getHours(); dH=dH.length<2?'0'+dH:dH;
  var dM=''+tmN.getMinutes(); dM=dM.length<2?'0'+dM:dM;
  var dS=''+tmN.getSeconds(); dS=dS.length<2?'0'+dS:dS;

	var suffix = "AM";
	if (dH >= 12) {
		suffix = "PM";
		dH = dH - 12;
	}

	if (dH == 0) { dH = 12; }

  var tmp=dH+':'+dM+':'+dS+' '+suffix;
  document.getElementById('tm').innerHTML=tmp;
  var t=setTimeout('showTimeSec()',1000);
}

function showTimeMin(){
  var tmN=new Date();
  var dH=''+tmN.getHours(); dH=dH.length<2?'0'+dH:dH;
  var dM=''+tmN.getMinutes(); dM=dM.length<2?'0'+dM:dM;
  var dS=tmN.getSeconds()%2; dS=dS==0?':':' ';

	var suffix = "AM";
	if (dH >= 12) {
		suffix = "PM";
		dH = dH - 12;
	}

	if (dH == 0) { dH = 12; }

  var tmp=dH+dS+dM+' '+suffix;
  document.getElementById('tm').innerHTML=tmp;
  var t=setTimeout('showTimeMin()',1000);
}
