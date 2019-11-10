function SHA256(r){var n=8,t=0;function e(r,n){var t=(65535&r)+(65535&n);return(r>>16)+(n>>16)+(t>>16)<<16|65535&t}function o(r,n){return r>>>n|r<<32-n}function a(r,n){return r>>>n}return function(r){for(var n=t?"0123456789ABCDEF":"0123456789abcdef",e="",o=0;o<4*r.length;o++)e+=n.charAt(r[o>>2]>>8*(3-o%4)+4&15)+n.charAt(r[o>>2]>>8*(3-o%4)&15);return e}(function(r,n){var t,f,u,h,C,c,i,g,A,d,v,S,l,m,y,w,b,p,B=new Array(1116352408,1899447441,3049323471,3921009573,961987163,1508970993,2453635748,2870763221,3624381080,310598401,607225278,1426881987,1925078388,2162078206,2614888103,3248222580,3835390401,4022224774,264347078,604807628,770255983,1249150122,1555081692,1996064986,2554220882,2821834349,2952996808,3210313671,3336571891,3584528711,113926993,338241895,666307205,773529912,1294757372,1396182291,1695183700,1986661051,2177026350,2456956037,2730485921,2820302411,3259730800,3345764771,3516065817,3600352804,4094571909,275423344,430227734,506948616,659060556,883997877,958139571,1322822218,1537002063,1747873779,1955562222,2024104815,2227730452,2361852424,2428436474,2756734187,3204031479,3329325298),D=new Array(1779033703,3144134277,1013904242,2773480762,1359893119,2600822924,528734635,1541459225),E=new Array(64);r[n>>5]|=128<<24-n%32,r[15+(n+64>>9<<4)]=n;for(var F=0;F<r.length;F+=16){t=D[0],f=D[1],u=D[2],h=D[3],C=D[4],c=D[5],i=D[6],g=D[7];for(var H=0;H<64;H++)E[H]=H<16?r[H+F]:e(e(e(o(p=E[H-2],17)^o(p,19)^a(p,10),E[H-7]),o(b=E[H-15],7)^o(b,18)^a(b,3)),E[H-16]),A=e(e(e(e(g,o(w=C,6)^o(w,11)^o(w,25)),(y=C)&c^~y&i),B[H]),E[H]),d=e(o(m=t,2)^o(m,13)^o(m,22),(v=t)&(S=f)^v&(l=u)^S&l),g=i,i=c,c=C,C=e(h,A),h=u,u=f,f=t,t=e(A,d);D[0]=e(t,D[0]),D[1]=e(f,D[1]),D[2]=e(u,D[2]),D[3]=e(h,D[3]),D[4]=e(C,D[4]),D[5]=e(c,D[5]),D[6]=e(i,D[6]),D[7]=e(g,D[7])}return D}(function(r){for(var t=Array(),e=(1<<n)-1,o=0;o<r.length*n;o+=n)t[o>>5]|=(r.charCodeAt(o/n)&e)<<24-o%32;return t}(r=function(r){r=r.replace(/\r\n/g,"\n");for(var n="",t=0;t<r.length;t++){var e=r.charCodeAt(t);e<128?n+=String.fromCharCode(e):e>127&&e<2048?(n+=String.fromCharCode(e>>6|192),n+=String.fromCharCode(63&e|128)):(n+=String.fromCharCode(e>>12|224),n+=String.fromCharCode(e>>6&63|128),n+=String.fromCharCode(63&e|128))}return n}(r)),r.length*n))}
function setCookie(cName, cValue, cDay){
    var expire = new Date();
    expire.setDate(expire.getDate() + cDay);
    cookies = cName + '=' + escape(cValue) + '; path=/ '; // 한글 깨짐을 막기위해 escape(cValue)를 합니다.
    if(typeof cDay != 'undefined') cookies += ';expires=' + expire.toGMTString() + ';';
    document.cookie = cookies;
}
function getCookie(cName) {
    cName = cName + '=';
    var cookieData = document.cookie;
    var start = cookieData.indexOf(cName);
    var cValue = '';
    if(start != -1){
        start += cName.length;
        var end = cookieData.indexOf(';', start);
        if(end == -1)end = cookieData.length;
        cValue = cookieData.substring(start, end);
    }
    return unescape(cValue);
}
function b64toBlob(b64Data, contentType, sliceSize) {
    contentType = contentType || '';
    sliceSize = sliceSize || 512;

    var byteCharacters = atob(b64Data);
    var byteArrays = [];

    for (var offset = 0; offset < byteCharacters.length; offset += sliceSize) {
        var slice = byteCharacters.slice(offset, offset + sliceSize);

        var byteNumbers = new Array(slice.length);
        for (var i = 0; i < slice.length; i++) {
            byteNumbers[i] = slice.charCodeAt(i);
        }

        var byteArray = new Uint8Array(byteNumbers);

        byteArrays.push(byteArray);
    }

  var blob = new Blob(byteArrays, {type: contentType});
  return blob;
}
function GETApiReq(host, verb, okcb, errorcb) {
    var req = new XMLHttpRequest();
    req.open("GET", host + "/" + verb, true);
    req.onload = function() {
        if (req.status == 200) {
            okcb(req.response);
        } else {
            errorcb();
        }
    };
    req.send();
}
function POSTApiReq(host, verb, param, okcb, errorcb) {
    var req = new XMLHttpRequest();
    req.open("POST", host + "/" + verb, true);
    req.setRequestHeader('Content-Type', 'application/json');
    req.onload = function() {
        if (req.status == 200) {
            okcb(req.response);
        } else {
            errorcb();
        }
    };
    req.send(param);
}